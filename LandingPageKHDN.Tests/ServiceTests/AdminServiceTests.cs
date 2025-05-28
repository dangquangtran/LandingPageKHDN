using FluentAssertions;
using LandingPageKHDN.Application.Common;
using LandingPageKHDN.Application.Interfaces;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.ViewModels;
using LandingPageKHDN.Domain.Entities;
using LandingPageKHDN.Infrastructure.Repositories;
using LandingPageKHDN.Infrastructure.ServiceImpls;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LandingPageKHDN.Tests.ServiceTests
{
    public class AdminServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<AdminService>> _mockLogger;
        private readonly Mock<IFirebaseStorageService> _mockFirebaseStorageService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ICompanyValidationService> _mockCompanyValidationService;
        private readonly Mock<INSFWDetectionService> _mockNsfwDetectionService;
        private readonly Mock<INsfwService> _mockNsfwService;

        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<AdminService>>();
            _mockFirebaseStorageService = new Mock<IFirebaseStorageService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockCompanyValidationService = new Mock<ICompanyValidationService>();
            _mockNsfwDetectionService = new Mock<INSFWDetectionService>();
            _mockNsfwService = new Mock<INsfwService>();

            _adminService = new AdminService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockFirebaseStorageService.Object,
                _mockEmailService.Object,
                _mockCompanyValidationService.Object,
                _mockNsfwDetectionService.Object,
                _mockNsfwService.Object
            );
        }

        [Fact]
        public async Task GetAllCompanyAsync_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var companies = new List<CompanyRegistration>
        {
            new CompanyRegistration
            {
                Id = 1, CompanyName = "Test Co", TaxCode = "123456", Address = "Addr",
                PhoneNumber = "0123456", Email = "test@example.com", LegalRepName = "John",
                LegalRepId = "CMND123", LegalRepPosition = "CEO", BusinessLicenseFilePath = "url1",
                LegalRepIdfilePath = "url2", CreatedAt = DateTime.Now, Status = true
            }
        };

            _mockUnitOfWork.Setup(u => u.CompanyRegistrations.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                           .ReturnsAsync((companies, companies.Count));

            // Act
            var result = await _adminService.GetAllCompanyAsync(1, 10);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Data);
            Assert.Equal("Test Co", result.Data.First().CompanyName);
        }

        [Fact]
        public async Task GetAllCompanyAsync_ReturnsEmpty_WhenNoData()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<AdminService>>();
            var mockFirebase = new Mock<IFirebaseStorageService>();
            var mockEmail = new Mock<IEmailService>();
            var mockValidation = new Mock<ICompanyValidationService>();
            var mockNsfw = new Mock<INSFWDetectionService>();
            var mockNsfw1 = new Mock<INsfwService>();

            var emptyCompanies = new List<CompanyRegistration>();
            mockUnitOfWork.Setup(u => u.CompanyRegistrations.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync((emptyCompanies, 0));

            var service = new AdminService(mockUnitOfWork.Object, mockLogger.Object,
                mockFirebase.Object, mockEmail.Object, mockValidation.Object,
                mockNsfw.Object, mockNsfw1.Object);

            // Act
            var result = await service.GetAllCompanyAsync(1, 10);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data);
            Assert.Equal("Không có dữ liệu", result.Message);
        }

        [Fact]
        public async Task GetAllCompanyAsync_ReturnsFailure_OnException()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<AdminService>>();
            var mockFirebase = new Mock<IFirebaseStorageService>();
            var mockEmail = new Mock<IEmailService>();
            var mockValidation = new Mock<ICompanyValidationService>();
            var mockNsfw = new Mock<INSFWDetectionService>();
            var mockNsfw1 = new Mock<INsfwService>();

            mockUnitOfWork.Setup(u => u.CompanyRegistrations.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                          .ThrowsAsync(new Exception("DB error"));

            var service = new AdminService(mockUnitOfWork.Object, mockLogger.Object,
                mockFirebase.Object, mockEmail.Object, mockValidation.Object,
                mockNsfw.Object, mockNsfw1.Object);

            // Act
            var result = await service.GetAllCompanyAsync(1, 10);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.StartsWith("Lỗi:", result.Message);
        }

        [Fact]
        public async Task GetAllCompanyAsync_ReturnsEmptyList_WhenNoData()
        {
            _mockUnitOfWork.Setup(u => u.CompanyRegistrations.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                           .ReturnsAsync((new List<CompanyRegistration>(), 0));

            var result = await _adminService.GetAllCompanyAsync(1, 10);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllCompanyAsync_ReturnsFailure_WhenExceptionThrown()
        {
            _mockUnitOfWork.Setup(u => u.CompanyRegistrations.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                           .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _adminService.GetAllCompanyAsync(1, 10);

            Assert.False(result.IsSuccess);
            Assert.Contains("Unexpected error", result.Message);
        }


        [Fact]
        public async Task RemoveCompanyAsync_ReturnsFailure_WhenCompanyNotFound()
        {
            _mockUnitOfWork.Setup(u => u.CompanyRegistrations.GetByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((CompanyRegistration)null);

            var result = await _adminService.RemoveCompanyAsync(999);

            Assert.False(result.IsSuccess);
            Assert.Equal("Không tìm thấy công ty cần xoá.", result.Message);
        }

        [Fact]
        public async Task UpdateCompanyAsync_Should_Update_Company_Successfully_When_All_Valid()
        {
            // Arrange
            var companyId = 123;
            var existingCompany = new CompanyRegistration
            {
                Id = companyId,
                CompanyName = "Old Name",
                Email = "old@example.com",
                TaxCode = "123456",
                Address = "Old Addr",
                PhoneNumber = "0123456789",
                LegalRepName = "Old Rep",
                LegalRepId = "ID123",
                LegalRepPosition = "Manager",
                BusinessLicenseFilePath = "old-license.png",
                LegalRepIdfilePath = "old-id.png",
                Status = false
            };

            var updateViewModel = new CompanyRegistrationUpdateViewModel
            {
                Id = companyId,
                CompanyName = "New Name",
                Email = "new@example.com",
                TaxCode = "999999999",
                Address = "New Address",
                PhoneNumber = "0987654321",
                LegalRepName = "New Rep",
                LegalRepId = "ID999",
                LegalRepPosition = "CEO",
                Status = false
            };

            _mockUnitOfWork.Setup(u => u.CompanyRegistrations.FirstOrDefaultAsync(It.IsAny<Expression<Func<CompanyRegistration, bool>>>()))
                .ReturnsAsync(existingCompany);

            _mockCompanyValidationService.Setup(x => x.GetDuplicateFieldErrorAsync(It.IsAny<CompanyRegistrationUpdateViewModel>(), It.IsAny<int>()))
                .ReturnsAsync(new Dictionary<string, string[]>());

            _mockEmailService.Setup(x => x.SendUpdateConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ResponseModel<bool>.SuccessResult(true, "Gửi mail thành công"));

            _mockUnitOfWork.Setup(x => x.ActionLogs.AddAsync(It.IsAny<ActionLog>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _adminService.UpdateCompanyAsync(updateViewModel);

            // Assert
            Assert.True(result.IsSuccess, $"❌ Expected success but got failure: {result.Message}");
            Assert.Equal("Cập nhật công ty thành công.", result.Message);
            Assert.Equal(updateViewModel.CompanyName, existingCompany.CompanyName);
            Assert.Equal(updateViewModel.Email, existingCompany.Email);
            Assert.True(existingCompany.Status);
            //   Assert.Equal(updateViewModel.Status, existingCompany.Status);
        }




    }

}
