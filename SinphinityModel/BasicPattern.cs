using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    /// <summary>
    /// The Pattern class has the information about duration in terms of ticks, so a pattern where we play 4 quarter notes is different
    /// of a pattern where we play 4 eight notes, even when the pitches are exactly the same
    /// 
    /// The BasicPattern class has the relative lenghts of the notes. For ex. the first note lasts for double the time of the second note. In this
    /// case the 2 patterns with the same pitches but one with quarters and the other with eights will have the same BasicPattern
    /// </summary>
    public class BasicPattern
    {

        public long Id { get; set; }

        public string AsString { get; set; }

        public int NumberOfNotes { get; set; }
        /// <summary>
        /// The difference between the highest pitch and the lowest pitch
        /// </summary>
        public int Range { get; set; }
        /// <summary>
        /// If true notes never go up or never go down
        /// </summary>
        public bool IsMonotone { get; set; }
        /// <summary>
        /// The difference between the pitch of the last and the first note
        /// </summary>
        public int Step { get; set; }

        public BasicPattern() { }
        public BasicPattern(string asString)
        {
            AsString = asString;

            int? highestNote = null;
            int? lowestNote = null;
            var noteAbsPitch = 0;
            IsMonotone = true;
            bool? IsGoingUp = null;
            NumberOfNotes = 0;
            foreach (Match m in Regex.Matches(asString, @"(\([0-9]+,[-]?[0-9]+\))"))
            {
                var values = Regex.Matches(m.Value, @"[-]?[0-9]+");
                var noteRelPitch = int.Parse(values[1].Value);
                if (IsGoingUp == null)
                {
                    if (noteRelPitch != 0)
                        IsGoingUp = noteRelPitch > 0;
                }
                else
                {
                    if (((bool)IsGoingUp && noteRelPitch < 0) ||
                        (!(bool)IsGoingUp && noteRelPitch < 0))
                        IsMonotone = false;
                }
                noteAbsPitch += noteRelPitch;
                if (highestNote == null || highestNote < noteAbsPitch)
                    highestNote = noteAbsPitch;
                if (lowestNote == null || lowestNote > noteAbsPitch)
                    lowestNote = noteAbsPitch;

            }
            NumberOfNotes -= 1;
            Range = (int)highestNote - (int)lowestNote;
            Step = noteAbsPitch;
        }
        public BasicPattern(Pattern p)
        {
            NumberOfNotes = p.NumberOfNotes;
            IsMonotone = p.IsMonotone;
            Step = p.Step;
            AsString = "";

            var relNotes = GetReativeNotes(p.AsString);
            var gdc = GreatestCommonDivisor(relNotes.Select(x => x.DeltaTick).ToList());

            foreach (Match m in Regex.Matches(p.AsString, @"(\([0-9]+,[-]?[0-9]+\))"))
            {
                var values = Regex.Matches(m.Value, @"[-]?[0-9]+");
                var noteRelPitch = int.Parse(values[1].Value);
                var noteDur = int.Parse(values[0].Value) / gdc;
                AsString += $"({noteDur},{noteRelPitch})";
            }

        }

        private static List<RelativeNote> GetReativeNotes(string asString)
        {
            var retObj = new List<RelativeNote>();
            foreach (Match m in Regex.Matches(asString, @"(\([0-9]+,[-]?[0-9]+\))"))
            {
                var values = Regex.Matches(m.Value, @"[-]?[0-9]+");
                var noteDuration = int.Parse(values[0].Value);
                var noteRelPitch = int.Parse(values[1].Value);
               
                var relNote = new RelativeNote
                {
                    DeltaPitch = noteRelPitch,
                    DeltaTick = noteDuration
                };
                retObj.Add(relNote);
            }
            return retObj;
        }


        private static long GreatestCommonDivisor(List<long> numbers)
        {
            var gcd = numbers[0];
            for (int i = 1; i < numbers.Count; i++)
            {
                gcd = GreatestCommonDivisor(numbers[i], gcd);
            }

            return gcd;
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
    }
}
