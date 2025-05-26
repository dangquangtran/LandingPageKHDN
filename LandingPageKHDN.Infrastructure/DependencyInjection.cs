using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.Interfaces;
using LandingPageKHDN.Infrastructure.DbContextFolder;
using LandingPageKHDN.Infrastructure.ServiceImpls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LandingPageKHDN.Application.Interfaces.Repositories;
using LandingPageKHDN.Infrastructure.Repositories;
using LandingPageKHDN.Application.ViewModels;

namespace LandingPageKHDN.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IRecaptchaService, RecaptchaService>();
            services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
            services.AddScoped<ICompanyValidationService, CompanyValidationService>();
            services.AddScoped<ICompanyRegistrationService, CompanyRegistrationService>();
            services.AddScoped<IAdminService, AdminService>();
            //  services.AddHttpClient<INSFWDetectionService, NSFWDetectionService>();
            services.AddScoped<INSFWDetectionService, NSFWDetectionService>();
            services.Configure<HuggingFaceOptions>(configuration.GetSection("HuggingFace"));
            services.AddHttpClient<INsfwService, NsfwService>();
            services.AddHttpClient();

            return services;
        }
    }
}
