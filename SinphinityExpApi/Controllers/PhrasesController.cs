using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Sinphinity.Models;
using SinphinityExpApi.Clients;
using SinphinityExpApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SinphinityExpApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PhrasesController : ControllerBase
    {
        private SysStoreClient _sysStoreClient;
        private ProcMidiClient _procMidiClient;
        private ProcMelodyAnalyserClient _procMelodyAnalyserClient;
        private GraphApiClient _graphApiClient;

        public PhrasesController(SysStoreClient sysStoreClient, ProcMidiClient procMidiClient, ProcMelodyAnalyserClient procMelodyAnalyserClient, GraphApiClient graphApiClient)
        {
            _sysStoreClient = sysStoreClient;
            _procMidiClient = procMidiClient;
            _procMelodyAnalyserClient = procMelodyAnalyserClient;
            _graphApiClient = graphApiClient;
        }

        // api/songs/patterns/processPhrases?songId=60d577ef035c715d2ea7ef60
        [HttpGet("processSingle")]
        public async Task<IActionResult> ProcessPhrasesForSong(long songId)
        {

            var phrases = await _procMelodyAnalyserClient.GetPhrasesOfSong(songId);
            await _sysStoreClient.InsertPhrasesAsync(phrases, songId);

            return Ok(new ApiOKResponse("Salvamos las phrases papi"));
        }

        /// <summary>
        /// Gets all songs that have the flag ArePatternsExtracted=false and extracts their patterns 
        /// </summary>
        /// <param name="contains"></param>
        /// <returns></returns>
        // api/songs/patterns/batch
        [HttpGet("batch")]
        public async Task<IActionResult> ProcessPhrasesForAllSongs(string? contains)
        {
            var keepLooping = true;
            var pageSize = 5;
            var page = 0;
            var alca = 1;
            Log.Information($"Starting phrase extraction");
            while (keepLooping)
            {
                PaginatedList<Song> songsBatch = await _sysStoreClient.GetSongsAsync(page, pageSize, contains);
                if (songsBatch.items?.Count > 0)
                {
                    foreach (var s in songsBatch.items)
                    {
                        if (s.ArePhrasesExtracted || !s.IsSongProcessed || !s.IsMidiCorrect || s.CantBeProcessed)
                            continue;
                        try
                        {
                            Log.Information($"{alca} - Start with song: {s.Name}");
                            var phrases = await _procMelodyAnalyserClient.GetPhrasesOfSong(s.Id);
                            Log.Information($"Phrase extracion completed OK for {s.Name}");
                            await _sysStoreClient.InsertPhrasesAsync(phrases, s.Id);
                            Log.Information($"Saved OK {s.Name}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Couldn't process song {s.Name}");
                          //  s.CantBeProcessed = true;
                          //  await _sysStoreClient.UpdateSong(s);
                        }
                        alca++;
                    }
                    page += 1;
                }
                else
                    keepLooping = false;
            }
            return Ok(new ApiOKResponse(null));
        }




        [HttpGet]
        public async Task<ActionResult> GetPhrasesAsync(long? styleId, long? bandId, long? songId, byte? voiceId, string phraseType, string contains, int? numberOfNotes,
            int? durationInTicks, int? range, bool? isMonotone, int? step, int pageNo = 0, int pageSize = 10)
        {
            var phrases = await _sysStoreClient.GetPhrasesAsync(styleId, bandId, songId, voiceId, contains, numberOfNotes, durationInTicks,
              range, isMonotone, step, pageNo, pageSize);
            return Ok(new ApiOKResponse(phrases));
        }
      

        [HttpGet("midi")]
        public async Task<ActionResult> GetPhraseMidiAsync(string metricsAsString, string pitchesAsString, int instrument = 0, 
            int tempoInBPM = 90, byte startingPitch = 60)
        {
            try
            {

                var base64encodedMidiBytes = await _procMidiClient.GetMidiOfPhrase(metricsAsString, pitchesAsString, instrument, tempoInBPM, startingPitch);
                var ms = new MemoryStream(Convert.FromBase64String(base64encodedMidiBytes));

                return File(ms, MediaTypeNames.Text.Plain, $"Phrase {metricsAsString}-{pitchesAsString}");
            }
            catch (Exception ex)
            {

            }
            return null;

        }
        [HttpGet("{phraseId}/occurrences")]
        public async Task<ActionResult> GetOccurrencesOfPhraseAsync(long phraseId, long songId = 0, int pageNo = 0, int pageSize = 20)
        {
            return Ok(new ApiOKResponse(await _sysStoreClient.GetOccurrencesOfPhrase(phraseId, songId, pageNo, pageSize)));
        }

        [HttpPost("processPhrasesLinksBatch")]
        public async Task<IActionResult> ProcessBatch(long styleId, long bandId)
        {
            var keepLooping = true;
            var pageSize = 40;
            var page = 0;
            var alca = 1;
            while (keepLooping)
            {
                PaginatedList<Song> songsBatch = await _sysStoreClient.GetSongsAsync(page, pageSize, null, styleId, bandId);
                if (songsBatch.items?.Count > 0)
                {
                    foreach (var s in songsBatch.items)
                    {
                        try
                        {
                            Log.Information($"{alca} - Start with song: {s.Name}");
                            await _sysStoreClient.GeneratePhrasesLinksForSong(s.Id);
                            Log.Information($"Processed OK {s.Name}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Couldn't process song {s.Name}");
                        }
                        alca++;
                    }
                    page += 1;
                }
                else
                    keepLooping = false;
            }
            return Ok(new ApiOKResponse(null));
        }


    }
}

