
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Sinphinity.Models;
using SinphinityExpApi.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;

namespace SinphinityExpApi.Clients
{
    public class ProcMelodyAnalyserClient
    {
        private AppConfiguration _appConfiguration;
        private IHttpClientFactory _clientFactory;
        public ProcMelodyAnalyserClient(IHttpClientFactory clientFactory, IOptions<AppConfiguration> AppConfiguration)
        {
            _appConfiguration = AppConfiguration.Value;
            _clientFactory = clientFactory;
        }

        public async Task<List<ExtractedPhrase>> GetPhrasesOfSong(long songId)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(60);
       

            var response = await httpClient.GetAsync($"{_appConfiguration.ProcMelodyAnalyserUrl}/api/phrases?songId={songId}");

            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<List<ExtractedPhrase>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't process song. {responseContent}";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

    }
}



