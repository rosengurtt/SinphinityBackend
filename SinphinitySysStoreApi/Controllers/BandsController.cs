using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using CommonRestLib.ErrorHandling;
using SinphinitySysStore.Data;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BandsController : ControllerBase
    {
        private BandsRepository _bandsRepository;

        public BandsController(BandsRepository bandsRepository)
        {
            _bandsRepository = bandsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable>> GetBandsAsync(int pageNo = 0, int pageSize = 10, string? contains = null, long? styleId = null)
        {
            (var totalBands,var bands) = await _bandsRepository.GetBandsAsync(pageNo, pageSize, contains, styleId);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totalBands,
                totalPages = (int)Math.Ceiling((double)totalBands / pageSize),
                items = bands
            };
            return Ok(new ApiOKResponse(retObj));
        }

    }
}


