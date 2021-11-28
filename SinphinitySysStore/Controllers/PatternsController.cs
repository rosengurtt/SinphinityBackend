using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Sinphinity.Models;
using SinphinitySysStore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatternsController : ControllerBase
    {
        private PatternsRepository _patternsRepository;
        private SongsRepository _songsRepository;

        public PatternsController(PatternsRepository patternsRepository, SongsRepository songsRepository)
        {
            _patternsRepository = patternsRepository;
            _songsRepository = songsRepository;
        }

        [HttpPost("{songId}"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadPatternMatrix(string songId, Dictionary<string, HashSet<Occurrence>> patterns)
        {
            var songInfo = await _songsRepository.GetSongInfoByIdAsync(songId);
            await _patternsRepository.InsertPatternsOfSongAsync(patterns, songInfo);
            songInfo.ArePatternsExtracted = true;
            await _songsRepository.UpdateSongInfo(songInfo);
            return Ok(new ApiOKResponse(null));
        }

        [HttpGet]
        public async Task<ActionResult> GetPatterns(string bandId, string styleId, string songInfoId, int pageNo = 0, int pageSize = 10)
        {
            (int total, var patterns) = await _patternsRepository.GetPatternsAsync(pageNo, pageSize, styleId, bandId, songInfoId);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = total,
                totalPages = (int)Math.Ceiling((double)total / pageSize),
                items = patterns
            };
            return Ok(new ApiOKResponse(retObj));
        }




        [HttpGet("Occurrences")]
        public async Task<ActionResult> GetPatternOccurrences(string patternId, int pageNo = 0, int pageSize = 10)
        {
            (var totalOccurrences, var occurrences) = await _patternsRepository.GetPatternOccurrencesAsync(pageNo, pageSize, patternId);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totalOccurrences,
                totalPages = (int)Math.Ceiling((double)totalOccurrences / pageSize),
                items = occurrences
            };
            return Ok(new ApiOKResponse(retObj));
        }
    }
}
