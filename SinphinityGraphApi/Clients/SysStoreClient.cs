using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using SinphinityGraphApi.Models;
using SinphinityModel.Pattern;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SinphinityGraphApi.Clients
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

        public async Task<PaginatedList<PatternSong>> GetPatternsSongsAsync(int pageNo = 0, int pageSize = 10, string contains = null, string styleId = null, string bandId = null)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.SysStoreUrl}/api/patterns/PatternsSongs?pageNo={pageNo}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(contains)) url += $"&contains={contains}";
            if (!string.IsNullOrEmpty(styleId)) url += $"&styleId={styleId}";
            if (!string.IsNullOrEmpty(bandId)) url += $"&bandId={bandId}";
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject<ExpandoObject>(responseContent);
                var result = JsonConvert.SerializeObject(apiResponse.result);
                return JsonConvert.DeserializeObject<PaginatedList<PatternSong>>(result);
            }
            else
            {
                var errorMessage = $"Couldn't get patternsSongs";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
    }
}
