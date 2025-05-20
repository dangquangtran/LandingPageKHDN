using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using LandingPageKHDN.Models;
using LandingPageKHDN.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var firebaseConfig = builder.Configuration.GetSection("Firebase");
string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), firebaseConfig["CredentialPath"]);

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(credentialPath)
});

// Add services to the container.
builder.Services.AddControllersWithViews();
//Dbcontext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<LandingPageKHDN.Services.EmailService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<RecaptchaService>();
builder.Services.AddSingleton<FirebaseStorageService>();

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
