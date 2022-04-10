using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class StyleEntity
    {
        public StyleEntity() { }
        public StyleEntity(Style s)
        {
            Id = s.Id;
            Name = s.Name;
        }
        public long Id { get; set; }
        public string Name { get; set; }

        public Style AsStyle()
        {
            return new Style { Id=Id, Name=Name};
        }
    }
}
