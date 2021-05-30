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
        public async Task<ActionResult<IEnumerable>> GetSongsAsync(int pageNo = 0, int pageSize = 10, string contains = null, string styleId = null, string bandId = null,
            string sortKey = "name", int sortDirection = 1)
        {
            var totalSongs = await _songsRepository.GetSongsCountAsync(contains);
            var songs = await _songsRepository.GetSongsAsync(pageSize, pageNo, contains, styleId, bandId, sortKey, sortDirection);
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

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult<Song>> SaveSongAsync(string bandId, string styleId)
        {
            //var files = Request.Form.Files;
            //foreach (var file in files)
            //{

            //    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            //    var fullPath = Path.Combine(pathToSave, file.Name + ".txt");
            //    using (var stream = new FileStream(fullPath, FileMode.Create))
            //    {
            //        file.CopyTo(stream);


            //        return Ok(new ApiOKResponse(await _songsRepository.InsertSongAsync(song)));
            return Ok(new ApiOKResponse("lolo te saluda"));
        }

    }
}

