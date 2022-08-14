
using Newtonsoft.Json;
using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class Phrase
    {
        public Phrase() { }

        public Phrase(ExtractedPhrase extractedPhrase)
        {
            MetricsAsString = extractedPhrase.Phrase.MetricsAsString;
            MetricsAccumAsString = extractedPhrase.Phrase.MetricsAccumAsString;
            PitchesAsString = extractedPhrase.Phrase.PitchesAsString;
            PitchesAccumAsString = extractedPhrase.Phrase.PitchesAccumAsString;
            DurationInTicks = extractedPhrase.Phrase.DurationInTicks;
            NumberOfNotes = extractedPhrase.Phrase.NumberOfNotes;
            Range = extractedPhrase.Phrase.Range;
            IsMonotone = extractedPhrase.Phrase.IsMonotone;
            Step = extractedPhrase.Phrase.Step;
            Equivalences = JsonConvert.SerializeObject(extractedPhrase.Equivalences);
        }
     


        public long Id { get; set; }
        public string MetricsAsString { get; set; }
        public string MetricsAccumAsString { get; set; }
        public string PitchesAsString { get; set; }
        public string PitchesAccumAsString { get; set; }

        public string Equivalences{ get; set; }
        public long DurationInTicks { get; set; }
        public int NumberOfNotes { get; set; }
        public int Range { get; set; }
        public bool IsMonotone { get; set; }
        public int Step { get; set; }

        public ICollection<Song> Songs { get; set; }
        public ICollection<Band> Bands { get; set; }
        public ICollection<Style> Styles { get; set; }



    }
}
