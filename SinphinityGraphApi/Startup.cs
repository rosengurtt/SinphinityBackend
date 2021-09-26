using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neo4j.Driver;
using SinphinityGraphApi.Clients;
using SinphinityGraphApi.Data;
using SinphinityGraphApi.Models;
using System;

namespace SinphinityGraphApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpClient();
            var neo4jUrl = Configuration.GetSection("neo4j:url").Value;
            var neo4jUsername = Configuration.GetSection("neo4j:username").Value;
            var neo4jPassword = Configuration.GetSection("neo4j:password").Value;
            var driver = GraphDatabase.Driver(neo4jUrl, AuthTokens.Basic(neo4jUsername, neo4jPassword));
            services.AddSingleton<IDriver>(driver);
            services.Configure<AppConfiguration>(options =>
            {
                options.SysStoreUrl = Configuration.GetSection("SysStoreUrl").Value;
            });

            services.AddSingleton<SysStoreClient>();
            services.AddSingleton<GraphDbRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
