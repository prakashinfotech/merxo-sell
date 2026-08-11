using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MerxoSell.API.Services.Interfaces;

public interface IMediaService
{
    /// <summary>
    /// Takes an image URL or data. If it's an external URL, downloads it and saves it locally.
    /// Returns the local URL.
    /// </summary>
    Task<string> ProcessImageAsync(string imageUrl);
    
    /// <summary>
    /// Saves an uploaded file and returns its URL.
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file);
}
