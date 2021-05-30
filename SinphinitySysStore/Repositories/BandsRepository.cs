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

        public async Task<long> GetBandsCountAsync(string contains = ".*", string styleId=null)
        {
            return await _bandsCollection.CountDocumentsAsync(Builders<Band>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i"));
        }

        public async Task<IReadOnlyList<Band>> GetBandsAsync(int pageSize = DefaultPageSize, int page = 0, string contains = ".*", string styleId = null,
       string sort = DefaultSortKey, int sortDirection = DefaultSortOrder,
       CancellationToken cancellationToken = default)
        {
            var skip = pageSize * page;
            var limit = pageSize;


            var sortFilter = new BsonDocument(sort, sortDirection);
            var nameFilter = Builders<Band>.Filter.Regex(s => s.Name, @$"/.*{contains}.*/i");
            var styleFilter = Builders<Band>.Filter.Eq(x => x.Style.Id, styleId);
            var combineFilter = string.IsNullOrEmpty(styleId)? nameFilter: Builders<Band>.Filter.And(nameFilter, styleFilter);
            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            };

            var bands = await _bandsCollection
                .Find(combineFilter, options)
                .Sort(sortFilter)
                .Limit(limit)
                .Skip(skip)
                .ToListAsync(cancellationToken);

            return bands;
        }
    }
}
