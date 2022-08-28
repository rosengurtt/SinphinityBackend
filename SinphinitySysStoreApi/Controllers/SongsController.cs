using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using Serilog;
using CommonRestLib.ErrorHandling;
using SinphinitySysStore.Data;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private StylesRepository _stylesRepository;
        private BandsRepository _bandssRepository;
        private SongsRepository _songsRepository;

        public SongsController(StylesRepository stylesRepository, BandsRepository bandssRepository, SongsRepository songsRepository)
        {
            _stylesRepository = stylesRepository;
            _bandssRepository = bandssRepository;
            _songsRepository = songsRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetSongsAsync(int pageNo = 0, int pageSize = 10, string? contains = null, long? styleId = null, long? bandId = null)
        {
            (var totalSongs, var songs) = await _songsRepository.GetSongsAsync(pageNo, pageSize, contains, styleId, bandId);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totalSongs,
                totalPages = (int)Math.Ceiling((double)totalSongs / pageSize),
                items = songs
            };
            return Ok(new ApiOKResponse(retObj));
        }

        // GET: api/Songs/5
        [HttpGet("{songId}")]
        public async Task<IActionResult> GetSongAsync(long songId, int? simplificationVersion)
        {
            try
            {
                var song = await _songsRepository.GetSongByIdAsync(songId, simplificationVersion);
                if (song == null)
                    return NotFound(new ApiResponse(404));

                return Ok(new ApiOKResponse(song));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult<Sinphinity.Models.Song>> AddSongAsync(Sinphinity.Models.Song song)
        {
            try
            {
                if (song.Style.Id == 0)
                {
                    if (string.IsNullOrEmpty(song.Style.Name))
                        return BadRequest(new ApiBadRequestResponse("No style was provided"));
                    song.Style = await _stylesRepository.GetStyleByNameAsync(song.Style.Name);
                    if (song.Style == null)
                        return BadRequest(new ApiBadRequestResponse("The style doesn' exist"));
                }
                if (song.Band.Id == 0)
                {
                    if (string.IsNullOrEmpty(song.Band.Name))
                        return BadRequest(new ApiBadRequestResponse("No band was provided"));
                    song.Band = await _bandssRepository.GetBandByNameAsync(song.Band.Name);
                    if (song.Band == null)
                        return BadRequest(new ApiBadRequestResponse("The band doesn' exist"));
                }

                return Ok(new ApiOKResponse(await _songsRepository.AddSong(song)));
            }
            catch (SongAlreadyExistsException ex)
            {
                return Conflict(new ApiConflictResponse("Song already exists"));
            }
        }

        [HttpPut, DisableRequestSizeLimit]
        public async Task<ActionResult> UpdateSongAsync(Sinphinity.Models.Song song)
        {
            Log.Information($"Me llego para updatear la song {song.Name}");
            await _songsRepository.UpdateSong(song);
            Log.Information($"Update la song {song.Name}");
            return Ok(new ApiOKResponse(null));
        }
    }
}


