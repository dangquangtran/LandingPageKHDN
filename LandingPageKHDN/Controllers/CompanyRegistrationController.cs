using LandingPageKHDN.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.ViewModels;
using LandingPageKHDN.Application.Common;

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
        private readonly IConfiguration _configuration;

        public CompanyRegistrationController(ILogger<CompanyRegistrationController> logger,
                                             ICompanyRegistrationService companyRegistrationService,
                                             IConfiguration configuration)
        {
            //_context = context;
            _logger = logger;
            //_env = env;
            //_emailService = emailService;
            //_recaptchaService = recaptchaService;
            //_firebaseStorageService = firebaseStorageService;
            _companyRegistrationService = companyRegistrationService;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        //    [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([FromForm]CompanyRegistrationViewModel viewModel)
        {
            ViewBag.RecaptchaSiteKey = _configuration["Recaptcha:SiteKey"];
            if (!ModelState.IsValid)
            {
                //var errors = ModelState
                //    .Where(e => e.Value.Errors.Count > 0)
                //    .ToDictionary(
                //        kvp => kvp.Key,
                //        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                //    );

                //var errorMessage = "Dữ liệu không hợp lệ.";

                //return BadRequest(ResponseModel<string>.FailureResult(errorMessage, errors));
                return View("~/Views/Home/Index.cshtml", viewModel);
            }
            var result = await _companyRegistrationService.RegisterCompanyAsync(viewModel);

            //if (result.Success)
            //{
            //    TempData["SuccessMessage"] = result.Message;
            //    return RedirectToAction("Success"); // hoặc return View("Success");
            //}

            //// Truyền lỗi về lại view để hiển thị
            ModelState.AddModelError(string.Empty, result.Message);
            return View("~/Views/Home/Index.cshtml", viewModel);

        }


        //Method test handler error
        public IActionResult TestException()
        {
            throw new Exception("Lỗi test GlobalExceptionFilter.");
        }
    }
}
