using System;
using System.Collections.Generic;

namespace Sinphinity.Models
{
    /// <summary>
    /// Represents a note in a slice. Instedad of having absolutes values like a real pitch and the ticks from the start of the song, it has values related to
    /// the start of the slice and the pitch of first note of the slice
    /// </summary>
    public class RelativeNote
    {
        public RelativeNote() { }
        public RelativeNote(Note n, Note previousNote, long startTick, KeySignature key)
        {
            if (previousNote == null)
            {
                DeltaPitchInSemitones = 0;
                DeltaPitch = 0;
                DeltaTick = n.StartSinceBeginningOfSongInTicks - startTick;
            }
            else
            {
                DeltaPitchInSemitones = n.Pitch - previousNote.Pitch;
                DeltaPitch = GetNotePositioninKey(n, key) - GetNotePositioninKey(previousNote, key);
                DeltaTick = n.StartSinceBeginningOfSongInTicks - previousNote.StartSinceBeginningOfSongInTicks;
            }
            TicksFromSliceStart = n.StartSinceBeginningOfSongInTicks - startTick;
        }
        public long TicksFromSliceStart { get; set; }
        public long DeltaTick { get; set; }

        public long DeltaTickNormalized
        {
            get
            {
                if (DeltaTick < 6) return DeltaTick;
                if (DeltaTick < 48) return NormalizeDeltaTick(DeltaTick, 2);
               return NormalizeDeltaTick(DeltaTick, 6);
            }
        }
        /// <summary>
        /// The idea is to aproximate a delta tick like 47 to 48 (basically, submultiples of 96)
        /// 
        /// </summary>
        /// <param name="deltaTick"></param>
        /// <param name="imprecision"></param>
        /// <returns></returns>
        private long NormalizeDeltaTick(long deltaTick, long imprecision)
        {
            long dif = deltaTick % imprecision;
            if (dif == 0) return deltaTick;
            long candidateTop = deltaTick + imprecision - dif;
            long candidateBottom = deltaTick - dif;
            if ( dif < imprecision - dif) return candidateTop;
            if (dif > imprecision - dif) return deltaTick + candidateBottom;
            if (GreatestCommonDivisor(candidateTop, 96) > GreatestCommonDivisor(candidateBottom, 96))
                return candidateTop;
            else
                return candidateBottom;
        }
        private static long GreatestCommonDivisor(long a, long b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }

        /// <summary>
        /// Represents the difference between the previous pitch and this one
        /// </summary>
        public int DeltaPitchInSemitones { get; set; }

        /// <summary>
        /// This is the difference in terms of a major scale, so C and D in C major have a DeltaPitch of 1 while the DeltaPitchInSemitones is 2
        /// </summary>
        public int DeltaPitch { get; set; }

        public string AsString
        {
            get
            {
                return $"({DeltaTick},{DeltaPitch})";
            }
        }
        public string AsNormalizedString
        {
            get
            {
                return $"({DeltaTickNormalized},{DeltaPitch})";
            }
        }

        /// <summary>
        /// If we are in C major scale and note is E, it returns 2, so a third has a value of 2, a fourth a value of 3, etc.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static int GetNotePositioninKey(Note n, KeySignature key)
        {
            var majorScalePitches = new List<int>();
            majorScalePitches.Add((60 + 7 * key.key) % 12);
            majorScalePitches.Add((62 + 7 * key.key) % 12);
            majorScalePitches.Add((64 + 7 * key.key) % 12);
            majorScalePitches.Add((65 + 7 * key.key) % 12);
            majorScalePitches.Add((67 + 7 * key.key) % 12);
            majorScalePitches.Add((69 + 7 * key.key) % 12);
            majorScalePitches.Add((71 + 7 * key.key) % 12);

            return 7 * (n.Pitch / 12) + majorScalePitches.IndexOf(n.Pitch % 12);
        }

        public bool AreEqual(RelativeNote n)
        {
            return (this.DeltaTick == n.DeltaTick && this.TicksFromSliceStart == n.TicksFromSliceStart && this.DeltaPitch == n.DeltaPitch);
        }
    }

}

