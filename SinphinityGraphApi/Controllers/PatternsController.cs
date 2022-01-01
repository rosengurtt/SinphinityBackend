using CommonRestLib.ErrorHandling;
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
        public async Task<ActionResult> GetPatterns(long? styleId, long? bandId, long? songId, string contains, int page = 0, int pageSize = 10)
        {
            (var totalPats, var pats) = await _graphDbRepository.GetPatternsAsync(styleId, bandId, songId, contains, page, pageSize);
            var retObj = new
            {
                page,
                pageSize,
                totalItems = totalPats,
                totalPages = (int)Math.Ceiling((double)totalPats / pageSize),
                items = pats
            };
            return Ok(new ApiOKResponse(retObj));
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

   
        [HttpGet("bandsSimilarityMatrix")]
        public async Task<ActionResult> GetSimilarityMatrixForBand(long bandId)
        {

            var info = await _graphDbRepository.GetSimilarityMatrixForBand(bandId);

            return Ok(info.Select(x => new { Band = x.Item1.Name, Quant = x.Item2 }));
        }
        [HttpGet("songsSimilarityMatrix")]
        public async Task<ActionResult> GetSimilarityMatrixForSong(long songId)
        {

            var info = await _graphDbRepository.GetSimilarityMatrixForSong(songId);

            return Ok(info.Select(x => new { Band = x.Item1.Name, Quant = x.Item2 }));
        }
    }
}
