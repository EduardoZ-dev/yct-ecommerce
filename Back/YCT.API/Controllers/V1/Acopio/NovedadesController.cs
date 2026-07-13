using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.API.Controllers.V1.Acopio;

/// <summary>
/// Novedades que los choferes reportan DURANTE la ruta (llanta averiada, trancón, finca
/// sin ordeño…). Llegan ANTES que el descargue: sirven para entender por qué un camión
/// viene retrasado o por qué va a faltar leche, sin esperar a que termine el recorrido.
/// </summary>
[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class NovedadesController : ControllerBase
{
    private readonly IGenericRepository<RutaNovedad> _repo;
    private readonly IGenericRepository<Conductor> _conductorRepo;
    private readonly IGenericRepository<Camion> _camionRepo;
    private readonly IGenericRepository<GranjeroCodigo> _codigoRepo;
    private readonly IGenericRepository<Ruta> _rutaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public NovedadesController(
        IGenericRepository<RutaNovedad> repo,
        IGenericRepository<Conductor> conductorRepo,
        IGenericRepository<Camion> camionRepo,
        IGenericRepository<GranjeroCodigo> codigoRepo,
        IGenericRepository<Ruta> rutaRepo,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _conductorRepo = conductorRepo;
        _camionRepo = camionRepo;
        _codigoRepo = codigoRepo;
        _rutaRepo = rutaRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>Lista de novedades, de la más reciente a la más antigua.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int dias = 30)
    {
        var desde = ColombiaTime.Today.AddDays(-Math.Clamp(dias, 1, 365));
        var novedades = (await _repo.FindAsync(n => n.ReportadoAt >= desde))
            .OrderByDescending(n => n.ReportadoAt)
            .ToList();

        if (novedades.Count == 0)
            return Ok(ResponseBase<List<NovedadDto>>.Ok(new List<NovedadDto>()));

        var conductores = (await _conductorRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.NombreCompleto);
        var camiones = (await _camionRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Nombre);
        var codigos = (await _codigoRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);
        var rutaIds = novedades.Where(n => n.RutaId.HasValue).Select(n => n.RutaId!.Value).ToHashSet();
        var rutas = rutaIds.Count == 0
            ? new Dictionary<int, Ruta>()
            : (await _rutaRepo.FindAsync(r => rutaIds.Contains(r.Id))).ToDictionary(r => r.Id, r => r);

        var dtos = novedades.Select(n =>
        {
            var codigo = n.GranjeroCodigoId.HasValue && codigos.TryGetValue(n.GranjeroCodigoId.Value, out var c) ? c : null;
            var ruta = n.RutaId.HasValue && rutas.TryGetValue(n.RutaId.Value, out var r) ? r : null;
            return new NovedadDto
            {
                Id = n.Id,
                Categoria = n.Categoria,
                Tipo = n.Tipo,
                Descripcion = n.Descripcion,
                Finca = codigo?.Finca,
                CodigoFinca = codigo?.Codigo,
                ConductorNombre = conductores.TryGetValue(n.ConductorId, out var cn) ? cn : $"#{n.ConductorId}",
                CamionNombre = camiones.TryGetValue(n.CamionId, out var kn) ? kn : $"#{n.CamionId}",
                ReportadoAt = n.ReportadoAt,
                GpsLat = n.GpsLat,
                GpsLng = n.GpsLng,
                RutaId = n.RutaId,
                RutaCodigo = ruta?.Codigo,
                RutaStatus = ruta?.Status,
                Revisada = n.Revisada,
                RevisadaAt = n.RevisadaAt,
                RevisadaPor = n.RevisadaPor
            };
        }).ToList();

        return Ok(ResponseBase<List<NovedadDto>>.Ok(dtos));
    }

    /// <summary>Marca la novedad como atendida (o la reabre).</summary>
    [HttpPost("{id}/revisar")]
    public async Task<IActionResult> Revisar(int id, [FromQuery] bool revisada = true)
    {
        var novedad = await _repo.GetByIdAsync(id);
        if (novedad == null) return NotFound(ResponseBase<object>.Fail("Novedad no encontrada"));

        novedad.Revisada = revisada;
        novedad.RevisadaAt = revisada ? ColombiaTime.Now : null;
        novedad.RevisadaPor = revisada ? (_currentUser.FullName ?? _currentUser.Username) : null;
        novedad.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(novedad);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ResponseBase<object>.Ok(new { novedad.Id, novedad.Revisada },
            revisada ? "Novedad marcada como revisada" : "Novedad reabierta"));
    }
}
