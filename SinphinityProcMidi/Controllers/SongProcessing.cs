
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
            string songPath = @"C:\music\midi\Classic\bach\bach\invent\invent1.mid";
            string band = "John Sebastian Bach";
            string style = "Classic";

            song =  ProcesameLaSong(songPath, band, style);

            return Ok(new ApiOKResponse(song));
        }
        private Song ProcesameLaSong(string songPath, string band, string style)
        {
            if (!songPath.ToLower().EndsWith(".mid")) return null;
            try
            {
                var lelo = MidiFile.Read(songPath, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Song {songPath} esta podrida");
                return null;
            }

            var midiBase64encoded = FileSystemUtils.GetBase64encodedFile(songPath);
            midiBase64encoded = MidiUtilities.NormalizeTicksPerQuarterNote(midiBase64encoded);

            Song song = new Song()
            {
                Name = Path.GetFileName(songPath),
                Band = new Band { Name = band },
                Style = new Style { Name = style },
                MidiBase64Encoded = midiBase64encoded,
            };
            song.MidiStats = MidiUtilities.GetMidiStats(midiBase64encoded);
            song.DurationInSeconds = song.MidiStats.DurationInSeconds;
            song.DurationInTicks = song.MidiStats.DurationInTicks;
     

            var simplificationZero = MidiUtilities.GetSimplificationZeroOfSong(midiBase64encoded);
            song.Bars = MidiUtilities.GetBarsOfSong(midiBase64encoded, simplificationZero);
       
            song.TempoChanges = MidiUtilities.GetTempoChanges(midiBase64encoded);           
         
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

