using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityGraphApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatternsController : ControllerBase
    {
        private IDriver _driver;

        public PatternsController(IDriver driver)
        {
            _driver = driver;
        }

        [HttpGet("import")]
        public async Task<ActionResult> ImportPatterns()
        {
            IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
            var cursor = await session.RunAsync($"MATCH (n) RETURN n");
            if (await cursor.FetchAsync())
            {
                var patternNode = cursor.Current.Values.FirstOrDefault().Value as INode;

                await cursor.ConsumeAsync();
                //var updateCommand = @$"MATCH (p)
                //                           WHERE ID(p) = {patternNode.Id}
                //                           MATCH (ss)
                //                           WHERE ID(ss) = {songSimplificationId}
                //                           CREATE (ss)-[:HasPattern {{Voice: {voice}, Tick: {tick}}}]->(p)";
                //cursor = await session.RunAsync(updateCommand);
                //await cursor.ConsumeAsync();
            }

            return Ok("Hola papi");
        }
    }
}
