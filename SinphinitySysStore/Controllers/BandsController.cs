using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinitySysStore.Repositories;
using CommonRestLib.ErrorHandling;

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
        public async Task<ActionResult<IEnumerable>> GetBandsAsync(int pageNo = 0, int pageSize = 10, string contains = null, string styleId = null, string sortKey = "name", int sortDirection = 1)
        {
            var totalBands = await _bandsRepository.GetBandsCountAsync(contains, styleId);
            var bands = await _bandsRepository.GetBandsAsync(pageSize, pageNo, contains, styleId, sortKey, sortDirection);
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


