using Neo4j.Driver;
using SinphinityGraphApi.Clients;
using SinphinityGraphApi.Data;
using SinphinityGraphApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                                    .ReadFrom.Configuration(hostingContext.Configuration));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
            var neo4jUrl = builder.Configuration.GetSection("neo4j:url").Value;
            var neo4jUsername = builder.Configuration.GetSection("neo4j:username").Value;
            var neo4jPassword = builder.Configuration.GetSection("neo4j:password").Value;
            var driver = GraphDatabase.Driver(neo4jUrl, AuthTokens.Basic(neo4jUsername, neo4jPassword));
            builder.Services.AddSingleton<IDriver>(driver);
            builder.Services.Configure<AppConfiguration>(options =>
            {
                options.SysStoreUrl = builder.Configuration.GetSection("SysStoreUrl").Value;
            });

            builder.Services.AddSingleton<SysStoreClient>();
            builder.Services.AddSingleton<GraphDbRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
;

app.UseRouting();

app.MapControllers();

app.Run();