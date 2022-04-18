

namespace SinphinitySysStore.Models
{
    public class Band
    {
        public Band() { }
        public Band(Sinphinity.Models.Band b) {
            Id = b.Id;
            Name = b.Name;
            Style = new Style(b.Style);
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public Style Style { get; set; }

        public ICollection<Phrase> Phrases { get; set; }

        public Sinphinity.Models.Band AsBand()
        {
            return new Sinphinity.Models.Band { Name = Name, Id = Id, Style = Style.AsStyle() };
        }
    }
}
