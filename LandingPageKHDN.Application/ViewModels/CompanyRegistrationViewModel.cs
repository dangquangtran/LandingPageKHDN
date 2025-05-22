using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.ViewModels
{
    public class CompanyRegistrationViewModel
    {
        [Required(ErrorMessage = "Tên công ty là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tên công ty không được vượt quá 200 ký tự.")]
        public string CompanyName { get; set; } = null!;

        [Required(ErrorMessage = "Mã số thuế là bắt buộc.")]
        [RegularExpression(@"^\d{10,13}$", ErrorMessage = "Mã số thuế phải từ 10 đến 13 chữ số.")]
        public string TaxCode { get; set; } = null!;

        [StringLength(300, ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Tên người đại diện là bắt buộc.")]
        public string? LegalRepName { get; set; }

        [Required(ErrorMessage = "CMND/CCCD của người đại diện là bắt buộc.")]
        public string? LegalRepId { get; set; }

        [Required(ErrorMessage = "Chức vụ người đại diện là bắt buộc.")]
        public string? LegalRepPosition { get; set; }

        [Required(ErrorMessage = "Giấy phép kinh doanh là bắt buộc.")]
        public IFormFile? BusinessLicenseFile { get; set; }

        [Required(ErrorMessage = "Giấy tờ tùy thân người đại diện là bắt buộc.")]
        public IFormFile? LegalRepIDFile { get; set; }


        [Required(ErrorMessage = "Token xác thực reCAPTCHA là bắt buộc.")]
        public string RecaptchaToken { get; set; } = null!;
    }
}
