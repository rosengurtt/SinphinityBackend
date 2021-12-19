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
        public class PatternsController : ControllerBase
        {
            private SysStoreClient _sysStoreClient;
            private ProcMidiClient _procMidiClient;
            private ProcPatternClient _procPatternClient;

            public PatternsController(SysStoreClient sysStoreClient, ProcMidiClient procMidiClient, ProcPatternClient procPatternClient)
            {
                _sysStoreClient = sysStoreClient;
                _procMidiClient = procMidiClient;
                _procPatternClient = procPatternClient;
            }

        // api/songs/patterns/processSong?songId=60d577ef035c715d2ea7ef60
        [HttpGet("processSong")]
        public async Task<IActionResult> ProcessPatternsForSong(long songId)
        {
            var song = await _sysStoreClient.GetSongByIdAsync(songId);

            var patterns = await _procPatternClient.GetPatternMatrixOfSong(song);
            await _sysStoreClient.InsertPatternsAsync(songId, patterns);

            return Ok(new ApiOKResponse(patterns));
        }

        // api/songs/patterns/batch
        [HttpGet("batch")]
        public async Task<IActionResult> ProcessPatternsForAllSongs(string contains)
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
                        if (!song.IsSongProcessed || song.ArePatternsExtracted) continue;
                        try
                        {
                            Log.Information($"{alca} - Start with song: {song.Name}");
                            if (song.SongSimplifications != null && song.SongSimplifications.Count > 0)
                            {
                                var patternMatrix = await _procPatternClient.GetPatternMatrixOfSong(song);
                                Log.Information($"Pattern extracion completed OK for {song.Name}");
                                await _sysStoreClient.InsertPatternsAsync(song.Id, patternMatrix);
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

        // api/patterns?songInfoId=6187a4cb1b0680d2e5e5ae60
        [HttpGet]
        public async Task<ActionResult> GetPatterns(string bandId, string styleId, string songInfoId, int pageNo = 0, int pageSize = 10)
        {
            var patterns = await _sysStoreClient.GetPatternsAsync(pageNo, pageSize, styleId, bandId, songInfoId);

            return Ok(new ApiOKResponse(patterns));
        }




        [HttpGet("Occurrences")]
        public async Task<ActionResult> GetPatternOccurrences(string patternId, int pageNo = 0, int pageSize = 10)
        {
            var occurrences = await _sysStoreClient.GetPatternOccurrencesAsync(pageNo, pageSize, patternId);
            return Ok(new ApiOKResponse(occurrences));
        }
    }
}

