using Microsoft.EntityFrameworkCore;
using Serilog;
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

        public async Task SavePhrasesMetricsOfSongAsync(Dictionary<string, List<SongLocation>> phrasesLocations, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var pm in phrasesLocations.Keys)
                {
                    (var embellishedPhraseMetrics, var phraseMetrics, var basicMetrics) = await ProcessMetrics(null, pm);
                    if (phraseMetrics != null)
                        await InsertSongPhrasesAndLocations(phrasesLocations[pm], PhraseTypeEnum.Metrics, songId, phraseMetrics.Id);
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
                    (var embellishedPhrasePitches, var phrasePitches) = await ProcessPitches(null, pp);
                    if (phrasePitches != null)
                        await InsertSongPhrasesAndLocations(phrasesLocations[pp], PhraseTypeEnum.Pitches, songId, phrasePitches.Id);
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
                    (var embellishedPhraseMetrics,  var phraseMetrics, var basicMetrics) = await ProcessMetrics(embellishedPhraseMetricsAsString, phraseMetricsAsString);
                    if (phraseMetrics != null)
                        await InsertSongPhrasesAndLocations(phrasesLocations[ep], PhraseTypeEnum.Metrics, songId, phraseMetrics.Id);
                    if (embellishedPhraseMetrics != null)
                        await InsertSongPhrasesAndLocations(phrasesLocations[ep], PhraseTypeEnum.EmbelishedMetrics, songId, embellishedPhraseMetrics.Id);
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
                    (var embellishedPhrasePitches, var phrasePitches) = await ProcessPitches(embellishedPhrasePitchesAsString, phrasePitchesAsString);
                    if (phrasePitches != null)
                        await InsertSongPhrasesAndLocations(phrasesLocations[ep], PhraseTypeEnum.Pitches, songId, phrasePitches.Id);
                    if (embellishedPhrasePitches != null)
                        await InsertSongPhrasesAndLocations(phrasesLocations[ep], PhraseTypeEnum.EmbelishedPitches, songId, embellishedPhrasePitches.Id);
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

                var embellishedPhraseMetInDb = await _dbContext.EmbelishedPhrasesMetrics.Where(x=>x.AsString== embellishedPhraseMetricsAsString).FirstOrDefaultAsync();
                    var embellishedPhrasePitchesInDb = await _dbContext.EmbellishedPhrasesPitches.Where(x => x.AsString == embellishedPhrasePitchesAsString).FirstOrDefaultAsync();

                    if (embellishedPhraseMetInDb == null || embellishedPhrasePitchesInDb == null)
                        throw new Exception("Me mandaron cualquier mierda");

                    var embellishedPhraseInDb= await _dbContext.EmbelishedPhrases
                        .Where(x=>x.EmbellishedPhraseMetricsId== embellishedPhraseMetInDb.Id && x.EmbellishedPhrasePitchesId== embellishedPhrasePitchesInDb.Id)
                        .FirstOrDefaultAsync();
                    if (embellishedPhraseInDb == null)
                    {
                        var embellishedPhrase = new EmbellishedPhrase(embellishedPhraseMetInDb.Id, embellishedPhrasePitchesInDb.Id);
                        _dbContext.EmbelishedPhrases.Add(embellishedPhrase);
                        try
                        {
                            await _dbContext.SaveChangesAsync();
                            embellishedPhraseInDb = embellishedPhrase;
                        }
                        catch (Exception sePudrioPapi)
                        {

                        }

                    }
                    if (embellishedPhraseInDb!=null)
                    await InsertSongPhrasesAndLocations(phrasesLocations[ep], PhraseTypeEnum.EmbellishedBoth, songId, embellishedPhraseInDb.Id);
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
                    (var metricsAsString, var pitchesAsString) = DecomposePhrase(p, "/");
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
                            phraseInDb = phrase;
                        }
                        catch (Exception sePudrioPapi)
                        {

                        }
                    }
                    if (phraseInDb != null)
                        await InsertSongPhrasesAndLocations(phrasesLocations[p], PhraseTypeEnum.Both, songId, phraseInDb.Id);
                }
                song.ArePhrasesExtracted = true;
                _dbContext.Entry(song).Property("ArePhrasesExtracted").IsModified = true;
                await _dbContext.SaveChangesAsync();

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
        
        /// <summary>
        /// To pass an embellished phrase as a single string, we concatenate the embellished version and the simplified version separated by a "|"
        /// </summary>
        /// <param name="phraseAsString"></param>
        /// <returns></returns>
        private ((string, string), (string, string)) DecomposeEmbellishedPhrase(string phraseAsString)
        {
            (var embellishedPart, var nonEmbellishedPart) = DecomposePhrase(phraseAsString, "|");
            return (DecomposePhrase(embellishedPart, "/"), DecomposePhrase(nonEmbellishedPart, "/"));
        }
        /// <summary>
        /// Used for EmbellishedPhraseMetrics and EmbellishedPhrasePitches to separate the embellished part of the non embellished parth
        /// </summary>
        /// <param name="phraseAsString"></param>
        /// <returns></returns>
        private  (string, string) DecomposeEmbellishedPhrasePart(string phraseAsString)
        {
            return  DecomposePhrase(phraseAsString, "|");
        }
        /// <summary>
        /// To pass a phrase as a single string, we concatenate the metrics with the pitches separated by a "/"
        /// </summary>
        /// <param name="phraseAsString"></param>
        /// <returns></returns>
        private (string, string) DecomposePhrase(string phraseAsString, string separator)
        {
            var metricsAsString = phraseAsString.Substring(0, phraseAsString.IndexOf(separator));
            var pitchesAsString = phraseAsString.Substring(phraseAsString.IndexOf(separator) + 1, phraseAsString.Length - metricsAsString.Length - 1);
            return (metricsAsString, pitchesAsString);
        }

        private async Task<(EmbellishedPhraseMetricsEntity?, PhraseMetricsEntity?, BasicMetricsEntity?)> ProcessMetrics(string? embellishedPhraseMetricsAsString, string phraseMetricsAsString)
        {

            var basicMetrics = new BasicMetricsEntity(new BasicMetrics(phraseMetricsAsString));
            // Insert basic metrics if it is not in collection already
            var basMetInDb = await _dbContext.BasicMetrics.Where(x => x.AsString == basicMetrics.AsString).FirstOrDefaultAsync();
            if (basMetInDb == null)
            {
                _dbContext.BasicMetrics.Add(basicMetrics);
                await _dbContext.SaveChangesAsync();
                basMetInDb = basicMetrics;
            }
            // Insert phrase metrics if not in collection already
            var phraseMetInDb = await _dbContext.PhrasesMetrics.Where(x => x.AsString == phraseMetricsAsString).FirstOrDefaultAsync();
            if (phraseMetInDb == null)
            {
                var phraseMetrics = new PhraseMetricsEntity(phraseMetricsAsString);
                phraseMetrics.BasicMetricsId = basMetInDb.Id;
                _dbContext.PhrasesMetrics.Add(phraseMetrics);
                await _dbContext.SaveChangesAsync();
                phraseMetInDb = phraseMetrics;
            }
            var embPhraseMetInDb = await _dbContext.EmbelishedPhrasesMetrics.Where(x => x.AsString == embellishedPhraseMetricsAsString).FirstOrDefaultAsync();
            if (embellishedPhraseMetricsAsString != null) {
                // Insert embellished phrase metrics if it is not in collection already
                if (embPhraseMetInDb == null)
                {
                    var embPhraseMetrics = new EmbellishedPhraseMetricsEntity(phraseMetInDb, embellishedPhraseMetricsAsString);
                    embPhraseMetrics.PhraseMetricsWithoutOrnamentsId = phraseMetInDb.Id;
                    _dbContext.EmbelishedPhrasesMetrics.Add(embPhraseMetrics);
                    await _dbContext.SaveChangesAsync();
                    embPhraseMetInDb = embPhraseMetrics;
                }
            }
            return (embPhraseMetInDb, phraseMetInDb, basMetInDb);
        }

        private async Task<(EmbellishedPhrasePitchesEntity?, PhrasePitchesEntity?)> ProcessPitches(string? embellishedPhrasePitchesAsString, string phrasePitchesAsString)
        {
            // Insert phrase pitches if not in collection allready
            var phrasePitchesInDb = await _dbContext.PhrasesPitches.Where(x => x.AsString == phrasePitchesAsString).FirstOrDefaultAsync();
            if (phrasePitchesInDb == null)
            {
                var phrasePitches = new PhrasePitchesEntity(phrasePitchesAsString);
                _dbContext.PhrasesPitches.Add(phrasePitches);
                await _dbContext.SaveChangesAsync();
                phrasePitchesInDb = phrasePitches;
            }
                var embPhrasePitchesInDb = await _dbContext.EmbellishedPhrasesPitches.Where(x => x.AsString == embellishedPhrasePitchesAsString).FirstOrDefaultAsync();
            if (embellishedPhrasePitchesAsString != null) {
                // Insert embellished phrase pitches it is not in collection already
                if (embPhrasePitchesInDb == null)
                {
                    var embPhrasePitches = new EmbellishedPhrasePitchesEntity(phrasePitchesInDb, embellishedPhrasePitchesAsString);
                    _dbContext.EmbellishedPhrasesPitches.Add(embPhrasePitches);
                    await _dbContext.SaveChangesAsync();
                }
            }
            return (embPhrasePitchesInDb, phrasePitchesInDb);
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
                    var occur = new PhraseOccurrence(loc, phraseId, type);
                    _dbContext.PhrasesOccurrences.Add(occur);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
  
    }
}
