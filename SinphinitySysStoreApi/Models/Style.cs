
namespace SinphinitySysStore.Models
{
    public class Style
    {
        public Style() { }
        public Style(Sinphinity.Models.Style s)
        {
            Id = s.Id;
            Name = s.Name;
        }
        public long Id { get; set; }
        public string Name { get; set; }


        public ICollection<Phrase> Phrases { get; set; }

        public Sinphinity.Models.Style AsStyle()
        {
            return new Sinphinity.Models.Style { Id=Id, Name=Name};
        }
    }
}
