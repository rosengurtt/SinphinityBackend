using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Sinphinity.Models;
using SinphinityExpApi.Models;
using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SinphinityExpApi.Clients
{
    public class GraphApiClient
    {
            private AppConfiguration _appConfiguration;
            private IHttpClientFactory _clientFactory;
            public GraphApiClient(IHttpClientFactory clientFactory, IOptions<AppConfiguration> AppConfiguration)
            {
                _appConfiguration = AppConfiguration.Value;
                _clientFactory = clientFactory;
            }

        public async Task<PaginatedList<Pattern>> GetPatternsAsync(long? styleId, long? bandId, long? songId, int? numberOfNotes,
            int? range, int? step, int? durationInTicks, bool? isMonotone, string? contains, int page = 0, int pageSize = 10)
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            var url = $"{_appConfiguration.GraphApiUrl}/api/patterns?page={page}&pageSize={pageSize}";
            if (styleId != null) url += $"&styleId={styleId}";
            if (bandId != null) url += $"&bandId={bandId}";
            if (songId != null) url += $"&songId={songId}";
            if (numberOfNotes != null) url += $"&numberOfNotes={numberOfNotes}";
            if (range != null) url += $"&range={range}";
            if (step != null) url += $"&step={step}";
            if (durationInTicks != null) url += $"&durationInTicks={durationInTicks}";
            if (durationInTicks != null) url += $"&isMonotone={isMonotone}";
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
                var errorMessage = $"Couldn't get styles";
                Log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
        }
}
