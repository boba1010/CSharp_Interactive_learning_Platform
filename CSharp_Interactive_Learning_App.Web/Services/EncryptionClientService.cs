using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;
using System.Text;

namespace CSharp_Interactive_Learning_App.Web.Services
{
    public class EncryptionClientService(ProtectedLocalStorage protectedLocalStorage) : IEncryptionClientService
    {
        private const string SaltKey = "encryption_salt";
        private byte[] _salt;
        private byte[] _key;

        public async Task InitializeAsync()
        {
            string? keyString = Environment.GetEnvironmentVariable("JWT_KEY");
            Console.WriteLine(keyString);
            var result = await protectedLocalStorage.GetAsync<string?>(SaltKey);
            var storedSalt = result.Value;

            if (storedSalt == null)
            {
                _salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(_salt);
                }
                await protectedLocalStorage.SetAsync(SaltKey, Convert.ToBase64String(_salt));
            }
            else
            {
                _salt = Convert.FromBase64String(storedSalt);
            }


            _key = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(keyString),
                _salt,
                10000,
                HashAlgorithmName.SHA256,
                32
            );
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            var buffer = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = _key;
            var iv = new byte[aes.IV.Length];
            Array.Copy(buffer, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
