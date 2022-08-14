namespace Sinphinity.Models
{
    public class ExtractedPhrase
    {

        public Phrase Phrase { get; set; }
        public List<string> Equivalences { get; set; }
        public List<PhraseLocation> Occurrences { get; set; }

    }



}
