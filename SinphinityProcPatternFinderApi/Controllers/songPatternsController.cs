using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Sinphinity.Models;
using SinphinityProcPatternFinderApi.PatternExtraction;

namespace SinphinityProcPatternFinderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class songPatternsController : ControllerBase
    {

        [HttpPost]
        public ActionResult GetSongPatterns(Song song, int SongSimplification = 1)
        {


            return Ok(new ApiOKResponse(PatternsExtraction.GetPatternsOfSongSimplification(song, SongSimplification)));
        }
    }
}


