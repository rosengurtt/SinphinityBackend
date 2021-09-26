using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Sinphinity.Models.Pattern;
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

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadPatternMatrix(PatternMatrix patternMatrix)
        {
            var songInfo = await _songsRepository.GetSongInfoByIdAsync(patternMatrix.SongId);
            await _patternsRepository.InsertPatternsOfSongAsync(patternMatrix, songInfo);
            songInfo.ArePatternsExtracted = true;
            await _songsRepository.UpdateSongInfo(songInfo);
            return Ok(new ApiOKResponse(null));
        }

        [HttpGet]
        public async Task<ActionResult> GetPatterns(int page, int pageSize)
        {
            var patterns = await _patternsRepository.GetPatternsAsync(pageSize, page);
            return Ok(new ApiOKResponse(patterns));
        }
        [HttpGet("{songInfoId}")]
        public async Task<ActionResult> GetPatternsOfSong(string songInfoId)
        {
            var patterns = await _patternsRepository.GetPatternsOfSongAsync(songInfoId);
            return Ok(new ApiOKResponse(patterns));
        }
        [HttpGet("PatternsSongs")]
        public async Task<ActionResult> GetPatternsSongs(int pageNo, int pageSize, string contains)
        {        
            var totalPatternsSongs = await _patternsRepository.GetPatternsSongsCountAsync(contains);
            var patternsSongs = await _patternsRepository.GetPatternsSongsAsync(pageSize, pageNo, contains);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totalPatternsSongs,
                totalPages = (int)Math.Ceiling((double)totalPatternsSongs / pageSize),
                items = patternsSongs
            };
            return Ok(new ApiOKResponse(retObj));

        }
    }
}
