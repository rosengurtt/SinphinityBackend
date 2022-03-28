﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Sinphinity.Models
{
    /// <summary>
    /// When the original version of a phrase has embellishments we create 2 objects
    /// 
    /// - the original phrase with the embellishments (that we store in this type of object)
    /// - the simplified version of the phrase with the embellisments removed (that we store in a PhraseMetrics object, that we link from here)
    /// 
    /// 
    /// </summary>
    public class EmbellishedPhraseMetrics
    {
        public EmbellishedPhraseMetrics() { }
        public EmbellishedPhraseMetrics(PhraseMetrics p, string asString)
        {
            PhraseMetricsWithoutOrnamentsId = p.Id;
            AsString = asString;
        }
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
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

        /// <summary>
        /// Link to the version of the phrase with the ornaments removed
        /// </summary>
        public long PhraseMetricsWithoutOrnamentsId { get; set; }

        public List<int> Items
        {
            get
            {
                return AsString.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }
  
    }
}