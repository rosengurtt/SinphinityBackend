using CommonRestLib.ErrorHandling;
using Melanchall.DryWetMidi.Core;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Sinphinity.Models;
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
    public class SongProcessingController : ControllerBase
    {

        [HttpPost]
        public ActionResult ImportMidiFile(Song song)
        {
            Log.Information($"Me llego para procesar la song {song.Name}");
            song =  ProcesameLaSong(song);

            return Ok(new ApiOKResponse(song));
        }

        [HttpPost("verify")]
        public ActionResult VerifyMidiFile(Song song)
        {
            try
            {
                var midiFile = MidiFile.Read(song.MidiBase64Encoded);
                return Ok(new ApiOKResponse(null));
            }
           catch(Exception ex)
            {
                return BadRequest(new ApiBadRequestResponse(ex.Message));
            }

        }


        /// <summary>
        /// Receives a song object and some parameters like a tempo value or a start point in seconds and returns a midi file
        /// coded in base64 that can be played by a midi player (after extracting the bytes from base 64 encoding)
        /// </summary>
        /// <param name="song"></param>
        /// <param name="songId"></param>
        /// <param name="tempoInBeatsPerMinute"></param>
        /// <param name="simplificationVersion"></param>
        /// <param name="startInSeconds"></param>
        /// <param name="mutedTracks"></param>
        /// <returns></returns>
        [HttpPost("{songId}")]
        public ActionResult GetSongMidi(Song song, string songId, int tempoInBeatsPerMinute, int simplificationVersion = 1, int startInSeconds = 0, string? mutedTracks = null)
        {

            int[] tracksToMute = mutedTracks?.Split(',').Select(x => int.Parse(x)).ToArray();


            var tempoInMicrosecondsPerBeat = 120 * 500000 / tempoInBeatsPerMinute;
            // If the tempoInBeatsPerMinute parameter is passed, we recalculate all the tempo changes. The tempo change shown to the
            // user in the UI is the first one, so if the user changes it, we get the proportion of the new tempo to the old one and
            // we change all tempos in the same proportion
            var tempoChanges = tempoInBeatsPerMinute != 0 ? song.TempoChanges.Select(x => new TempoChange
            {
                TicksSinceBeginningOfSong = x.TicksSinceBeginningOfSong,
                MicrosecondsPerQuarterNote = (int)Math.Round((double)x.MicrosecondsPerQuarterNote * tempoInMicrosecondsPerBeat / song.TempoChanges[0].MicrosecondsPerQuarterNote)
            }).ToList() :
            song.TempoChanges;
            var songSimplification = song.SongSimplifications.Where(x => x.Version == simplificationVersion).FirstOrDefault();
            if (mutedTracks != null)
            {
                foreach (var i in tracksToMute)
                {
                    songSimplification.Notes = songSimplification.Notes.Where(x => x.Voice != i).ToList();
                }
            }
            songSimplification.Notes = songSimplification.Notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var base64encodedMidiBytes = MidiUtilities.GetMidiBytesFromNotes(songSimplification.Notes, tempoChanges);
            var ms = new MemoryStream(MidiUtilities.GetMidiBytesFromPointInTime(base64encodedMidiBytes, startInSeconds));
            var bytes = ms.ToArray();
            return Ok(new ApiOKResponse(Convert.ToBase64String(bytes)));
        }

        [HttpGet("phrase")]
        public ActionResult GetPhraseMidi(PhraseTypeEnum phraseType, string asString, int tempoInBPM = 90, int instrument = 0, byte startingPitch=60)
        {
            var tempoChanges = new List<TempoChange> { new TempoChange { MicrosecondsPerQuarterNote = 500000 * 120 / tempoInBPM } };
            var notes = PhraseConverter.GetPhraseNotes(phraseType, asString, instrument, startingPitch);
            var base64encodedMidiBytes = MidiUtilities.GetMidiBytesFromNotes(notes, tempoChanges);
            var ms = new MemoryStream(MidiUtilities.GetMidiBytesFromPointInTime(base64encodedMidiBytes, 0));
            var bytes = ms.ToArray();
            return Ok(new ApiOKResponse(Convert.ToBase64String(bytes)));
        }


        private Song ProcesameLaSong(Song song)
        {
            song.MidiBase64Encoded = MidiUtilities.NormalizeTicksPerQuarterNote(song.MidiBase64Encoded);
            song.MidiStats = MidiUtilities.GetMidiStats(song.MidiBase64Encoded);


            var simplificationZero = MidiUtilities.GetSimplificationZeroOfSong(song.MidiBase64Encoded);
            song.Bars = MidiUtilities.GetBarsOfSong(song.MidiBase64Encoded, simplificationZero);

            song.TempoChanges = MidiUtilities.GetTempoChanges(song.MidiBase64Encoded);
            song.AverageTempoInBeatsPerMinute = MidiUtilities.GetAverageTempoInBeatsPerMinute(song.TempoChanges, song.MidiStats.DurationInTicks);

            song.SongSimplifications = new List<SongSimplification>();
            song.SongSimplifications.Add(simplificationZero);
            var simplificationOne = MidiUtilities.GetSimplification1ofSong(song);
            song.SongSimplifications.Add(simplificationOne);

            return song;
        }

        

    }
}

