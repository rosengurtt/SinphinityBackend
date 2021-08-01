using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Sinphinity.Models;
using Sinphinity.Models.Pattern;
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
        public async Task<PaginatedList<Song>> GetSongsAsync(int pageNo = 0, int pageSize = 10,  string contains = null, string styleId = null, string bandId = null)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/Songs?pageNo={pageNo}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(contains)) url += $"&contains={contains}";
            if (!string.IsNullOrEmpty(styleId)) url += $"&styleId={styleId}";
            if (!string.IsNullOrEmpty(bandId)) url += $"&bandId={bandId}";
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
        public async Task<Song> GetSongByIdAsync(string songId, int? SongSimplification = null)
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

        public async Task InsertPatterns(PatternMatrix patternMatrix)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(patternMatrix));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync($"{_appConfiguration.SysStoreUrl}/api/Patterns", content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = $"Couldn't insert patterns";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
    }
}
