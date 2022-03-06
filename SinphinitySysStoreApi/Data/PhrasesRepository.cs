using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public class PhrasesRepository
    {

        private readonly SinphinityDbContext _dbContext;
        public PhrasesRepository(SinphinityDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task SavePhrasessOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var p in phrasesLocations.Keys)
                {
                    var phrase = new Phrase(p);
                    var phraseMetrics = new PhraseMetricsEntity(phrase.PhraseMetrics);
                    // Insert basic metrics if it is not in collection already
                    var basicMetrics = new BasicMetricsEntity(new BasicMetrics(phrase.PhraseMetrics));
                    var basicMetricsAsString = basicMetrics.AsString;
                    var basMetInDb = await _dbContext.BasicMetrics.Where(x => x.AsString == basicMetricsAsString).FirstOrDefaultAsync();
                    if (basMetInDb == null)
                    {
                        if (basicMetrics.AsString.Length > 300)
                        {
                            var sacamela = 4;
                        }
                        _dbContext.BasicMetrics.Add(basicMetrics);
                        await _dbContext.SaveChangesAsync();
                        basMetInDb = basicMetrics;
                    }

                    // Insert phrase metrics if it is not in collection already
                    var phraseMetInDb = await _dbContext.PhrasesMetrics.Where(x => x.AsString == phraseMetrics.AsString).FirstOrDefaultAsync();
                    if (phraseMetInDb == null)
                    {
                        _dbContext.PhrasesMetrics.Add(phraseMetrics);
                        await _dbContext.SaveChangesAsync();
                        phraseMetInDb = phraseMetrics;
                    }

                    // Insert link between phrase metric and basic metric
                    var basMetPhraseMetInDb = await _dbContext.BasicMetricsPhrasesMetrics
                        .Where(x => x.PhraseMetricsId == phraseMetInDb.Id && x.BasicMetricsId == basMetInDb.Id).FirstOrDefaultAsync();
                    if (basMetPhraseMetInDb == null)
                    {
                        var basMetPhraseMet = new BasicMetricsPhraseMetrics { BasicMetricsId = basMetInDb.Id, PhraseMetricsId = phraseMetInDb.Id };
                        _dbContext.BasicMetricsPhrasesMetrics.Add(basMetPhraseMet);
                        await _dbContext.SaveChangesAsync();
                        basMetPhraseMetInDb = basMetPhraseMet;
                    }

                    // Insert phrase pitches
                    try
                    {
                        var sacamela= new PhrasePitchesEntity(phrase.PhrasePitches);
                    }
                    catch(Exception ex)
                    {

                    }
                    var phrasePitches = new PhrasePitchesEntity(phrase.PhrasePitches);
                    var phrasePitInDb = await _dbContext.PhrasesPitches.Where(x => x.AsString == phrasePitches.AsString).FirstOrDefaultAsync();
                    if (phrasePitInDb == null)
                    {
                        _dbContext.PhrasesPitches.Add(phrasePitches);
                        await _dbContext.SaveChangesAsync();
                        phrasePitInDb = phrasePitches;
                    }

                    // Insert phrase
                    phrase.PhraseMetricsId = phraseMetInDb.Id;
                    phrase.PhrasePitchesId = phrasePitInDb.Id;
                    var phraseInDb = await _dbContext.Phrases.Where(x => x.PhraseMetricsId == phrase.PhraseMetricsId&& x.PhrasePitchesId == phrase.PhrasePitchesId).FirstOrDefaultAsync();
                    if (phraseInDb == null)
                    {
                        _dbContext.Phrases.Add(phrase);
                        await _dbContext.SaveChangesAsync();
                        phraseInDb = phrase;
                    }

                    // Insert phraseSong
                    var phraseSong = await _dbContext.PhrasesSongs.Where(x => x.SongId == songId && x.PhraseId == phraseInDb.Id).FirstOrDefaultAsync();
                    if (phraseSong == null)
                    {
                        var ps = new PhraseSong()
                        {
                            PhraseId = phraseInDb.Id,
                            SongId = songId,
                            Repetitions= phrasesLocations.Count
                        };
                        _dbContext.PhrasesSongs.Add(ps);
                        await _dbContext.SaveChangesAsync();
                    }

                    // Insert Occurrences

                    foreach (var loc in phrasesLocations[p])
                    {
                        var occ = await _dbContext.PhrasesOccurrences.Where(x => x.SongId == songId && x.Voice == loc.Voice && x.Tick == loc.Tick).FirstOrDefaultAsync();
                        if (occ == null)
                        {
                            var occur = new PhrasesOccurrence(loc, phraseInDb.Id);
                            _dbContext.PhrasesOccurrences.Add(occur);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
                song.ArePhrasesExtracted = true;
                _dbContext.Entry(song).Property("ArePhrasesExtracted").IsModified = true;
                await _dbContext.SaveChangesAsync();

            }
            catch (Exception fadfasd)
            {

            }
        }
    }
}
