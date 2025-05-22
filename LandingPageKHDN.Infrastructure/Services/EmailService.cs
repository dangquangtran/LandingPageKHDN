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
using LandingPageKHDN.Infrastructure.Templates.Email;

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
                    Body = $@"
<div style='font-family: Arial, sans-serif; max-width: 700px; margin: auto; border: 1px solid #e0e0e0; box-shadow: 0 0 10px rgba(0,0,0,0.05); border-radius: 8px; overflow: hidden;'>
    <div style='text-align: center; background-color: #ffffff;'>
        <img src='https://drive.google.com/uc?export=view&id=1g7dEMH1fDo_oJvTSfQbMCCuLbq9DL6-t' alt='Welcome' style='width: 100%; max-width: 700px; display: block;' />
    </div>
    <div style='padding: 20px; color: #333333; background-color: #ffffff;'>
        <p style='font-size: 16px;'>Chào <strong>{companyName}</strong>,</p>
        <p style='font-size: 16px;'>Chúng tôi đã nhận được đăng ký mở tài khoản doanh nghiệp của bạn.</p>
        <p style='font-size: 16px;'>KienlongBank sẽ liên hệ với bạn trong thời gian sớm nhất để hỗ trợ bạn hoàn tất các bước tiếp theo.</p>
        <p style='font-size: 16px;'>Trân trọng!</p>

        <hr style='border: none; border-top: 1px dashed #ccc; margin: 20px 0;' />

        <table style='width: 100%; font-size: 14px;'>
            <tr>
                <td style='width: 130px;'>
                    <img src='https://drive.google.com/uc?export=view&id=1_JgncxUN2D_2ZBbfPJpHaSa6YeGFPnDp' alt='KienlongBank' style='height: 40px;' />
                </td>
                <td>
                    <strong>Bé Thị Hồng Nhung (Mrs)</strong><br />
                    Chuyên viên Tuyển dụng và Quy hoạch Nhân sự<br />
                    <strong>PHÒNG NHÂN SỰ</strong><br /><br />

                    <img src='https://img.icons8.com/ios-filled/16/000000/phone.png' style='vertical-align: middle;' />
                    &nbsp; 0985 449 888<br />

                    <img src='https://img.icons8.com/ios-filled/16/000000/email.png' style='vertical-align: middle;' />
                    &nbsp; <a href='mailto:nhungbth1@kienlongbank.com'>nhungbth1@kienlongbank.com</a><br />

                    <img src='https://img.icons8.com/ios-filled/16/000000/domain.png' style='vertical-align: middle;' />
                    &nbsp; <a href='https://www.kienlongbank.com' target='_blank'>https://www.kienlongbank.com</a><br />

                    <img src='https://img.icons8.com/ios-filled/16/000000/marker.png' style='vertical-align: middle;' />
                    &nbsp; Tầng 6, Sunshine Center, 16 Phạm Hùng, P. Mỹ Đình 2, Từ Liêm, Hà Nội<br /><br />

                    <a href='https://www.facebook.com/kienlongbank' target='_blank'>
                        <img src='https://img.icons8.com/color/32/000000/facebook.png' style='margin-right:5px;' />
                    </a>
                    <a href='https://zalo.me/123456789' target='_blank'>
                        <img src='https://img.icons8.com/color/32/zalo.png' style='margin-right:5px;' />
                    </a>
                    <a href='https://www.youtube.com/kienlongbank' target='_blank'>
                        <img src='https://img.icons8.com/color/32/000000/youtube-play.png' style='margin-right:5px;' />
                    </a>
                    <a href='https://m.me/kienlongbank' target='_blank'>
                        <img src='https://img.icons8.com/color/32/000000/facebook-messenger.png' style='margin-right:5px;' />
                    </a>
                </td>
            </tr>
        </table>
    </div>
</div>",

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
