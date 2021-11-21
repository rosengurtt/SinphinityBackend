using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Sinphinity.Models;
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

        public async Task<(int, List<Pattern>)> GetPatternsAsync(int pageNo, int pageSize, string styleId, string bandId, string songInfoId)
        {
            var bandFilter = bandId == null ? Builders<PatternSong>.Filter.Exists(x => x.BandId) : Builders<PatternSong>.Filter.Eq(x => x.BandId, bandId);
            var songFilter = songInfoId == null ? Builders<PatternSong>.Filter.Exists(x => x.SongInfoId) : Builders<PatternSong>.Filter.Eq(x => x.SongInfoId, songInfoId);
            var styleFilter = styleId == null ? Builders<PatternSong>.Filter.Exists(x => x.StyleId) : Builders<PatternSong>.Filter.Eq(x => x.StyleId, styleId);
            var combineFilter = Builders<PatternSong>.Filter.And(bandFilter, styleFilter, songFilter);

            var patsSongs = await _patternSongCollection
                .Find(combineFilter)
                .ToListAsync();

            var pats = patsSongs.Select(x => new Pattern { Id = x.PatternId, AsString = x.PatternAsString })
                .Distinct()
                .OrderBy(x => x.AsString)
                .Skip(pageSize * pageNo)
                .Take(pageSize)
                .ToList();
            var retObj = new List<Pattern>();
            foreach (var p in pats)
            {
                var filter = Builders<Pattern>.Filter.Eq(x => x.Id, p.Id);
                var pat = await _patternsCollection.Find(filter).FirstOrDefaultAsync();
                retObj.Add(pat);
            }

            return (pats.Count(), retObj);
        }




        public async Task<IReadOnlyList<PatternSong>> GetPatternsOfSongAsync(string songInfoId)
        {
            var filter = Builders<PatternSong>.Filter.Eq(x => x.SongInfoId, songInfoId);

            var patternsSong = await _patternSongCollection
                .Find(filter)
                .ToListAsync();

            return patternsSong;
        }
        public async Task<long> GetPatternsSongsCountAsync(string contains = ".*")
        {
            var nameFilter = Builders<PatternSong>.Filter.Regex(s => s.PatternAsString, @$"/.*{contains}.*/i");
            return await _patternSongCollection.Find(nameFilter).CountDocumentsAsync();
        }
        public async Task<IReadOnlyList<PatternSong>> GetPatternsSongsAsync(int pageSize, int page, string contains)
        {

            var patternsSong = await _patternSongCollection
                .Find(_ => true)
                .SortBy(e => e.PatternAsString)
                .ThenBy(x=>x.SongInfoId)
                .Limit(pageSize)
                .Skip(pageSize * page)
                .ToListAsync();

            return patternsSong;
        }

        public async Task InsertPatternsOfSongAsync(Dictionary<string, HashSet<Occurrence>> patterns, SongInfo song)
        {

                foreach (var pat in patterns)
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
                    var songFilter = Builders<PatternSong>.Filter.Eq(x => x.SongInfoId, song.Id);
                    var combineFilter = Builders<PatternSong>.Filter.And(paternFilter, songFilter);
                    var patSong = await _patternSongCollection.Find(combineFilter).FirstOrDefaultAsync();
                    if (patSong == null)
                    {
                        var ps = new PatternSong()
                        {
                            PatternId = patInDb.Id,
                            PatternAsString = patternAsString,
                            SongInfoId = song.Id,
                            BandId = song.Band.Id,
                            StyleId = song.Style.Id
                        };
                        await _patternSongCollection.InsertOneAsync(ps);
                    }

                    // Insert occurrences
                    foreach (var occ in pat.Value)
                    {
                        var occurrence = new PatternOccurrence
                        {
                            PatternId = patInDb.Id,
                            SongInfoId = song.Id,
                            Voice = occ.Voice,
                            BarNumber = (int)occ.BarNumber,
                            Beat = (int)occ.Beat,
                            Tick=occ.Tick
                        };
                        await _patternOccurrencesCollection.InsertOneAsync(occurrence);
                    }
            }
        }

 

    }
}
