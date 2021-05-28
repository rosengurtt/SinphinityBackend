using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinityExpApi.Clients;
using Sinphinity.Models;

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
 

        [HttpPost]
        public async Task<ActionResult<Song>> UploadSong(Song song)
        {
            var processedSong = await _procMidiClient.ProcessSong(song);
            await _sysStoreClient.SaveSong(processedSong);
            return Ok("Todo OK papi");
        }

    }
}


