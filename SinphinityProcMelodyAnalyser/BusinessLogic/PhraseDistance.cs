using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static class PhraseDistance
    {
        /// <summary>
        /// The distance is the sum of the pitch differences between the notes in the first phrase and the corresponding notes on the second phrase
        /// 
        /// If we have for example C,E,F,D,C and D,F,G,E,D (that are equivalent phrases) their accumulated pitches will be:
        /// 4,5,2,0 and 3,5,2,0
        /// 
        /// We then compare 4 with 3, 5 with 5, 2 with 2, and 0 with 0
        /// 
        /// The distance will be 1
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static int GetDistance(List<int> p1, List<int> p2)
        {
            if (p1.Count != p2.Count)
                return int.MaxValue;
            var totalDifference = 0;

            for (var i = 0; i < p1.Count; i++)
            {
                totalDifference += Math.Abs(p1[i] - p2[i]);
            }
            return totalDifference;
        }

        public static int GetDistance(PhrasePitches p1, PhrasePitches p2)
        {
            return GetDistance(p1.ItemsAccum, p2.ItemsAccum);  
        }

        public static int GetDistance(Phrase p1, Phrase p2)
        {
            return GetDistance(p1.PhrasePitches, p2.PhrasePitches);
        }

        public static int GetDistance(EmbellishedPhrasePitches p1, EmbellishedPhrasePitches p2)
        {
            return GetDistance(p1.ItemsAccum, p2.ItemsAccum);
        }
   
        public static int GetDistance(EmbellishedPhrase p1, EmbellishedPhrase p2)
        {
            return GetDistance(p1.EmbellishedPhrasePitches, p2.EmbellishedPhrasePitches);
        }

        public static int GetDistance(PhraseTypeEnum type, string asString1, string asString2)
        {
            switch (type)
            {
                case PhraseTypeEnum.Pitches:
                    return GetDistance(new PhrasePitches(asString1), new PhrasePitches(asString2));
                case PhraseTypeEnum.Both:
                    return GetDistance(new Phrase(asString1), new Phrase(asString2));
                case PhraseTypeEnum.EmbelishedPitches:
                    return GetDistance(new EmbellishedPhrasePitches(asString1, asString1), new EmbellishedPhrasePitches(asString2, asString2));
                case PhraseTypeEnum.EmbellishedBoth:
                    return GetDistance(new EmbellishedPhrase(asString1, asString1), new EmbellishedPhrase(asString2, asString2));
            }
            return int.MaxValue;
        }  


        public static int MaxDistanceToBeConsideredEquivalent(PhraseTypeEnum type, string asString)
        {
            switch (type)
            {
                case PhraseTypeEnum.Pitches:
                    var phrasePitches = new PhrasePitches(asString);
                    return (int)Math.Floor(phrasePitches.NumberOfNotes / (double)3);
                case PhraseTypeEnum.Both:
                    var phrase = new Phrase(asString);
                    return (int)Math.Floor(phrase.NumberOfNotes / (double)3);
                case PhraseTypeEnum.EmbelishedPitches:
                    var embPhrasePitches = new EmbellishedPhrasePitches(asString, asString);
                    return (int)Math.Floor((double)(embPhrasePitches.NumberOfNotes * 2)/5);
                case PhraseTypeEnum.EmbellishedBoth:
                    var embPhrase = new EmbellishedPhrase(asString, asString);
                    return (int)Math.Floor(embPhrase.NumberOfNotes / (double)3);              
            }
            return 0;
        }
    }
}
