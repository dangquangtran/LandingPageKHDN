using LandingPageKHDN.Application.Common;
using LandingPageKHDN.Application.ViewModels;
using LandingPageKHDN.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Services
{
    public interface ICompanyRegistrationService
    {
       // Task<ResponseModel<string>> RegisterCompanyAsync(CompanyRegistration model, IFormFile businessLicenseFile, IFormFile legalRepIDFile, string recaptchaToken);
        Task<ResponseModel<string>> RegisterCompanyAsync(CompanyRegistrationViewModel viewModel);
    }
}
