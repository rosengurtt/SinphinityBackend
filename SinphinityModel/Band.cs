namespace Sinphinity.Models
{
    public class Band
    {
        public virtual string Id { get; set; }
        public string Name { get; set; }

        public Style Style { get; set; }
    }
}
