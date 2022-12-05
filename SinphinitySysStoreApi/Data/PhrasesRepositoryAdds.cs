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


        public async Task SavePhrasesAsync(List<Sinphinity.Models.ExtractedPhrase> extractePhrases, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            foreach (var extractedPhrase in extractePhrases)
            {
                Log.Information("Saving segment");
                var segment = await SaveSegmentOfPhrase(extractedPhrase.Phrase);

                Log.Information($"Saving phrase {extractedPhrase.Phrase.MetricsAsString}/{extractedPhrase.Phrase.PitchesAsString}");
                try
                {
                    var currentPhrase = await _dbContext.Phrases.Where(x => x.MetricsAsString == extractedPhrase.Phrase.MetricsAsString && x.PitchesAsString == extractedPhrase.Phrase.PitchesAsString)
                        .FirstOrDefaultAsync();
                    if (currentPhrase == null)
                    {
                        var phraseEntity = new Models.Phrase(extractedPhrase.Phrase, extractedPhrase.Equivalences);
                        phraseEntity.SegmentId = segment.Id;
                        _dbContext.Phrases.Add(phraseEntity);
                        await _dbContext.SaveChangesAsync();
                        currentPhrase = phraseEntity;

                        //if (!string.IsNullOrEmpty(currentPhrase.SkeletonMetricsAsString))
                        //{  var currentSkeleton = await _dbContext.Phrases.Where(x => x.MetricsAsString == currentPhrase.SkeletonMetricsAsString &&
                        //                    x.PitchesAsString == currentPhrase.SkeletonPitchesAsString)
                        //    .FirstOrDefaultAsync();
                        //    if (currentSkeleton == null)
                        //    {
                        //        var phraseSkeleton = new Sinphinity.Models.Phrase(currentPhrase.SkeletonMetricsAsString, currentPhrase.SkeletonPitchesAsString);
                        //        var skeletonSegment = await SaveSegmentOfPhrase(phraseSkeleton);
                        //        var skeleton = new Models.Phrase(phraseSkeleton, new List<string>());
                        //        skeleton.SegmentId = skeletonSegment.Id;
                        //        _dbContext.Phrases.Add(skeleton);
                        //        await _dbContext.SaveChangesAsync();
                        //    }
                        //}
                    }
                    Log.Information("Saving associations");
                    await SaveAssociationsOfPhrase(currentPhrase.Id, song);
                    Log.Information("Saving occurrences");
                    await InsertOccurrences(extractedPhrase.Occurrences, songId, currentPhrase.Id);
                }
                catch (Exception ex)
                {

                }
            }
        }
        private async Task<SegmentEntity> SaveSegmentOfPhrase(Sinphinity.Models.Phrase p)
        {
            var seg = new SegmentEntity(p);
            try
            {
                var currentSegment = await _dbContext.Segments.Where(x => x.Hash == seg.Hash).FirstOrDefaultAsync();
                if (currentSegment == null)
                {

                    _dbContext.Segments.Add(seg);
                    await _dbContext.SaveChangesAsync();
                    return seg;
                }
                else
                    return currentSegment;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Couldn't save segment");
                throw;
            }
        }

        private async Task SaveAssociationsOfPhrase(long phraseId, SinphinitySysStore.Models.Song song)
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
            var currentSong = await _dbContext.Songs
                            .FromSqlRaw($"UPDATE Songs SET ArePhrasesExtracted=1 WHERE id ={songId}; SELECT TOP 1 * FROM songs")
                            .ToListAsync();
            try
            {
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
