﻿using Azure;
using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.ViewModels;
using LandingPageKHDN.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;

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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (model.Username == "admin" && model.Password == "12345")
            {
                // Đăng nhập thành công, lưu session
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }
            var response = await _adminService.GetAllCompanyAsync(1, 1000);
            var items = response.Data ?? new List<CompanyRegistrationGetViewModel>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                items = items
                    .Where(x => (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(search))
                             || (!string.IsNullOrEmpty(x.TaxCode) && x.TaxCode.ToLower().Contains(search)))
                    .ToList();
                ViewBag.Search = search;
            }

            return View(items);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }
            // Lấy danh sách công ty (hoặc tốt nhất là tạo hàm GetCompanyByIdAsync trong service)
            var response = await _adminService.GetAllCompanyAsync(1, 1000); // hoặc số lớn hơn tổng số công ty
            var company = response.Data?.FirstOrDefault(x => x.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }


        [HttpGet]
        public async Task<IActionResult> GetCompany([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var result = await _adminService.GetAllCompanyAsync(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompanyRegistrationCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var result = await _adminService.CreateCompanyAsync(viewModel);
            if (result.Status == 200)
                return RedirectToAction(nameof(Index));

            // Đẩy lỗi từng field vào ModelState
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    foreach (var msg in error.Value)
                    {
                        ModelState.AddModelError(error.Key, msg);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromForm] CompanyRegistrationCreateViewModel viewModel)
        {
            var result = await _adminService.CreateCompanyAsync(viewModel);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }
            var response = await _adminService.GetAllCompanyAsync(1, 1000);
            var company = response.Data?.FirstOrDefault(x => x.Id == id);
            if (company == null)
                return NotFound();

            // Map sang UpdateViewModel nếu cần
            var updateModel = new CompanyRegistrationUpdateViewModel
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                TaxCode = company.TaxCode,
                Address = company.Address,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                LegalRepName = company.LegalRepName,
                LegalRepId = company.LegalRepId,
                LegalRepPosition = company.LegalRepPosition,
                // File không map ở đây
            };
            return View(updateModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CompanyRegistrationUpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var result = await _adminService.UpdateCompanyAsync(viewModel);
            if (result.Status == 200)
                return RedirectToAction(nameof(Index));

            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    foreach (var msg in error.Value)
                    {
                        ModelState.AddModelError(error.Key, msg);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCompany([FromForm] CompanyRegistrationUpdateViewModel viewModel)
        {
            var result = await _adminService.UpdateCompanyAsync(viewModel);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }
            var response = await _adminService.GetAllCompanyAsync(1, 1000);
            var company = response.Data?.FirstOrDefault(x => x.Id == id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _adminService.RemoveCompanyAsync(id);
            if (result.Status == 200)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, result.Message);
            // Lấy lại model để hiển thị nếu xóa lỗi
            var response = await _adminService.GetAllCompanyAsync(1, 1000);
            var company = response.Data?.FirstOrDefault(x => x.Id == id);
            return View(company);
        }


        [HttpDelete]
        public async Task<IActionResult> RemoveCompany(int id)
        {
            var result = await _adminService.RemoveCompanyAsync(id);
            return Ok(result);
        }

    }
}
