using LandingPageKHDN.Application.Services;
using Microsoft.Extensions.Configuration;
using LandingPageKHDN.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ResponseModel<bool>> SendEmailAsync(string to, string companyName)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var fromEmail = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"]);
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"]);

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, "KienlongBank"),
                    Subject = "Xác nhận đăng ký mở tài khoản",
                    Body = $"<p>Chào <strong>{companyName}</strong>,</p>" +
                           "<p>Chúng tôi đã nhận được đăng ký mở tài khoản doanh nghiệp của bạn.</p>" +
                           "<p>KienlongBank sẽ liên hệ với bạn trong thời gian sớm nhất.</p>" +
                           "<p>Trân trọng,</p><p><strong>KienlongBank</strong></p>",
                    IsBodyHtml = true
                };

                message.To.Add(to);

                using var smtpClient = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = enableSsl
                };

                await smtpClient.SendMailAsync(message);
                return ResponseModel<bool>.SuccessResult(default, "Send thành công");
            }
            catch (Exception ex)
            {
                return ResponseModel<bool>.FailureResult($"Gửi email thất bại: {ex.Message}");
            }
        }
    }
}
