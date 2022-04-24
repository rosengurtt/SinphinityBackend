﻿using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Sinphinity.Models;
using SinphinityExpApi.Clients;
using SinphinityExpApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [HttpGet("processPhrases")]
        public async Task<IActionResult> ProcessPhrasesForSong(long songId, int songSimplification)
        {
            var song = await _sysStoreClient.GetSongByIdAsync(songId);

            var phrases = await _procMelodyAnalyserClient.GetPhrasesOfSong(song, songSimplification);
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
                        if (!s.IsSongProcessed || s.ArePhrasesExtracted) continue;
                        var song = await _sysStoreClient.GetSongByIdAsync(s.Id, null);
                        // we don't need MidiBase64Encoded
                        song.MidiBase64Encoded = "";
                        if (song.Bars == null)
                            continue;
                        try
                        {
                            Log.Information($"{alca} - Start with song: {song.Name}");
                            if (song.SongSimplifications != null && song.SongSimplifications.Count > 0)
                            {
                                var phrases = await _procMelodyAnalyserClient.GetPhrasesOfSong(song, 1);
                                Log.Information($"Phrase extracion completed OK for {song.Name}");
                                await _sysStoreClient.InsertPhrasesAsync(phrases, song.Id);
                                Log.Information($"Saved OK {song.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Couldn't process song {song.Name}");
                            s.CantBeProcessed = true;
                            await _sysStoreClient.UpdateSong(s);
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
        public async Task<ActionResult> GetPhrasesAsync(long? styleId, long? bandId, long? songId, string phraseType, string contains, int? numberOfNotes,
            int? durationInTicks, int? range, bool? isMonotone, int? step, int pageNo = 0, int pageSize = 10)
        {
            var type = phraseType == null ? PhraseTypeEnum.Metrics : (PhraseTypeEnum)Enum.Parse(typeof(PhraseTypeEnum), phraseType, true);
            switch (type)
            {
                case PhraseTypeEnum.Metrics:
                    return Ok(new ApiOKResponse(await _sysStoreClient.GetPhraseMetricsAsync(styleId, bandId, songId,  contains, numberOfNotes, durationInTicks,
                      range, isMonotone, step, pageNo, pageSize)));
                case PhraseTypeEnum.Pitches:
                    return Ok(new ApiOKResponse(await _sysStoreClient.GetPhrasePitchesAsync(styleId, bandId, songId, contains, numberOfNotes, durationInTicks,
                      range, isMonotone, step, pageNo, pageSize)));
                case PhraseTypeEnum.Both:
                    return Ok(new ApiOKResponse(await _sysStoreClient.GetPhrasesAsync(styleId, bandId, songId, contains, numberOfNotes, durationInTicks,
                      range, isMonotone, step, pageNo, pageSize)));
                case PhraseTypeEnum.EmbelishedMetrics:
                    return Ok(new ApiOKResponse(await _sysStoreClient.GetEmbellishedPhrasesMertricsAsync(styleId, bandId, songId, contains, numberOfNotes, durationInTicks,
                      range, isMonotone, step, pageNo, pageSize)));
                case PhraseTypeEnum.EmbelishedPitches:
                    return Ok(new ApiOKResponse(await _sysStoreClient.GetEmbellishedPhrasesPitchesAsync(styleId, bandId, songId, contains, numberOfNotes, durationInTicks,
                      range, isMonotone, step, pageNo, pageSize)));
                case PhraseTypeEnum.EmbellishedBoth:
                    return Ok(new ApiOKResponse(await _sysStoreClient.GetEmbellishedPhrasesAsync(styleId, bandId, songId, contains, numberOfNotes, durationInTicks,
                      range, isMonotone, step, pageNo, pageSize)));
                default:
                    throw new Exception("Que mierda esta pidiendo?");
            }
        }
    }
}

