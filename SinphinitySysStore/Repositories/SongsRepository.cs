using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Repositories
{
    public class SongsRepository
    {
        private const int DefaultPageSize = 10;
        private const string DefaultSortKey = "name";
        private const int DefaultSortOrder = -1;
        private readonly IMongoCollection<Song> _songsCollection;
        private readonly IMongoClient _mongoClient;

        public SongsRepository(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

            _songsCollection = mongoClient.GetDatabase("sinphinity").GetCollection<Song>("songs");
        }

        public async Task<long> GetSongsCountAsync(string contains = ".*")
        {
            return await _songsCollection.CountDocumentsAsync(Builders<Song>.Filter.Regex(s=>s.Name, @$"/.*{contains}.*/i"));
        }

        public async Task<IReadOnlyList<Song>> GetSongsAsync(int pageSize = DefaultPageSize, int page = 0, string contains = ".*",
       string sort = DefaultSortKey, int sortDirection = DefaultSortOrder,
       CancellationToken cancellationToken = default)
        {
            var skip = pageSize * page;
            var limit = pageSize;


            var sortFilter = new BsonDocument(sort, sortDirection);
            var songs = await _songsCollection
                .Find(Builders<Song>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i"))
                .Limit(limit)
                .Skip(skip)
                .Sort(sortFilter)
                .ToListAsync(cancellationToken);

            return songs;
        }

        public async Task<Song> InsertSongAsync(Song song)
        {
             await _songsCollection.InsertOneAsync(song);
            return song;
        }
    }
}

