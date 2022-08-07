using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    /// <summary>
    /// Represents the use of 2 phrases together, because they are played at the same time in different voices or they are played 
    /// consecutively in the same voice
    /// </summary>
    public class PhrasesLink
    {
        public long Id { get; set; }
        public long PhraseId1 { get; set; }
        public long PhraseId2 { get; set; }

        public byte Instrument1 { get; set; }

        public byte Instrument2 { get; set; }

        public long SongId { get; set; }
       // public long TicksFromStart { get; set; }
        /// <summary>
        /// The start time of the second phrase in relation to the first one
        /// If the 2 phrases are simultanewous it would be 0
        /// If the second phrase is played after the first one, the shift is equal or longer than the duration of the first phrase
        /// We select as phrase 1 the one that starts first, so this value is always positive or zero
        /// </summary>
        public long ShiftInTicks { get; set; }

        /// <summary>
        /// It is the number of semitones of the difference of the first note of the second phrase in relation to the first note of the first phrase
        /// </summary>
        public int PitchShift { get; set; }

        public PhraseTypeEnum PhraseType { get; set; }
    }
}
