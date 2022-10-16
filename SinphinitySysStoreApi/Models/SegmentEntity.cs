using Sinphinity.Models;
using SinphinitySysStoreApi.Helpers;

namespace SinphinitySysStore.Models
{
    public class SegmentEntity
    {
        public SegmentEntity()
        {

        }
        public SegmentEntity(Sinphinity.Models.Phrase p)
        {
            var asSegment = new Segment(p);
            TotalNotes = asSegment.TotalNotes;
            DurationInTicks = asSegment.DurationInTicks;
            NoteDensityTimes10 = asSegment.NoteDensity;
            MaxDurationVariationTimes10 = asSegment.MaxDurationVariation;
            PitchDirections = asSegment.PitchDirections;
            PitchStep = asSegment.PitchStep;
            PitchRange = asSegment.PitchRange;
            AbsPitchVariationTimes10 = asSegment.AbsPitchVariation;
            RelPitchVariationTimes10 = asSegment.RelPitchVariation;
            AverageIntervalTimes10 = asSegment.AverageInterval;
            MonotonyTimes10 = asSegment.Monotony;
            Hash = Hashing.CreateMD5(TotalNotes.ToString() + "," + DurationInTicks.ToString() + "," + NoteDensityTimes10.ToString() +
                                    "," + MaxDurationVariationTimes10.ToString() + "," + PitchDirections.ToString() + "," +
                                    PitchStep.ToString() + "," + PitchRange.ToString() + "," + AbsPitchVariationTimes10.ToString() +
                                    "," + RelPitchVariationTimes10.ToString() + "," + AverageIntervalTimes10.ToString() + "," + MonotonyTimes10.ToString());
        }
        /// <summary>
        /// Primary key
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Value used to check easily if 2 segments are equal. The hash is an md5 hash of the segment values 
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TotalNotes { get; set; }

        // METRIC RELATED
        /// <summary>
        /// Duration in ticks from the start of the first note to the end of the last note
        /// </summary>
        public long DurationInTicks { get; set; }

        /// <summary>
        /// Average number of notes per 384 ticks (rounded to the nearest integer)
        /// </summary>
        public int NoteDensityTimes10 { get; set; }
        /// <summary>
        /// Logarithm in base 2 of duration of longest note divided by duration of shortest note (actually the rounding of that value 
        /// multiplied by 10, so it is an integer)
        /// </summary>
        public int MaxDurationVariationTimes10 { get; set; }



        // PITCH RELATED
        /// <summary>
        /// A comma separated list of "+","-" and "0" that express the sign of the relative pitches
        /// </summary>
        public string PitchDirections { get; set; }
        /// <summary>
        /// Difference in pitch between last note and first
        /// </summary>
        public int PitchStep { get; set; }
        /// <summary>
        /// The difference between the highest and the lowest pitch
        /// </summary>
        public int PitchRange { get; set; }
        /// <summary>
        /// Gives an idea of how many different pitches are played, where all Cs for ex. are considered the same pitch, regardless if
        /// they are C1 or C3, etc. 
        /// Total different pitches divided by total notes (actually the rounding of that value multiplied by 10, so it is an integer)
        /// </summary>
        public int AbsPitchVariationTimes10 { get; set; }
        /// <summary>
        /// Gives an idea of how many different pitches are played, where notes separated by an octave are considered different (C3 and C4
        /// for ex. are considered different pitches)
        /// Total different pitches divided by total notes (actually the rounding of that value multiplied by 10, so it is an integer)
        /// </summary>
        public int RelPitchVariationTimes10 { get; set; }
        /// <summary>
        /// Average of the absolute values of relative pitches (actually the rounding of that value multiplied by 10, so it is an integer)
        /// </summary>
        public int AverageIntervalTimes10 { get; set; }
        /// <summary>
        /// The number of "no-changes" in pitch direction divided by the max possible changes (actually the rounding 
        /// of that value multiplied by 10, so it is an integer)
        /// 
        /// The maximum possible value is 10 (when it is going always up or always down) and the lowest 0 (when it changes direction every time)
        /// A change of + to 0, or - to 0 counts as half change 
        /// </summary>
        public int MonotonyTimes10 { get; set; }
    }
}
