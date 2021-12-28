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

        [HttpGet]
        public async Task<ActionResult> GetPatternsOfSong(long songId)
        {

           var pats= await _graphDbRepository.GetPatternsOfSong(songId);

            return Ok(pats);
        }

        [HttpGet("usage")]
        public async Task<ActionResult> GetUsageOfPattern(string asString)
        {

            var  pats = await _graphDbRepository.GetUsageOfPattern(asString);

            return Ok(pats);
        }

        [HttpGet("basicPatternApplications")]
        public async Task<ActionResult> GetApplicationsOfBasicPattern(string asString)
        {

            var  pats = await _graphDbRepository.GetApplicationsOfBasicPattern(asString);

            return Ok(pats);
        }

        [HttpGet("bands")]
        public async Task<ActionResult> GePatternsOfBand(long bandId, int? numberOfNotes, int? step, int? range, bool? isMonotone, int? durationInTicks)
        {

            var pats = await _graphDbRepository.GePatternsOfBand(bandId, numberOfNotes, step, range, isMonotone, durationInTicks);

            return Ok(pats.Select(x=>new {Pattern=x.Item1, Quant=x.Item2}));
        }
        [HttpGet("bandsSimilarityMatrix")]
        public async Task<ActionResult> GetSimilarityMatrixForBand(long bandId)
        {

            var info = await _graphDbRepository.GetSimilarityMatrixForBand(bandId);

            return Ok(info.Select(x => new { Band = x.Item1, Quant = x.Item2 }));
        }
    }
}
