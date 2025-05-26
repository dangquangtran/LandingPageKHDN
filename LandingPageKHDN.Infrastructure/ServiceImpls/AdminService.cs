using LandingPageKHDN.Application.Common;
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
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminService> _logger;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly IEmailService _emailService;
        private readonly ICompanyValidationService _companyValidationService;
        public AdminService(IUnitOfWork unitOfWork,
                            ILogger<AdminService> logger,
                            IFirebaseStorageService firebaseStorageService,
                            IEmailService emailService,
                            ICompanyValidationService companyValidationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _firebaseStorageService = firebaseStorageService;
            _emailService = emailService;
            _companyValidationService = companyValidationService;
        }

        public async Task<ResponseModel<List<CompanyRegistrationGetViewModel>>> GetAllCompanyAsync(int pageIndex, int pageSize)
        {
            _logger.LogInformation("Bắt đầu lấy danh sách công ty");
            try
            {
                var (pagedData, totalCount) = await _unitOfWork.CompanyRegistrations.GetPagedAsync(pageIndex, pageSize);

                var result = pagedData.Select(c => new CompanyRegistrationGetViewModel
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    TaxCode = c.TaxCode,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    LegalRepName = c.LegalRepName,
                    LegalRepId = c.LegalRepId,
                    LegalRepPosition = c.LegalRepPosition,
                    BusinessLicenseFilePath = c.BusinessLicenseFilePath,
                    LegalRepIdfilePath = c.LegalRepIdfilePath,
                    CreatedAt = c.CreatedAt,
                    Status = c.Status
                }).ToList();

                string message = result.Count == 0
                    ? "Không có dữ liệu"
                    : $"Tổng dữ liệu: {totalCount}, Trang hiện tại: {result.Count} bản ghi";

                _logger.LogInformation("Lấy danh sách công ty thành công: {Message}", message);

                return ResponseModel<List<CompanyRegistrationGetViewModel>>.SuccessResult(result, message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lỗi khi lấy thông tin danh sách công ty");
                return ResponseModel<List<CompanyRegistrationGetViewModel>>.FailureResult($"Lỗi: {ex.Message}");
            }
        }


        public async Task<ResponseModel<object>> CreateCompanyAsync(CompanyRegistrationCreateViewModel viewModel)
        {
            _logger.LogInformation("Bắt đầu tạo mới công ty cho {CompanyName}", viewModel.CompanyName);

            try
            {
                // 1. Kiểm tra định dạng và kích thước file
                string[] allowedExts = [".pdf", ".jpg", ".jpeg", ".png"];
                long maxFileSize = 5 * 1024 * 1024; // 5MB

                if (!_companyValidationService.IsValidFile(viewModel.BusinessLicenseFile, allowedExts, maxFileSize) ||
                    !_companyValidationService.IsValidFile(viewModel.LegalRepIDFile, allowedExts, maxFileSize))
                {
                    _logger.LogWarning("Tệp không hợp lệ hoặc vượt quá kích thước: {CompanyName}", viewModel.CompanyName);
                    return ResponseModel<object>.FailureResult("Tệp không hợp lệ hoặc quá lớn (tối đa 5MB).");
                }

                _logger.LogInformation("Tệp hợp lệ, tiến hành upload lên Firebase");

                // 2. Upload file
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
                    Status = false // CHỈ chuyển thành true nếu gửi email thành công
                };

                var duplicateFieldErrors = await _companyValidationService.GetDuplicateFieldErrorsAsync(entity);
                if (duplicateFieldErrors.Any())
                {
                    return ResponseModel<object>.FailureResult("Thông tin đã tồn tại trong hệ thống", duplicateFieldErrors);
                }

                await _unitOfWork.CompanyRegistrations.AddAsync(entity);
                _logger.LogInformation("Đã lưu thông tin công ty vào DB");

                var log = new ActionLog
                {
                    AdminId = null,
                    Action = $"Khách hàng tạo mới: {entity.CompanyName} - MST: {entity.TaxCode}",
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.ActionLogs.AddAsync(log);

                await _unitOfWork.SaveChangesAsync();

                // 3. Gửi email xác nhận
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

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ResponseModel<object>.SuccessResult(entity, "Tạo công ty thành công.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lỗi khi tạo công ty");
                return ResponseModel<object>.FailureResult($"Lỗi: {ex.Message}");
            }
        }


        public async Task<ResponseModel<object>> UpdateCompanyAsync(CompanyRegistrationUpdateViewModel viewModel)
        {
            _logger.LogInformation("Bắt đầu cập nhật công ty có ID {Id}", viewModel.Id);

            try
            {
                var company = await _unitOfWork.CompanyRegistrations.FirstOrDefaultAsync(x=> x.Id == viewModel.Id);
                if (company == null)
                {
                    _logger.LogWarning("Không tìm thấy công ty với ID {Id}", viewModel.Id);
                    return ResponseModel<object>.FailureResult("Không tìm thấy công ty cần cập nhật.");
                }

                // Kiểm tra lỗi trùng thông tin (trừ chính nó)
                var duplicateFieldErrors = await _companyValidationService.GetDuplicateFieldErrorAsync(viewModel, company.Id);
                if (duplicateFieldErrors.Any())
                {
                    return ResponseModel<object>.FailureResult("Thông tin đã tồn tại trong hệ thống", duplicateFieldErrors);
                }

                string[] allowedExts = [".pdf", ".jpg", ".jpeg", ".png"];
                long maxFileSize = 5 * 1024 * 1024;

                string licenseUrl = company.BusinessLicenseFilePath;
                string idUrl = company.LegalRepIdfilePath;

                if (viewModel.BusinessLicenseFile is not null)
                {
                    if (!_companyValidationService.IsValidFile(viewModel.BusinessLicenseFile, allowedExts, maxFileSize))
                        return ResponseModel<object>.FailureResult("File giấy phép kinh doanh không hợp lệ.");

                    var licenseFileName = Guid.NewGuid() + Path.GetExtension(viewModel.BusinessLicenseFile.FileName);
                    using var licenseStream = viewModel.BusinessLicenseFile.OpenReadStream();
                    var upload = await _firebaseStorageService.UploadFileAsync(
                        licenseStream, licenseFileName, viewModel.BusinessLicenseFile.ContentType);

                    if (upload.Status != 200)
                        return ResponseModel<object>.FailureResult("Tải lên giấy phép kinh doanh thất bại.");

                    licenseUrl = upload.Data;
                }

                if (viewModel.LegalRepIDFile is not null)
                {
                    if (!_companyValidationService.IsValidFile(viewModel.LegalRepIDFile, allowedExts, maxFileSize))
                        return ResponseModel<object>.FailureResult("File CMND/CCCD không hợp lệ.");

                    var idFileName = Guid.NewGuid() + Path.GetExtension(viewModel.LegalRepIDFile.FileName);
                    using var idStream = viewModel.LegalRepIDFile.OpenReadStream();
                    var upload = await _firebaseStorageService.UploadFileAsync(
                        idStream, idFileName, viewModel.LegalRepIDFile.ContentType);

                    if (upload.Status != 200)
                        return ResponseModel<object>.FailureResult("Tải lên CMND/CCCD thất bại.");

                    idUrl = upload.Data;
                }

                await _unitOfWork.BeginTransactionAsync();

                company.CompanyName = viewModel.CompanyName;
                company.TaxCode = viewModel.TaxCode;
                company.Address = viewModel.Address;
                company.PhoneNumber = viewModel.PhoneNumber;
                company.Email = viewModel.Email;
                company.LegalRepName = viewModel.LegalRepName;
                company.LegalRepId = viewModel.LegalRepId;
                company.LegalRepPosition = viewModel.LegalRepPosition;
                company.BusinessLicenseFilePath = licenseUrl;
                company.LegalRepIdfilePath = idUrl;
                company.Status = viewModel.Status;

                _unitOfWork.CompanyRegistrations.Update(company);

                var log = new ActionLog
                {
                    AdminId = null,
                    Action = $"Khách hàng cập nhật công ty: {company.CompanyName} - MST: {company.TaxCode}",
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.ActionLogs.AddAsync(log);

                var emailSent = await _emailService.SendUpdateConfirmationEmailAsync(company.Email, company.CompanyName);
                if (emailSent.Status == 200)
                {
                    company.Status = true;
                    _logger.LogInformation("Đã gửi email xác nhận đến: {Email}", company.Email);
                }
                else
                {
                    company.Status = false;
                    _logger.LogWarning("Không gửi được email xác nhận đến: {Email} - Lỗi: {Error}", company.Email, emailSent.Message);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Cập nhật công ty thành công: {CompanyName}", company.CompanyName);

                return ResponseModel<object>.SuccessResult(company, "Cập nhật công ty thành công.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật công ty");
                return ResponseModel<object>.FailureResult($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ResponseModel<object>> RemoveCompanyAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xoá công ty với ID: {Id}", id);

            try
            {
                var company = await _unitOfWork.CompanyRegistrations.FirstOrDefaultAsync(x => x.Id == id);
                if (company == null)
                {
                    _logger.LogWarning("Không tìm thấy công ty với ID: {Id}", id);
                    return ResponseModel<object>.FailureResult("Không tìm thấy công ty cần xoá.");
                }

                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.CompanyRegistrations.Remove(company);

                var log = new ActionLog
                {
                    AdminId = null,
                    Action = $"Khách hàng xoá công ty: {company.CompanyName} - MST: {company.TaxCode}",
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.ActionLogs.AddAsync(log);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Xoá công ty thành công: {CompanyName}", company.CompanyName);

                return ResponseModel<object>.SuccessResult(null, "Xoá công ty thành công.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lỗi khi xoá công ty");
                return ResponseModel<object>.FailureResult($"Lỗi: {ex.Message}");
            }
        }

    }
}
