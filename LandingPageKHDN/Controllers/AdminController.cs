using Azure;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LandingPageKHDN.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<CompanyRegistrationController> _logger;
        private readonly IAdminService _adminService;

        public AdminController(ILogger<CompanyRegistrationController> logger,
                               IAdminService adminService )
        {
            _adminService = adminService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCompany([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var result = await _adminService.GetAllCompanyAsync(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromForm] CompanyRegistrationCreateViewModel viewModel)
        {
            var result = await _adminService.CreateCompanyAsync(viewModel);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompany([FromForm] CompanyRegistrationUpdateViewModel viewModel)
        {
            var result = await _adminService.UpdateCompanyAsync(viewModel);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCompany(int id)
        {
            var result = await _adminService.RemoveCompanyAsync(id);
            return Ok(result);
        }

    }
}
