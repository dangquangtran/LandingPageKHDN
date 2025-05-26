using LandingPageKHDN.Application.Common;
using LandingPageKHDN.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Services
{
    public interface IAdminService
    {
        Task<ResponseModel<List<CompanyRegistrationGetViewModel>>> GetAllCompanyAsync(int pageIndex, int pageSize);
        Task<ResponseModel<object>> CreateCompanyAsync(CompanyRegistrationCreateViewModel viewModel);
        Task<ResponseModel<object>> UpdateCompanyAsync(CompanyRegistrationUpdateViewModel viewModel);
        Task<ResponseModel<object>> RemoveCompanyAsync(int id);
    }
}
