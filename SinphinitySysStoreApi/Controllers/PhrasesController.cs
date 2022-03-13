using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinitySysStore.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinitySysStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhrasesController : ControllerBase
    {
        private PhrasesRepository _phrasesRepository;

        public PhrasesController(PhrasesRepository phrasesRepository)
        {
            _phrasesRepository = phrasesRepository;
        }

        [HttpPost("{songId}"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadPhrasesOfSong(long songId, List<Dictionary<string, List<SongLocation>>> phrases)
        {
            await _phrasesRepository.SavePhrasessOfSongAsync(phrases, songId);
            return Ok(new ApiOKResponse(null));
        }





    }
}

