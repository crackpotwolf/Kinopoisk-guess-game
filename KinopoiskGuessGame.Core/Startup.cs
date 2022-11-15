using AutoMapper;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using KinopoiskGuessGame.Common.Infrastructure;

namespace KinopoiskGuessGame.Core;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        #region Базовая инициализация DI

        services.AddBaseModuleDI(Configuration.GetConnectionString("DefaultConnection"));

        #endregion
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app,
        IApiVersionDescriptionProvider provider,
        IWebHostEnvironment env,
        ILogger<Startup> logger)
    {
        app.UseSentryTracing();
        
        app.MigrateDatabase(logger);

        app.UseBaseServices(env, provider);
    }
}