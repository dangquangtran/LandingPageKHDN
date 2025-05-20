using System.Text.Json;

namespace LandingPageKHDN.Services
{
    public class RecaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public RecaptchaService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsValidAsync(string token)
        {
            //var secret = _configuration["Recaptcha:SecretKey"];
            //var client = _httpClientFactory.CreateClient();
            //var response = await client.PostAsync(
            //    "https://www.google.com/recaptcha/api/siteverify",
            //    new FormUrlEncodedContent(new Dictionary<string, string>
            //    {
            //        { "secret", secret },
            //        { "response", token }
            //    }));

            //var json = await response.Content.ReadAsStringAsync();
            //using var doc = JsonDocument.Parse(json);
            //return doc.RootElement.GetProperty("success").GetBoolean();

            await Task.CompletedTask;
            return true;
        }
    }
}
