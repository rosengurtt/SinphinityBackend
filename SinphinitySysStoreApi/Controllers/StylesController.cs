using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using CommonRestLib.ErrorHandling;
using SinphinitySysStore.Data;

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
        public async Task<ActionResult<IEnumerable>> GetStylesAsync(int pageNo = 0, int pageSize = 10, string? contains = null)
        {
            (var totaStyles, var styles) = await _stylesRepository.GetStylesAsync(pageNo, pageSize, contains);
            var retObj = new
            {
                pageNo,
                pageSize,
                totalItems = totaStyles,
                totalPages = (int)Math.Ceiling((double)totaStyles / pageSize),
                items = styles
            };
            return Ok(new ApiOKResponse(retObj));
        }
    }
}

