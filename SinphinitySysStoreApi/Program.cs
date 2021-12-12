using Microsoft.EntityFrameworkCore;
using SinphinitySysStore.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var connection = builder.Configuration.GetConnectionString("Sinphinity");
builder.Services.AddDbContext<SinphinityDbContext>(options => options.UseSqlServer(connection,
    opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds)));
builder.Services.AddTransient<StylesRepository>();
builder.Services.AddTransient<BandsRepository>();
builder.Services.AddTransient<SongsRepository>();

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
