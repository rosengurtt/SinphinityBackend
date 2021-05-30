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
    public class StylesRepository
    {
        private const int DefaultPageSize = 10;
        private const string DefaultSortKey = "name";
        private const int DefaultSortOrder = -1;
        private readonly IMongoCollection<Style> _stylesCollection;
        private readonly IMongoClient _mongoClient;

        public StylesRepository(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

            _stylesCollection = mongoClient.GetDatabase("sinphinity").GetCollection<Style>("styles");
        }

        public async Task<long> GetStylesCountAsync(string contains = ".*")
        {
            return await _stylesCollection.CountDocumentsAsync(Builders<Style>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i"));
        }

        public async Task<IReadOnlyList<Style>> GetStylesAsync(int pageSize = DefaultPageSize, int page = 0, string contains = ".*",
       string sort = DefaultSortKey, int sortDirection = DefaultSortOrder,
       CancellationToken cancellationToken = default)
        {
            var skip = pageSize * page;
            var limit = pageSize;


            var sortFilter = new BsonDocument(sort, sortDirection);
            var styles = await _stylesCollection
                .Find(Builders<Style>.Filter.Regex(s=>s.Name, @$"/.*{contains}.*/i"))
                .Sort(sortFilter)
                .Limit(limit)
                .Skip(skip)
                .ToListAsync(cancellationToken);

            return styles;
        }
    }
}
