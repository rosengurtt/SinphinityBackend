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
        public async Task<PaginatedList<Band>> GetBandsAsync(int pageNo = 0, int pageSize = 10, string contains = null, string styleId = null)
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
        public async Task<PaginatedList<Song>> GetSongsAsync(int pageNo = 0, int pageSize = 10,  string contains = null, long styleId = 0, long bandId = 0)
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

        public async Task InsertPatternsAsync(long songId, Dictionary<string, HashSet<Occurrence>> patterns)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(patterns));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            var response = await httpClient.PostAsync($"{_appConfiguration.SysStoreUrl}/api/Patterns/{songId}", content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = $"Couldn't insert patterns";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        public async Task<PaginatedList<Pattern>> GetPatternsAsync(int pageNo, int pageSize, string styleId, string bandId, string songInfoId)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/Patterns?pageNo={pageNo}&pageSize={pageSize}";
            if (styleId != null) url += $"&styleId={styleId}";
            if (bandId != null) url += $"&bandId={bandId}";
            if (songInfoId != null) url += $"&songInfoId={songInfoId}";
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<PaginatedList<Pattern>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get patterns for sytleId={styleId}&bandId={bandId}&songInfoId={songInfoId}";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        public async Task<PaginatedList<Occurrence>> GetPatternOccurrencesAsync(int pageNo, int pageSize, string patternId)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/Patterns/Occurrences?pageNo={pageNo}&pageSize={pageSize}&patternId={patternId}";
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<PaginatedList<Occurrence>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get occurrences or pattern {patternId}";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
    }
}
