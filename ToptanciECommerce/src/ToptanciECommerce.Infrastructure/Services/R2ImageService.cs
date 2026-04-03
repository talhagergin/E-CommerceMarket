using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ToptanciECommerce.Application.Interfaces.Services;

namespace ToptanciECommerce.Infrastructure.Services;

public class R2ImageService : IImageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _publicUrl;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public R2ImageService(IConfiguration configuration)
    {
        var r2 = configuration.GetSection("CloudflareR2");
        _bucket = r2["BucketName"]!;
        _publicUrl = r2["PublicUrl"]!.TrimEnd('/');

        _s3 = new AmazonS3Client(
            r2["AccessKeyId"],
            r2["SecretAccessKey"],
            new AmazonS3Config
            {
                ServiceURL = r2["Endpoint"],
                ForcePathStyle = true
            });
    }

    public async Task<string> SaveImageAsync(IFormFile file, string folder, int maxWidth = 1200)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"Desteklenmeyen dosya türü: {ext}");

        var fileName = $"{folder}/{Guid.NewGuid()}.webp";

        using var image = await Image.LoadAsync(file.OpenReadStream());

        if (image.Width > maxWidth)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, 0),
                Mode = ResizeMode.Max
            }));
        }

        using var ms = new MemoryStream();
        await image.SaveAsWebpAsync(ms);
        ms.Position = 0;

        // R2 does not support S3 ACLs. R2 also does not support the streaming SigV4 + default
        // checksum behavior used by recent AWSSDK.S3 — see Cloudflare R2 aws-sdk-net example.
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = fileName,
            InputStream = ms,
            ContentType = "image/webp",
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        });

        return $"{_publicUrl}/{fileName}";
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;

        // Extract key from full URL: https://pub.../folder/file.webp → folder/file.webp
        var key = imageUrl.Replace(_publicUrl + "/", "").TrimStart('/');
        if (string.IsNullOrWhiteSpace(key)) return;

        try
        {
            await _s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = key
            });
        }
        catch
        {
            // Non-critical — ignore if object doesn't exist
        }
    }
}
