using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class PhraseEntity
    {
        public PhraseEntity() { }
        public PhraseEntity(PhraseMetrics pm)
        {
            PhraseType = PhraseTypeEnum.Metrics;
            AsString = pm.AsString;
            AsStringBasic = (new BasicMetrics(pm)).AsString;
            AsStringWithoutOrnaments = null;
            NumberOfNotes = pm.NumberOfNotes;
            DurationInTicks = pm.DurationInTicks;
        }
        public PhraseEntity(PhrasePitches pp)
        {
            PhraseType = PhraseTypeEnum.Pitches;
            AsString = pp.AsString;
            AsStringBasic = null;
            AsStringWithoutOrnaments = null;
            NumberOfNotes = pp.NumberOfNotes;
            Range = pp.Range;
            IsMonotone = pp.IsMonotone;
            Step = pp.Step;
        }
        public PhraseEntity(Phrase p)
        {
            PhraseType = PhraseTypeEnum.Both;
            AsString = p.AsString;
            AsStringBasic = (new BasicMetrics(p.PhraseMetrics)).AsString;
            AsStringWithoutOrnaments = null;
            NumberOfNotes = p.PhraseMetrics.NumberOfNotes;
            DurationInTicks = p.PhraseMetrics.DurationInTicks;
            Range = p.PhrasePitches.Range;
            IsMonotone = p.PhrasePitches.IsMonotone;
            Step = p.PhrasePitches.Step;
        }
        public PhraseEntity(EmbellishedPhraseMetrics epm)
        {
            PhraseType = PhraseTypeEnum.EmbelishedMetrics;
            AsString = epm.AsString;
            AsStringBasic = (new BasicMetrics(epm.AsStringWithoutOrnaments)).AsString;
            AsStringWithoutOrnaments = epm.AsStringWithoutOrnaments;
            NumberOfNotes = epm.NumberOfNotes;
            DurationInTicks = epm.DurationInTicks;
        }
        public PhraseEntity(EmbellishedPhrasePitches epp)
        {
            PhraseType = PhraseTypeEnum.EmbelishedPitches;
            AsString = epp.AsString;
            AsStringBasic = null;
            AsStringWithoutOrnaments = epp.AsStringWithoutOrnaments;
            NumberOfNotes = epp.NumberOfNotes;
            Range = epp.Range;
            IsMonotone = epp.IsMonotone;
            Step = epp.Step;
        }
        public PhraseEntity(EmbellishedPhrase ep)
        {
            PhraseType = PhraseTypeEnum.EmbellishedBoth;
            AsString = ep.AsString;
            AsStringBasic = (new BasicMetrics(ep.EmbellishedPhraseMetrics.AsStringWithoutOrnaments)).AsString;
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
        public PhraseTypeEnum PhraseType { get; set; }

        public ICollection<SongEntity> Songs { get; set; }
        public ICollection<Band> Bands { get; set; }
        public ICollection<Style> Styles { get; set; }


  
        public PhraseMetrics AsPhraseMetrics()
        {
            return new PhraseMetrics(AsString, Id);
        }
        public PhrasePitches AsPhrasePirches()
        {
            return new PhrasePitches(AsString, Id);
        }
        public Phrase AsPhrase()
        {
            return new Phrase(AsString, Id);
        }
        public EmbellishedPhraseMetrics AsEmbellishedPhraseMetrics()
        {
            return new EmbellishedPhraseMetrics(AsStringWithoutOrnaments, AsString, Id);
        }
        public EmbellishedPhrasePitches AsEmbellishePhrasePitches()
        {
            return new EmbellishedPhrasePitches(AsStringWithoutOrnaments, AsString, Id);
        }
        public EmbellishedPhrase AsEmbellishePhrase()
        {
            return new EmbellishedPhrase(AsStringWithoutOrnaments, AsString, Id);
        }
    }
}
