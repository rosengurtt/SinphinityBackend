
using Melanchall.DryWetMidi.Core;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Sinphinity.Models;
using SinphinityProcMidi.ErrorHandling;
using SinphinityProcMidi.Helpers;
using SinphinityProcMidi.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcMidi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongProcessing : ControllerBase
    {

        [HttpPost]
        public ActionResult ImportMidiFile(Song song)
        {
            song =  ProcesameLaSong(song);

            return Ok(new ApiOKResponse(song));
        }
        private Song ProcesameLaSong(Song song)
        {
            song.MidiStats = MidiUtilities.GetMidiStats(song.MidiBase64Encoded);
            song.DurationInSeconds = song.MidiStats.DurationInSeconds;
            song.DurationInTicks = song.MidiStats.DurationInTicks;
     

            var simplificationZero = MidiUtilities.GetSimplificationZeroOfSong(song.MidiBase64Encoded);
            song.Bars = MidiUtilities.GetBarsOfSong(song.MidiBase64Encoded, simplificationZero);
       
            song.TempoChanges = MidiUtilities.GetTempoChanges(song.MidiBase64Encoded);
            song.AverageTempoInBeatsPerMinute = MidiUtilities.GetAverageTempoInBeatsPerMinute(song.TempoChanges, song.DurationInTicks);

            song.SongSimplifications = new List<SongSimplification>();
            song.SongSimplifications.Add(simplificationZero);
            song.SongSimplifications.Add(MidiUtilities.GetSimplification1ofSong(song));

            return song;
        }

     

        private List<(int, long, byte)> UbicameLosProgramChange(MidiFile songi)
        {
            var retObj = new List<(int, long, byte)>();
            var acum = MidiUtilities.ConvertDeltaTimeToAccumulatedTime(songi);
            var chunky = -1;
            foreach (TrackChunk chunk in songi.Chunks)
            {
                chunky++;
                var eventsToRemove = new List<MidiEvent>();

                foreach (MidiEvent eventito in chunk.Events)
                {
                    if (eventito is ProgramChangeEvent)
                    {
                        var soret = eventito as ProgramChangeEvent;
                        retObj.Add((chunky, eventito.DeltaTime, soret.ProgramNumber));
                    }
                }
            }
            return retObj;
        }

    }
}

