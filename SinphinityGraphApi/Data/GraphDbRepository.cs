using Neo4j.Driver;
using SinphinityModel.Pattern;
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
            catch(Exception fdsfasdf)
            {

            }
       
        }
    }
}
