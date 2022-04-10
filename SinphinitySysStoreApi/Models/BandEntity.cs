using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class BandEntity
    {
        public BandEntity() { }
        public BandEntity(Band b) {
            Id = b.Id;
            Name = b.Name;
            Style = new StyleEntity(b.Style);
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public StyleEntity Style { get; set; }

        public Band AsBand()
        {
            return new Band { Name = Name, Id = Id, Style = Style.AsStyle() };
        }
    }
}
