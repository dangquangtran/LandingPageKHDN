using LandingPageKHDN.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using LandingPageKHDN.Services;

namespace LandingPageKHDN.Controllers
{
    public class CompanyRegistrationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompanyRegistrationController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;
        private readonly RecaptchaService _recaptchaService;
        private readonly FirebaseStorageService _firebaseStorageService;

        public CompanyRegistrationController(AppDbContext context, 
                                             ILogger<CompanyRegistrationController> logger, 
                                             IWebHostEnvironment env, 
                                             EmailService emailService,
                                             RecaptchaService recaptchaService,
                                             FirebaseStorageService firebaseStorageService)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _emailService = emailService;
            _recaptchaService = recaptchaService;
            _firebaseStorageService = firebaseStorageService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
    //    [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            [FromForm] CompanyRegistration model,
            IFormFile businessLicenseFile,
            IFormFile legalRepIDFile,
            string recaptchaToken)
        {
            // 1. Validate reCAPTCHA từ FE gửi lên
            if (!await _recaptchaService.IsValidAsync(recaptchaToken))
            {
                return BadRequest("Xác thực reCAPTCHA không thành công.");
            }

            // 2. Validate định dạng file
            string[] allowedExts = [".pdf", ".jpg", ".jpeg", ".png"];
            if (!IsValidFile(businessLicenseFile, allowedExts) || !IsValidFile(legalRepIDFile, allowedExts))
                return BadRequest("Tệp không hợp lệ.");

            // 3. Upload file lên Firebase Storage
            string licenseFileName = Guid.NewGuid() + Path.GetExtension(businessLicenseFile.FileName);
            string idFileName = Guid.NewGuid() + Path.GetExtension(legalRepIDFile.FileName);

            string licenseUrl, idUrl;

            using (var licenseStream = businessLicenseFile.OpenReadStream())
            {
                licenseUrl = await _firebaseStorageService.UploadFileAsync(licenseStream, licenseFileName, businessLicenseFile.ContentType);
            }

            using (var idStream = legalRepIDFile.OpenReadStream())
            {
                idUrl = await _firebaseStorageService.UploadFileAsync(idStream, idFileName, legalRepIDFile.ContentType);
            }

            // 4. Lưu DB
            model.BusinessLicenseFilePath = licenseUrl;
            model.LegalRepIdfilePath = idUrl;
            model.CreatedAt = DateTime.Now;

            _context.CompanyRegistrations.Add(model);
            await _context.SaveChangesAsync();

            // 5. Ghi log
            var log = new ActionLog
            {
                AdminId = null,
                Action = $"Khách hàng đăng ký: {model.CompanyName} - MST: {model.TaxCode}",
                CreatedAt = DateTime.Now
            };
            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();

            // 6. Gửi email
            await _emailService.SendEmailAsync(model.Email, model.CompanyName);

            return Ok("Đăng ký thành công");
        }

        private bool IsValidFile(IFormFile file, string[] allowedExts)
        {
            if (file == null) return false;
            var ext = Path.GetExtension(file.FileName).ToLower();
            return allowedExts.Contains(ext);
        }

    }
}
