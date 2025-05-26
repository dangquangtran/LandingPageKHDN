using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.ViewModels
{
    public class CompanyRegistrationUpdateViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string TaxCode { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string LegalRepName { get; set; } = null!;
        public string LegalRepId { get; set; } = null!;
        public string LegalRepPosition { get; set; } = null!;
        public IFormFile? BusinessLicenseFile { get; set; }
        public IFormFile? LegalRepIDFile { get; set; }
    }
}
