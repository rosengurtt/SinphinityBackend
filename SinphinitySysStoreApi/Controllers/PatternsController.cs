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

        [HttpPost("{songId}"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadPatternMatrix(long songId, Dictionary<string, HashSet<Occurrence>> patterns)
        {
            await _patternsRepository.SavePatternsOfSong(patterns, songId);
            return Ok(new ApiOKResponse(null));
        }

    }
}

