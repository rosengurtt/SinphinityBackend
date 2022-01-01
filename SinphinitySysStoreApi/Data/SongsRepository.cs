using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public class SongsRepository
    {
        private readonly SinphinityDbContext _dbContext;
        public SongsRepository(SinphinityDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<(int, List<Song>)> GetSongsAsync(
           int pageNo = 0,
           int pageSize = 10,
           string? contains = null,
           long? styleId = null,
           long? bandId = null)
        {
            try
            {
                var source = _dbContext.Songs.AsQueryable()
                        .Include(z => z.Style)
                        .Include(y => y.Band)
                        .Include(z => z.MidiStats)
                        .Where(x =>
                    (styleId == null || x.Style.Id == styleId) &&
                    (bandId == null || x.Band.Id == bandId) &&
                    (contains == null || x.Name.Contains(contains))
                );
                var total = await source.CountAsync();

                var pageSongs = await source
                    .OrderBy(x => x.Name)
                    .Skip((pageNo) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.AsSong())
                    .ToListAsync();
                return (total, pageSongs);
            }
            catch(Exception fsadf)
            {
                
            }
            return (0,null);
        }


        public async Task<Song> GetSongByIdAsync(long songId, int? simplificationVersion)
        {
            var songsSimplifications = await _dbContext.SongsSimplifications
                .Where(x => x.Song.Id == songId && (simplificationVersion == null || x.Version == simplificationVersion)).ToListAsync();
            var song = await _dbContext.Songs
                .Include(x => x.MidiStats)
                .Include(k=>k.Style)
                .Include(m=>m.Band)
                .Where(y => y.Id == songId)
                .Select(z => z.AsSong())
                .FirstOrDefaultAsync();
            if (songsSimplifications != null && songsSimplifications.Count > 0)
                song.SongSimplifications = songsSimplifications.Select(x => x.AsSongSimplification()).ToList();
            return song;
        }


        public async Task<Song> AddSong(Song song)
        {
            var songExistsAlready = await _dbContext.Songs.Where(x => x.Name == song.Name && x.Band.Name == song.Band.Name).CountAsync() > 0;
            if (songExistsAlready) throw new SongAlreadyExistsException();

            _dbContext.Songs.Add(new Models.SongEntity(song));
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception fdsdsfsad)
            {

            }
            return song;
        }

        public async Task<Song> UpdateSong(Song song)
        {
            var currentSong = await _dbContext.Songs.Where(x => x.Id==song.Id).FirstOrDefaultAsync();
            if (currentSong==null) throw new SongDoesntExistException();
            currentSong.ArePatternsExtracted = song.ArePatternsExtracted;
            currentSong.AverageTempoInBeatsPerMinute = song.AverageTempoInBeatsPerMinute;
            currentSong.CantBeProcessed = song.CantBeProcessed;
            currentSong.IsMidiCorrect = song.IsMidiCorrect;
            currentSong.IsSongProcessed = song.IsSongProcessed;
            currentSong.MidiStats = new MidiStatsEntity(song.MidiStats, song);
            currentSong.SongSimplifications = song.SongSimplifications.Select(x => new SongSimplificationEntity(x, song)).ToList();
            currentSong.TempoChanges = JsonConvert.SerializeObject(song.TempoChanges);
            currentSong.Bars = JsonConvert.SerializeObject(song.Bars);

            _dbContext.Songs.Update(currentSong);
            await _dbContext.SaveChangesAsync();
            return song;
        }
    }
}
