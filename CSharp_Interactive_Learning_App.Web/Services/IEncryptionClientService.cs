namespace CSharp_Interactive_Learning_App.Web.Services
{
    public interface IEncryptionClientService
    {
        public Task InitializeAsync();
        public string Encrypt(string plainText);
        public string Decrypt(string cipherText);
    }
}
