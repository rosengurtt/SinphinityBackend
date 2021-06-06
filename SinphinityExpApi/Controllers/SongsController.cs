﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinityExpApi.Clients;
using Sinphinity.Models;
using Sinphinity.Models.ErrorHandling;
using System.IO;
using SinphinityExpApi.Models;
using System.Net.Mime;

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
        public async Task<ActionResult> GetSongs(int pageNo = 0, int pageSize = 10, string contains = null, string styleId = null, string bandId = null)
        {
            return Ok(new ApiOKResponse(await _sysStoreClient.GetSongsAsync(pageNo, pageSize, contains, styleId, bandId)));
        }

        // GET: api/Songs/5
        [HttpGet("{songId}")]
        public async Task<IActionResult> GetSong(string songId, int? simplificationVersion)
        {
            Song song = await _sysStoreClient.GetSongByIdAsync(songId, simplificationVersion);
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
                Band = new Band { Id = Request.Form["bandId"], Name = Request.Form["bandName"] },
                Style = new Style { Id = Request.Form["styleId"], Name = Request.Form["styleName"] },
                MidiBase64Encoded = Convert.ToBase64String(bytes)
            };
            var processedSong = await _procMidiClient.ProcessSong(song);
            try
            {
                await _sysStoreClient.SaveSong(processedSong);
                return Ok(new ApiOKResponse(null));
            }
            catch (SongAlreadyExistsException ex)
            {
                return Conflict(new ApiConflictResponse("Song already exists"));
            }
        }

        [HttpGet("{songId}/midi")]
        public async Task<IActionResult> GetSongMidi(string songId, int tempoInBeatsPerMinute, int simplificationVersion = 1, int startInSeconds = 0, string mutedTracks = null)
        {
            try
            {
                Song song = await _sysStoreClient.GetSongByIdAsync(songId, simplificationVersion);
                if (song == null) return null;
                var base64encodedMidiBytes = await _procMidiClient.GetMidiFragmentOfSong(song, tempoInBeatsPerMinute, simplificationVersion, startInSeconds, mutedTracks);
                var ms = new MemoryStream(Convert.FromBase64String(base64encodedMidiBytes));

                return File(ms, MediaTypeNames.Text.Plain, song.Name);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

    }
}


