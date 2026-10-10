using MediatR;
using Microsoft.Extensions.Configuration;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;

public class ConfirmarEntregaTerceroCommandHandler : IRequestHandler<ConfirmarEntregaTerceroCommand, ResponseBase<EntregaTerceroDto>>
{
    private const int MaxCantinas = 999;
    private const int MaxObservacion = 500;

    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IWhatsAppNotifier _whatsApp;
    private readonly IConfiguration _config;

    public ConfirmarEntregaTerceroCommandHandler(
        IGenericRepository<EntregaTercero> entregaRepository,
        IGenericRepository<Ruta> rutaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IWhatsAppNotifier whatsApp,
        IConfiguration config)
    {
        _entregaRepository = entregaRepository;
        _rutaRepository = rutaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _currentUser = currentUser;
        _whatsApp = whatsApp;
        _config = config;
    }

    public async Task<ResponseBase<EntregaTerceroDto>> Handle(ConfirmarEntregaTerceroCommand request, CancellationToken cancellationToken)
    {
        // Se incluye el cliente para no pedir su nombre en una segunda consulta.
        var coincidencias = await _entregaRepository.FindAsync(e => e.Id == request.Id, e => e.ClienteTercero);
        var entrega = coincidencias.FirstOrDefault();
        if (entrega == null)
            return ResponseBase<EntregaTerceroDto>.Fail("Entrega no encontrada");

        // Idempotente: un reenvío no altera quién confirmó, cuándo, ni lo que midió.
        if (entrega.ConfirmadaEnPlantaAt.HasValue)
            return ResponseBase<EntregaTerceroDto>.Ok(await ToDtoAsync(entrega), "La entrega ya estaba confirmada");

        if (request.Medicion != null)
        {
            var error = ValidarMedicion(request.Medicion);
            if (error != null)
                return ResponseBase<EntregaTerceroDto>.Fail(error);

            entrega.CantinasPlanta = request.Medicion.Cantinas;
            entrega.SaldoPlanta = request.Medicion.SaldoLitros;
            entrega.LitrosPlanta = request.Medicion.Cantinas * EntregaTercero.LitrosPorCantina + request.Medicion.SaldoLitros;
            entrega.ObservacionPlanta = string.IsNullOrWhiteSpace(request.Medicion.Observacion)
                ? null
                : request.Medicion.Observacion.Trim();
        }

        entrega.ConfirmadaEnPlantaAt = DateTime.UtcNow;
        entrega.ConfirmadaPorUserId = _currentUser.UserId;
        entrega.ConfirmadaPorNombre = _currentUser.FullName ?? _currentUser.Username;
        entrega.UpdatedAt = DateTime.UtcNow;
        await _entregaRepository.UpdateAsync(entrega);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var medido = entrega.LitrosPlanta.HasValue
            ? $"recibido {entrega.LitrosPlanta:0.##} L de {entrega.Litros:0.##} L registrados"
            : $"{entrega.Litros:0.##} L registrados, sin medir";
        await _audit.LogAsync("ConfirmarEntrega", "EntregaTercero", entrega.Id,
            $"Llegada confirmada en planta: {entrega.ClienteTercero.NombreCompleto}, {medido} ({entrega.Fecha:yyyy-MM-dd})",
            new
            {
                entrega.ClienteTerceroId, entrega.Fecha, entrega.Litros, entrega.LitrosPlanta,
                entrega.CantinasPlanta, entrega.SaldoPlanta, entrega.ObservacionPlanta,
                entrega.ConfirmadaEnPlantaAt, entrega.ConfirmadaPorNombre
            },
            ct: cancellationToken);

        // Solo se avisa cuando hay medición: sin ella no hay variación que reportar.
        if (entrega.LitrosPlanta.HasValue)
            await NotificarAsync(entrega, cancellationToken);

        return ResponseBase<EntregaTerceroDto>.Ok(await ToDtoAsync(entrega), "Llegada confirmada");
    }

    private static string? ValidarMedicion(MedicionTercero m)
    {
        if (m.Cantinas < 0)
            return "Las cantinas no pueden ser negativas";
        if (m.Cantinas > MaxCantinas)
            return $"Revisa las cantinas: máximo {MaxCantinas}";
        if (m.SaldoLitros < 0)
            return "El saldo no puede ser negativo";
        if (m.SaldoLitros >= EntregaTercero.LitrosPorCantina)
            return $"El saldo debe ser menor a {EntregaTercero.LitrosPorCantina:0} L: si es una cantina completa, súmala a las cantinas";
        if (m.Cantinas * EntregaTercero.LitrosPorCantina + m.SaldoLitros <= 0)
            return "Indica cuánta leche llegó: cantinas o saldo";
        if ((m.Observacion?.Trim().Length ?? 0) > MaxObservacion)
            return $"La observación admite hasta {MaxObservacion} caracteres";
        return null;
    }

    /// <summary>WhatsApp con la variación. Best-effort: si falla, la confirmación ya quedó guardada.</summary>
    private async Task NotificarAsync(EntregaTercero entrega, CancellationToken cancellationToken)
    {
        try
        {
            var recibido = entrega.LitrosPlanta!.Value;
            var diferencia = recibido - entrega.Litros;
            var resultado = diferencia == 0 ? "✅ Llegó completa"
                : diferencia < 0 ? "🚨 Llegó MENOS leche"
                : "⬆️ Llegó MÁS leche";
            var adminBase = _config["AppUrls:Admin"] ?? "http://localhost:4300";

            await _whatsApp.SendTerceroAsync(new WhatsAppTerceroModel(
                Resultado: resultado,
                Cliente: entrega.ClienteTercero.NombreCompleto,
                Fecha: entrega.Fecha,
                LitrosRegistrados: entrega.Litros,
                LitrosRecibidos: recibido,
                Diferencia: diferencia,
                Observacion: entrega.ObservacionPlanta ?? string.Empty,
                RecibidoPor: entrega.ConfirmadaPorNombre ?? "Planta",
                PanelUrl: $"{adminBase}/clientes-terceros"), cancellationToken);
        }
        catch { /* un WhatsApp que falla nunca deshace ni bloquea la confirmación */ }
    }

    /// <summary>Resuelve el código de la ruta cuando la entrega llegó en un descargue (registro desde tablet).</summary>
    private async Task<EntregaTerceroDto> ToDtoAsync(EntregaTercero entrega)
    {
        var ruta = entrega.RutaId.HasValue ? await _rutaRepository.GetByIdAsync(entrega.RutaId.Value) : null;
        return ClientesTercerosMapper.ToDto(entrega, entrega.ClienteTercero.NombreCompleto, ruta?.Codigo);
    }
}
