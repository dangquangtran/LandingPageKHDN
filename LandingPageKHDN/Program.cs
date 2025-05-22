using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
//using LandingPageKHDN.Services;
using Microsoft.EntityFrameworkCore;
using LandingPageKHDN.Infrastructure;
using System.Text.Json;
using LandingPageKHDN.Filters;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console() 
    .WriteTo.File("Logs/app_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var firebaseConfig = builder.Configuration.GetSection("Firebase");
string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), firebaseConfig["CredentialPath"]);

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(credentialPath)
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Host.UseSerilog();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});


// G?i extension method ?? ??ng k?DI
builder.Services.AddInfrastructure(builder.Configuration);


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
