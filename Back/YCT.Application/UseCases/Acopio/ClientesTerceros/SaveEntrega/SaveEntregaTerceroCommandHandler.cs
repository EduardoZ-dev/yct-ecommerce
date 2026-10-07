using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.SaveEntrega;

public class SaveEntregaTerceroCommandHandler : IRequestHandler<SaveEntregaTerceroCommand, ResponseBase<EntregaTerceroDto>>
{
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IGenericRepository<ClienteTercero> _clienteRepository;
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public SaveEntregaTerceroCommandHandler(
        IGenericRepository<EntregaTercero> entregaRepository,
        IGenericRepository<ClienteTercero> clienteRepository,
        IGenericRepository<Ruta> rutaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _entregaRepository = entregaRepository;
        _clienteRepository = clienteRepository;
        _rutaRepository = rutaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ResponseBase<EntregaTerceroDto>> Handle(SaveEntregaTerceroCommand request, CancellationToken cancellationToken)
    {
        bool isNew = !request.Id.HasValue || request.Id == 0;

        // Reintento de un envío ya procesado (misma UUID): se devuelve la original sin duplicarla.
        var yaRegistrada = isNew ? await BuscarPorUuidAsync(request.ClientUuid) : null;
        if (yaRegistrada != null)
            return ResponseBase<EntregaTerceroDto>.Ok(
                await ToDtoAsync(yaRegistrada, yaRegistrada.ClienteTercero.NombreCompleto),
                "Entrega ya registrada");

        var error = Validar(request);
        if (error != null)
            return ResponseBase<EntregaTerceroDto>.Fail(error);

        var entrega = isNew
            ? new EntregaTercero { ClienteTerceroId = request.ClienteTerceroId }
            : await _entregaRepository.GetByIdAsync(request.Id!.Value);
        if (entrega == null)
            return ResponseBase<EntregaTerceroDto>.Fail("Entrega no encontrada");

        // Solo las entregas nuevas exigen cliente activo: el histórico de un inactivo sigue siendo editable.
        var cliente = await _clienteRepository.GetByIdAsync(entrega.ClienteTerceroId);
        if (cliente == null || (isNew && !cliente.IsActive))
            return ResponseBase<EntregaTerceroDto>.Fail("El cliente tercero no existe o está inactivo");

        Aplicar(entrega, request, cliente);
        if (isNew)
        {
            MarcarRegistroDesdePanel(entrega, request.ClientUuid);
            await _entregaRepository.AddAsync(entrega);
        }
        else
        {
            entrega.UpdatedAt = DateTime.UtcNow;
            await _entregaRepository.UpdateAsync(entrega);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(isNew ? "Create" : "Update", "EntregaTercero", entrega.Id,
            $"Entrega de tercero {(isNew ? "registrada" : "actualizada")}: {cliente.NombreCompleto}, {entrega.Litros:0.##} L ({entrega.Fecha:yyyy-MM-dd})",
            new { entrega.ClienteTerceroId, entrega.Fecha, entrega.Cantinas, entrega.SaldoLitros, entrega.Litros, entrega.PrecioLitro, entrega.Origen },
            ct: cancellationToken);

        return ResponseBase<EntregaTerceroDto>.Ok(
            await ToDtoAsync(entrega, cliente.NombreCompleto),
            isNew ? "Entrega registrada" : "Entrega actualizada");
    }

    /// <summary>Devuelve el mensaje de error o null si el comando es válido.</summary>
    private static string? Validar(SaveEntregaTerceroCommand request)
    {
        if (request.Cantinas < 0)
            return "Las cantinas no pueden ser negativas";
        if (request.SaldoLitros < 0)
            return "El saldo de litros no puede ser negativo";
        if (request.SaldoLitros >= EntregaTercero.LitrosPorCantina)
            return $"El saldo debe ser menor a {EntregaTercero.LitrosPorCantina:0} L: si es una cantina completa, súmala a las cantinas";
        if (CalcularLitros(request) <= 0)
            return "La entrega debe tener litros: indica cantinas o saldo";
        if (request.PrecioLitro.HasValue && request.PrecioLitro < 0)
            return "El precio por litro no puede ser negativo";
        // Hoy en Colombia: con UTC, de 7 p. m. a medianoche ya se aceptaba la fecha de mañana.
        if (request.Fecha.Date > ColombiaTime.Today)
            return "La fecha de la entrega no puede ser futura";
        return null;
    }

    private static decimal CalcularLitros(SaveEntregaTerceroCommand request)
        => request.Cantinas * EntregaTercero.LitrosPorCantina + request.SaldoLitros;

    private async Task<EntregaTercero?> BuscarPorUuidAsync(Guid? clientUuid)
    {
        if (!clientUuid.HasValue)
            return null;
        var coincidencias = await _entregaRepository.FindAsync(e => e.ClientUuid == clientUuid, e => e.ClienteTercero);
        return coincidencias.FirstOrDefault();
    }

    private static void Aplicar(EntregaTercero entrega, SaveEntregaTerceroCommand request, ClienteTercero cliente)
    {
        entrega.Fecha = request.Fecha.Date;
        entrega.Cantinas = request.Cantinas;
        entrega.SaldoLitros = request.SaldoLitros;
        entrega.Litros = CalcularLitros(request);
        entrega.PrecioLitro = request.PrecioLitro ?? cliente.PrecioLitro;
        entrega.Observacion = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim();
    }

    /// <summary>Datos que solo se fijan al crear: origen, quién registró y la UUID de idempotencia.</summary>
    private void MarcarRegistroDesdePanel(EntregaTercero entrega, Guid? clientUuid)
    {
        entrega.Origen = EntregaTercero.OrigenPanel;
        entrega.RegistradoPorUserId = _currentUser.UserId;
        entrega.RegistradoPorNombre = _currentUser.FullName ?? _currentUser.Username;
        entrega.ClientUuid = clientUuid;
    }

    /// <summary>Resuelve el código de la ruta cuando la entrega llegó en un descargue (registro desde tablet).</summary>
    private async Task<EntregaTerceroDto> ToDtoAsync(EntregaTercero entrega, string clienteNombre)
    {
        var ruta = entrega.RutaId.HasValue ? await _rutaRepository.GetByIdAsync(entrega.RutaId.Value) : null;
        return ClientesTercerosMapper.ToDto(entrega, clienteNombre, ruta?.Codigo);
    }
}
