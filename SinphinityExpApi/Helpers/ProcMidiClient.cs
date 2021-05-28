using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SinphinityExpApi.Helpers
{
    public class ProcMidiClient
    {
        private IHttpClientFactory _clientFactory;

        public ProcMidiClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task ProcessMidiFile()
        {
            HttpClient httpClient = _clientFactory.CreateClient();
     
        }
    }
}
