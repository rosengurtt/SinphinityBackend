using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Sinphinity.Models.Pattern;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Repositories
{
    public class PatternsRepository
    {
        private const int DefaultPageSize = 10;
        private const string DefaultSortKey = "name";
        private const int DefaultSortOrder = -1;
        private readonly IMongoCollection<Pattern> _patternsCollection;
        private readonly IMongoCollection<PatternOccurrence> _patternOccurrencessCollection;
        private readonly IMongoClient _mongoClient;

        public PatternsRepository(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

            _patternsCollection = mongoClient.GetDatabase("sinphinity").GetCollection<Pattern>("patterns");
            _patternOccurrencessCollection = mongoClient.GetDatabase("sinphinity").GetCollection<PatternOccurrence>("patternOccurrences");
        }


        public async Task InsertPatternsOfSongAsync(PatternMatrix patternMatrix)
        {

            for (int i = 0; i < patternMatrix.PatternsOfNnotes.Count; i++)
            {
                if (patternMatrix.PatternsOfNnotes[i] == null)
                    continue;
                foreach (var pat in patternMatrix.PatternsOfNnotes[i])
                {
                    var patternAsString = pat.Key;
                    var filter = Builders<Pattern>.Filter.Eq(s => s.AsString, patternAsString);
                    var patInDb = await _patternsCollection.Find(filter).FirstOrDefaultAsync();
                    if (patInDb == null)
                    {
                        var paton = new Pattern(patternAsString);
                        await _patternsCollection.InsertOneAsync(paton);
                        patInDb = paton;
                    }
                    foreach (var occ in pat.Value)
                    {
                        var occurrence = new PatternOccurrence
                        {
                            PatternId = patInDb.Id,
                            SongInfoId = occ.SongId,
                            Voice = occ.Voice,
                            BarNumber = (int)occ.BarNumber,
                            Beat = (int)occ.Beat
                        };
                        await _patternOccurrencessCollection.InsertOneAsync(occurrence);
                    }
                }
            }
        }

 

    }
}
