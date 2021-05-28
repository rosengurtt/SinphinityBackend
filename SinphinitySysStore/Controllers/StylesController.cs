using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using SinphinitySysStore.Repositories;
using Sinphinity.Models.ErrorHandling;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StylesController : ControllerBase
    {
        private StylesRepository _stylesRepository;

        public StylesController(StylesRepository stylesRepository)
        {
            _stylesRepository = stylesRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable>> GetStylesAsync(int pageNo = 0, int pageSize = 10, string contains = null, string sortKey="Name",  int sortDirection=1)
        {
            var totaStyles = await _stylesRepository.GetStylesCountAsync(contains);
            var styles = await _stylesRepository.GetStylesAsync(pageSize, pageNo, contains,sortKey, sortDirection );
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totaStyles,
                totalPages = (int)Math.Ceiling((double)totaStyles / pageSize),
                styles
            };
            return Ok(new ApiOKResponse(retObj));
        }

    }
}

