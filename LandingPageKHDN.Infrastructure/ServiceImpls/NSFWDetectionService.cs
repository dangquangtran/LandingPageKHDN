using LandingPageKHDN.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LandingPageKHDN.Infrastructure.ServiceImpls
{
    public class NSFWDetectionService : INSFWDetectionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NSFWDetectionService> _logger;
        private readonly string _apiKey;

        public NSFWDetectionService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NSFWDetectionService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["DeepAI:ApiKey"];
            _logger = logger;
        }

        public async Task<bool> IsSafeImageAsync(Stream imageStream)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Api-Key", _apiKey);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(imageStream), "image", "upload.jpg");
            _logger.LogInformation("Using DeepAI API Key: {ApiKey}", _apiKey?.Substring(0, 5) + "*****");

            try
            {
                var response = await client.PostAsync("https://api.deepai.org/api/nsfw-detector", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("NSFW API call failed with status code: {StatusCode}", response.StatusCode);
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("DeepAI Response JSON: {Json}", json);

                var result = JsonSerializer.Deserialize<DeepAiNsfwResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                double score = result?.Output?.NsfwScore ?? 0.0;
                _logger.LogInformation("DeepAI NSFW Score: {Score}", score);

                return score < 0.8;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while detecting NSFW content.");
                return false;
            }
        }


        private class DeepAiNsfwResponse
        {
            public OutputData Output { get; set; }

            public class OutputData
            {
                public double NsfwScore { get; set; }
            }
        }
    }
}
