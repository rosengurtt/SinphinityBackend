namespace Sinphinity.Models
{
    public interface IPhrase
    {
        public string AsString { get; set; }
        public Song AsSong  { get; }
        public PhraseTypeEnum PhraseType { get; }
    }
}
