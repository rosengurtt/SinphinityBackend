using Microsoft.EntityFrameworkCore;
using Serilog;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public partial class PhrasesRepository
    {

        private readonly SinphinityDbContext _dbContext;
        public PhrasesRepository(SinphinityDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task SavePhrasesMetricsOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var pm in phrasesLocations.Keys)
                {
                    var currentPhrase = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.Metrics && x.AsString == pm).FirstOrDefaultAsync();
                    if (currentPhrase == null)
                    {
                        var phraseEntity = new PhraseEntity(new PhraseMetrics(pm));
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentPhrase = phraseEntity;
                    }
                    await InsertOccurrences(phrasesLocations[pm], songId, currentPhrase.Id);
                }
            }
            catch (Exception sePudrioPapi)
            {

            }
        }


        public async Task SavePhrasesPitchesOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var pp in phrasesLocations.Keys)
                {
                    var currentPhrase = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.Pitches && x.AsString == pp).FirstOrDefaultAsync();
                    if (currentPhrase == null)
                    {
                        var phraseEntity = new PhraseEntity(new PhrasePitches(pp));
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentPhrase = phraseEntity;
                    }
                    await InsertOccurrences(phrasesLocations[pp], songId, currentPhrase.Id);
                }
            }
            catch (Exception sePudrioPapi)
            {

            }
        }


        public async Task SaveEmbellishedPhrasesMetricsOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var ep in phrasesLocations.Keys)
                {
                    // Extract the embellished version and the version without embelllishments from the string
                    (var embellishedPhraseMetricsAsString, var phraseMetricsAsString) = DecomposeEmbellishedPhrasePart(ep);

                    var currentPhraseMetrics = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.Metrics && x.AsString == phraseMetricsAsString).FirstOrDefaultAsync();
                    if (currentPhraseMetrics == null)
                    {
                        var phraseEntity = new PhraseEntity(new PhraseMetrics(phraseMetricsAsString));
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentPhraseMetrics = phraseEntity;
                    }
                    var currentEmbellishedPhraseMetrics = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.EmbelishedMetrics && x.AsString == embellishedPhraseMetricsAsString).FirstOrDefaultAsync();
                    if (currentEmbellishedPhraseMetrics == null)
                    {
                        var phraseEntity = new PhraseEntity(new EmbellishedPhraseMetrics(phraseMetricsAsString, embellishedPhraseMetricsAsString));
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentEmbellishedPhraseMetrics = phraseEntity;
                    }
                    await InsertOccurrences(phrasesLocations[ep], songId, currentEmbellishedPhraseMetrics.Id);
                }
            }
            catch (Exception sePudrioPapi)
            {

            }
        }
        public async Task SaveEmbellishedPhrasesPitchesOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var ep in phrasesLocations.Keys)
                {
                    // Extract the embellished version and the version without embelllishments from the string
                    (var embellishedPhrasePitchesAsString, var phrasePitchesAsString) = DecomposeEmbellishedPhrasePart(ep);


                    var currentPhrasePitches = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.Pitches && x.AsString == phrasePitchesAsString).FirstOrDefaultAsync();
                    if (currentPhrasePitches == null)
                    {
                        var phraseEntity = new PhraseEntity(new PhrasePitches(phrasePitchesAsString));
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentPhrasePitches = phraseEntity;
                    }
                    var currentEmbellishedPhrasePitches = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.EmbelishedPitches && x.AsString == embellishedPhrasePitchesAsString).FirstOrDefaultAsync();
                    if (currentEmbellishedPhrasePitches == null)
                    {
                        var phraseEntity = new PhraseEntity(new EmbellishedPhrasePitches(phrasePitchesAsString, embellishedPhrasePitchesAsString));
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentEmbellishedPhrasePitches = phraseEntity;
                    }
                    await InsertOccurrences(phrasesLocations[ep], songId, currentEmbellishedPhrasePitches.Id);
                }
            }
            catch (Exception sePudrioPapi)
            {

            }
        }

        public async Task SaveEmbellishedPhrasesOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var ep in phrasesLocations.Keys)
                {
                    // Extract the embellished version and the version without embelllishments from the string
                    ((var embellishedPhraseMetricsAsString, var embellishedPhrasePitchesAsString),(var phraseMetricsAsString, var phrasePitchesAsString)) = DecomposeEmbellishedPhrase(ep);

                    var fraseMetInDb = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.EmbelishedMetrics && x.AsString == embellishedPhraseMetricsAsString)
                        .FirstOrDefaultAsync();
                    var phrasePitInDb = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.EmbelishedPitches && x.AsString == embellishedPhrasePitchesAsString)
                        .FirstOrDefaultAsync();

                    if (fraseMetInDb == null || phrasePitInDb == null)
                        throw new Exception("Me mandaron cualquier mierda");

                    var embellishedPhrase = GetEmbellishedPhrase(ep);

                    var currentPhrase = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.EmbellishedBoth && x.AsString == embellishedPhrase).FirstOrDefaultAsync();
                    if (currentPhrase == null)
                    {
                        var phraseEntity = new PhraseEntity(new EmbellishedPhrase(ep));
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentPhrase = phraseEntity;
                    }
                    await InsertOccurrences(phrasesLocations[ep], songId, currentPhrase.Id);

                }
            }
            catch (Exception sePudrioPapi)
            {

            }
        }



        public async Task SavePhrasesOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var p in phrasesLocations.Keys)
                {
                    var parts = p.Split('/');
                    var fraseMetInDb = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.Metrics && x.AsString == parts[0]).FirstOrDefaultAsync();
                    var phrasePitInDb = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.Pitches && x.AsString == parts[1]).FirstOrDefaultAsync();

                    if (fraseMetInDb == null || phrasePitInDb == null)
                        throw new Exception("Me mandaron cualquier mierda");

                    var currentPhrase = await _dbContext.Phrases.Where(x => x.PhraseType == PhraseTypeEnum.Both && x.AsString == p).FirstOrDefaultAsync();
                    if (currentPhrase == null)
                    {
                        var phraseEntity = new PhraseEntity(new Phrase(p));
                        _dbContext.Phrases.Add(phraseEntity);
                        currentPhrase = phraseEntity;
                    }
                    await InsertOccurrences(phrasesLocations[p], songId, currentPhrase.Id);
                }
            }
            catch (Exception sePudrioPapi)
            {

            }
        }


        public async Task UpateSong(long songId)
        {
            var currentSong = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (currentSong == null) throw new SongDoesntExistException();

            currentSong.ArePhrasesExtracted = true;


            try
            {
                _dbContext.Songs.Update(currentSong);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception sePudrioPapi)
            {
                Log.Error(sePudrioPapi, "Exception raised when trying to set ArePhrasesExtracted flag to true");
            }
        }
        
        public string GetEmbellishedPhrase(string p)
        {
            var parts = p.Split('|');
            return parts[0];
        }
        /// <summary>
        /// To pass an embellished phrase as a single string, we concatenate the embellished version and the simplified version separated by a "|"
        /// </summary>
        /// <param name="phraseAsString"></param>
        /// <returns></returns>
        private ((string, string), (string, string)) DecomposeEmbellishedPhrase(string phraseAsString)
        {
            var parts = phraseAsString.Split('|');
            (var embellishedPart, var nonEmbellishedPart) = (parts[0], parts[1]);
            var embellishedParts = embellishedPart.Split('/');
            var nonEmbellishedParts = nonEmbellishedPart.Split('/');
            return ((embellishedParts[0], embellishedParts[1]), (nonEmbellishedParts[0], nonEmbellishedParts[1]));
        }
        /// <summary>
        /// Used for EmbellishedPhraseMetrics and EmbellishedPhrasePitches to separate the embellished part of the non embellished parth
        /// </summary>
        /// <param name="phraseAsString"></param>
        /// <returns></returns>
        private  (string, string) DecomposeEmbellishedPhrasePart(string phraseAsString)
        {
            var parts = phraseAsString.Split('|');
            return  (parts[0], parts[1]);
        }





        private async Task InsertOccurrences(List<SongLocation> locations, long songId, long phraseId)
        {
            // Insert Occurrences
            foreach (var loc in locations)
            {
                var occ = await _dbContext.PhrasesOccurrences.Where(x => x.SongId == songId && x.Voice == loc.Voice && x.Tick == loc.Tick )
                    .FirstOrDefaultAsync();
                if (occ == null)
                {
                    var occur = new PhraseOccurrence(loc, phraseId);
                    _dbContext.PhrasesOccurrences.Add(occur);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
  
    }
}
