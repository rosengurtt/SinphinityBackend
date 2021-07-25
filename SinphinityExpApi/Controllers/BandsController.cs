using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinityExpApi.Clients;
using Sinphinity.Models;
using CommonRestLib.ErrorHandling;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BandsController : ControllerBase
    {
        private SysStoreClient _sysStoreClient;
        private ProcMidiClient _procMidiClient;

        public BandsController(SysStoreClient sysStoreClient, ProcMidiClient procMidiClient)
        {
            _sysStoreClient = sysStoreClient;
            _procMidiClient = procMidiClient;
        }


        [HttpGet]
        public async Task<ActionResult> GetBands(int pageNo = 0, int pageSize = 10, string contains = null, string styleId = null)
        {
            return Ok(new ApiOKResponse(await _sysStoreClient.GetBandsAsync(pageNo, pageSize, contains, styleId)));
        }
    }
}



