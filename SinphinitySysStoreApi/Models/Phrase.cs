
using Newtonsoft.Json;
using Sinphinity.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SinphinitySysStore.Models
{
    public class Phrase
    {
        public Phrase() { }

        public Phrase(Sinphinity.Models.Phrase p, List<string> equivalences)
        {
            MetricsAsString = p.MetricsAsString;
            MetricsAccumAsString = p.MetricsAccumAsString;
            PitchesAsString = p.PitchesAsString;
            PitchesAccumAsString = p.PitchesAccumAsString;
            DurationInTicks = p.DurationInTicks;
            NumberOfNotes = p.NumberOfNotes;
            Range = p.Range;
            IsMonotone = p.IsMonotone;
            Step = p.Step;
            Equivalences = JsonConvert.SerializeObject(equivalences);
            SkeletonMetricsAsString = p.SkeletonMetricsAsString;
            SkeletonPitchesAsString= p.SkeletonPitchesAsString;
        }

        public Sinphinity.Models.Phrase AsSynphinityModelsPhrase()
        {
            return new Sinphinity.Models.Phrase
            {
                Id = this.Id,
                MetricsAsString = this.MetricsAsString,
                PitchesAsString = this.PitchesAsString,
                Equivalences = this.Equivalences == "[]" ?
                     new List<string>() :
                     JsonConvert.DeserializeObject<List<string>>(this.Equivalences)
            };
        }



        public long Id { get; set; }
        public string MetricsAsString { get; set; }
        public string MetricsAccumAsString { get; set; }

        public string SkeletonMetricsAsString { get; set; }

        public string PitchesAsString { get; set; }
        public string PitchesAccumAsString { get; set; }
        public string SkeletonPitchesAsString { get; set; }

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
