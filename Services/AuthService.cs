using NetCoreCommonLibrary.Services.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services
{
    public class AuthService : IAuthService
    {
        public Task<string> HashPasswordAsync(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            
            return Task.FromResult(hash);
        }

        public Task<bool> VerifyPasswordAsync(string password, string hashedPassword)
        {
            var passwordHash = HashPasswordAsync(password).Result;
            return Task.FromResult(passwordHash == hashedPassword);
        }
    }
} 