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


        public async Task SavePhrasessOfSongAsync(List<Dictionary<string, List<SongLocation>>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            var retObjPhrasesMetrics = phrasesLocations[0];
            var retObjPhrasesPitches = phrasesLocations[1];
            var retObjPhrases = phrasesLocations[2];
            try
            {
                // Phrase metrics
                foreach (var pm in retObjPhrasesMetrics.Keys)
                {
                    var phraseMetrics = new PhraseMetricsEntity(pm);
                    // Insert basic metrics if it is not in collection already
                    var basicMetrics = new BasicMetricsEntity(new BasicMetrics(pm));
                    var basMetInDb = await _dbContext.BasicMetrics.Where(x => x.AsString == basicMetrics.AsString).FirstOrDefaultAsync();
                    if (basMetInDb == null)
                    {
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
                        try
                        {
                            await _dbContext.SaveChangesAsync();
                        }
                        catch (Exception fdsfds)
                        {

                        }
                        basMetPhraseMetInDb = basMetPhraseMet;
                    }
                    await InsertSongPhrasesAndLocations(retObjPhrasesMetrics[pm], PhraseTypeEnum.Metrics, songId, phraseMetInDb.Id);

                }
                // Phrase pitches
                foreach (var pp in retObjPhrasesPitches.Keys)
                {

                    // Insert phrase pitches                    
                    var phrasePitInDb = await _dbContext.PhrasesPitches.Where(x => x.AsString == pp).FirstOrDefaultAsync();
                    if (phrasePitInDb == null)
                    {
                        var phrasePitches = new PhrasePitchesEntity(pp);
                        _dbContext.PhrasesPitches.Add(phrasePitches);
                        await _dbContext.SaveChangesAsync();
                        phrasePitInDb = phrasePitches;
                    }

                    await InsertSongPhrasesAndLocations(retObjPhrasesPitches[pp], PhraseTypeEnum.Pitches, songId, phrasePitInDb.Id);
                }
                // Phrases
                foreach (var p in retObjPhrases.Keys)
                {
                    var metricsAsString = p.Substring(0, p.IndexOf("/"));
                    var pitchesAsString = p.Substring(p.IndexOf("/") + 1, p.Length - metricsAsString.Length - 1);
                    var fraseMetInDb = await _dbContext.PhrasesMetrics.Where(x => x.AsString == metricsAsString).FirstOrDefaultAsync();
                    var phrasePitInDb = await _dbContext.PhrasesPitches.Where(x => x.AsString == pitchesAsString).FirstOrDefaultAsync();

                    if (fraseMetInDb == null || phrasePitInDb == null)
                        throw new Exception("Me mandaron cualquier mierda");

                    var phraseInDb = await _dbContext.Phrases.Where(x => x.PhraseMetricsId == fraseMetInDb.Id && x.PhrasePitchesId == phrasePitInDb.Id)
                        .FirstOrDefaultAsync();
                    if (phraseInDb == null)
                    {
                        var phrase = new Phrase(fraseMetInDb.Id, phrasePitInDb.Id);
                        _dbContext.Phrases.Add(phrase);
                        try
                        {
                            await _dbContext.SaveChangesAsync();
                        }
                        catch(Exception fdsfsa)
                        {

                        }
                        phraseInDb = phrase;
                    }
                    await InsertSongPhrasesAndLocations(retObjPhrases[p], PhraseTypeEnum.Both, songId, phraseInDb.Id);
                }
                song.ArePhrasesExtracted = true;
                _dbContext.Entry(song).Property("ArePhrasesExtracted").IsModified = true;
                await _dbContext.SaveChangesAsync();

            }
            catch (Exception fadfasd)
            {

            }
        }
        private async Task InsertSongPhrasesAndLocations(List<SongLocation> locations, PhraseTypeEnum type, long songId, long phraseId)
        {
            // Insert phraseSong
            var phraseSong = await _dbContext.PhrasesSongs.Where(x => x.SongId == songId && x.PhraseId == phraseId && x.PhraseType == type)
                .FirstOrDefaultAsync();
            if (phraseSong == null)
            {
                var ps = new PhraseSong()
                {
                    PhraseId = phraseId,
                    SongId = songId,
                    Repetitions = locations.Count,
                    PhraseType = type
                };
                _dbContext.PhrasesSongs.Add(ps);
                await _dbContext.SaveChangesAsync();
            }

            // Insert Occurrences
            foreach (var loc in locations)
            {
                var occ = await _dbContext.PhrasesOccurrences.Where(x => x.SongId == songId && x.Voice == loc.Voice && x.Tick == loc.Tick && x.PhraseType == type)
                    .FirstOrDefaultAsync();
                if (occ == null)
                {
                    var occur = new PhrasesOccurrence(loc, phraseId, type);
                    _dbContext.PhrasesOccurrences.Add(occur);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
