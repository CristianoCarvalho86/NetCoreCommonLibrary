using System.IO;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services.Interfaces
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(string key, Stream fileStream, string contentType);
        Task<byte[]> DownloadFileAsync(string key);
        Task<bool> DeleteFileAsync(string key);
        string GetFileUrl(string key);
    }
} 