using CommonRestLib.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Sinphinity.Models;
using SinphinityProcMelodyAnalyser.BusinessLogic;
using SinphinityProcMelodyAnalyser.MelodyLogic;
using System.Linq;

namespace SinphinityProcMelodyAnalyser.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhrasesController : ControllerBase
    {
        private Main _main;
        public PhrasesController(Main main)
        {
            _main = main;
        }

        [HttpGet]
        public async Task<ActionResult> GetSongPhrases(long songId)
        {
           // var phrases = MelodyFinder.FindAllPhrases(song, songSimplification);
            var phrases = await _main.ProcessSong(songId);

            return Ok(new ApiOKResponse(phrases));
        }
    }
}


