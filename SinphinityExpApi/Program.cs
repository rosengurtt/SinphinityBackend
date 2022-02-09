
using SinphinityExpApi.Clients;
using SinphinityExpApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddHttpClient();

var baseUrl = builder.Configuration.GetSection("SinphinityUI:BaseUrl").Value;

builder.Services.Configure<AppConfiguration>(options =>
            {
                options.ProcMidiUrl = builder.Configuration.GetSection("ProcMidiUrl").Value;
                options.SysStoreUrl = builder.Configuration.GetSection("SysStoreUrl").Value;
                options.ProcPatternUrl = builder.Configuration.GetSection("ProcPatternUrl").Value;
                options.GraphApiUrl = builder.Configuration.GetSection("GraphApiUrl").Value;
            });

            builder.Services.AddSingleton<SysStoreClient>();
            builder.Services.AddSingleton<ProcMidiClient>();
            builder.Services.AddSingleton<ProcPatternClient>();
            builder.Services.AddSingleton<GraphApiClient>();
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