using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    public class Pattern
    {
        public long Id { get; set; }

        public string AsString { get; set; }

        public long DurationInTicks { get; set; }
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

        public Pattern() { }
        public Pattern(string asString)
        {
            AsString = asString;

            var Duration = 0;
            int? highestNote = 0;
            int? lowestNote = 0;
            var noteAbsPitch = 0;
            IsMonotone = true;
            bool? IsGoingUp = null;
            NumberOfNotes = 1;
            foreach (Match m in Regex.Matches(asString, @"(\([0-9]+,[-]?[0-9]+\))"))
            {
                NumberOfNotes++;
                var values = Regex.Matches(m.Value, @"[-]?[0-9]+");
                var noteDuration = int.Parse(values[0].Value);
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
                Duration += noteDuration;
                if (highestNote < noteAbsPitch)
                    highestNote = noteAbsPitch;
                if (lowestNote > noteAbsPitch)
                    lowestNote = noteAbsPitch;
            }
            DurationInTicks = Duration;
            Range = (int)highestNote - (int)lowestNote;
            Step = noteAbsPitch;
        }
    }
}
