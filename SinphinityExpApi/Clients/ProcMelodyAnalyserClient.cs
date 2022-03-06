﻿
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
    public class ProcMelodyAnalyserClient
    {
        private AppConfiguration _appConfiguration;
        private IHttpClientFactory _clientFactory;
        public ProcMelodyAnalyserClient(IHttpClientFactory clientFactory, IOptions<AppConfiguration> AppConfiguration)
        {
            _appConfiguration = AppConfiguration.Value;
            _clientFactory = clientFactory;
        }

        public async Task<Dictionary<string, List<SongLocation>>> GetPhrasesOfSong(Song song)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(500);
            var content = new StringContent(JsonConvert.SerializeObject(song));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.PostAsync($"{_appConfiguration.ProcMelodyAnalyserUrl}/api/phrases", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<Dictionary<string, List<SongLocation>>>(result);
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


