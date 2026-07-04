using CSharp_Interactive_Learning_App.Shared.Services;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CSharp_Interactive_Learning_App.WinUI.Services
{
    public class SecureStorage : ISecureStorage
    {
        private static readonly ApplicationDataContainer _settings =
            ApplicationData.Current.LocalSettings;

        // The "LOCAL=user" descriptor guarantees that only this specific logged-in 
        // Windows user can decrypt the value. Other accounts on this PC are blocked.
        private const string ProtectionDescriptor = "LOCAL=user";

        public async Task SetAsync(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _settings.Values.Remove(key);
                return;
            }

            try
            {
                // Convert the plain text string into a WinRT binary buffer
                IBuffer inputBuffer = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);

                // Initialize the native Windows Data Protection Provider
                var provider = new DataProtectionProvider(ProtectionDescriptor);

                // Encrypt the buffer asynchronously using DPAPI
                IBuffer encryptedBuffer = await provider.ProtectAsync(inputBuffer);

                // Convert the secure encrypted buffer into a safe Base64 string format
                string base64EncryptedValue = CryptographicBuffer.EncodeToBase64String(encryptedBuffer);

                // Store the encrypted string inside your normal LocalSettings database
                _settings.Values[key] = base64EncryptedValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Secure write failure for key '{key}': {ex.Message}");
                throw;
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            // Check if the target storage key even exists
            if (!_settings.Values.ContainsKey(key) || _settings.Values[key] is not string base64Input)
            {
                return null;
            }

            try
            {
                // Reconstitute the base64 string container back into a binary buffer
                IBuffer encryptedBuffer = CryptographicBuffer.DecodeFromBase64String(base64Input);

                // Initialize an empty provider pass (it reads metadata out of the encrypted buffer automatically)
                var provider = new DataProtectionProvider();

                // Decrypt the binary blob back into plain text bits asynchronously
                IBuffer decryptedBuffer = await provider.UnprotectAsync(encryptedBuffer);

                // Translate the bytes back into a native C# readable string layout
                return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptedBuffer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Secure read failure for key '{key}': {ex.Message}");
                return null;
            }
        }

        public async Task DeleteAsync(string key)
        {
            if (_settings.Values.ContainsKey(key))
            {
                _settings.Values.Remove(key);
            }
        }

        public void Clear()
        {
            _settings.Values.Clear();
        }
    }
}
