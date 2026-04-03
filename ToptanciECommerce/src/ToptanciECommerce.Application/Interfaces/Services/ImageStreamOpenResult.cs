namespace ToptanciECommerce.Application.Interfaces.Services;

/// <summary>Result of opening a stored image for streaming (e.g. R2 GetObject).</summary>
public sealed class ImageStreamOpenResult(Stream stream, string contentType)
{
    public Stream Stream { get; } = stream;
    public string ContentType { get; } = contentType;
}
