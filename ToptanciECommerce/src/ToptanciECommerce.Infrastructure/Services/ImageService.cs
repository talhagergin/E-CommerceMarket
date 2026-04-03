using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ToptanciECommerce.Application.Interfaces.Services;

namespace ToptanciECommerce.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public ImageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImageAsync(IFormFile file, string folder, int maxWidth = 1200)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"Desteklenmeyen dosya türü: {ext}");

        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsRoot);

        // Always save as WebP for optimal compression
        var fileName = $"{Guid.NewGuid()}.webp";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());

        // Resize keeping aspect ratio if wider than maxWidth
        if (image.Width > maxWidth)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, 0),
                Mode = ResizeMode.Max
            }));
        }

        await image.SaveAsWebpAsync(fullPath);

        return $"/uploads/{folder}/{fileName}";
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return Task.CompletedTask;

        var physicalPath = Path.Combine(_env.WebRootPath,
            imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        return Task.CompletedTask;
    }
}
