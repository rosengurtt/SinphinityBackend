
using Newtonsoft.Json;
using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class Phrase
    {
        public Phrase() { }

        public Phrase(ExtractedPhrase extractedPhrase)
        {
            PhraseType = extractedPhrase.PhraseType;
            AsString = extractedPhrase.AsString;
            AsStringBasic = null;
            AsStringWithoutOrnaments = extractedPhrase.AsStringWithoutOrnaments;
            AsStringWithoutOrnamentsAccum = null;
            Equivalences = JsonConvert.SerializeObject(extractedPhrase.Equivalences);

            switch (PhraseType)
            {
                case PhraseTypeEnum.Metrics:
                    var phraseMetrics = new PhraseMetrics(AsString);
                    AsStringBasic = (new Sinphinity.Models.BasicMetrics(phraseMetrics)).AsString;
                    NumberOfNotes = phraseMetrics.NumberOfNotes;
                    DurationInTicks = phraseMetrics.DurationInTicks;
                    AsStringAccum = null;
                    break;
                case PhraseTypeEnum.Pitches:
                    var phrasePitches = new PhrasePitches(AsString);
                    AsStringAccum = phrasePitches.AsStringAccum;
                    NumberOfNotes = phrasePitches.NumberOfNotes;
                    Range = phrasePitches.Range;
                    IsMonotone = phrasePitches.IsMonotone;
                    Step = phrasePitches.Step;
                    break;
                case PhraseTypeEnum.Both:
                    var phrase = new Sinphinity.Models.Phrase(AsString);
                    AsStringBasic = (new BasicMetrics(phrase.PhraseMetrics)).AsString;
                    AsStringAccum = phrase.AsStringAccum;
                    NumberOfNotes = phrase.PhraseMetrics.NumberOfNotes;
                    DurationInTicks = phrase.PhraseMetrics.DurationInTicks;
                    Range = phrase.PhrasePitches.Range;
                    IsMonotone = phrase.PhrasePitches.IsMonotone;
                    Step = phrase.PhrasePitches.Step;
                    break;
                case PhraseTypeEnum.EmbelishedMetrics:
                    var embelishedMetrics = new EmbellishedPhraseMetrics(AsStringWithoutOrnaments, AsString);
                    AsStringAccum = null;
                    NumberOfNotes = embelishedMetrics.NumberOfNotes;
                    DurationInTicks = embelishedMetrics.DurationInTicks;
                    break;
                case PhraseTypeEnum.EmbelishedPitches:
                    var embelishedPitches = new EmbellishedPhrasePitches(AsStringWithoutOrnaments, AsString);
                    AsStringAccum = embelishedPitches.AsStringAccum;
                    AsStringWithoutOrnaments = embelishedPitches.AsStringWithoutOrnaments;
                    AsStringWithoutOrnamentsAccum = embelishedPitches.AsStringWithoutOrnamentsAccum;
                    NumberOfNotes = embelishedPitches.NumberOfNotes;
                    Range = embelishedPitches.Range;
                    IsMonotone = embelishedPitches.IsMonotone;
                    Step = embelishedPitches.Step;
                    break;
                case PhraseTypeEnum.EmbellishedBoth:
                    var embelishedPhrase = new EmbellishedPhrase(AsStringWithoutOrnaments, AsString);
                    AsStringAccum = embelishedPhrase.AsStringAccum;
                    AsStringBasic = (new Sinphinity.Models.BasicMetrics(embelishedPhrase.EmbellishedPhraseMetrics.AsStringWithoutOrnaments)).AsString;
                    AsStringWithoutOrnamentsAccum = embelishedPhrase.AsStringWithoutOrnamentsAccum;
                    NumberOfNotes = embelishedPhrase.EmbellishedPhraseMetrics.NumberOfNotes;
                    DurationInTicks = embelishedPhrase.EmbellishedPhraseMetrics.DurationInTicks;
                    Range = embelishedPhrase.EmbellishedPhrasePitches.Range;
                    IsMonotone = embelishedPhrase.EmbellishedPhrasePitches.IsMonotone;
                    Step = embelishedPhrase.EmbellishedPhrasePitches.Step;
                    break;
            }
        }

        //public Phrase(Sinphinity.Models.PhraseMetrics pm)
        //{
        //    PhraseType = Sinphinity.Models.PhraseTypeEnum.Metrics;
        //    AsString = pm.AsString;
        //    AsStringBasic = (new Sinphinity.Models.BasicMetrics(pm)).AsString;
        //    AsStringAccum = pm.AsStringAccum;
        //    AsStringWithoutOrnaments = null;
        //    AsStringWithoutOrnamentsAccum = null;
        //    Equivalences = null;
        //    NumberOfNotes = pm.NumberOfNotes;
        //    DurationInTicks = pm.DurationInTicks;
        //}
        //public Phrase(Sinphinity.Models.PhrasePitches pp)
        //{
        //    PhraseType = Sinphinity.Models.PhraseTypeEnum.Pitches;
        //    AsString = pp.AsString;
        //    AsStringAccum = pp.AsStringAccum;
        //    AsStringBasic = null;
        //    AsStringWithoutOrnaments = null;
        //    AsStringWithoutOrnamentsAccum = null;
        //    Equivalences = JsonConvert.SerializeObject(pp.Equivalences);
        //    NumberOfNotes = pp.NumberOfNotes;
        //    Range = pp.Range;
        //    IsMonotone = pp.IsMonotone;
        //    Step = pp.Step;
        //}
        //public Phrase(Sinphinity.Models.Phrase p)
        //{
        //    PhraseType = Sinphinity.Models.PhraseTypeEnum.Both;
        //    AsString = p.AsString;
        //    AsStringAccum = p.AsStringAccum;
        //    AsStringBasic = (new Sinphinity.Models.BasicMetrics(p.PhraseMetrics)).AsString;
        //    AsStringWithoutOrnaments = null;
        //    AsStringWithoutOrnamentsAccum = null;
        //    Equivalences = JsonConvert.SerializeObject(p.Equivalences);
        //    NumberOfNotes = p.PhraseMetrics.NumberOfNotes;
        //    DurationInTicks = p.PhraseMetrics.DurationInTicks;
        //    Range = p.PhrasePitches.Range;
        //    IsMonotone = p.PhrasePitches.IsMonotone;
        //    Step = p.PhrasePitches.Step;
        //}
        //public Phrase(Sinphinity.Models.EmbellishedPhraseMetrics epm)
        //{
        //    PhraseType = Sinphinity.Models.PhraseTypeEnum.EmbelishedMetrics;
        //    AsString = epm.AsString;
        //    AsStringAccum = epm.AsStringAccum;
        //    AsStringBasic = (new Sinphinity.Models.BasicMetrics(epm.AsStringWithoutOrnaments)).AsString;
        //    AsStringWithoutOrnaments = epm.AsStringWithoutOrnaments;
        //    AsStringWithoutOrnamentsAccum = epm.AsStringWithoutOrnamentsAccum;
        //    Equivalences = null;
        //    NumberOfNotes = epm.NumberOfNotes;
        //    DurationInTicks = epm.DurationInTicks;
        //}
        //public Phrase(Sinphinity.Models.EmbellishedPhrasePitches epp)
        //{
        //    PhraseType = Sinphinity.Models.PhraseTypeEnum.EmbelishedPitches;
        //    AsString = epp.AsString;
        //    AsStringAccum = epp.AsStringAccum;
        //    AsStringBasic = null;
        //    AsStringWithoutOrnaments = epp.AsStringWithoutOrnaments;
        //    AsStringWithoutOrnamentsAccum = epp.AsStringWithoutOrnamentsAccum;
        //    Equivalences = JsonConvert.SerializeObject(epp.Equivalences);
        //    NumberOfNotes = epp.NumberOfNotes;
        //    Range = epp.Range;
        //    IsMonotone = epp.IsMonotone;
        //    Step = epp.Step;
        //}
        //public Phrase(Sinphinity.Models.EmbellishedPhrase ep)
        //{
        //    PhraseType = Sinphinity.Models.PhraseTypeEnum.EmbellishedBoth;
        //    AsString = ep.AsString;
        //    AsStringAccum = ep.AsStringAccum;
        //    AsStringBasic = (new Sinphinity.Models.BasicMetrics(ep.EmbellishedPhraseMetrics.AsStringWithoutOrnaments)).AsString;
        //    AsStringWithoutOrnaments = ep.AsStringWithoutOrnaments;
        //    AsStringWithoutOrnamentsAccum = ep.AsStringWithoutOrnamentsAccum;
        //    Equivalences = JsonConvert.SerializeObject(ep.Equivalences);
        //    NumberOfNotes = ep.EmbellishedPhraseMetrics.NumberOfNotes;
        //    DurationInTicks = ep.EmbellishedPhraseMetrics.DurationInTicks;
        //    Range = ep.EmbellishedPhrasePitches.Range;
        //    IsMonotone = ep.EmbellishedPhrasePitches.IsMonotone;
        //    Step = ep.EmbellishedPhrasePitches.Step;
        //}

        public long Id { get; set; }
        public string AsString { get; set; }
        public string AsStringAccum { get; set; }
        public string AsStringBasic { get; set; }
        public string AsStringWithoutOrnaments { get; set; }
        public string AsStringWithoutOrnamentsAccum { get; set; }

        public string Equivalences{ get; set; }
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
