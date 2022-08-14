using Microsoft.Extensions.Options;
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
    public class SysStoreClient
    {
        private AppConfiguration _appConfiguration;
        private IHttpClientFactory _clientFactory;
        public SysStoreClient(IHttpClientFactory clientFactory, IOptions<AppConfiguration> AppConfiguration)
        {
            _appConfiguration = AppConfiguration.Value;
            _clientFactory = clientFactory;
        }

        public async Task<bool> DoesSongExistAlready(string songName, string bandName)
        {
            var bands = await GetBandsAsync(contains: bandName);
            if (bands.totalItems == 0) return false;
            var bandId = bands.items.FirstOrDefault().Id;
            var songs = await GetSongsAsync(contains: songName, bandId: bandId);
            return songs.totalItems > 0;
        }
        public async Task<PaginatedList<Style>> GetStylesAsync(int page = 0, int pageSize = 10, string contains = "")
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/Styles?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(contains)) url += $"&contains={contains}";
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<PaginatedList<Style>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get styles";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        public async Task<PaginatedList<Band>> GetBandsAsync(int pageNo = 0, int pageSize = 10, string? contains = null, string? styleId = null)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/Bands?pageNo={pageNo}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(contains)) url += $"&contains={contains}";
            if (!string.IsNullOrEmpty(styleId)) url += $"&styleId={styleId}";
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<PaginatedList<Band>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get bands";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        public async Task<PaginatedList<Song>> GetSongsAsync(int pageNo = 0, int pageSize = 10,  string? contains = null, long styleId = 0, long bandId = 0)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/Songs?pageNo={pageNo}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(contains)) url += $"&contains={contains}";
            if (styleId!=0) url += $"&styleId={styleId}";
            if (bandId!=0) url += $"&bandId={bandId}";
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<PaginatedList<Song>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get songs";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }


        public async Task<PaginatedList<Phrase>> GetPhrasesAsync(
        long? styleId,
            long? bandId,
            long? songId,
            string? contains,
            int? numberOfNotes,
            long? durationInTicks,
            int? range,
            bool? isMonotone,
            int? step,
            int pageNo = 0,
            int pageSize = 10)
        {
            var responseContent = await GetPhrasesData(styleId, bandId, songId, contains, numberOfNotes, durationInTicks, range, isMonotone, step, pageNo, pageSize);
            dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
            var result = JsonConvert.SerializeObject(apiResponse.result);
            return JsonConvert.DeserializeObject<PaginatedList<Phrase>>(result);
        }
           private async Task<string> GetPhrasesData(
            long? styleId,
            long? bandId,
            long? songId,
            string contains,
            int? numberOfNotes,
            long? durationInTicks,
            int? range,
            bool? isMonotone,
            int? step,
            int pageNo,
            int pageSize)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/phrases" +
                BuildGetPhraseQueryString(styleId, bandId, songId, contains, numberOfNotes, durationInTicks, range, isMonotone, step, pageNo, pageSize);

            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();

            }
            else
            {
                var errorMessage = $"Couldn't get phrases";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        private string BuildGetPhraseQueryString(
            long? styleId,
            long? bandId,
            long? songId,
            string? contains,
            int? numberOfNotes,
            long? durationInTicks,
            int? range,
            bool? isMonotone,
            int? step,
            int pageNo,
            int pageSize)
        {
            var retObj = $"?pageNo={pageNo}&pageSize={pageSize}";
            if (styleId != null) retObj += $"&styleId={styleId}";
            if (bandId != null) retObj += $"&bandId={bandId}";
            if (songId != null) retObj += $"&songId={songId}";
            if (numberOfNotes != null) retObj += $"&numberOfNotes={numberOfNotes}";
            if (contains != null) retObj += $"&contains={contains}";
            if (durationInTicks != null) retObj += $"&durationInTicks={durationInTicks}";
            if (range != null) retObj += $"&range={range}";
            if (isMonotone != null) retObj += $"&isMonotone={isMonotone}";
            if (step != null) retObj += $"&step={step}";
            return retObj;
        }


        public async Task<PaginatedList<PhraseOccurrence>> GetOccurrencesOfPhrase(long phraseId, long songId = 0, int pageNo = 0, int pageSize = 20)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/phrases/{phraseId}/occurrences?songId={songId}&pageNo={pageNo}&pageSize={pageSize}";

            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<PaginatedList<PhraseOccurrence>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get phrases occurrences";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        public async Task<Song> GetSongByIdAsync(long songId, int? SongSimplification = null)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/Songs/{songId}";
            if (SongSimplification != null)
                url += $"?songSimplification={SongSimplification}";
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<Song>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get song";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }


        public async Task<Song> InsertSong(Song song)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(song));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync($"{_appConfiguration.SysStoreUrl}/api/Songs", content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<Song>(result);
            }
            else
            {
                var errorMessage = $"Couldn't insert song";
                if (response.StatusCode == HttpStatusCode.Conflict)
                    throw new SongAlreadyExistsException();
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        public async Task<Song> UpdateSong(Song song)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(song));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PutAsync($"{_appConfiguration.SysStoreUrl}/api/Songs", content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<Song>(result);
            }
            else
            {
                var errorMessage = $"Couldn't update song";
                if (response.StatusCode == HttpStatusCode.Conflict)
                    throw new SongAlreadyExistsException();
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        public async Task InsertPhrasesAsync(List<ExtractedPhrase> phrases, long songId)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(phrases));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpClient.Timeout = TimeSpan.FromMinutes(60);
            var url = $"{_appConfiguration.SysStoreUrl}/api/Phrases/{songId}";
            var response = await httpClient.PostAsync(url, content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = $"Couldn't insert patterns";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }



        public async Task GeneratePhrasesLinksForSong(long songId)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var response = await httpClient.PostAsync($"{_appConfiguration.SysStoreUrl}/api/Phrases/PhrasesLinks?songId={songId}", null);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var message = JsonConvert.SerializeObject(apiResponse.message);
                Log.Error(message);
                throw new ApplicationException(message);
            }

        }
    }
}
