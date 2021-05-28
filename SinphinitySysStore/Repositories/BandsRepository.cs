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
    public class BandsRepository
    {
        private const int DefaultPageSize = 10;
        private const string DefaultSortKey = "name";
        private const int DefaultSortOrder = -1;
        private readonly IMongoCollection<Band> _bandsCollection;
        private readonly IMongoClient _mongoClient;

        public BandsRepository(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

            _bandsCollection = mongoClient.GetDatabase("sinphinity").GetCollection<Band>("bands");
        }

        public async Task<long> GetBandsCountAsync(string contains = ".*")
        {
            return await _bandsCollection.CountDocumentsAsync(Builders<Band>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i"));
        }

        public async Task<IReadOnlyList<Band>> GetBandsAsync(int pageSize = DefaultPageSize, int page = 0, string contains = ".*",
       string sort = DefaultSortKey, int sortDirection = DefaultSortOrder,
       CancellationToken cancellationToken = default)
        {
            var skip = pageSize * page;
            var limit = pageSize;


            var sortFilter = new BsonDocument(sort, sortDirection);
            var bands = await _bandsCollection
                .Find(Builders<Band>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i"))
                .Limit(limit)
                .Skip(skip)
                .Sort(sortFilter)
                .ToListAsync(cancellationToken);

            return bands;
        }
    }
}
