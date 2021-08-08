
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinphinity.Models.Pattern
{
    public class MelodyPattern
    {
        public MelodyPattern(MelodyMatch match)
        {
            Duration = match.End - match.Start;
            RelativeNotes = match.Slice1.RelativeNotes;
            var qtyNotes = match.Slice1.RelativeNotes.Count;
            var lastNote = match.Slice1.RelativeNotes[qtyNotes - 1];
            DurationOfLastNote = match.End - lastNote.TicksFromSliceStart;
        }
        private long DurationOfLastNote { get; set; }
        public List<int> Pitches
        {
            get
            {
                return RelativeNotes.Select(x => x.DeltaPitch).ToList();
            }
        }
      
        /// <summary>
        /// Represents the pattern as a single string. 
        /// What we do is to write the relative notes as a pair of (deltaTick, deltaPitch) and we add an extra element at the
        /// end for the duration of the last note. So the first element and the last element have always a pitch of 0. The deltaTick of the first note represents the time
        /// of the first dlement from the beginning of the pattern and the time of the last element represents the duration of the last note 
        /// We use a a comma to separate the deltaTick and deltaPith and a colon to separate the elements
        /// 
        /// Example: (48,0);(24,2);(24,-1);(48,0)
        /// 
        /// With this denition, if we add the times 
        /// </summary>
        public string AsString
        {
            get
            {
                var asString = "";
                foreach (var rn in RelativeNotes)
                {
                    asString += $"({rn.DeltaTick},{rn.DeltaPitch});";
                }
                asString += $"({DurationOfLastNote},0)";
                return asString;
            }
        }

      
        public long Duration { get; set; }

        public List<RelativeNote> RelativeNotes { get; set; }

        /// <summary>
        /// I don't override "Equals" because I don't bother implementing GetHashCode
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool AreEqual(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var mp = obj as MelodyPattern;
                if (this.Duration != mp.Duration) return false;
                if (mp.RelativeNotes.Count != RelativeNotes.Count) return false;
                for (var i = 0; i < RelativeNotes.Count; i++)
                    if (RelativeNotes[i].DeltaPitch != mp.RelativeNotes[i].DeltaPitch || RelativeNotes[i].DeltaTick != mp.RelativeNotes[i].DeltaTick) return false;
                return true;
            }
        }
    }
}
