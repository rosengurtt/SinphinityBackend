using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Sinphinity.Models;
using SinphinityExpApi.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SinphinityExpApi.Clients
{
    public class ProcPatternClient
    {
        private AppConfiguration _appConfiguration;
        private IHttpClientFactory _clientFactory;
        public ProcPatternClient(IHttpClientFactory clientFactory, IOptions<AppConfiguration> AppConfiguration)
        {
            _appConfiguration = AppConfiguration.Value;
            _clientFactory = clientFactory;
        }

        public async Task<Dictionary<string, HashSet<Occurrence>>> GetPatternMatrixOfSong(Song song)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(500);
            var content = new StringContent(JsonConvert.SerializeObject(song));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.PostAsync($"{_appConfiguration.ProcPatternUrl}/api/songPatterns", content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<Dictionary<string, HashSet<Occurrence>>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't process song";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

    }
}


