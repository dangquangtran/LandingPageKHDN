using LandingPageKHDN.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Services
{
    public interface IEmailService
    {
        Task<ResponseModel<bool>> SendEmailAsync(string to, string companyName);
        Task<ResponseModel<bool>> SendUpdateConfirmationEmailAsync(string to, string companyName);
    }
}
