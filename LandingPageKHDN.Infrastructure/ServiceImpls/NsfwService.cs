using LandingPageKHDN.Application.Services;
using LandingPageKHDN.Application.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LandingPageKHDN.Infrastructure.ServiceImpls
{
    public class NsfwService : INsfwService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private const double NSFW_Threshold = 0.4;
        public NsfwService(HttpClient httpClient, IOptions<HuggingFaceOptions> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _endpoint = options.Value.Endpoint;

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("NSFW-Checker");
        }

        public async Task<bool> IsSafeImageAsync(Stream imageStream)
        {
            imageStream.Position = 0;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_endpoint),
                Headers =
                {
                    { "Authorization", $"Bearer {_apiKey}" }
                },
                Content = new StreamContent(imageStream)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to call HuggingFace: {(int)response.StatusCode} - {response.ReasonPhrase}\n{err}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var predictions = JsonConvert.DeserializeObject<List<Prediction>>(responseString);

            var nsfwScore = predictions.FirstOrDefault(p => p.Label == "nsfw")?.Score ?? 0;

            return nsfwScore < NSFW_Threshold;
        }

        public class Prediction
        {
            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("score")]
            public float Score { get; set; }
        }
    }
}
