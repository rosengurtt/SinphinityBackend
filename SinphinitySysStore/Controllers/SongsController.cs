using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinitySysStore.Repositories;
using SinphinitySysStore.Models;
using Sinphinity.Models.ErrorHandling;
using Newtonsoft.Json;
using System.IO;
using SinphinitySysStore.Models.Exceptions;
using Serilog;

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
        public async Task<ActionResult<IEnumerable>> GetSongsAsync(int pageNo = 0, int pageSize = 10, string contains = null, string styleId = null, string bandId = null,
            string sortKey = "name", int sortDirection = 1)
        {
            Log.Information($"Me llego GetSongs con pageNo={pageNo}, pageSize={pageSize}");
            var totalSongs = await _songsRepository.GetSongsCountAsync(contains, styleId, bandId);
            var songs = await _songsRepository.GetSongsAsync(pageSize, pageNo, contains, styleId, bandId, sortKey, sortDirection);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totalSongs,
                totalPages = (int)Math.Ceiling((double)totalSongs / pageSize),
                items = songs
            };
            Log.Information($"Retorno estas songs {string.Join(",", songs.Select(s => s.Name))}");
            return Ok(new ApiOKResponse(retObj));
        }

        // GET: api/Songs/5
        [HttpGet("{songId}")]
        public async Task<IActionResult> GetSong(string songId, int? simplificationVersion)
        {
            Song song = await _songsRepository.GetSongByIdAsync(songId, simplificationVersion);
            if (song == null)
                return NotFound(new ApiResponse(404));

            return Ok(new ApiOKResponse(song));
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult<Song>> UploadSong(Song song)
        {
            try
            {
                if (string.IsNullOrEmpty(song.Style.Id) && !string.IsNullOrEmpty(song.Style.Name))
                {
                    song.Style = await _stylesRepository.GetStyleByNameAsync(song.Style.Name);
                }
                if (string.IsNullOrEmpty(song.Band.Id) && !string.IsNullOrEmpty(song.Band.Name))
                {
                    song.Band = await _bandssRepository.GetBandByNameAsync(song.Band.Name);
                }
              
                return Ok(new ApiOKResponse(await _songsRepository.InsertSongInfoAndMidiAsync(song)));
            }
            catch (SongAlreadyExistsException ex)
            {
                return Conflict(new ApiConflictResponse("Song already exists"));
            }
        }
        [HttpPut, DisableRequestSizeLimit]
        public async Task<ActionResult> AddProcessedDataToSong(Song song)
        {
            Log.Information($"Me llego para updatear la song {song.Name}");
            await _songsRepository.InsertSongDataAsync(song);
            Log.Information($"Update la song {song.Name}");
            return Ok(new ApiOKResponse(null));
        }
    }
}

