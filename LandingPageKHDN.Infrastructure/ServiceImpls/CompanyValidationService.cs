using LandingPageKHDN.Application.Interfaces;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.ViewModels;
using LandingPageKHDN.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Infrastructure.ServiceImpls
{
    public class CompanyValidationService : ICompanyValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CompanyValidationService> _logger;

        public CompanyValidationService(IUnitOfWork unitOfWork,
                                        ILogger<CompanyValidationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public bool IsValidFile(IFormFile? file, string[] allowedExtensions, long maxSize)
        {
            //if (file == null) return false;

            //var ext = Path.GetExtension(file.FileName).ToLower();
            //if (!allowedExtensions.Contains(ext)) return false;

            //if (file.Length > maxSize) return false;

            //return true;

            if (file == null)
            {
                _logger.LogWarning("❌ File is null.");
                return false;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                _logger.LogWarning("❌ Invalid file extension: {Extension}. Allowed: {Allowed}", ext, string.Join(", ", allowedExtensions));
                return false;
            }

            if (file.Length > maxSize)
            {
                _logger.LogWarning("❌ File too large: {Size} bytes. Max allowed: {MaxSize} bytes.", file.Length, maxSize);
                return false;
            }

            _logger.LogInformation("✅ Valid file: {FileName}, Size: {Size} bytes, Extension: {Extension}", file.FileName, file.Length, ext);
            return true;
        }

        public async Task<Dictionary<string, string[]>> GetDuplicateFieldErrorsAsync(CompanyRegistration entity)
        {
            var existingRecords = await _unitOfWork.CompanyRegistrations
                .FindAsync(x =>
                    x.CompanyName == entity.CompanyName ||
                    x.TaxCode == entity.TaxCode ||
                    x.PhoneNumber == entity.PhoneNumber ||
                    x.Email == entity.Email ||
                    x.LegalRepId == entity.LegalRepId);

            var errors = new Dictionary<string, string[]>();

            if (existingRecords.Any(x => x.CompanyName == entity.CompanyName))
                errors["CompanyName"] = new[] { "Tên doanh nghiệp đã tồn tại." };

            if (existingRecords.Any(x => x.TaxCode == entity.TaxCode))
                errors["TaxCode"] = new[] { "Mã số thuế đã tồn tại." };

            if (existingRecords.Any(x => x.PhoneNumber == entity.PhoneNumber))
                errors["PhoneNumber"] = new[] { "Số điện thoại đã tồn tại." };

            if (existingRecords.Any(x => x.Email == entity.Email))
                errors["Email"] = new[] { "Email đã tồn tại." };

            if (existingRecords.Any(x => x.LegalRepId == entity.LegalRepId))
                errors["LegalRepId"] = new[] { "CMND/CCCD đã tồn tại." };

            return errors;
        }

        public async Task<Dictionary<string, string[]>> GetDuplicateFieldErrorAsync(CompanyRegistrationUpdateViewModel entity, int excludeId)
        {
            var existingRecords = await _unitOfWork.CompanyRegistrations
                .FindAsync(x =>
                    x.Id != excludeId &&
                    (
                        x.CompanyName == entity.CompanyName ||
                        x.TaxCode == entity.TaxCode ||
                        x.PhoneNumber == entity.PhoneNumber ||
                        x.Email == entity.Email ||
                        x.LegalRepId == entity.LegalRepId
                    )
                );

            var errors = new Dictionary<string, string[]>();

            if (existingRecords.Any(x => x.CompanyName == entity.CompanyName))
                errors["CompanyName"] = new[] { "Tên doanh nghiệp đã tồn tại." };

            if (existingRecords.Any(x => x.TaxCode == entity.TaxCode))
                errors["TaxCode"] = new[] { "Mã số thuế đã tồn tại." };

            if (existingRecords.Any(x => x.PhoneNumber == entity.PhoneNumber))
                errors["PhoneNumber"] = new[] { "Số điện thoại đã tồn tại." };

            if (existingRecords.Any(x => x.Email == entity.Email))
                errors["Email"] = new[] { "Email đã tồn tại." };

            if (existingRecords.Any(x => x.LegalRepId == entity.LegalRepId))
                errors["LegalRepId"] = new[] { "CMND/CCCD đã tồn tại." };

            return errors;
        }

    }
}
