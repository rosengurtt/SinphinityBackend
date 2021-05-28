using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinitySysStore.Repositories;
using SinphinitySysStore.Models;
using Sinphinity.Models.ErrorHandling;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private SongsRepository _songsRepository;

        public SongsController(SongsRepository songsRepository)
        {
            _songsRepository = songsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable>> GetSongsAsync(int pageNo = 0, int pageSize = 10, string contains = null, string sortKey = "Name", int sortDirection = 1)
        {
            var totaStyles = await _songsRepository.GetSongsCountAsync(contains);
            var songs = await _songsRepository.GetSongsAsync(pageSize, pageNo, contains, sortKey, sortDirection);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totaStyles,
                totalPages = (int)Math.Ceiling((double)totaStyles / pageSize),
                songs
            };
            return Ok(new ApiOKResponse(retObj));
        }

        [HttpPost]
        public async Task<ActionResult<Song>> SaveSongAsync(Song song)
        {
            return Ok(new ApiOKResponse(await _songsRepository.InsertSongAsync(song)));
        }

    }
}

