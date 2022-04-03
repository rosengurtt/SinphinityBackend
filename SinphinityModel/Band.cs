namespace Sinphinity.Models
{
    public class Band
    {
        public long  Id { get; set; }
        public string Name { get; set; }

        public Style? Style { get; set; }
    }
}
