using LandingPageKHDN.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using LandingPageKHDN.Application.Services;

namespace LandingPageKHDN.Controllers
{
    public class CompanyRegistrationController : Controller
    {
        //private readonly AppDbContext _context;
        //private readonly ILogger<CompanyRegistrationController> _logger;
        //private readonly IWebHostEnvironment _env;
        //private readonly EmailService _emailService;
        //private readonly RecaptchaService _recaptchaService;
        //private readonly FirebaseStorageService _firebaseStorageService;
        private readonly ICompanyRegistrationService _companyRegistrationService;
        private readonly ILogger<CompanyRegistrationController> _logger;

        public CompanyRegistrationController(ILogger<CompanyRegistrationController> logger,
                                             ICompanyRegistrationService companyRegistrationService)
        {
            //_context = context;
            _logger = logger;
            //_env = env;
            //_emailService = emailService;
            //_recaptchaService = recaptchaService;
            //_firebaseStorageService = firebaseStorageService;
            _companyRegistrationService = companyRegistrationService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        //    [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit( [FromForm] CompanyRegistration model,
                                                 IFormFile businessLicenseFile,
                                                 IFormFile legalRepIDFile,
                                                 string recaptchaToken)
        {
            var result = await _companyRegistrationService.RegisterCompanyAsync(
                model, businessLicenseFile, legalRepIDFile, recaptchaToken);

            //if (result.Success)
            //{
            //    TempData["SuccessMessage"] = result.Message;
            //    return RedirectToAction("Success"); // hoặc return View("Success");
            //}

            //// Truyền lỗi về lại view để hiển thị
            //ModelState.AddModelError(string.Empty, result.Message);
            //return View("Index", model);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.Message
            });
        }
    }
}
