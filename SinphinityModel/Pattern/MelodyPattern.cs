
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
        }

        public List<int> Pitches
        {
            get
            {
                return RelativeNotes.Select(x => x.DeltaPitch).ToList();
            }
        }
        //public List<long> Metric
        //{
        //    get
        //    {
        //        return RelativeNotes.Select(x => x.Tick).ToList();
        //    }
        //}

        /// <summary>
        /// Representation of the pattern as a sequence of pairs of numbers, like (24,2),(48,1(24,-2)
        /// The first digit is the number of ticks from the previous note, the second is the number of semitones over the previous note
        /// The first element is actually the second note, because the first note is asumed to be (0,0) and is not included
        /// </summary>
        public string AsString
        {
            get
            {
                return string.Join(",", RelativeNotes.Where(y => y.DeltaTick != 0 && y.DeltaPitch != 0).Select(x => $"({x.DeltaTick}, {x.DeltaPitch})"));
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
