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

        // api/songs/patterns?songId=60d577ef035c715d2ea7ef60
        [HttpGet]
        public async Task<IActionResult> ProcessPatternsForSong(string songId)
        {
            if (!Regex.IsMatch(songId, "^[0-9a-zA-Z]{20,28}$"))
                return BadRequest(new ApiBadRequestResponse("Invalid songId"));

            var song = await _sysStoreClient.GetSongByIdAsync(songId);


            var patternMatrix = await _procPatternClient.GetPatternMatrixOfSong(song);
            await _sysStoreClient.InsertPatterns(patternMatrix);


            return Ok(new ApiOKResponse(patternMatrix));
        }

        // api/songs/patterns/batch
        [HttpGet("batch")]
        public async Task<IActionResult> ProcessPatternsForAllSongs(string contains)
        {
            var keepLooping = true;
            var pageSize = 5;
            var page = 0;
            var alca = 1;
            while (keepLooping)
            {
                PaginatedList<Song> songsBatch = await _sysStoreClient.GetSongsAsync(page, pageSize, contains);
                if (songsBatch.items?.Count > 0)
                {
                    foreach (var s in songsBatch.items)
                    {
                        var song = await _sysStoreClient.GetSongByIdAsync(s.Id, null);
                        if (song.ArePatternsExtracted) continue;
                        try
                        {
                            Log.Information($"{alca} - Start with song: {song.Name}");
                            if (song.SongSimplifications != null && song.SongSimplifications.Count > 0)
                            {
                                var patternMatrix = await _procPatternClient.GetPatternMatrixOfSong(song);
                                Log.Information($"ProcMidi completed OK for {song.Name}");
                                await _sysStoreClient.InsertPatterns(patternMatrix);
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

    }
}

