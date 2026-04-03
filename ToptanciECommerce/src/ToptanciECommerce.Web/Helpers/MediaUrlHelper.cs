using Microsoft.Extensions.Configuration;

namespace ToptanciECommerce.Web.Helpers;

/// <summary>
/// Rewrites R2 public URLs to same-origin /media/r2/... so the browser talks TLS only to your app
/// (avoids net::ERR_SSL_PROTOCOL_ERROR to pub-*.r2.dev on some clients). Server fetches bytes from R2 via S3 API.
/// </summary>
public static class MediaUrlHelper
{
    public static bool UseAppDelivery(IConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config["CloudflareR2:Endpoint"])) return false;
        var s = config["CloudflareR2:DeliverImagesViaApp"];
        if (bool.TryParse(s, out var explicitVal)) return explicitVal;
        return true;
    }

    public static string ForHtmlImg(string? storedUrl, IConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(storedUrl)) return "";
        if (!UseAppDelivery(config)) return storedUrl;

        var pub = config["CloudflareR2:PublicUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(pub)) return storedUrl;

        if (storedUrl.StartsWith("/media/r2/", StringComparison.OrdinalIgnoreCase)) return storedUrl;

        if (!storedUrl.StartsWith(pub + "/", StringComparison.OrdinalIgnoreCase)) return storedUrl;

        return "/media/r2/" + storedUrl[(pub.Length + 1)..];
    }
}
