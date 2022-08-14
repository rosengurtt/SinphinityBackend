using Microsoft.EntityFrameworkCore;
using Serilog;
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


        public async Task SavePhrasesAsync(List<Sinphinity.Models.ExtractedPhrase> extractePhrases, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            foreach (var extractedPhrase in extractePhrases)
            {
                try
                {
                    var currentPhrase = await _dbContext.Phrases.Where(x =>  x.MetricsAsString == extractedPhrase.Phrase.MetricsAsString && x.PitchesAsString==extractedPhrase.Phrase.PitchesAsString)
                        .FirstOrDefaultAsync();
                    if (currentPhrase == null)
                    {
                        var phraseEntity = new Phrase(extractedPhrase);
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentPhrase = phraseEntity;
                    }
                    await SaveAssociationsOfPhrase(currentPhrase.Id, song);
                    await InsertOccurrences(extractedPhrase.Occurrences, songId, currentPhrase.Id);
                }
                catch (Exception ex)
                {

                }
            }
        }



        private async Task SaveAssociationsOfPhrase(long phraseId, Song song)
        {
            var bandId = song.BandId;
            var styleId = await _dbContext.Bands.Where(b => b.Id == song.BandId).Include(y => y.Style).Select(x => x.Style.Id).FirstOrDefaultAsync();
            try
            {
                await _dbContext.Songs
                                .FromSqlRaw(@$"IF ((SELECT count(*) FROM PhraseSong WHERE SongsId={song.Id} AND PhrasesId={phraseId})=0) 
                                                    INSERT INTO PhraseSong(SongsId, PhrasesId) VALUES ({song.Id},{phraseId})
                                                    IF ((SELECT count(*) FROM BandPhrase WHERE BandsId={bandId} AND PhrasesId={phraseId})=0) 
                                                    INSERT INTO BandPhrase(BandsId, PhrasesId) VALUES ({bandId},{phraseId})
                                                    IF ((SELECT count(*) FROM PhraseStyle WHERE StylesId={styleId} AND PhrasesId={phraseId})=0) 
                                                    INSERT INTO PhraseStyle(StylesId, PhrasesId) VALUES ({styleId},{phraseId})
                                                    SELECT TOP 1 * FROM Songs").ToListAsync();
            }
            catch (Exception fsfsad)
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
            catch (Exception sePudrioPapi)        ///// <summary>
       
            {
                Log.Error(sePudrioPapi, "Exception raised when trying to set ArePhrasesExtracted flag to true");
            }
        }

        public string GetEmbellishedPhrase(string p)
        {
            var parts = p.Split('|');
            return parts[0];
        }






        private async Task InsertOccurrences(List<Sinphinity.Models.PhraseLocation> locations, long songId, long phraseId)
        {
            // Insert Occurrences
            foreach (var loc in locations)
            {
                var occ = await _dbContext.PhrasesOccurrences.Where(x => x.SongId == songId && x.PhraseId == phraseId && x.Voice == loc.Voice && x.StartTick == loc.StartTick)
                    .FirstOrDefaultAsync();
                if (occ == null)
                {
                    var occur = new PhraseOccurrenceEntity(loc, phraseId);
                    _dbContext.PhrasesOccurrences.Add(occur);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
