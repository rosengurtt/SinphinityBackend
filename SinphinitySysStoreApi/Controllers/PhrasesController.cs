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
            if (phrases.Count != 6)
                throw new Exception("Me mandaron cualquier mierda");
            // PhrasesMetrics, PhrasesPitches, Phrases, EmbellishedPhrasesMetrics, EmbellishedPhrasesPitches, jEmbellishedPhrases
            await _phrasesRepository.SavePhrasesMetricsOfSongAsync(phrases[0], songId);
            await _phrasesRepository.SavePhrasesPitchesOfSongAsync(phrases[1], songId);
            await _phrasesRepository.SavePhrasesOfSongAsync(phrases[2], songId);
            await _phrasesRepository.SaveEmbellishedPhrasesMetricsOfSongAsync(phrases[3], songId);
            await _phrasesRepository.SaveEmbellishedPhrasesPitchesOfSongAsync(phrases[4], songId);
            await _phrasesRepository.SaveEmbellishedPhrasesOfSongAsync(phrases[5], songId);
            await _phrasesRepository.UpateSong(songId);

            return Ok(new ApiOKResponse(null));
        }

      





    }
}

