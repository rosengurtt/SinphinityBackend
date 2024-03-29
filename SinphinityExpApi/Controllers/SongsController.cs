﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinityExpApi.Clients;
using Sinphinity.Models;
using System.IO;
using SinphinityExpApi.Models;
using System.Net.Mime;
using Serilog;
using CommonRestLib.ErrorHandling;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private SysStoreClient _sysStoreClient;
        private ProcMidiClient _procMidiClient;

        public SongsController(SysStoreClient sysStoreClient, ProcMidiClient procMidiClient)
        {
            _sysStoreClient = sysStoreClient;
            _procMidiClient = procMidiClient;
        }

        [HttpGet]
        public async Task<ActionResult> GetSongs(int pageNo = 0, int pageSize = 10, string? contains = null, long styleId = 0, long bandId = 0)
        {
            return Ok(new ApiOKResponse(await _sysStoreClient.GetSongsAsync(pageNo, pageSize, contains, styleId, bandId)));
        }

        // GET: api/Songs/5
        [HttpGet("{songId}")]
        public async Task<IActionResult> GetSong(long songId, int? simplificationVersion)
        {
            Song song = await _sysStoreClient.GetSongByIdAsync(songId, simplificationVersion);
            var mierda = JsonConvert.SerializeObject( song.SongSimplifications[0].Notes);
            if (song == null)
                return NotFound(new ApiResponse(404));

            song.MidiBase64Encoded = null;

            return Ok(new ApiOKResponse(song));
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult<Song>> UploadSong()
        {
            var files = Request.Form.Files;

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                files[0].CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }
            var song = new Song
            {
                Name = Request.Form["songName"],
                Band = new Band { Id =long.Parse(Request.Form["bandId"]), Name = Request.Form["bandName"] },
                Style = new Style { Id = long.Parse(Request.Form["styleId"]), Name = Request.Form["styleName"] },
                MidiBase64Encoded = Convert.ToBase64String(bytes)
            };
            var processedSong = await _procMidiClient.ProcessSong(song);
            try
            {
                await _sysStoreClient.InsertSong(processedSong);
                return Ok(new ApiOKResponse(null));
            }
            catch (SongAlreadyExistsException ex)
            {
                return Conflict(new ApiConflictResponse("Song already exists"));
            }
        }

        [HttpGet("{songId}/voices")]
        public async Task<ActionResult> GetMelodicVoicesOfSong(long songId)
        {
            return Content(await _sysStoreClient.GetMelodicVoicesOfSongAsync(songId), "application/json");
        }


        [HttpGet("{songId}/midi")]
        public async Task<IActionResult> GetSongMidi(
            long songId,
            int tempoInBeatsPerMinute,
            int simplificationVersion = 1,
            int startInSeconds = 0,
            string? mutedTracks = null,
            long? fromTick = null,
            long? toTick = null)
        {
            try
            {
                Song song = await _sysStoreClient.GetSongByIdAsync(songId, simplificationVersion);
                if (song == null) return null;
                var base64encodedMidiBytes = await _procMidiClient.GetMidiFragmentOfSong(song, tempoInBeatsPerMinute, simplificationVersion, startInSeconds, mutedTracks, fromTick, toTick);
                var ms = new MemoryStream(Convert.FromBase64String(base64encodedMidiBytes));

                return File(ms, MediaTypeNames.Text.Plain, song.Name);
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        [HttpGet("importBatch")]
        public async Task<IActionResult> ImportBatch(string folder = @"C:\music\test\midi")
        {
            foreach (var styleDir in Directory.GetDirectories(folder))
            {
                var style = Path.GetFileName(styleDir);
                foreach (var bandDir in Directory.GetDirectories(styleDir))
                {
                    var band = Path.GetFileName(bandDir);
                    foreach (var songPath in Directory.GetFiles(bandDir, "*.mid"))
                    {
                        var songName = Path.GetFileNameWithoutExtension(songPath);
                        if (await _sysStoreClient.DoesSongExistAlready(songName, band)) 
                            continue;
                        var song = new Song
                        {
                            Name = songName,
                            Band = new Band { Name = band },
                            Style = new Style { Name = style },
                            MidiBase64Encoded = Convert.ToBase64String(System.IO.File.ReadAllBytes(songPath))
                        };
                        try
                        {
                            song.IsMidiCorrect = await _procMidiClient.VerifySong(song);
                            await _sysStoreClient.InsertSong(song);
                        }
                        catch (SongAlreadyExistsException ex)
                        {
                            Log.Error(ex, $"Couldn't process song {songPath}");
                        }
                    }
                }
            }
            return Ok(new ApiOKResponse(null));
        }

        [HttpGet("processSingle")]
        public async Task<IActionResult> ProcessSingleSong(long songId)
        {

            var song = await _sysStoreClient.GetSongByIdAsync(songId);

            try
            {
                Song processedSong;
                if (song.IsMidiCorrect)
                {
                    Log.Information($"Start with song: {song.Name}");
                    processedSong = await _procMidiClient.ProcessSong(song);
                    Log.Information($"ProcMidi completed OK for {song.Name}");
                    await _sysStoreClient.UpdateSong(processedSong);
                    Log.Information($"Saved OK {song.Name}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Couldn't process song {song.Name}");
            }

            return Ok(new ApiOKResponse(null));
        }





        [HttpGet("processBatch")]
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
                        if (!s.IsMidiCorrect || s.IsSongProcessed || s.CantBeProcessed)
                            continue;
                        var song = await _sysStoreClient.GetSongByIdAsync(s.Id, null);
                        try
                        {
                            Song processedSong;
                            if (song.IsMidiCorrect && !song.IsSongProcessed && !song.CantBeProcessed)
                            {
                                Log.Information($"{alca} - Start with song: {song.Name}");
                                processedSong = await _procMidiClient.ProcessSong(song);
                                processedSong.IsSongProcessed = true;
                                Log.Information($"ProcMidi completed OK for {song.Name}");
                                await _sysStoreClient.UpdateSong(processedSong);
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


