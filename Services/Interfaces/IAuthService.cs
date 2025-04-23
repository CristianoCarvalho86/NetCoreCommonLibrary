using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> HashPasswordAsync(string password);
        Task<bool> VerifyPasswordAsync(string password, string hashedPassword);
    }
} 