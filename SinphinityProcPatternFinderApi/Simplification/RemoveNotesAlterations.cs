﻿using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityProcPatternFinderApi.Simplification
{
    static partial class SimplificationUtilities
    {
        public static List<Note> RemoveNotesAlterations(List<Note> notes, List<Bar> bars)
        {
            var retObj = new List<Note>();
            foreach (var bar in bars)
            {
                var notesOfBar =
                    notes.Where(n => n.StartSinceBeginningOfSongInTicks >= bar.TicksFromBeginningOfSong && n.StartSinceBeginningOfSongInTicks < bar.EndTick).ToList();
                notesOfBar.ForEach(x => retObj.Add(NormalizeNote(x, bar.KeySignature)));
            }
            return retObj;
        }


        private static Note NormalizeNote(Note n, KeySignature key)
        {
            if (IsNoteinScale(n, key)) return (Note)n.Clone();
            return GetUnalteredNote(n, key);
        }

        private static bool IsNoteinScale(Note n, KeySignature key)
        {
            return GetPitchesOfScale(key).Contains(n.Pitch % 12);
        }
        private static List<int> GetPitchesOfScale(KeySignature key)
        {
            var retObj = new List<int>();

            retObj.Add((120 + key.key * 7) % 12);
            retObj.Add((120 + key.key * 7 + 2) % 12);
            retObj.Add((120 + key.key * 7 + 4) % 12);
            retObj.Add((120 + key.key * 7 + 5) % 12);
            retObj.Add((120 + key.key * 7 + 7) % 12);
            retObj.Add((120 + key.key * 7 + 9) % 12);
            retObj.Add((120 + key.key * 7 + 11) % 12);

            return retObj;
        }

        private static Note GetUnalteredNote(Note n, KeySignature key)
        {
            var retObj = (Note)n.Clone();
            var normalizedPitch = (120 + n.Pitch - key.key * 7) % 12;

            if (normalizedPitch == 6 | normalizedPitch == 1 | normalizedPitch == 8) retObj.Pitch--;
            else retObj.Pitch++;
            return retObj;
        }
    }
}
