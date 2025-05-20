using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace LandingPageKHDN.Services
{
    public class FirebaseStorageService
    {
        private readonly IConfiguration _config;
        private readonly StorageClient _storageClient;
        private readonly string _bucket;

        public FirebaseStorageService(IConfiguration config)
        {
            _config = config;
            _bucket = _config["Firebase:Bucket"];
            // Đọc đường dẫn file credential từ appsettings
            string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), _config["Firebase:CredentialPath"]);

            // Load credentials từ file
            var credential = GoogleCredential.FromFile(credentialPath);

            // Tạo StorageClient từ credential
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            string token = Guid.NewGuid().ToString();

            // Tạo object metadata để gán token
            var storageObject = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _bucket,
                Name = fileName,
                ContentType = contentType,
                Metadata = new Dictionary<string, string>
                {
                    { "firebaseStorageDownloadTokens", token }
                }
            };

            // Upload object với metadata
            await _storageClient.UploadObjectAsync(
                storageObject,
                fileStream);

            string urlEncodedFileName = Uri.EscapeDataString(fileName);
            return $"https://firebasestorage.googleapis.com/v0/b/{_bucket}/o/{urlEncodedFileName}?alt=media&token={token}";
        }
    }
}
