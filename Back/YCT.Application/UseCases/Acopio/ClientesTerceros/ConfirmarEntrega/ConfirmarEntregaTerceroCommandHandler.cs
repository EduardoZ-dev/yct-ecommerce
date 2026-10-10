using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;

public class ConfirmarEntregaTerceroCommandHandler : IRequestHandler<ConfirmarEntregaTerceroCommand, ResponseBase<EntregaTerceroDto>>
{
    private const int MaxObservacion = 500;

    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IColaAvisos _avisos;
    private readonly TimeProvider _reloj;

    public ConfirmarEntregaTerceroCommandHandler(
        IGenericRepository<EntregaTercero> entregaRepository,
        IGenericRepository<Ruta> rutaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IColaAvisos avisos,
        TimeProvider reloj)
    {
        _entregaRepository = entregaRepository;
        _rutaRepository = rutaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _currentUser = currentUser;
        _avisos = avisos;
        _reloj = reloj;
    }

    public async Task<ResponseBase<EntregaTerceroDto>> Handle(ConfirmarEntregaTerceroCommand request, CancellationToken cancellationToken)
    {
        // Se incluye el cliente para no pedir su nombre en una segunda consulta.
        var coincidencias = await _entregaRepository.FindAsync(e => e.Id == request.Id, e => e.ClienteTercero);
        var entrega = coincidencias.FirstOrDefault();
        if (entrega == null)
            return ResponseBase<EntregaTerceroDto>.NotFound("Entrega no encontrada");

        var ahora = _reloj.GetUtcNow().UtcDateTime;
        if (request.DesdePlanta && !EntregaTercero.SePuedeConfirmarEnPlanta(entrega.Fecha, ColombiaTime.FromUtc(ahora)))
            return ResponseBase<EntregaTerceroDto>.Fail(
                $"Esa entrega es de hace más de {EntregaTercero.DiasParaConfirmarEnPlanta} días: se confirma desde el panel");

        // Idempotente: un reenvío no altera quién confirmó, cuándo, ni lo que midió.
        if (entrega.ConfirmadaEnPlantaAt.HasValue)
            return ResponseBase<EntregaTerceroDto>.Ok(await ToDtoAsync(entrega), "La entrega ya estaba confirmada");

        var error = request.Medicion is null ? null : ValidarMedicion(request.Medicion);
        if (error != null)
            return ResponseBase<EntregaTerceroDto>.Fail(error);

        if (request.Medicion is not null)
            AplicarMedicion(entrega, request.Medicion);
        MarcarConfirmada(entrega, ahora);
        await _entregaRepository.UpdateAsync(entrega);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await AuditarAsync(entrega, cancellationToken);

        // El WhatsApp sale en segundo plano: confirmar nunca espera a Meta ni falla por ella.
        if (entrega.LitrosPlanta.HasValue)
        {
            var aviso = new EntregaTerceroConfirmada(entrega.Id);
            _avisos.Encolar<IPublisher>($"WhatsApp de la entrega de tercero {entrega.Id}",
                (publisher, ct) => publisher.Publish(aviso, ct));
        }

        return ResponseBase<EntregaTerceroDto>.Ok(await ToDtoAsync(entrega), "Llegada confirmada");
    }

    private static string? ValidarMedicion(MedicionTercero medicion)
    {
        var errorMedida = EntregaTercero.ValidarMedida(medicion.Cantinas, medicion.SaldoLitros);
        if (errorMedida != null)
            return errorMedida;
        if ((medicion.Observacion?.Trim().Length ?? 0) > MaxObservacion)
            return $"La observación admite hasta {MaxObservacion} caracteres";
        return null;
    }

    private static void AplicarMedicion(EntregaTercero entrega, MedicionTercero medicion)
    {
        entrega.CantinasPlanta = medicion.Cantinas;
        entrega.SaldoPlanta = medicion.SaldoLitros;
        entrega.LitrosPlanta = EntregaTercero.CalcularLitros(medicion.Cantinas, medicion.SaldoLitros);
        entrega.ObservacionPlanta = string.IsNullOrWhiteSpace(medicion.Observacion) ? null : medicion.Observacion.Trim();
    }

    private void MarcarConfirmada(EntregaTercero entrega, DateTime ahora)
    {
        entrega.ConfirmadaEnPlantaAt = ahora;
        entrega.ConfirmadaPorUserId = _currentUser.UserId;
        entrega.ConfirmadaPorNombre = _currentUser.FullName ?? _currentUser.Username;
        entrega.UpdatedAt = ahora;
    }

    private Task AuditarAsync(EntregaTercero entrega, CancellationToken cancellationToken)
    {
        var medido = entrega.LitrosPlanta.HasValue
            ? $"recibido {entrega.LitrosPlanta:0.##} L de {entrega.Litros:0.##} L registrados"
            : $"{entrega.Litros:0.##} L registrados, sin medir";
        return _audit.LogAsync("ConfirmarEntrega", "EntregaTercero", entrega.Id,
            $"Llegada confirmada en planta: {entrega.ClienteTercero.NombreCompleto}, {medido} ({entrega.Fecha:yyyy-MM-dd})",
            new
            {
                entrega.ClienteTerceroId, entrega.Fecha, entrega.Litros, entrega.LitrosPlanta,
                entrega.CantinasPlanta, entrega.SaldoPlanta, entrega.ObservacionPlanta,
                entrega.ConfirmadaEnPlantaAt, entrega.ConfirmadaPorNombre
            },
            ct: cancellationToken);
    }

    /// <summary>Resuelve el código de la ruta cuando la entrega llegó en un descargue (registro desde tablet).</summary>
    private async Task<EntregaTerceroDto> ToDtoAsync(EntregaTercero entrega)
    {
        var ruta = entrega.RutaId.HasValue ? await _rutaRepository.GetByIdAsync(entrega.RutaId.Value) : null;
        return ClientesTercerosMapper.ToDto(entrega, entrega.ClienteTercero.NombreCompleto, ruta?.Codigo);
    }
}
