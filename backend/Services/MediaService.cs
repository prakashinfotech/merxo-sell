using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class MediaService : IMediaService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MediaService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;

    public MediaService(
        IWebHostEnvironment env, 
        ILogger<MediaService> logger,
        IHttpContextAccessor httpContextAccessor,
        HttpClient httpClient)
    {
        _env = env;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
    }

    public async Task<string> ProcessImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return imageUrl;

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";

        // If it's already a local URL, don't download it again
        if (!string.IsNullOrEmpty(baseUrl) && imageUrl.StartsWith(baseUrl))
        {
            return imageUrl;
        }

        // If it's not a URL (e.g. data:image), we might want to handle it too, 
        // but for now focus on external URLs and already local ones.
        if (!imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return imageUrl;
        }

        try
        {
            _logger.LogInformation("Downloading external image for backup: {Url}", imageUrl);
            var response = await _httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download image from {Url}: {Status}", imageUrl, response.StatusCode);
                return imageUrl; // Fallback to original URL if download fails
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            var ext = GetExtensionFromContentType(contentType) ?? ".jpg";
            
            var fileName = $"{Guid.NewGuid()}{ext}";
            var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileName);
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }

            return $"{baseUrl}/uploads/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image from URL: {Url}", imageUrl);
            return imageUrl;
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) return string.Empty;

        // Map known extensions; treat .jfif and .jpe as .jpg; default unknown to .jpg.
        var allowed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg",  ".jpg"  },
            { ".jpeg", ".jpg"  },
            { ".jfif", ".jpg"  },
            { ".jpe",  ".jpg"  },
            { ".png",  ".png"  },
            { ".webp", ".webp" },
            { ".gif",  ".gif"  },
        };
        var inputExt = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
        var ext = allowed.TryGetValue(inputExt, out var mapped) ? mapped : ".jpg";

        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";
        return $"{baseUrl}/uploads/{fileName}";
    }

    private string? GetExtensionFromContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return null;
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => null
        };
    }
}
