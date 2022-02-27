using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityModel
{
    /// <summary>
    /// Represents the metric of a melodic phrase
    /// </summary>
    public class PhraseMetric
    {
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// A string representation of the phrase metrics. It consists of numbers separated by commas
        /// The numbers represent the separations between the start of the notes
        /// The last number is the duration of the last note
        /// 
        /// 2 quarters followed by 2 sixteenths would be
        /// 96,96,48,48
        /// 
        /// Accentuations can be indicated with a plus sign after a numer, like:
        /// 96,96+,48,48
        /// </summary>
        public string AsString { get; set; }

        public long DurationInTicks
        {
            get
            {
                return Items.Sum(); ;
            }
        }
        public int NumberOfNotes
        {
            get
            {
                return Items.Count;
            }
        }

        public List<int> Items
        {
            get
            {
                return AsString.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }



        public PhraseMetric() { }
        public PhraseMetric(string asString)
        {
            AsString = asString;
        }

        public PhraseMetric(List<Note> notes)
        {
            AsString = "";
            for (int i = 0; i < notes.Count - 1; i++)
            {
                AsString += (notes[i + 1].StartSinceBeginningOfSongInTicks - notes[i].StartSinceBeginningOfSongInTicks) + ",";
            }
            AsString += notes[notes.Count - 1].DurationInTicks;
        }
    }
}
