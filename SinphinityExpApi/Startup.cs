using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SinphinityExpApi.Clients;
using SinphinityExpApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityExpApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpClient();

            services.Configure<AppConfiguration>(options =>
            {
                options.ProcMidiUrl = Configuration.GetSection("ProcMidiUrl").Value;
                options.SysStoreUrl = Configuration.GetSection("SysStoreUrl").Value;
                options.ProcPatternUrl = Configuration.GetSection("ProcPatternUrl").Value;
                options.GraphApiUrl = Configuration.GetSection("GraphApiUrl").Value;
            });

            services.AddSingleton<SysStoreClient>();
            services.AddSingleton<ProcMidiClient>();
            services.AddSingleton<ProcPatternClient>();
            services.AddSingleton<GraphApiClient>();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                       builder =>
                       {
                           builder.WithOrigins(Configuration.GetSection("SinphinityUI:BaseUrl").Value).AllowAnyHeader().AllowAnyMethod();
                       });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();
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
