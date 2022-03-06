using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Sinphinity.Models;
using SinphinityProcMelodyAnalyser.BusinessLogic;
using System.Linq;

namespace SinphinityProcMelodyAnalyser.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhrasesController : ControllerBase
    {

        [HttpPost]
        public ActionResult GetSongPhrases(Song song, int songSimplification = 1)
        {
            var phrases = MelodyFinder.FindAllPhrases(song, songSimplification);

            return Ok(new ApiOKResponse(phrases));
        }
    }
}


