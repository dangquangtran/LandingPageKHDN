﻿using LandingPageKHDN.Application.Common;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Domain.Entities;
using LandingPageKHDN.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandingPageKHDN.Application.ViewModels;

namespace LandingPageKHDN.Infrastructure.ServiceImpls
{
    public class CompanyRegistrationService : ICompanyRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly IRecaptchaService _recaptchaService;
        private readonly IEmailService _emailService;
        private readonly ICompanyValidationService _companyValidationService;
        private readonly ILogger<CompanyRegistrationService> _logger;
        private readonly INsfwService _nsfwService1;

        public CompanyRegistrationService(
            IUnitOfWork unitOfWork,
            IFirebaseStorageService firebaseStorageService,
            IRecaptchaService recaptchaService,
            IEmailService emailService,
            ILogger<CompanyRegistrationService> logger,
            ICompanyValidationService companyValidationService,
            INsfwService nsfwService1)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
            _recaptchaService = recaptchaService;
            _emailService = emailService;
            _logger = logger;
            _companyValidationService = companyValidationService;
            _nsfwService1 = nsfwService1;
        }

        //public async Task<ResponseModel<string>> RegisterCompanyAsync(
        //    CompanyRegistration model,
        //    IFormFile businessLicenseFile,
        //    IFormFile legalRepIDFile,
        //    string recaptchaToken)
        //{
        //    try
        //    {
        //        // 1. Xác thực reCAPTCHA
        //        var recaptchaResult = await _recaptchaService.IsValidAsync(recaptchaToken);
        //        if (recaptchaResult.Status !=200 || recaptchaResult.Data == false)
        //        {
        //            return ResponseModel<string>.FailureResult("Xác thực reCAPTCHA không thành công.");
        //        }

        //        // 2. Kiểm tra file hợp lệ
        //        string[] allowedExts = [".pdf", ".jpg", ".jpeg", ".png"];
        //        if (!IsValidFile(businessLicenseFile, allowedExts) || !IsValidFile(legalRepIDFile, allowedExts))
        //        {
        //            return ResponseModel<string>.FailureResult("Tệp không hợp lệ.");
        //        }

        //        // 3. Upload lên Firebase Storage
        //        string licenseFileName = Guid.NewGuid() + Path.GetExtension(businessLicenseFile.FileName);
        //        string idFileName = Guid.NewGuid() + Path.GetExtension(legalRepIDFile.FileName);

        //        string licenseUrl, idUrl;

        //        using (var licenseStream = businessLicenseFile.OpenReadStream())
        //        {
        //            var uploadResult = await _firebaseStorageService.UploadFileAsync(
        //                licenseStream, licenseFileName, businessLicenseFile.ContentType);

        //            if (uploadResult.Status != 200)
        //                return ResponseModel<string>.FailureResult("Tải lên giấy phép kinh doanh thất bại.");

        //            licenseUrl = uploadResult.Data;
        //        }

        //        using (var idStream = legalRepIDFile.OpenReadStream())
        //        {
        //            var uploadResult = await _firebaseStorageService.UploadFileAsync(
        //                idStream, idFileName, legalRepIDFile.ContentType);

        //            if (uploadResult.Status != 200)
        //                return ResponseModel<string>.FailureResult("Tải lên CMND/CCCD người đại diện thất bại.");

        //            idUrl = uploadResult.Data;
        //        }

        //        await _unitOfWork.BeginTransactionAsync();

        //        // 4. Lưu thông tin đăng ký
        //        model.BusinessLicenseFilePath = licenseUrl;
        //        model.LegalRepIdfilePath = idUrl;
        //        model.CreatedAt = DateTime.Now;

        //        await _unitOfWork.CompanyRegistrations.AddAsync(model);

        //        // 5. Ghi log
        //        var log = new ActionLog
        //        {
        //            AdminId = null,
        //            Action = $"Khách hàng đăng ký: {model.CompanyName} - MST: {model.TaxCode}",
        //            CreatedAt = DateTime.Now
        //        };
        //        await _unitOfWork.ActionLogs.AddAsync(log);

        //        await _unitOfWork.SaveChangesAsync();
        //        await _unitOfWork.CommitTransactionAsync();
        //        // 6. Gửi email
        //        await _emailService.SendEmailAsync(model.Email, model.CompanyName);

        //        return ResponseModel<string>.SuccessResult( model.Id.ToString(),"Đăng ký thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackTransactionAsync();
        //        _logger.LogError(ex, "Lỗi khi xử lý đăng ký khách hàng");
        //        return ResponseModel<string>.FailureResult("Đã xảy ra lỗi. Vui lòng thử lại sau.");
        //    }
        //}

        public async Task<ResponseModel<object>> RegisterCompanyAsync(CompanyRegistrationViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Bắt đầu xử lý đăng ký công ty cho {CompanyName}", viewModel.CompanyName);
                // 1. Xác thực reCAPTCHA
                var recaptchaResult = await _recaptchaService.IsValidAsync(viewModel.RecaptchaToken);
                if (recaptchaResult.Status != 200 || recaptchaResult.Data == false)
                {
                    _logger.LogWarning("reCAPTCHA không hợp lệ cho công ty: {CompanyName}", viewModel.CompanyName);
                    return ResponseModel<object>.FailureResult("Xác thực reCAPTCHA không thành công.");
                }
                _logger.LogInformation("reCAPTCHA hợp lệ");

                // 2. Kiểm tra file hợp lệ
                string[] allowedExts = [".pdf", ".jpg", ".jpeg", ".png"];
                if (!_companyValidationService.IsValidFile(viewModel.BusinessLicenseFile,allowedExts, 5 * 1024 * 1024) || !_companyValidationService.IsValidFile(viewModel.LegalRepIDFile, allowedExts, 5 * 1024 * 1024))
             //   if (!IsValidFile(viewModel.BusinessLicenseFile, allowedExts) || !IsValidFile(viewModel.LegalRepIDFile, allowedExts))
                {
                    _logger.LogWarning("Tệp không hợp lệ cho công ty: {CompanyName}", viewModel.CompanyName);
                    return ResponseModel<object>.FailureResult("Tệp không hợp lệ.");
                }
                _logger.LogInformation("Tệp hợp lệ, tiến hành upload");

                // 3. Upload lên Firebase
                using var licenseMem = new MemoryStream();
                using var idMem = new MemoryStream();
                await viewModel.BusinessLicenseFile.CopyToAsync(licenseMem);
                await viewModel.LegalRepIDFile.CopyToAsync(idMem);
                licenseMem.Position = 0;
                idMem.Position = 0;

                if (!await _nsfwService1.IsSafeImageAsync(idMem))
                {
                    _logger.LogWarning("Ảnh CMND/CCCD chứa nội dung không phù hợp.");
                    return ResponseModel<object>.FailureResult("Ảnh CMND/CCCD chứa nội dung không phù hợp (18+).");
                }
                idMem.Position = 0;

                if (!await _nsfwService1.IsSafeImageAsync(licenseMem))
                {
                    _logger.LogWarning("Ảnh giấy phép kinh doanh chứa nội dung không phù hợp.");
                    return ResponseModel<object>.FailureResult("Ảnh giấy phép kinh doanh chứa nội dung không phù hợp (18+).");
                }
                licenseMem.Position = 0;

                _logger.LogInformation("Ảnh hợp lệ, tiến hành upload lên Firebase");


                string licenseFileName = Guid.NewGuid() + Path.GetExtension(viewModel.BusinessLicenseFile!.FileName);
                string idFileName = Guid.NewGuid() + Path.GetExtension(viewModel.LegalRepIDFile!.FileName);

                string licenseUrl, idUrl;

                using (var licenseStream = viewModel.BusinessLicenseFile.OpenReadStream())
                {
                    var uploadResult = await _firebaseStorageService.UploadFileAsync(
                        licenseStream, licenseFileName, viewModel.BusinessLicenseFile.ContentType);

                    if (uploadResult.Status != 200)
                        return ResponseModel<object>.FailureResult("Tải lên giấy phép kinh doanh thất bại.");

                    licenseUrl = uploadResult.Data;
                }

                using (var idStream = viewModel.LegalRepIDFile.OpenReadStream())
                {
                    var uploadResult = await _firebaseStorageService.UploadFileAsync(
                        idStream, idFileName, viewModel.LegalRepIDFile.ContentType);

                    if (uploadResult.Status != 200)
                        return ResponseModel<object>.FailureResult("Tải lên CMND/CCCD người đại diện thất bại.");

                    idUrl = uploadResult.Data;
                }

                await _unitOfWork.BeginTransactionAsync();
                _logger.LogInformation("Upload file thành công");
                // 4. Mapping từ ViewModel sang Entity
                var entity = new CompanyRegistration
                {
                    CompanyName = viewModel.CompanyName,
                    TaxCode = viewModel.TaxCode,
                    Address = viewModel.Address,
                    PhoneNumber = viewModel.PhoneNumber,
                    Email = viewModel.Email,
                    LegalRepName = viewModel.LegalRepName,
                    LegalRepId = viewModel.LegalRepId,
                    LegalRepPosition = viewModel.LegalRepPosition,
                    BusinessLicenseFilePath = licenseUrl,
                    LegalRepIdfilePath = idUrl,
                    CreatedAt = DateTime.Now,
                    Status = false,
                };

                var duplicateFieldErrors = await _companyValidationService.GetDuplicateFieldErrorsAsync(entity);
                if (duplicateFieldErrors.Any())
                {
                    var message = "Thông tin đã tồn tại trong hệ thống";
                    return ResponseModel<object>.FailureResult(message, duplicateFieldErrors);
                }


                await _unitOfWork.CompanyRegistrations.AddAsync(entity);
                _logger.LogInformation("Lưu thông tin đăng ký công ty vào DB: {CompanyName}", entity.CompanyName);
                // 5. Ghi log
                var log = new ActionLog
                {
                    AdminId = null,
                    Action = $"Khách hàng đăng ký: {entity.CompanyName} - MST: {entity.TaxCode}",
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.ActionLogs.AddAsync(log);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Ghi log hành động vào DB");

                // 6. Gửi email
                //await _emailService.SendEmailAsync(entity.Email, entity.CompanyName);
                //_logger.LogInformation("Đã gửi email xác nhận đến: {Email}", entity.Email);
                var emailSent = await _emailService.SendEmailAsync(entity.Email, entity.CompanyName);
                if (emailSent.Status == 200)
                {
                    entity.Status = true;
                    _logger.LogInformation("Đã gửi email xác nhận đến: {Email}", entity.Email);
                }
                else
                {
                    _logger.LogWarning("Không gửi được email xác nhận đến: {Email} - Lỗi: {Error}", entity.Email, emailSent.Message);
                }
                return ResponseModel<object>.SuccessResult(entity, "Đăng ký thành công");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lỗi khi xử lý đăng ký khách hàng");
                return ResponseModel<object>.FailureResult($"Lỗi: {ex.Message}");
            }
        }

        //private bool IsValidFile(IFormFile file, string[] allowedExts)
        //{
        //    if (file == null) return false;
        //    var ext = Path.GetExtension(file.FileName).ToLower();
        //    return allowedExts.Contains(ext);
        //}

//        private async Task<Dictionary<string, string[]>> GetDuplicateFieldErrorsAsync(CompanyRegistration entity)
//{
//            var existingRecords = await _unitOfWork.CompanyRegistrations
//                .FindAsync(x =>
//                x.CompanyName == entity.CompanyName ||
//                x.TaxCode == entity.TaxCode ||
//                x.PhoneNumber == entity.PhoneNumber ||
//                x.Email == entity.Email ||
//                x.LegalRepId == entity.LegalRepId);

//            var errors = new Dictionary<string, string[]>();

//            if (existingRecords.Any(x => x.CompanyName == entity.CompanyName))
//                errors["CompanyName"] = new[] { "Tên doanh nghiệp đã tồn tại." };

//            if (existingRecords.Any(x => x.TaxCode == entity.TaxCode))
//                errors["TaxCode"] = new[] { "Mã số thuế đã tồn tại." };

//            if (existingRecords.Any(x => x.PhoneNumber == entity.PhoneNumber))
//                errors["PhoneNumber"] = new[] { "Số điện thoại đã tồn tại." };

//            if (existingRecords.Any(x => x.Email == entity.Email))
//                errors["Email"] = new[] { "Email đã tồn tại." };

//            if (existingRecords.Any(x => x.LegalRepId == entity.LegalRepId))
//                errors["LegalRepId"] = new[] { "CMND/CCCD đã tồn tại." };

//            return errors;
//            }


    }
}
