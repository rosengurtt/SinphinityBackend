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
        private readonly IMongoCollection<Song> _songsCollection;

        public SongsRepository(IMongoClient mongoClient)
        {
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

            _songsCollection = mongoClient.GetDatabase("sinphinity").GetCollection<Song>("songs");
        }

        public async Task<long> GetSongsCountAsync(string contains = ".*", string styleId = null, string bandId = null)
        {
            var nameFilter = Builders<Song>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i");
            var bandFilter = Builders<Song>.Filter.Eq(x => x.Band.Id, bandId);
            var styleFilter = Builders<Song>.Filter.Eq(x => x.Style.Id, styleId);
            var combineFilter = string.IsNullOrEmpty(bandId) ? nameFilter : Builders<Song>.Filter.And(nameFilter, bandFilter, styleFilter);
            return await _songsCollection.Find(combineFilter).CountDocumentsAsync();
        }

        public async Task<IReadOnlyList<Song>> GetSongsAsync(int pageSize = DefaultPageSize, int page = 0, bool includeMidi = false, string contains = ".*", string styleId = null, string bandId = null,
            string sort = DefaultSortKey, int sortDirection = DefaultSortOrder, CancellationToken cancellationToken = default)
        {
            var sortFilter = new BsonDocument(sort, sortDirection);
            var nameFilter = Builders<Song>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i");
            var bandFilter = bandId == null ? Builders<Song>.Filter.Exists(x => x.Band.Id) : Builders<Song>.Filter.Eq(x => x.Band.Id, bandId);
            var styleFilter = styleId == null ? Builders<Song>.Filter.Exists(x => x.Style.Id) : Builders<Song>.Filter.Eq(x => x.Style.Id, styleId);
            var combineFilter = Builders<Song>.Filter.And(nameFilter, bandFilter, styleFilter);
            var projection = includeMidi ? Builders<Song>.Projection.Exclude(x => x.SongSimplifications):
                                           Builders<Song>.Projection.Exclude(x => x.SongSimplifications).Exclude(x => x.MidiBase64Encoded);
            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            };


            var songs = await _songsCollection
                .Find(combineFilter, options)
                .Project<Song>(projection)
                .Sort(sortFilter)
                .Limit(pageSize)
                .Skip(pageSize * page)
                .ToListAsync(cancellationToken);

            return songs;
        }

        public async Task<Song> GetSongByIdAsync(string songId, int? songSimplification)
        {
            var filter = Builders<Song>.Filter.Eq(x => x.Id, songId);

            var song = await _songsCollection.Find(filter).FirstOrDefaultAsync();

            // the base64 encoded midi is not needed
            song.MidiBase64Encoded = null;
            if (songSimplification != null)
            {
                var newSongSimplifications = new List<SongSimplification>();
                var songSimplificationToKeep = song.SongSimplifications.Where(x => x.Version == songSimplification).FirstOrDefault();
                if (songSimplificationToKeep != null)
                    newSongSimplifications.Add(songSimplificationToKeep);
                song.SongSimplifications = newSongSimplifications;
            }
            return song;
        }

        public async Task<Song> InsertSongAsync(Song song)
        {
            var nameFilter = Builders<Song>.Filter.Eq(s => s.Name, song.Name);
            var bandFilter = Builders<Song>.Filter.Eq(x => x.Band.Id, song.Band.Id);
            var styleFilter = Builders<Song>.Filter.Eq(x => x.Style.Id, song.Style.Id);
            var combineFilter = Builders<Song>.Filter.And(nameFilter, bandFilter, styleFilter);
            var count = await _songsCollection.Find(combineFilter).CountDocumentsAsync();
            if (count > 0) throw new SongAlreadyExistsException();
            await _songsCollection.InsertOneAsync(song);
            return song;
        }

        public async Task UpdateSongAsync(Song song)
        {
            try
            {
                await _songsCollection.ReplaceOneAsync(s => s.Id == song.Id, song);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Couldnt update song {song.Name} to MongoDb");
                // Try to save with flag CantBeProcessed = true and without the SongSimplifications
                song.SongSimplifications = null;
                song.CantBeProcessed = true;
                try
                {
                    await _songsCollection.ReplaceOneAsync(s => s.Id == song.Id, song);
                    Log.Information($"Updated song {song.Name} setting flag CantBeProcessed to true");
                }
                catch(Exception ex)
                {
                    Log.Information("Couldn't set flag CantBeProcessed to true");
                }
            }
        }
    }
}

