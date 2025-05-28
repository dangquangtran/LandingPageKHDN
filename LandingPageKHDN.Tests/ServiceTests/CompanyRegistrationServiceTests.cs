using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.Common;
using LandingPageKHDN.Application.ViewModels;
using LandingPageKHDN.Domain.Entities;
using LandingPageKHDN.Application.Interfaces;
using LandingPageKHDN.Infrastructure.ServiceImpls;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Collections.Generic;
using System;
using LandingPageKHDN.Application.Interfaces.Repositories;
using System.Text;

namespace LandingPageKHDN.Tests.ServiceTests
{
    public class CompanyRegistrationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFirebaseStorageService> _firebaseStorageMock;
        private readonly Mock<IRecaptchaService> _recaptchaMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ICompanyValidationService> _validationMock;
        private readonly Mock<INsfwService> _nsfwMock;
        private readonly Mock<ILogger<CompanyRegistrationService>> _loggerMock;
        private readonly CompanyRegistrationService _service;

        public CompanyRegistrationServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _firebaseStorageMock = new Mock<IFirebaseStorageService>();
            _recaptchaMock = new Mock<IRecaptchaService>();
            _emailServiceMock = new Mock<IEmailService>();
            _validationMock = new Mock<ICompanyValidationService>();
            _nsfwMock = new Mock<INsfwService>();
            _loggerMock = new Mock<ILogger<CompanyRegistrationService>>();

            // Mock các phương thức UnitOfWork cần thiết
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Khởi tạo service với đúng thứ tự tham số
            _service = new CompanyRegistrationService(
                _unitOfWorkMock.Object,
                _firebaseStorageMock.Object,
                _recaptchaMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object,
                _validationMock.Object,
                _nsfwMock.Object
            );
        }


        private IFormFile CreateFakeFormFile(string fileName)
        {
            var content = "fake file content";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

        [Fact]
        public async Task RegisterCompanyAsync_ShouldReturnFailure_WhenRecaptchaInvalid()
        {
            // Arrange
            var viewModel = new CompanyRegistrationViewModel
            {
                CompanyName = "Test Company",
                RecaptchaToken = "invalid-token"
            };

            _recaptchaMock.Setup(x => x.IsValidAsync(It.IsAny<string>()))
                .ReturnsAsync(ResponseModel<bool>.FailureResult("Invalid"));

            // Act
            var result = await _service.RegisterCompanyAsync(viewModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Xác thực reCAPTCHA không thành công.", result.Message);
        }

        [Fact]
        public async Task RegisterCompanyAsync_ShouldReturnFailure_WhenFileInvalid()
        {
            // Arrange
            var viewModel = new CompanyRegistrationViewModel
            {
                CompanyName = "Test Co",
                RecaptchaToken = "valid-token",
                BusinessLicenseFile = CreateFakeFormFile("test.jpg"),
                LegalRepIDFile = CreateFakeFormFile("id.png")
            };

            _recaptchaMock.Setup(x => x.IsValidAsync(It.IsAny<string>()))
                .ReturnsAsync(ResponseModel<bool>.SuccessResult(true, "Valid"));

            _validationMock.Setup(x => x.IsValidFile(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>()))
                .Returns(false);

            // Act
            var result = await _service.RegisterCompanyAsync(viewModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Tệp không hợp lệ.", result.Message);
        }

        [Fact]
        public async Task RegisterCompanyAsync_ShouldReturnSuccess_WhenAllValid()
        {
            // Arrange
            var viewModel = new CompanyRegistrationViewModel
            {
                CompanyName = "Valid Company",
                TaxCode = "1234567890",
                Address = "123 Example Street",
                PhoneNumber = "0901234567",
                Email = "valid@example.com",
                LegalRepName = "Nguyen Van B",
                LegalRepId = "987654321",
                LegalRepPosition = "Director",
                RecaptchaToken = "valid-recaptcha-token",
                BusinessLicenseFile = CreateFakeImageFile("license.png", "image/png"),
                LegalRepIDFile = CreateFakeImageFile("id.png", "image/png"),
            };

            _recaptchaMock.Setup(x => x.IsValidAsync(It.IsAny<string>()))
                .ReturnsAsync(ResponseModel<bool>.SuccessResult(true,"valid"));

            _validationMock.Setup(x => x.IsValidFile(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>()))
                .Returns(true);

            _nsfwMock.Setup(x => x.IsSafeImageAsync(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            _firebaseStorageMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ResponseModel<string>.SuccessResult("https://firebase.storage/testfile.png", "ok"));

            _validationMock.Setup(x => x.GetDuplicateFieldErrorsAsync(It.IsAny<CompanyRegistration>()))
                .ReturnsAsync(new Dictionary<string, string[]>());

            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ResponseModel<bool>.SuccessResult(true, "valid"));

            _unitOfWorkMock.Setup(x => x.CompanyRegistrations.AddAsync(It.IsAny<CompanyRegistration>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.ActionLogs.AddAsync(It.IsAny<ActionLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.RegisterCompanyAsync(viewModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Đăng ký thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.IsType<CompanyRegistration>(result.Data);

            var company = (CompanyRegistration)result.Data;
            Assert.Equal(viewModel.CompanyName, company.CompanyName);
        }

        // Hàm helper tạo file fake
        private IFormFile CreateFakeImageFile(string fileName, string contentType)
        {
            // Đây là 1 ảnh PNG 1x1 pixel màu trong suốt
            byte[] imageBytes = new byte[]
            {
        137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,
        0,0,0,1,0,0,0,1,8,6,0,0,0,31,21,196,
        137,0,0,0,12,73,68,65,84,8,29,99,0,1,0,0,
        5,0,1,13,10,45,161,0,0,0,0,73,69,78,68,174,
        66,96,130
            };

            var stream = new MemoryStream(imageBytes);
            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

    }
}
