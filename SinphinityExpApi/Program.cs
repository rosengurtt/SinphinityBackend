
using SinphinityExpApi.Clients;
using SinphinityExpApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                                    .ReadFrom.Configuration(hostingContext.Configuration));


builder.Services.AddControllers();
builder.Services.AddHttpClient();

var baseUrl = builder.Configuration.GetSection("SinphinityUI:BaseUrl").Value;

builder.Services.Configure<AppConfiguration>(options =>
            {
                options.ProcMidiUrl = builder.Configuration.GetSection("ProcMidiUrl").Value;
                options.SysStoreUrl = builder.Configuration.GetSection("SysStoreUrl").Value;
                options.GraphApiUrl = builder.Configuration.GetSection("GraphApiUrl").Value;
                options.ProcMelodyAnalyserUrl = builder.Configuration.GetSection("ProcMelodyAnalyserUrl").Value;
            });

            builder.Services.AddSingleton<SysStoreClient>();
            builder.Services.AddSingleton<ProcMidiClient>();
            builder.Services.AddSingleton<GraphApiClient>();
            builder.Services.AddSingleton<ProcMelodyAnalyserClient>();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                       builder =>
                       {
                           builder.WithOrigins(baseUrl).AllowAnyHeader().AllowAnyMethod();
                       });
            });
			
			var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
;

app.UseCors();

app.UseRouting();

app.MapControllers();

app.Run();