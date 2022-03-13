namespace SinphinitySysStore.Models
{
    public class PhraseSong
    {
        public long Id { get; set; }
        public long PhraseId { get; set; }
        public long SongId { get; set; }
        public int Repetitions { get; set; }
 
       public PhraseTypeEnum PhraseType { get; set; }
    }
}
