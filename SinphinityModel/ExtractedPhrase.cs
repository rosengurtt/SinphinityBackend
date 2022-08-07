using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class ExtractedPhrase
    {

        public string? AsString { get; set; }
        public string? AsStringWithoutOrnaments { get; set; }
        public List<string> Equivalences { get; set; }
        public PhraseTypeEnum PhraseType { get; set; }
        public List<PhraseLocation> Occurrences { get; set; }


    }



}
