using SinphinitySysStore.Data;
using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public class PatternsRepository
    {

        private readonly SinphinityDbContext _dbContext;
        public PatternsRepository(SinphinityDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task SavePatternsOfSongAsync(Dictionary<string, HashSet<Occurrence>> patterns, long songId)
        {
            try
            {
                var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
                if (song == null) throw new Exception($"Song with id = {songId} does not exist");
                foreach (var pat in patterns)
                {
                    // Insert basic pattern if it is not in collection already
                    var basicPattern = new BasicPattern(pat.Key);
                    var basicPatternAsString = basicPattern.AsString;
                    var basPatInDb = await _dbContext.BasicPatterns.Where(x => x.AsString == basicPatternAsString).FirstOrDefaultAsync();
                    if (basPatInDb == null)
                    {
                        _dbContext.BasicPatterns.Add(basicPattern);
                        await _dbContext.SaveChangesAsync();
                        basPatInDb = basicPattern;
                    }

                    // Insert pattern if it is not in collection already
                    var patternAsString = pat.Key;
                    var patInDb = await _dbContext.Patterns.Where(x => x.AsString == patternAsString).FirstOrDefaultAsync();
                    if (patInDb == null)
                    {
                        var paton = new Pattern(patternAsString);
                        _dbContext.Patterns.Add(paton);
                        await _dbContext.SaveChangesAsync();
                        patInDb = paton;
                    }

                    // Insert link between pattern and basic pattern
                    var basPatPatInDb = await _dbContext.BasicPatternsPatterns
                        .Where(x => x.PatternId == patInDb.Id && x.BasicPatternId == basPatInDb.Id).FirstOrDefaultAsync();
                    if (basPatPatInDb == null)
                    {
                        var basPatPat = new BasicPatternPattern { PatternId = patInDb.Id, BasicPatternId = basPatInDb.Id };
                        _dbContext.BasicPatternsPatterns.Add(basPatPat);
                        await _dbContext.SaveChangesAsync();
                        basPatPatInDb = basPatPat;
                    }

                    // Insert record to PatternsSongs if not there
                    var patSong = await _dbContext.PatternsSongs.Where(x => x.SongId == songId && x.PatternId == patInDb.Id).FirstOrDefaultAsync();
                    if (patSong == null)
                    {
                        var ps = new PatternSong()
                        {
                            PatternId = patInDb.Id,
                            SongId = songId
                        };
                        _dbContext.PatternsSongs.Add(ps);
                        await _dbContext.SaveChangesAsync();
                    }

                    // Insert occurrences
                    foreach (var occ in pat.Value)
                    {
                        var currentOc = await _dbContext.PatternOccurrences
                            .Where(x => x.PatternId == patInDb.Id &&
                            x.SongId == songId &&
                            x.Voice == occ.Voice &&
                            x.Tick == occ.Tick).FirstOrDefaultAsync();
                        if (currentOc == null)
                        {
                            var occurrence = new PatternOccurrence
                            {
                                PatternId = patInDb.Id,
                                SongId = songId,
                                Voice = occ.Voice,
                                BarNumber = (int)occ.BarNumber,
                                Beat = (int)occ.Beat,
                                Tick = occ.Tick
                            };
                            _dbContext.PatternOccurrences.Add(occurrence);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
                song.ArePatternsExtracted = true;
                _dbContext.Songs.Update(song);
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception fadfasd)
            {

            }
        }
   
       
    }
}
