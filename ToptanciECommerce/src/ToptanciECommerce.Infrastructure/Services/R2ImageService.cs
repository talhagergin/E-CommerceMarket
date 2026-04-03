using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ToptanciECommerce.Application.Interfaces.Services;

namespace ToptanciECommerce.Infrastructure.Services;

public class R2ImageService : IImageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _publicUrl;
    private readonly ILogger<R2ImageService> _logger;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public R2ImageService(IConfiguration configuration, ILogger<R2ImageService> logger)
    {
        _logger = logger;
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

        _logger.LogInformation(
            "R2 image save start. OriginalName={OriginalName}, Ext={Ext}, Length={Length}, TargetKey={Key}",
            file.FileName,
            ext,
            file.Length,
            fileName);

        try
        {
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

            _logger.LogInformation(
                "R2 PutObject starting. Bucket={Bucket}, Key={Key}, WebpBytes={Bytes}",
                _bucket,
                fileName,
                ms.Length);

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

            var url = $"{_publicUrl}/{fileName}";
            _logger.LogInformation("R2 PutObject succeeded. PublicUrl={Url}", url);
            return url;
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(
                s3Ex,
                "R2 S3 API error. ErrorCode={ErrorCode}, Status={Status}, RequestId={RequestId}, Bucket={Bucket}, Key={Key}",
                s3Ex.ErrorCode,
                s3Ex.StatusCode,
                s3Ex.RequestId,
                _bucket,
                fileName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "R2 image save failed (non-S3). Key={Key}, Message={Message}",
                fileName,
                ex.Message);
            throw;
        }
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
