using Microsoft.AspNetCore.Http;

namespace ToptanciECommerce.Application.Interfaces.Services;

public interface IImageService
{
    /// <summary>
    /// Saves an uploaded image, optimizes it (resize + compress) and returns the relative URL.
    /// </summary>
    Task<string> SaveImageAsync(IFormFile file, string folder, int maxWidth = 1200);

    /// <summary>Deletes an image by its relative URL.</summary>
    Task DeleteImageAsync(string imageUrl);
}
