using LandingPageKHDN.Application.Services;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandingPageKHDN.Application.Common;

namespace LandingPageKHDN.Infrastructure.ServiceImpls
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly IConfiguration _config;
        private readonly StorageClient _storageClient;
        private readonly string _bucket;

        public FirebaseStorageService(IConfiguration config)
        {
            _config = config;
            _bucket = _config["Firebase:Bucket"];
            // string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), _config["Firebase:CredentialPath"]);
            string credentialPath = Path.Combine(AppContext.BaseDirectory, _config["Firebase:CredentialPath"]);

            var credential = GoogleCredential.FromFile(credentialPath);
            _storageClient = StorageClient.Create(credential);
        }

        //public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        //{
        //    string token = Guid.NewGuid().ToString();

        //    var storageObject = new Google.Apis.Storage.v1.Data.Object
        //    {
        //        Bucket = _bucket,
        //        Name = fileName,
        //        ContentType = contentType,
        //        Metadata = new Dictionary<string, string>
        //        {
        //            { "firebaseStorageDownloadTokens", token }
        //        }
        //    };

        //    await _storageClient.UploadObjectAsync(storageObject, fileStream);

        //    string urlEncodedFileName = Uri.EscapeDataString(fileName);
        //    return $"https://firebasestorage.googleapis.com/v0/b/{_bucket}/o/{urlEncodedFileName}?alt=media&token={token}";
        //}
        public async Task<ResponseModel<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                string token = Guid.NewGuid().ToString();

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

                await _storageClient.UploadObjectAsync(storageObject, fileStream);

                string urlEncodedFileName = Uri.EscapeDataString(fileName);
                string downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucket}/o/{urlEncodedFileName}?alt=media&token={token}";

                return ResponseModel<string>.SuccessResult(downloadUrl, "Upload ảnh thành công");
            }
            catch (Exception ex)
            {
                return ResponseModel<string>.FailureResult("Không thể upload file lên Firebase: " + ex.Message);
            }
        }
    }
}
