using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;

namespace testo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SacamelaController : ControllerBase
    {


        [HttpGet]
        public ActionResult<IEnumerable> GetStylesAsync()
        {

            return Ok("gffgsfd");
        }
    }
}


