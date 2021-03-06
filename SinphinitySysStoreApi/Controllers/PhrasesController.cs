using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinitySysStore.Data;
using SinphinitySysStore.Models;
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
        public async Task<ActionResult> UploadPhrasesOfSongAsync(long songId, List<Dictionary<string, List<PhraseLocation>>> phrases)
        {
            if (phrases.Count != 6)
                throw new Exception("Me mandaron cualquier mierda");
            await _phrasesRepository.SavePhrasesMetricsOfSongAsync(phrases[0], songId);
            await _phrasesRepository.SavePhrasesPitchesOfSongAsync(phrases[1], songId);
            await _phrasesRepository.SavePhrasesOfSongAsync(phrases[2], songId);
            await _phrasesRepository.SaveEmbellishedPhrasesMetricsOfSongAsync(phrases[3], songId);
            await _phrasesRepository.SaveEmbellishedPhrasesPitchesOfSongAsync(phrases[4], songId);
            await _phrasesRepository.SaveEmbellishedPhrasesOfSongAsync(phrases[5], songId);
            await _phrasesRepository.UpateSong(songId);

            return Ok(new ApiOKResponse(null));
        }

        [HttpGet]
        public async Task<ActionResult> GetPhrasesAsync(
            long? styleId,
            long? bandId,
            long? songId,
            string type,
            string contains,
            int? numberOfNotes,
            long? durationInTicks,
            int? range,
            bool? isMonotone,
            int? step,
            int pageNo = 0,
            int pageSize = 10)
        {
            var phraseType = type == null ? PhraseTypeEnum.Metrics : (PhraseTypeEnum)Enum.Parse(typeof(PhraseTypeEnum), type, true);
            (var totaPhrases, var phrases) = await _phrasesRepository.GetPhrasesAsync(styleId, bandId, songId, phraseType, contains, numberOfNotes,
                durationInTicks, range, isMonotone, step, pageNo, pageSize);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totaPhrases,
                totalPages = (int)Math.Ceiling((double)totaPhrases / pageSize),
                items = phrases
            };
            return Ok(new ApiOKResponse(retObj));
        }
        [HttpGet("{phraseId}/occurrences")]
        public async Task<ActionResult> GetOccurrencesOfPhraseAsync(long phraseId, long songId = 0, int pageNo = 0, int pageSize = 20)
        {
            (var totaOccs, var occurreces) = await _phrasesRepository.GetOccurrencesOfPhraseAsync(phraseId, songId, pageNo, pageSize);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totaOccs,
                totalPages = (int)Math.Ceiling((double)totaOccs / pageSize),
                items = occurreces
            };
            return Ok(new ApiOKResponse(retObj));
        }
  
        [HttpPost("phrasesLinks")]
        public async Task<ActionResult> GeneratePhrasesLinksForSong(long songId)
        {
            await _phrasesRepository.GeneratePhrasesLinksForSong(songId);
            return Ok(new ApiOKResponse(null));

        }
    }
}

