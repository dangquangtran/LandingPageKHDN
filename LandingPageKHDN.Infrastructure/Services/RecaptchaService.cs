using Google.Apis.Http;
using LandingPageKHDN.Application.Common;
using LandingPageKHDN.Application.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LandingPageKHDN.Infrastructure.Services
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public RecaptchaService(IConfiguration configuration, System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<ResponseModel<bool>> IsValidAsync(string token)
        {
            //var secretKey = _configuration["Recaptcha:SecretKey"];
            //var url = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}";

            //var response = await _httpClient.GetAsync(url);
            //var content = await response.Content.ReadAsStringAsync();
            //var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(content);

            //if (recaptchaResponse is { Success: true })
            //{
            //    return ResponseModel<bool>.SuccessResult(true, "Xác thực thành công", 200);
            //}

            //return ResponseModel<bool>.FailureResult("Xác thực reCAPTCHA thất bại.");

            await Task.CompletedTask;
            return ResponseModel<bool>.SuccessResult(true, "success");
        }

        private class RecaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
        }
    }
}
