﻿using Sinphinity.Models;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityProcMidi.Midi
{
    public static partial class MidiUtilities
    {
        public static List<Note> GetNotesOfBar(Bar bar, SongSimplification songSimplification)
        {
            int standardTicksPerQuarterNote = 96;
            var retObj = new List<Note>();
            foreach (var n in songSimplification.Notes)
            {
                int barLengthInTicks = bar.TimeSignature.Numerator * (int)standardTicksPerQuarterNote;
                var barStart = bar.TicksFromBeginningOfSong;
                var noteStart = n.StartSinceBeginningOfSongInTicks;
                var noteEnd = n.EndSinceBeginningOfSongInTicks;
                var barEnd = bar.EndTick;
                if (barEnd < noteStart || noteEnd <= barStart) continue;

                if (!retObj.Contains(n))
                    retObj.Add(n);
            }
            return retObj.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
        }
    }
}
