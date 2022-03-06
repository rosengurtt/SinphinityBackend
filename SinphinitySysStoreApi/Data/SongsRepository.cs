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
                    .Select(x => x.AsSong(null))
                    .ToListAsync();
                return (total, pageSongs);
            }
            catch (Exception fsadf)
            {

            }
            return (0, null);
        }


        public async Task<Song> GetSongByIdAsync(long songId, int? simplificationVersion)
        {
            var songsSimplifications = await _dbContext.SongsSimplifications
                .Where(x => x.SongData.SongId == songId && (simplificationVersion == null || x.Version == simplificationVersion)).ToListAsync();
            var songEntity = await _dbContext.Songs
                .Include(x => x.MidiStats)
                .Include(k => k.Style)
                .Include(m => m.Band)
                .Where(y => y.Id == songId)
                .FirstOrDefaultAsync();
            var songData = await _dbContext.SongsData
                .Where(y => y.SongId == songId)
                .FirstOrDefaultAsync();
            var song = songEntity?.AsSong(songData);

            if (songsSimplifications != null && songsSimplifications.Count > 0)
                song.SongSimplifications = songsSimplifications.Select(x => x.AsSongSimplification()).ToList();
            return song;
        }


        public async Task<Song> AddSong(Song song)
        {
            var songExistsAlready = await _dbContext.Songs.Where(x => x.Name == song.Name && x.Band.Name == song.Band.Name).CountAsync() > 0;
            if (songExistsAlready) throw new SongAlreadyExistsException();

            var songRecord = new Models.SongEntity(song);
            _dbContext.Songs.Add(songRecord);
            try
            {
                await _dbContext.SaveChangesAsync();
                var songData = new SongData(song);
                songData.SongId = songRecord.Id;
                _dbContext.SongsData.Add(songData);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception fdsdsfsad)
            {

            }
            return song;
        }

        public async Task<Song> UpdateSong(Song song)
        {
            var currentSong = await _dbContext.Songs.Where(x => x.Id == song.Id).FirstOrDefaultAsync();
            var currentSongData= await _dbContext.SongsData.Where(x => x.SongId == song.Id).FirstOrDefaultAsync();
            if (currentSong == null || currentSongData == null) throw new SongDoesntExistException();

            currentSong.ArePhrasesExtracted = song.ArePhrasesExtracted;
            currentSong.AverageTempoInBeatsPerMinute = song.AverageTempoInBeatsPerMinute;
            currentSong.CantBeProcessed = song.CantBeProcessed;
            currentSong.IsMidiCorrect = song.IsMidiCorrect;
            currentSong.IsSongProcessed = song.IsSongProcessed;
            currentSong.MidiStats = new MidiStatsEntity(song.MidiStats, song);
            currentSongData.SongSimplifications = song.SongSimplifications.Select(x => new SongSimplificationEntity(x, song, currentSongData)).ToList();
            currentSongData.Bars= JsonConvert.SerializeObject(song.Bars); 
            currentSongData.TempoChanges = JsonConvert.SerializeObject(song.TempoChanges);


            try
            {
                _dbContext.Songs.Update(currentSong);
                _dbContext.SongsData.Update(currentSongData);
                await _dbContext.SaveChangesAsync();
                return song;

            }
            catch (Exception fdsdsfsad)
            {

            }
            return song;
        }
    }
}
