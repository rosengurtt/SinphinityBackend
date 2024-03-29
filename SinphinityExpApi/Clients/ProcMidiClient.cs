﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Sinphinity.Models;
using SinphinityExpApi.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SinphinityExpApi.Clients
{
    public class ProcMidiClient
    {
        private AppConfiguration _appConfiguration;
        private IHttpClientFactory _clientFactory;
        public ProcMidiClient(IHttpClientFactory clientFactory, IOptions<AppConfiguration> AppConfiguration)
        {
            _appConfiguration = AppConfiguration.Value;
            _clientFactory = clientFactory;
        }

        public async Task<Song> ProcessSong(Song song)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(30);
            var content = new StringContent(JsonConvert.SerializeObject(song));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.PostAsync($"{_appConfiguration.ProcMidiUrl}/api/SongProcessing", content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<Song>(result);
            }
            else
            {
                var errorMessage = $"Couldn't process song";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        public async Task<bool> VerifySong(Song song)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(song));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync($"{_appConfiguration.ProcMidiUrl}/api/SongProcessing/verify", content);

            if (response.StatusCode == HttpStatusCode.OK)
                return true;
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return false;
            else
            {
                var errorMessage = $"Couldn't verify song";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        public async Task<string> GetMidiFragmentOfSong(
            Song song,
            int tempoInBeatsPerMinute,
            int simplificationVersion,
            int startInSeconds = 0,
            string? mutedTracks = null,
            long? fromTick = null,
            long? toTick = null)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(song));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var url = $"{_appConfiguration.ProcMidiUrl}/api/SongProcessing/{song.Id}?tempoInBeatsPerMinute={tempoInBeatsPerMinute}" +
                $"&simplificationVersion={simplificationVersion}&startInSeconds={startInSeconds}&mutedTracks={mutedTracks}&fromTick={fromTick}&toTick={toTick}";
            var response = await httpClient.PostAsync(url, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                return apiResponse.result;
            }
            else
            {
                var errorMessage = $"Couldn't process song";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        public async Task<string> GetMidiOfPhrase(string metricsAsString, string pitchesAsString, int instrument, int tempoInBPM, byte startingPitch = 60)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.ProcMidiUrl}/api/SongProcessing/phrase?&metricsAsString={metricsAsString}&pitchesAsString={pitchesAsString}&tempoInBPM={tempoInBPM}&instrument={instrument}&startingPitch={startingPitch}";
            var response = await httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                return apiResponse.result;
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
