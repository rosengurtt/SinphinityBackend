using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using SinphinityGraphApi.Clients;
using SinphinityGraphApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityGraphApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatternsController : ControllerBase
    {
        private IDriver _driver;
        private SysStoreClient _sysStoreClient;
        private GraphDbRepository _graphDbRepository;

        public PatternsController(IDriver driver, SysStoreClient sysStoreClient, GraphDbRepository graphDbRepository)
        {
            _driver = driver;
            _sysStoreClient = sysStoreClient;
            _graphDbRepository = graphDbRepository;
        }

        [HttpGet("import")]
        public async Task<ActionResult> ImportPatterns()
        {
            var page = 0;
            var pageSize = 5;
            var keepLoping = true;
            while (keepLoping)
            {
                var patternsSongs = await _sysStoreClient.GetPatternsSongsAsync(page, pageSize);
                foreach (var ps in patternsSongs.items)
                    await _graphDbRepository.AddPattern(ps);
                if (patternsSongs.totalPages == page + 1)
                    keepLoping = false;
                else
                    page++;
            }

            return Ok("Job done");
        }
    }
}
