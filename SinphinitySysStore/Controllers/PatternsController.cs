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

        public PatternsController(PatternsRepository patternsRepository)
        {
            _patternsRepository = patternsRepository;
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadPatternMatrix(PatternMatrix patternMatrix)
        {
            await _patternsRepository.InsertPatternsOfSongAsync(patternMatrix);
            return Ok(new ApiOKResponse(null));
        }
    }
}
