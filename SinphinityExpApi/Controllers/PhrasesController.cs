using CommonRestLib.ErrorHandling;
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
        public async Task<IActionResult> ProcessPhrasesForSong(long songId)
        {
            var song = await _sysStoreClient.GetSongByIdAsync(songId);

            var phrases = await _procMelodyAnalyserClient.GetPhrasesOfSong(song);
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
        public async Task<IActionResult> ProcessPatternsForAllSongs(string? contains)
        {
            var keepLooping = true;
            var pageSize = 5;
            var page = 0;
            var alca = 1;
            Log.Information($"Starting pattern extraction");
            while (keepLooping)
            {
                PaginatedList<Song> songsBatch = await _sysStoreClient.GetSongsAsync(page, pageSize, contains);
                if (songsBatch.items?.Count > 0)
                {
                    foreach (var s in songsBatch.items)
                    {
                        var song = await _sysStoreClient.GetSongByIdAsync(s.Id, null);
                        // we don't need MidiBase64Encoded
                        song.MidiBase64Encoded = "";
                        if (song.Bars == null)
                            continue;
                        if (!song.IsSongProcessed || song.ArePhrasesExtracted) continue;
                        try
                        {
                            Log.Information($"{alca} - Start with song: {song.Name}");
                            if (song.SongSimplifications != null && song.SongSimplifications.Count > 0)
                            {
                                var phrases = await _procMelodyAnalyserClient.GetPhrasesOfSong(song);
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
     

        private long StartDifferenceInTicks(Pattern pat1, Pattern pat2)
        {
            var shorter = pat1.AsString.Length < pat2.AsString.Length ? pat1.AsString : pat2.AsString;
            var longer = pat1.AsString.Length < pat2.AsString.Length ? pat2.AsString : pat1.AsString;
            var notesBeforeSubPattern = longer.Substring(0, longer.IndexOf(shorter));
            if (notesBeforeSubPattern.Length == 0) return 0;

            var matches = Regex.Matches(notesBeforeSubPattern, @"[(][0-9]+,[-]?[0-9]+[)]");

            long retNumber = 0;
            var relativeNotesAsStrings = matches.Select(x => x.Value).ToList();
            for (var i = 0; i < relativeNotesAsStrings.Count; i++) {
                var noteDuration = long.Parse(relativeNotesAsStrings[i].Substring(1, relativeNotesAsStrings[i].IndexOf(",")));
                retNumber += noteDuration;
            }
            return retNumber;
        }

        // api/patterns?songInfoId=6187a4cb1b0680d2e5e5ae60
        [HttpGet]
        public async Task<ActionResult> GetPatternsAsync(long? styleId, long? bandId, long? songId, int? numberOfNotes,
            int? range, int? step, int? durationInTicks, bool? isMonotone, string? contains, int pageNo = 0, int pageSize = 10)
        {
            var patterns = await _graphApiClient.GetPatternsAsync(styleId, bandId, songId, numberOfNotes, range, step, durationInTicks, 
                isMonotone, contains, pageNo, pageSize);

            return Ok(new ApiOKResponse(patterns));
        }




    }
}

