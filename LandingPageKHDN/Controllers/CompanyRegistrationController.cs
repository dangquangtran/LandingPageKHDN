using LandingPageKHDN.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace LandingPageKHDN.Controllers
{
    public class CompanyRegistrationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompanyRegistrationController> _logger;
        private readonly IWebHostEnvironment _env;

        public CompanyRegistrationController(AppDbContext context, ILogger<CompanyRegistrationController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
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
            if (!await IsValidRecaptcha(recaptchaToken))
            {
                return BadRequest("Xác thực reCAPTCHA không thành công.");
            }

            // 2. Validate định dạng file
            string[] allowedExts = [".pdf", ".jpg", ".jpeg", ".png"];
            if (!IsValidFile(businessLicenseFile, allowedExts) || !IsValidFile(legalRepIDFile, allowedExts))
                return BadRequest("Tệp không hợp lệ.");

            // 3. Tạo folder uploads nếu chưa có
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadPath);

            // 4. Lưu file
            string licenseFileName = Guid.NewGuid() + Path.GetExtension(businessLicenseFile.FileName);
            string idFileName = Guid.NewGuid() + Path.GetExtension(legalRepIDFile.FileName);

            var licenseFullPath = Path.Combine(uploadPath, licenseFileName);
            var idFullPath = Path.Combine(uploadPath, idFileName);

            using (var stream = new FileStream(licenseFullPath, FileMode.Create))
            {
                await businessLicenseFile.CopyToAsync(stream);
            }

            using (var stream = new FileStream(idFullPath, FileMode.Create))
            {
                await legalRepIDFile.CopyToAsync(stream);
            }

            // 5. Lưu DB
            model.BusinessLicenseFilePath = "/uploads/" + licenseFileName;
            model.LegalRepIdfilePath = "/uploads/" + idFileName;
            model.CreatedAt = DateTime.Now;

            _context.CompanyRegistrations.Add(model);
            await _context.SaveChangesAsync();

            // 6. Ghi log
            var log = new ActionLog
            {
                AdminId = null,
                Action = $"Khách hàng đăng ký: {model.CompanyName} - MST: {model.TaxCode}",
                CreatedAt = DateTime.Now
            };
            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();

            // 7. Gửi email
            await SendConfirmationEmail(model.Email, model.CompanyName);

            return Ok("Đăng ký thành công");
        }

        private bool IsValidFile(IFormFile file, string[] allowedExts)
        {
            if (file == null) return false;
            var ext = Path.GetExtension(file.FileName).ToLower();
            return allowedExts.Contains(ext);
        }

        private async Task<bool> IsValidRecaptcha(string token)
        {
            //string secret = "YOUR_RECAPTCHA_SECRET_KEY";
            //using var client = new HttpClient();
            //var values = new Dictionary<string, string>
            //{
            //    { "secret", secret },
            //    { "response", token }
            //};
            //var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(values));
            //var json = await response.Content.ReadAsStringAsync();
            //return json.Contains("\"success\": true");
            await Task.CompletedTask;   
            return true; // ✅ luôn đúng để dễ test
        }

        private async Task SendConfirmationEmail(string toEmail, string companyName)
        {
            var mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.From = new MailAddress("your-email@gmail.com", "KienlongBank");
            mail.Subject = "Xác nhận đăng ký mở tài khoản";
            mail.Body = $"<p>Chào {companyName},</p><p>Chúng tôi đã nhận được đăng ký mở tài khoản doanh nghiệp của bạn. KienlongBank sẽ liên hệ trong thời gian sớm nhất.</p>";
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
