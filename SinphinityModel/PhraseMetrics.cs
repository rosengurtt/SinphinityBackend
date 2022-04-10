using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Sinphinity.Models
{
    /// <summary>
    /// Represents the metric of a melodic phrase
    /// </summary>
    public class PhraseMetrics
    {
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// A string representation of the phrase metrics. It consists of numbers separated by commas
        /// The numbers represent the separations between the start of the notes
        /// The first number is the duration in ticks from the beginning of the phrase to the first note
        /// The last number is the duration from the beginning of the last note to the start of the next phrase
        /// or the end of the bar if the next phrase doesnt start until more than a bar later
        /// 
        /// 2 quarters starting from 0 and followed by 2 sixteenths would be
        /// 0,96,96,48,48
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
                return Items.Count - 1;
            }
        }

        public long BasicMetricsId { get; set; }

        public List<int> Items
        {
            get
            {
                return AsString.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }



        public PhraseMetrics() { }
        public PhraseMetrics(string asString, long? id = null)
        {
            AsString = asString;
            Id = id == null ? 0 : (long)id;
        }

        /// <summary>
        /// endTick is where the phrase should end (in most cases the start of the first note after the phrase)
        /// But when there is a large gap before the next note, we end the phrase at the end of the bar
        /// 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="noteAfterPhrase"></param>
        public PhraseMetrics(List<Note> notes, long start, long endTick)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            AsString = $"{orderedNotes[0].StartSinceBeginningOfSongInTicks - start},";
            for (int i = 0; i < orderedNotes.Count - 1; i++)
            {
                AsString += (orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks) + ",";
            }
            AsString += endTick - orderedNotes[orderedNotes.Count - 1].StartSinceBeginningOfSongInTicks;
        }
    }
}
