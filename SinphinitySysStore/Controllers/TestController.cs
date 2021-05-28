using Microsoft.AspNetCore.Mvc;
using Sinphinity.Models.ErrorHandling;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SinphinitySysStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Testeame()
        {
      
            return Ok(new ApiOKResponse(new { ApiIsWorkingOK= true, Problems= new List<string>() }));
        }
    }
}
