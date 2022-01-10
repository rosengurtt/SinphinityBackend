using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Sinphinity.Models;
using SinphinitySysStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinitySysStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatternsController : ControllerBase
    {
        private PatternsRepository _patternsRepository;

        public PatternsController(PatternsRepository patternsRepository)
        {
            _patternsRepository = patternsRepository;
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadPatternMatrix(long? songId, Dictionary<string, HashSet<Occurrence>> patterns)
        {
            await _patternsRepository.SavePatternsOfSongAsync(patterns, songId);
            return Ok(new ApiOKResponse(null));
        }

        [HttpGet]
        public async Task<ActionResult> GetPatternsPaginatedAsync(int pageNo = 0, int pageSize = 10, string contains = null)
        {
            (var totaPatterns, var patterns) = await _patternsRepository.GetPatternsAsync(pageNo, pageSize, contains);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totaPatterns,
                totalPages = (int)Math.Ceiling((double)totaPatterns / pageSize),
                items = patterns
            };
            return Ok(new ApiOKResponse(retObj));
        }

        [HttpGet("occurrences")]
        public async Task<ActionResult> GetPatternsOccurrencesPaginatedAsync(long patternId, int pageNo = 0, int pageSize = 10)
        {
            (var totaOccs, var occs) = await _patternsRepository.GetPatternOccurencesAsync(pageNo, pageSize, patternId);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totaOccs,
                totalPages = (int)Math.Ceiling((double)totaOccs / pageSize),
                items = occs
            };
            return Ok(new ApiOKResponse(retObj));
        }

    }
}

