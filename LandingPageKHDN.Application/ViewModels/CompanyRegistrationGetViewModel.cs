using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.ViewModels
{
    public class CompanyRegistrationGetViewModel
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = null!;

        public string TaxCode { get; set; } = null!;

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? LegalRepName { get; set; }

        public string? LegalRepId { get; set; }

        public string? LegalRepPosition { get; set; }

        public string? BusinessLicenseFilePath { get; set; }

        public string? LegalRepIdfilePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool? Status { get; set; }
    }
}
