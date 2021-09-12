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
        private readonly IMongoCollection<PatternOccurrence> _patternOccurrencesCollection;
        private readonly IMongoCollection<PatternSong> _patternSongCollection;
        private readonly IMongoClient _mongoClient;

        public PatternsRepository(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

            _patternsCollection = mongoClient.GetDatabase("sinphinity").GetCollection<Pattern>("patterns");
            _patternOccurrencesCollection = mongoClient.GetDatabase("sinphinity").GetCollection<PatternOccurrence>("patternOccurrences");
            _patternSongCollection = mongoClient.GetDatabase("sinphinity").GetCollection<PatternSong>("patternsSongs");

        }

        public async Task<IReadOnlyList<Pattern>> GetPatternsAsync(int pageSize, int page)
        {
            var sortFilter = new BsonDocument("asString", -1);

            var patterns = await _patternsCollection
                .Find(_ => true)
                .Sort(sortFilter)
                .Limit(pageSize)
                .Skip(pageSize * page)
                .ToListAsync();

            return patterns;
        }

        public async Task<IReadOnlyList<PatternSong>> GetPatternsOfSongAsync(string songInfoId)
        {
            var filter = Builders<PatternSong>.Filter.Eq(x => x.SongInfoId, songInfoId);

            var patternsSong = await _patternSongCollection
                .Find(filter)
                .ToListAsync();

            return patternsSong;
        }

        public async Task InsertPatternsOfSongAsync(PatternMatrix patternMatrix)
        {

            for (int i = 0; i < patternMatrix.PatternsOfNnotes.Count; i++)
            {
                if (patternMatrix.PatternsOfNnotes[i] == null)
                    continue;
                foreach (var pat in patternMatrix.PatternsOfNnotes[i])
                {
                    // Insert pattern if it is not in collection already
                    var patternAsString = pat.Key;
                    var filter = Builders<Pattern>.Filter.Eq(s => s.AsString, patternAsString);
                    var patInDb = await _patternsCollection.Find(filter).FirstOrDefaultAsync();
                    if (patInDb == null)
                    {
                        var paton = new Pattern(patternAsString);
                        await _patternsCollection.InsertOneAsync(paton);
                        patInDb = paton;
                    }

                    // Insert record to PatternsSongs if not there
                    var paternFilter = Builders<PatternSong>.Filter.Eq(x => x.PatternAsString, patternAsString);
                    var songFilter = Builders<PatternSong>.Filter.Eq(x => x.SongInfoId, patternMatrix.SongId);
                    var combineFilter = Builders<PatternSong>.Filter.And(paternFilter, songFilter);
                    var patSong = await _patternSongCollection.Find(combineFilter).FirstOrDefaultAsync();
                    if (patSong == null)
                    {
                        var ps = new PatternSong() { PatternId=patInDb.Id,  PatternAsString = patternAsString, SongInfoId = patternMatrix.SongId };
                        await _patternSongCollection.InsertOneAsync(ps);
                    }

                    // Insert occurrences
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
                        await _patternOccurrencesCollection.InsertOneAsync(occurrence);
                    }
                }
            }
        }

 

    }
}
