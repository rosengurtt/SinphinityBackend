
namespace SinphinitySysStore.Models
{
    public class Phrase
    {
        public Phrase() { }
        public Phrase(Sinphinity.Models.PhraseMetrics pm)
        {
            PhraseType = Sinphinity.Models.PhraseTypeEnum.Metrics;
            AsString = pm.AsString;
            AsStringBasic = (new Sinphinity.Models.BasicMetrics(pm)).AsString;
            AsStringWithoutOrnaments = null;
            NumberOfNotes = pm.NumberOfNotes;
            DurationInTicks = pm.DurationInTicks;
        }
        public Phrase(Sinphinity.Models.PhrasePitches pp)
        {
            PhraseType = Sinphinity.Models.PhraseTypeEnum.Pitches;
            AsString = pp.AsString;
            AsStringBasic = null;
            AsStringWithoutOrnaments = null;
            NumberOfNotes = pp.NumberOfNotes;
            Range = pp.Range;
            IsMonotone = pp.IsMonotone;
            Step = pp.Step;
        }
        public Phrase(Sinphinity.Models.Phrase p)
        {
            PhraseType = Sinphinity.Models.PhraseTypeEnum.Both;
            AsString = p.AsString;
            AsStringBasic = (new Sinphinity.Models.BasicMetrics(p.PhraseMetrics)).AsString;
            AsStringWithoutOrnaments = null;
            NumberOfNotes = p.PhraseMetrics.NumberOfNotes;
            DurationInTicks = p.PhraseMetrics.DurationInTicks;
            Range = p.PhrasePitches.Range;
            IsMonotone = p.PhrasePitches.IsMonotone;
            Step = p.PhrasePitches.Step;
        }
        public Phrase(Sinphinity.Models.EmbellishedPhraseMetrics epm)
        {
            PhraseType = Sinphinity.Models.PhraseTypeEnum.EmbelishedMetrics;
            AsString = epm.AsString;
            AsStringBasic = (new Sinphinity.Models.BasicMetrics(epm.AsStringWithoutOrnaments)).AsString;
            AsStringWithoutOrnaments = epm.AsStringWithoutOrnaments;
            NumberOfNotes = epm.NumberOfNotes;
            DurationInTicks = epm.DurationInTicks;
        }
        public Phrase(Sinphinity.Models.EmbellishedPhrasePitches epp)
        {
            PhraseType = Sinphinity.Models.PhraseTypeEnum.EmbelishedPitches;
            AsString = epp.AsString;
            AsStringBasic = null;
            AsStringWithoutOrnaments = epp.AsStringWithoutOrnaments;
            NumberOfNotes = epp.NumberOfNotes;
            Range = epp.Range;
            IsMonotone = epp.IsMonotone;
            Step = epp.Step;
        }
        public Phrase(Sinphinity.Models.EmbellishedPhrase ep)
        {
            PhraseType = Sinphinity.Models.PhraseTypeEnum.EmbellishedBoth;
            AsString = ep.AsString;
            AsStringBasic = (new Sinphinity.Models.BasicMetrics(ep.EmbellishedPhraseMetrics.AsStringWithoutOrnaments)).AsString;
            AsStringWithoutOrnaments = ep.AsStringWithoutOrnaments;
            NumberOfNotes = ep.EmbellishedPhraseMetrics.NumberOfNotes;
            DurationInTicks = ep.EmbellishedPhraseMetrics.DurationInTicks;
            Range = ep.EmbellishedPhrasePitches.Range;
            IsMonotone = ep.EmbellishedPhrasePitches.IsMonotone;
            Step = ep.EmbellishedPhrasePitches.Step;
        }

        public long Id { get; set; }
        public string AsString { get; set; }
        public string AsStringBasic { get; set; }
        public string AsStringWithoutOrnaments { get; set; }
        public long DurationInTicks { get; set; }
        public int NumberOfNotes { get; set; }
        public int Range { get; set; }
        public bool IsMonotone { get; set; }
        public int Step { get; set; }
        public Sinphinity.Models.PhraseTypeEnum PhraseType { get; set; }

        public ICollection<Song> Songs { get; set; }
        public ICollection<Band> Bands { get; set; }
        public ICollection<Style> Styles { get; set; }


  
        public Sinphinity.Models.PhraseMetrics AsPhraseMetrics()
        {
            return new Sinphinity.Models.PhraseMetrics(AsString, Id);
        }
        public Sinphinity.Models.PhrasePitches AsPhrasePirches()
        {
            return new Sinphinity.Models.PhrasePitches(AsString, Id);
        }
        public Sinphinity.Models.Phrase AsPhrase()
        {
            return new Sinphinity.Models.Phrase(AsString, Id);
        }
        public Sinphinity.Models.EmbellishedPhraseMetrics AsEmbellishedPhraseMetrics()
        {
            return new Sinphinity.Models.EmbellishedPhraseMetrics(AsStringWithoutOrnaments, AsString, Id);
        }
        public Sinphinity.Models.EmbellishedPhrasePitches AsEmbellishePhrasePitches()
        {
            return new Sinphinity.Models.EmbellishedPhrasePitches(AsStringWithoutOrnaments, AsString, Id);
        }
        public Sinphinity.Models.EmbellishedPhrase AsEmbellishePhrase()
        {
            return new Sinphinity.Models.EmbellishedPhrase(AsStringWithoutOrnaments, AsString, Id);
        }
    }
}
