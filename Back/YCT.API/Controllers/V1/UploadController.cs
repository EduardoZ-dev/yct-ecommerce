using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.Common;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class UploadController : ControllerBase
{
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("image")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ResponseBase<string>.Fail("No se recibió ningún archivo"));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ResponseBase<string>.Fail("La imagen excede 2 MB"));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(ResponseBase<string>.Fail("Formato no permitido. Use JPG, PNG, WEBP o GIF"));

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(ResponseBase<string>.Fail("Tipo de archivo inválido"));

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(folder);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/products/{fileName}";
        return Ok(ResponseBase<string>.Ok(relativeUrl, "Imagen subida exitosamente"));
    }
}
