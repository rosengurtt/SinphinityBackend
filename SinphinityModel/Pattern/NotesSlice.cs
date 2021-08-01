using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinphinity.Models.Pattern
{
    /// <summary>
    /// Represents the notes of some time interval in a voice
    /// </summary>
    public class NotesSlice
    {

        public NotesSlice(List<Note> notes, long startTick, long endTick, byte voice, Bar bar, long beatNumber)
        {
            Bar = bar;
            BeatNumberFromBarStart = beatNumber;
            var beatStart = bar.TicksFromBeginningOfSong + (beatNumber - 1) * bar.TimeSignature.Numerator * 96 / bar.TimeSignature.Denominator;
            StartTick = startTick;
            EndTick = endTick;
            Voice = voice;
            Notes = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= startTick &&
                        x.StartSinceBeginningOfSongInTicks < EndTick &&
                        x.Voice == voice)
                .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                .ThenByDescending(z => z.Pitch)
                .ToList();
            RelativeNotes = new List<RelativeNote>();
            if (Notes.Count > 0)
                RelativeNotes.Add(new RelativeNote(Notes[0], null, beatStart, bar.KeySignature));
            for (var i = 1; i < Notes.Count; i++)
                RelativeNotes.Add(new RelativeNote(Notes[i], Notes[i - 1], beatStart, bar.KeySignature));
        }
        public long BarNumber
        {
            get
            {
                return Bar.BarNumber;
            }
        }
        public Bar Bar { get; set; }

        public long BeatNumberFromBarStart { get; set; }


        public byte Voice { get; set; }
        public long StartTick { get; set; }
        public long EndTick { get; set; }

        public int FirstPitch
        {
            get
            {
                if (Notes.Count == 0) return 0;
                return Notes[0].Pitch;
            }
        }

        public long Duration
        {
            get
            {
                return EndTick - StartTick;
            }
        }
        public List<Note> Notes { get; set; }

        public List<RelativeNote> RelativeNotes { get; set; }

        /// <summary>
        /// Relative notes don't have ticks from beginning of song or from an arbitrary point, only delta ticks from previous note
        /// This method returns the set of the relative notes that are between 2 particular ticks counted from the beginning of the slice
        /// </summary>
        /// <param name="slice"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        public List<RelativeNote> GetRelativeNotesBetweenTicks(long startTick, long endTick)
        {
            List<RelativeNote> retObj = new List<RelativeNote>();
            long ticksFromStart = 0;
            foreach (var n in RelativeNotes)
            {
                ticksFromStart += n.DeltaTick;
                if (ticksFromStart >= startTick && ticksFromStart < endTick)
                    retObj.Add(n);
            }
            return retObj;
        }

        public long GetTickOfFirstNoteAfterTick(long tick)
        {
            long ticksFromStart = 0;
            foreach (var n in RelativeNotes)
            {
                ticksFromStart += n.DeltaTick;
                if (ticksFromStart >= tick)
                    return ticksFromStart;
            }
            return ticksFromStart;
        }

        /// <summary>
        /// Compares this slice with another one and returns the points (the tick number from the start of the slice) where there is a difference between the 2 slices
        /// </summary>
        /// <param name="slice2"></param>
        /// <returns></returns>
        public HashSet<long> GetDifferencePoints(NotesSlice slice2)
        {
            var DifferencePoints = new HashSet<long>();
            long ticksFromBeginningOfSlice1 = 0;
            long ticksFromBeginningOfSlice2;
            foreach (var n in RelativeNotes)
            {
                ticksFromBeginningOfSlice1 += n.DeltaTick;
                ticksFromBeginningOfSlice2 = 0;
                bool matched = false;
                foreach (var m in slice2.RelativeNotes)
                {
                    ticksFromBeginningOfSlice2 += m.DeltaTick;
                    if (ticksFromBeginningOfSlice1 == ticksFromBeginningOfSlice2 && m.DeltaPitch == n.DeltaPitch)
                        matched = true;
                }

                if (!matched)
                    DifferencePoints.Add(ticksFromBeginningOfSlice1);
            }
            ticksFromBeginningOfSlice2 = 0;
            foreach (var n in slice2.RelativeNotes)
            {
                ticksFromBeginningOfSlice2 += n.DeltaTick;
                ticksFromBeginningOfSlice1 = 0;
                bool matched = false;
                foreach (var m in RelativeNotes)
                {
                    ticksFromBeginningOfSlice1 += m.DeltaTick;
                    if (ticksFromBeginningOfSlice1 == ticksFromBeginningOfSlice2 && m.DeltaPitch == n.DeltaPitch)
                        matched = true;
                }
                if (!matched)
                    DifferencePoints.Add(ticksFromBeginningOfSlice2);
            }
            return DifferencePoints;
        }
    }
}

