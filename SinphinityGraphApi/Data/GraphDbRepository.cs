using Neo4j.Driver;
using Serilog;
using Sinphinity.Models;
using SinphinityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityGraphApi.Data
{
    public class GraphDbRepository
    {
        private IDriver _driver;

        public GraphDbRepository(IDriver driver)
        {
            _driver = driver;
        }


        public async Task AddPattern(PatternSong patternSong)
        {
            try
            {
                IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
                var command = @$"MERGE (p:Pattern {{AsString: '{patternSong.PatternAsString}', id: '{patternSong.PatternId}'}}) 
                                 MERGE (s: Song {{id: '{patternSong.Song.Id}', songName: '{patternSong.Song.Name}', band: '{patternSong.Band.Name}', style:'{patternSong.Style.Name}'}})
                                 MERGE (p) -[:IsUsedInSong]-> (s)";

                var cursor = await session.RunAsync(command);
                await cursor.ConsumeAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Thrown an exception when trying to add pattern {patternSong.PatternAsString}");
                throw;
            }
        }
        public async Task<List<Pattern>> GetPatternsOfSong(long songId)
        {

            var retObj = new List<Pattern>();
            try
            {
                IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
                var command = @$"MATCH (:Song {{songId:{songId} }})<-[:USED_IN]-(p:Pattern) RETURN p";

                var cursor = await session.RunAsync(command);
                while (await cursor.FetchAsync())
                {
                    var node = cursor.Current["p"].As<INode>();
                    retObj.Add(new Pattern { Id = (long)node.Properties["patternId"], AsString = (string)node.Properties["asString"] });
                }
                return retObj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Thrown an exception when trying to retrieve patterns for song with id {songId}");
                throw;
            }
        }

        public async Task<List<Song>> GetUsageOfPattern(string asString)
        {

            var retObj = new List<Song>();
            try
            {
                IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
                var command = @$"MATCH (s:Song)<-[:USED_IN]-(:Pattern {{asString: ""{asString}""}}) RETURN s";

                var cursor = await session.RunAsync(command);
                while (await cursor.FetchAsync())
                {
                    var node = cursor.Current["s"].As<INode>();
                    retObj.Add(new Song { Id = (long)node.Properties["songId"], Name = (string)node.Properties["name"] });
                }
                return retObj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Thrown an exception when trying to retrieve usage of pattern {asString}");
                throw;
            }
        }
        public async Task<List<Pattern>> GetApplicationsOfBasicPattern(string asString)
        {

            var retObj = new List<Pattern>();
            try
            {
                IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
                var command = @$"MATCH (p:Pattern)<-[:USED_AS]-(:BasicPattern {{asString: ""{asString}""}}) RETURN p";

                var cursor = await session.RunAsync(command);
                while (await cursor.FetchAsync())
                {
                    var node = cursor.Current["p"].As<INode>();
                    retObj.Add(new Pattern { Id = (long)node.Properties["patternId"], AsString = (string)node.Properties["asString"] });
                }
                return retObj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Thrown an exception when trying to retrieve applications of basic pattern  {asString}");
                throw;
            }
        }

        public async Task<List<(Pattern, long)>> GePatternsOfBand(long bandId, int? numberOfNotes, int? step, int? range, bool? isMonotone, int? durationInTicks)
        {

            var retObj = new List<(Pattern, long)>();
            try
            {
                IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
                var filter = GetFilter(numberOfNotes, step, range, isMonotone, durationInTicks);
                var command = @$"MATCH (p:Pattern {filter})-[:USED_IN]->(:Song)-[:COMPOSED_BY]->(:Band {{bandId: {bandId}}}) 
RETURN p.asString AS AsString, p.patternId AS PatternId, count(p) as Quant
ORDER BY Quant DESC";

                var cursor = await session.RunAsync(command);
                while (await cursor.FetchAsync())
                {
                    var node = cursor.Current;
                    var pato = new Pattern((string)node["AsString"]);
                    pato.Id = (long)node["PatternId"];
                    retObj.Add((pato, (long)node["Quant"]));
                }
                return retObj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Thrown an exception when trying to retrieve patterns for band with id  {bandId}");
                throw;
            }
        }
        public async Task<List<(Band, long)>> GetSimilarityMatrixForBand(long bandId)
        {
            var retObj = new List<(Band, long)>();
            try
            {
                IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
                var command = @$"MATCH (b:Band)-[:COMPOSED_BY]-(:Song)-[:USED_IN]-(p:Pattern)-[:USED_IN]->(:Song)-[:COMPOSED_BY]->(:Band {{bandId: {bandId}}}) 
RETURN b.name AS name, b.bandId AS bandId, count(p) AS patterns
ORDER BY patterns DESC";

                var cursor = await session.RunAsync(command);
                while (await cursor.FetchAsync())
                {
                    var node = cursor.Current;
                    var banda = new Band { Name = (string)node["name"], Id = (long)node["bandId"] };
                    var quant= (long)node["patterns"];
                    retObj.Add((banda,quant));
                }
                return retObj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Thrown an exception when trying to retrieve similarity matrix for band with id  {bandId}");
                throw;
            }
        }

        public async Task<List<(Band, long)>> GetSimilarityMatrixForSong(long songId)
        {
            var retObj = new List<(Band, long)>();
            try
            {
                IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
                var command = @$"MATCH (b:Band)-[:COMPOSED_BY]-(:Song)-[:USED_IN]-(p:Pattern)-[:USED_IN]->(:Song {{songId: {songId}}}) 
RETURN b.name AS name, b.bandId AS bandId, count(p) AS patterns
ORDER BY patterns DESC";

                var cursor = await session.RunAsync(command);
                while (await cursor.FetchAsync())
                {
                    var node = cursor.Current;
                    var banda = new Band { Name = (string)node["name"], Id = (long)node["bandId"] };
                    var quant = (long)node["patterns"];
                    retObj.Add((banda, quant));
                }
                return retObj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Thrown an exception when trying to retrieve similarity matrix for song with id  {songId}");
                throw;
            }
        }


        private string GetFilter(int? numberOfNotes, int? step, int? range, bool? isMonotone, int? durationInTicks)
        {
            if (numberOfNotes == null && step == null && range == null && isMonotone == null && durationInTicks == null)
                return "";
            var retObj = "{";
            if (numberOfNotes != null)
                retObj += $"numberOfNotes: {numberOfNotes},";
            if (step != null)
                retObj += $"step: {step},";
            if (isMonotone != null)
                retObj += $"isMonotone: '{isMonotone}',";
            if (durationInTicks != null)
                retObj += $"durationInTicks: {durationInTicks},";
            retObj = retObj.Remove(retObj.Length - 1);
            return retObj += "}";
        }

    }
}