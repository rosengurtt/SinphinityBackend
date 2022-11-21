using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public async Task<(int, List<Sinphinity.Models.Song>)> GetSongsAsync(
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


        public async Task<Sinphinity.Models.Song> GetSongByIdAsync(long songId, int? simplificationVersion)
        {
            var songsSimplifications = await _dbContext.SongsSimplifications
                .Where(x => x.SongData.SongId == songId && (simplificationVersion == null || x.Version == simplificationVersion)).ToListAsync();
            var songEntity = await _dbContext.Songs
                .Include(x => x.MidiStats)
                .Include(k => k.Style)
                .Include(m => m.Band)
                .Where(y => y.Id == songId)
                .FirstOrDefaultAsync();
            var songData = await _dbContext.SongData
                .Where(y => y.SongId == songId)
                .FirstOrDefaultAsync();
            var song = songEntity?.AsSong(songData);

            if (songsSimplifications != null && songsSimplifications.Count > 0)
                song.SongSimplifications = songsSimplifications.Select(x => x.AsSongSimplification()).ToList();
            return song;
        }


        public async Task<Sinphinity.Models.Song> AddSong(Sinphinity.Models.Song song)
        {
            var songExistsAlready = await _dbContext.Songs.Where(x => x.Name == song.Name && x.Band.Name == song.Band.Name).CountAsync() > 0;
            if (songExistsAlready) throw new SongAlreadyExistsException();

            var songRecord = new SinphinitySysStore.Models.Song(song);
            _dbContext.Songs.Add(songRecord);
            try
            {
                await _dbContext.SaveChangesAsync();
                var songData = new SongData(song);
                songData.SongId = songRecord.Id;
                _dbContext.SongData.Add(songData);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception fdsdsfsad)
            {

            }
            return song;
        }

        public async Task<Sinphinity.Models.Song> UpdateSong(Sinphinity.Models.Song song)
        {
            var currentSong = await _dbContext.Songs.Where(x => x.Id == song.Id).FirstOrDefaultAsync();
            var currentSongData= await _dbContext.SongData.Where(x => x.SongId == song.Id).FirstOrDefaultAsync();
            if (currentSong == null || currentSongData == null) throw new SongDoesntExistException();

            currentSong.ArePhrasesExtracted = song.ArePhrasesExtracted;
            currentSong.AverageTempoInBeatsPerMinute = song.AverageTempoInBeatsPerMinute;
            currentSong.CantBeProcessed = song.CantBeProcessed;
            currentSong.IsMidiCorrect = song.IsMidiCorrect;
            currentSong.IsSongProcessed = song.IsSongProcessed;
            currentSong.MidiStats = new MidiStatsEntity(song.MidiStats, song);
            currentSongData.SongSimplifications = song.SongSimplifications.Select(x => new SongSimplification(x, song, currentSongData)).ToList();
            currentSongData.Bars= JsonConvert.SerializeObject(song.Bars); 
            currentSongData.TempoChanges = JsonConvert.SerializeObject(song.TempoChanges);


            try
            {
                _dbContext.Songs.Update(currentSong);
                _dbContext.SongData.Update(currentSongData);
                await _dbContext.SaveChangesAsync();
                return song;

            }
            catch (Exception fdsdsfsad)
            {

            }
            return song;
        }
    
        /// <summary>
        /// Returns the voices of a song that have phrases
        /// The voices that have drums or chords are not returned
        /// </summary>
        /// <param name="songId"></param>
        /// <returns></returns>
        public async Task<List<object>> GetMelodicVoicesOfSong(long songId)
        {
            var voices = await _dbContext.PhrasesOccurrences.Where(x => x.SongId == songId)
                .Select(y => new { voiceNumber = y.Voice, instrumentNumber = y.Instrument })
                .Distinct()
                .ToListAsync();

            // Remove duplicates, that happen when a voice uses more than 1 instrument
            var uniqueVoices = new HashSet<int>();
            var retObj = new List<object>();
            foreach (var v in voices)
            {
                if (!uniqueVoices.Contains(v.voiceNumber))
                    retObj.Add((object)v);
                uniqueVoices.Add(v.instrumentNumber);

            }

            return retObj;
        }
    }
}
