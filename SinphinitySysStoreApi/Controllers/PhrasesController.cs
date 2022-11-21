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
        public async Task<ActionResult> UploadPhrasesOfSongAsync(long songId, List<ExtractedPhrase> extractedPhrases)
        {
            try
            {
                await _phrasesRepository.SavePhrasesAsync(extractedPhrases, songId);
                await _phrasesRepository.GeneratePhrasesLinksForSong(songId);
                await _phrasesRepository.UpateSong(songId);
            }
            catch(Exception ex)
            {


            }
            return Ok(new ApiOKResponse(null));
        }

        [HttpGet]
        public async Task<ActionResult> GetPhrasesAsync(
            long? styleId,
            long? bandId,
            long? songId,
            byte? voiceId,
            string contains,
            int? numberOfNotes,
            long? durationInTicks,
            int? range,
            bool? isMonotone,
            int? step,
            int pageNo = 0,
            int pageSize = 10)
        {
            int totalPhrases;
            List<Sinphinity.Models.Phrase> phrases;
            if (songId != null && voiceId != null)
                (totalPhrases, phrases) = await _phrasesRepository.GetPhrasesOfSongAndVoiceAsync((long)songId, (byte)voiceId, pageNo, pageSize);
            else
                (totalPhrases, phrases) = await _phrasesRepository.GetPhrasesAsync(styleId, bandId, songId, contains, numberOfNotes,
                    durationInTicks, range, isMonotone, step, pageNo, pageSize);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totalPhrases,
                totalPages = (int)Math.Ceiling((double)totalPhrases / pageSize),
                items = phrases.Select(x =>
                new
                {
                    Id = x.Id,
                    MetricsAsString = x.MetricsAsString,
                    PitchesAsString = x.PitchesAsString,
                    Equivalences = x.Equivalences
                })
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
  
        //[HttpPost("phrasesLinks")]
        //public async Task<ActionResult> GeneratePhrasesLinksForSong(long songId)
        //{
        //    await _phrasesRepository.GeneratePhrasesLinksForSong(songId);
        //    return Ok(new ApiOKResponse(null));

        //}
    }
}

