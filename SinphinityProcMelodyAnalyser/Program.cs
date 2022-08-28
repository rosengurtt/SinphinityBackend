using Serilog;
using SinphinityProcMelodyAnalyser.Clients;
using SinphinityProcMelodyAnalyser.MelodyLogic;
using SinphinityProcMelodyAnalyser.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                                    .ReadFrom.Configuration(hostingContext.Configuration));


builder.Services.Configure<AppConfiguration>(options =>
{
    options.SysStoreUrl = builder.Configuration.GetSection("SysStoreUrl").Value;
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<SysStoreClient>();
builder.Services.AddSingleton<Main>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
