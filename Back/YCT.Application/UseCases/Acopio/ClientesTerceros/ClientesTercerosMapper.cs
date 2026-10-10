using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros;

/// <summary>
/// Mapeo entidad → DTO compartido por los casos de uso de clientes terceros, para que
/// listar y guardar devuelvan exactamente la misma forma al frontend.
/// </summary>
public static class ClientesTercerosMapper
{
    /// <param name="entregas">Entregas históricas del cliente; de ellas salen los totales del DTO.</param>
    public static ClienteTerceroDto ToDto(ClienteTercero cliente, IEnumerable<EntregaTercero> entregas)
    {
        var historico = entregas.ToList();
        return new ClienteTerceroDto
        {
            Id = cliente.Id,
            NombreCompleto = cliente.NombreCompleto,
            Cedula = cliente.Cedula,
            Telefono = cliente.Telefono,
            Municipio = cliente.Municipio,
            PrecioLitro = cliente.PrecioLitro,
            Notas = cliente.Notas,
            IsActive = cliente.IsActive,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt,
            TotalEntregas = historico.Count,
            TotalLitros = historico.Sum(e => e.Litros),
            // Max sobre DateTime? devuelve null con lista vacía (sin excepción).
            UltimaEntrega = historico.Max(e => (DateTime?)e.Fecha)
        };
    }

    public static EntregaTerceroDto ToDto(EntregaTercero entrega, string clienteNombre, string? rutaCodigo)
    {
        return new EntregaTerceroDto
        {
            Id = entrega.Id,
            ClienteTerceroId = entrega.ClienteTerceroId,
            ClienteNombre = clienteNombre,
            Fecha = entrega.Fecha,
            Cantinas = entrega.Cantinas,
            SaldoLitros = entrega.SaldoLitros,
            Litros = entrega.Litros,
            PrecioLitro = entrega.PrecioLitro,
            Valor = entrega.PrecioLitro.HasValue ? entrega.Litros * entrega.PrecioLitro.Value : null,
            Observacion = entrega.Observacion,
            Origen = entrega.Origen,
            RutaId = entrega.RutaId,
            RutaCodigo = rutaCodigo,
            RegistradoPorNombre = entrega.RegistradoPorNombre,
            CreatedAt = entrega.CreatedAt,
            Confirmada = entrega.ConfirmadaEnPlantaAt.HasValue,
            ConfirmadaEnPlantaAt = entrega.ConfirmadaEnPlantaAt,
            ConfirmadaPorNombre = entrega.ConfirmadaPorNombre,
            CantinasPlanta = entrega.CantinasPlanta,
            SaldoPlanta = entrega.SaldoPlanta,
            LitrosPlanta = entrega.LitrosPlanta,
            DiferenciaLitros = entrega.LitrosPlanta.HasValue ? entrega.LitrosPlanta.Value - entrega.Litros : null,
            ObservacionPlanta = entrega.ObservacionPlanta
        };
    }
}
