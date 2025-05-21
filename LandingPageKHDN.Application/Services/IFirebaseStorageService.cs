using LandingPageKHDN.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Services
{
    public interface IFirebaseStorageService
    {
        Task<ResponseModel<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    }
}
