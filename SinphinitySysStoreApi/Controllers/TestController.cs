using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;

namespace SinphinitySysStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {

        [HttpGet]
        public ActionResult Testeame()
        {

            return Ok(new ApiOKResponse("Todo bien papi"));
        }
    }
}