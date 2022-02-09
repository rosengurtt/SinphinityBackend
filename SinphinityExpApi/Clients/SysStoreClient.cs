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

        public async Task<PaginatedList<Pattern>> GetPatternsPaginatedAsync(int pageNo = 0, int pageSize = 10, string contains = null)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/patterns?pageNo={pageNo}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(contains)) url += $"&contains={contains}";
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
                var errorMessage = $"Couldn't get patterns";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        public async Task<HashSet<Pattern>> GetPatternsAsync(string contains = null)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var keepLooping = true;
            var page = 0;
            var retObj = new HashSet<Pattern>();
            while (keepLooping)
            {
                var url = $"{_appConfiguration.SysStoreUrl}/api/patterns/patterns?pageNo={page}&pageSize=10";
                if (!string.IsNullOrEmpty(contains)) url += $"&contains={contains}";
                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                    var result = JsonConvert.SerializeObject(apiResponse.result);
                    var pageData = JsonConvert.DeserializeObject<PaginatedList<Pattern>>(result);

                    if (pageData.items?.Count > 0)
                    {
                        foreach (Pattern o in pageData.items)
                            retObj.Add(o);
                    }
                    else
                        keepLooping = false;
                    page++;
                }
                else
                {
                    var errorMessage = $"Couldn't get patterns";
                    Log.Error(errorMessage);
                    throw new ApplicationException(errorMessage);
                }

            }
            return retObj;
        }
        public async Task<HashSet<Occurrence>> GetOccurrencesOfPatternAsync(long patternId)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var keepLooping = true;
            var page = 0;
            var retObj = new HashSet<Occurrence>();
            while (keepLooping)
            {
                var url = $"{_appConfiguration.SysStoreUrl}/api/patterns/occurrences?patternId={patternId}&pageNo={page}&pageSize=10";
                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                    var result = JsonConvert.SerializeObject(apiResponse.result);
                   var pageData = JsonConvert.DeserializeObject<PaginatedList<Occurrence>>(result);

                    if (pageData.items?.Count > 0)
                    {
                        foreach (Occurrence o in pageData.items)
                            retObj.Add(o);
                    }
                    else
                        keepLooping = false;
                    page++;
                }
                else
                {
                    var errorMessage = $"Couldn't get occurrences of pattern {patternId}";
                    Log.Error(errorMessage);
                    throw new ApplicationException(errorMessage);
                }

            }
            return retObj;
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

        public async Task InsertPatternsAsync(HashSet<string> patterns, long songId)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(patterns));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpClient.Timeout = TimeSpan.FromMinutes(50);
            var url = $"{_appConfiguration.SysStoreUrl}/api/Patterns?songId={songId}";
            var response = await httpClient.PostAsync(url, content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = $"Couldn't insert patterns";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
    

    }
}
