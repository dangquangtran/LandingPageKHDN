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
    public interface ICompanyValidationService
    {
        bool IsValidFile(IFormFile? file, string[] allowedExtensions, long maxSize);
        Task<Dictionary<string, string[]>> GetDuplicateFieldErrorsAsync(CompanyRegistration entity);
        Task<Dictionary<string, string[]>> GetDuplicateFieldErrorAsync(CompanyRegistrationUpdateViewModel entity, int excludeId);
    }
}
