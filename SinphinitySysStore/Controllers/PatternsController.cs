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
            await _patternsRepository.InsertPatternsOfSongAsync(patternMatrix);
            var songInfo = await _songsRepository.GetSongInfoByIdAsync(patternMatrix.SongId);
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
    }
}
