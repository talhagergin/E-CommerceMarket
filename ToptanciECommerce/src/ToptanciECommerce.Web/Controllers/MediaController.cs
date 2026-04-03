using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.Interfaces.Services;

namespace ToptanciECommerce.Web.Controllers;

/// <summary>Streams R2 objects through the app (same-origin) for browsers that fail TLS to pub-*.r2.dev.</summary>
public class MediaController : Controller
{
    private readonly IImageService _imageService;

    public MediaController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet("/media/r2/{**objectKey}")]
    [ResponseCache(Duration = 604800, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> R2(string objectKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(objectKey) || objectKey.Contains("..", StringComparison.Ordinal))
            return NotFound();

        if (!objectKey.StartsWith("products/", StringComparison.OrdinalIgnoreCase)
            && !objectKey.StartsWith("categories/", StringComparison.OrdinalIgnoreCase))
            return NotFound();

        var result = await _imageService.TryOpenReadByObjectKeyAsync(objectKey, cancellationToken);
        if (result == null) return NotFound();

        Response.Headers.CacheControl = "public,max-age=604800";
        return File(result.Stream, result.ContentType, enableRangeProcessing: true);
    }
}
