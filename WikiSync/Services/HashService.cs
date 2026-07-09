using System.Security.Cryptography;
using WikiSync.Interfaces;

namespace WikiSync.Services
{
    public class HashService : IHashService
    {
        public string GenerateHash(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found: ", filePath);

            using var sha265 = SHA256.Create();
            using var stream = File.OpenRead(filePath);

            var hashBytes = sha265.ComputeHash(stream);

            return Convert.ToHexString(hashBytes);
        }
    }
}
