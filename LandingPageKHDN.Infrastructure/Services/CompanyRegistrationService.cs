using LandingPageKHDN.Application.Common;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Domain.Entities;
using LandingPageKHDN.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Infrastructure.Services
{
    public class CompanyRegistrationService : ICompanyRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly IRecaptchaService _recaptchaService;
        private readonly IEmailService _emailService;
        private readonly ILogger<CompanyRegistrationService> _logger;

        public CompanyRegistrationService(
            IUnitOfWork unitOfWork,
            IFirebaseStorageService firebaseStorageService,
            IRecaptchaService recaptchaService,
            IEmailService emailService,
            ILogger<CompanyRegistrationService> logger)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
            _recaptchaService = recaptchaService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ResponseModel<string>> RegisterCompanyAsync(
            CompanyRegistration model,
            IFormFile businessLicenseFile,
            IFormFile legalRepIDFile,
            string recaptchaToken)
        {
            try
            {
                // 1. Xác thực reCAPTCHA
                var recaptchaResult = await _recaptchaService.IsValidAsync(recaptchaToken);
                if (!recaptchaResult.Success || recaptchaResult.Data == false)
                {
                    return ResponseModel<string>.FailureResult("Xác thực reCAPTCHA không thành công.");
                }

                // 2. Kiểm tra file hợp lệ
                string[] allowedExts = [".pdf", ".jpg", ".jpeg", ".png"];
                if (!IsValidFile(businessLicenseFile, allowedExts) || !IsValidFile(legalRepIDFile, allowedExts))
                {
                    return ResponseModel<string>.FailureResult("Tệp không hợp lệ.");
                }

                // 3. Upload lên Firebase Storage
                string licenseFileName = Guid.NewGuid() + Path.GetExtension(businessLicenseFile.FileName);
                string idFileName = Guid.NewGuid() + Path.GetExtension(legalRepIDFile.FileName);

                string licenseUrl, idUrl;

                using (var licenseStream = businessLicenseFile.OpenReadStream())
                {
                    var uploadResult = await _firebaseStorageService.UploadFileAsync(
                        licenseStream, licenseFileName, businessLicenseFile.ContentType);

                    if (!uploadResult.Success)
                        return ResponseModel<string>.FailureResult("Tải lên giấy phép kinh doanh thất bại.");

                    licenseUrl = uploadResult.Data;
                }

                using (var idStream = legalRepIDFile.OpenReadStream())
                {
                    var uploadResult = await _firebaseStorageService.UploadFileAsync(
                        idStream, idFileName, legalRepIDFile.ContentType);

                    if (!uploadResult.Success)
                        return ResponseModel<string>.FailureResult("Tải lên CMND/CCCD người đại diện thất bại.");

                    idUrl = uploadResult.Data;
                }

                await _unitOfWork.BeginTransactionAsync();

                // 4. Lưu thông tin đăng ký
                model.BusinessLicenseFilePath = licenseUrl;
                model.LegalRepIdfilePath = idUrl;
                model.CreatedAt = DateTime.Now;

                await _unitOfWork.CompanyRegistrations.AddAsync(model);

                // 5. Ghi log
                var log = new ActionLog
                {
                    AdminId = null,
                    Action = $"Khách hàng đăng ký: {model.CompanyName} - MST: {model.TaxCode}",
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.ActionLogs.AddAsync(log);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                // 6. Gửi email
                await _emailService.SendEmailAsync(model.Email, model.CompanyName);

                return ResponseModel<string>.SuccessResult("Đăng ký thành công");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lỗi khi xử lý đăng ký khách hàng");
                return ResponseModel<string>.FailureResult("Đã xảy ra lỗi. Vui lòng thử lại sau.");
            }
        }

        private bool IsValidFile(IFormFile file, string[] allowedExts)
        {
            if (file == null) return false;
            var ext = Path.GetExtension(file.FileName).ToLower();
            return allowedExts.Contains(ext);
        }
    }
}
