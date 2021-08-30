using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Serilog;
using SinphinitySysStore.Models;
using SinphinitySysStore.Models.Exceptions;

namespace SinphinitySysStore.Repositories
{
    public class SongsRepository
    {
        private const int DefaultPageSize = 10;
        private const string DefaultSortKey = "name";
        private const int DefaultSortOrder = -1;
        private readonly IMongoCollection<SongInfo> _songsInfoCollection;
        private readonly IMongoCollection<SongData> _songsDataCollection;
        private readonly IMongoCollection<SongMidi> _songsMidiCollection;

        public SongsRepository(IMongoClient mongoClient)
        {
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

            _songsInfoCollection = mongoClient.GetDatabase("sinphinity").GetCollection<SongInfo>("songsInfo");
            _songsDataCollection = mongoClient.GetDatabase("sinphinity").GetCollection<SongData>("songsData");
            _songsMidiCollection = mongoClient.GetDatabase("sinphinity").GetCollection<SongMidi>("songsMidi");

        }

        public async Task<long> GetSongsCountAsync(string contains = ".*", string styleId = null, string bandId = null)
        {
            var nameFilter = Builders<SongInfo>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i");
            var bandFilter = Builders<SongInfo>.Filter.Eq(x => x.Band.Id, bandId);
            var styleFilter = Builders<SongInfo>.Filter.Eq(x => x.Style.Id, styleId);
            var combineFilter = string.IsNullOrEmpty(bandId) ? nameFilter : Builders<SongInfo>.Filter.And(nameFilter, bandFilter, styleFilter);
            return await _songsInfoCollection.Find(combineFilter).CountDocumentsAsync();
        }

        public async Task<IReadOnlyList<SongInfo>> GetSongsAsync(int pageSize = DefaultPageSize, int page = 0, string contains = ".*", string styleId = null, string bandId = null,
            string sort = DefaultSortKey, int sortDirection = DefaultSortOrder, CancellationToken cancellationToken = default)
        {
            var sortFilter = new BsonDocument(sort, sortDirection);
            var nameFilter = Builders<SongInfo>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i");
            var bandFilter = bandId == null ? Builders<SongInfo>.Filter.Exists(x => x.Band.Id) : Builders<SongInfo>.Filter.Eq(x => x.Band.Id, bandId);
            var styleFilter = styleId == null ? Builders<SongInfo>.Filter.Exists(x => x.Style.Id) : Builders<SongInfo>.Filter.Eq(x => x.Style.Id, styleId);
            var combineFilter = Builders<SongInfo>.Filter.And(nameFilter, bandFilter, styleFilter);
            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            };


            var songs = await _songsInfoCollection
                .Find(combineFilter, options)
                .Sort(sortFilter)
                .Limit(pageSize)
                .Skip(pageSize * page)
                .ToListAsync(cancellationToken);

            return songs;
        }
        public async Task<SongInfo> GetSongInfoByIdAsync(string songId)
        {
            var filter = Builders<SongInfo>.Filter.Eq(x => x.Id, songId);         
            return await _songsInfoCollection.Find(filter).FirstOrDefaultAsync();
        }


        public async Task<Song> GetSongByIdAsync(string songId, int? songSimplification)
        {
            var filter = Builders<SongInfo>.Filter.Eq(x => x.Id, songId);

            var songInfo = await _songsInfoCollection.Find(filter).FirstOrDefaultAsync();

            var songData = await _songsDataCollection.Find(Builders<SongData>.Filter.Eq(x => x.Id, songInfo.SongDataId)).FirstOrDefaultAsync();

            var songMidi = await _songsMidiCollection.Find(Builders<SongMidi>.Filter.Eq(x => x.Id, songInfo.SongMidiId)).FirstOrDefaultAsync();

            if (songSimplification != null)
            {
                var newSongSimplifications = new List<SongSimplification>();
                var songSimplificationToKeep = songData.SongSimplifications.Where(x => x.Version == songSimplification).FirstOrDefault();
                if (songSimplificationToKeep != null)
                    newSongSimplifications.Add(songSimplificationToKeep);
                songData.SongSimplifications = newSongSimplifications;
            }
            return new Song(songInfo, songData, songMidi);
        }

        public async Task<Song> GetSongMidiByIdAsync(string songId)
        {
            var filter = Builders<SongInfo>.Filter.Eq(x => x.Id, songId);

            var songInfo = await _songsInfoCollection.Find(filter).FirstOrDefaultAsync();

            var songMidi = await _songsMidiCollection.Find(Builders<SongMidi>.Filter.Eq(x => x.Id, songInfo.SongMidiId)).FirstOrDefaultAsync();

            return new Song(songInfo, null, songMidi);
        }

        public async Task<Song> InsertSongInfoAndMidiAsync(Song song)
        {
            var nameFilter = Builders<SongInfo>.Filter.Eq(s => s.Name, song.Name);
            var bandFilter = Builders<SongInfo>.Filter.Eq(x => x.Band.Id, song.Band.Id);
            var styleFilter = Builders<SongInfo>.Filter.Eq(x => x.Style.Id, song.Style.Id);
            var combineFilter = Builders<SongInfo>.Filter.And(nameFilter, bandFilter, styleFilter);
            var count = await _songsInfoCollection.Find(combineFilter).CountDocumentsAsync();
            if (count > 0) throw new SongAlreadyExistsException();
            var songInfo = new SongInfo(song);          
            await _songsInfoCollection.InsertOneAsync(songInfo);
            song.Id = songInfo.Id;

            if (!string.IsNullOrEmpty(song.MidiBase64Encoded))
            {
                await _songsMidiCollection.DeleteOneAsync(Builders<SongMidi>.Filter.Eq(s => s.SongInfoId, song.Id));
                var songMidi = new SongMidi(song);
                songMidi.SongInfoId = songInfo.Id;
                await _songsMidiCollection.InsertOneAsync(songMidi);
                song.SongMidiId = songMidi.Id;
                await _songsInfoCollection.UpdateOneAsync(Builders<SongInfo>.Filter.Eq(s => s.Id, song.Id),
                    Builders<SongInfo>.Update.Set(t => t.SongMidiId, songMidi.Id));
            }

            return song;
        }

        public async Task UpdateSongInfo(SongInfo songInfo)
        {
            try
            {
                await _songsInfoCollection.ReplaceOneAsync(Builders<SongInfo>.Filter.Eq(s => s.Id, songInfo.Id), songInfo);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Couldnt update songINfo for  {songInfo.Name} to MongoDb");
            }
        }

        public async Task AddInfoToSong(Song song)
        {
            try
            {
                await _songsDataCollection.DeleteOneAsync(Builders<SongData>.Filter.Eq(s => s.SongInfoId, song.Id));
                var songData = new SongData(song);
                songData.SongInfoId = song.Id;
                await _songsDataCollection.InsertOneAsync(songData);

                song.SongDataId = songData.Id;
                await _songsInfoCollection.UpdateOneAsync(Builders<SongInfo>.Filter.Eq(s => s.Id, song.Id),
                    Builders<SongInfo>.Update
                    .Set(t => t.SongDataId, songData.Id)
                    .Set(t => t.IsSongProcessed, true)
                    .Set(t => t.MidiStats, song.MidiStats)
                    .Set(t => t.DurationInSeconds, song.DurationInSeconds)
                    .Set(t => t.DurationInTicks, song.DurationInTicks)
                    .Set(t => t.AverageTempoInBeatsPerMinute, song.AverageTempoInBeatsPerMinute)
                    );
            }
            catch (Exception e)
            {
                Log.Error(e, $"Couldnt update song {song.Name} to MongoDb");
                // Try to save with flag CantBeProcessed = true and without the SongSimplifications
                song.SongSimplifications = null;
                song.CantBeProcessed = true;
                try
                {
                    await _songsInfoCollection.UpdateOneAsync(Builders<SongInfo>.Filter.Eq(s => s.Id, song.Id),
                    Builders<SongInfo>.Update.Set(t => t.CantBeProcessed, true));
                    Log.Information($"Updated song {song.Name} setting flag CantBeProcessed to true");
                }
                catch (Exception ex)
                {
                    Log.Information("Couldn't set flag CantBeProcessed to true");
                }
            }
        }
    }
}

