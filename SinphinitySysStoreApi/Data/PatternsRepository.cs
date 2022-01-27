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

        public async Task SavePatternsOfSongAsync(HashSet<string> patternsSet, long songId)
        {
            var song = await _dbContext.Songs.Where(x => x.Id == songId).FirstOrDefaultAsync();
            if (song == null) throw new Exception($"Song with id = {songId} does not exist");
            try
            {
                foreach (var pat in patternsSet)
                {
                    // Insert basic pattern if it is not in collection already
                    var basicPattern = new BasicPattern(pat);
                    var basicPatternAsString = basicPattern.AsString;
                    var basPatInDb = await _dbContext.BasicPatterns.Where(x => x.AsString == basicPatternAsString).FirstOrDefaultAsync();
                    if (basPatInDb == null)
                    {
                        _dbContext.BasicPatterns.Add(basicPattern);
                        await _dbContext.SaveChangesAsync();
                        basPatInDb = basicPattern;
                    }

                    // Insert pattern if it is not in collection already
                    var patInDb = await _dbContext.Patterns.Where(x => x.AsString == pat).FirstOrDefaultAsync();
                    if (patInDb == null)
                    {
                        var paton = new Pattern(pat);
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
                }
                song.ArePatternsExtracted = true;
                _dbContext.Entry(song).Property("ArePatternsExtracted").IsModified = true;
                await _dbContext.SaveChangesAsync();

            }
            catch (Exception fadfasd)
            {

            }
        }

        public async Task<(int, List<Pattern>)> GetPatternsAsync(
           int pageNo = 0,
           int pageSize = 10,
           string? contains = null)
        {
            try
            {
                var source = _dbContext.Patterns.AsQueryable()
                        .Where(x =>
                    (contains == null || x.AsString.Contains(contains))
                );
                var total = await source.CountAsync();

                var pagePatterns = await source
                    .OrderBy(x=>x.NumberOfNotes)
                    .ThenBy(x => x.AsString)
                    .Skip((pageNo) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (total, pagePatterns);
            }
            catch (Exception fsadf)
            {

            }
            return (0, null);
        }
        public async Task<(int, List<Occurrence>)> GetPatternOccurencesAsync(int pageNo, int pageSize, long patternId)
        {
            try
            {
                var source = _dbContext.PatternOccurrences.AsQueryable()
                        .Where(x =>x.PatternId==patternId);
                var total = await source.CountAsync();

                var occ = await source
                    .OrderBy(x => x.SongId)
                    .ThenBy(y => y.Voice)
                    .ThenBy(z=>z.Tick)
                    .Skip((pageNo) * pageSize)
                    .Take(pageSize)
                    .Select(o => new Occurrence { SongId=o.SongId, Tick=o.Tick, Voice=o.Voice, BarNumber=o.BarNumber, Beat=o.Beat})
                    .ToListAsync();
                return (total, occ);
            }
            catch (Exception fsadf)
            {

            }
            return (0, null);
        }

    }
}
