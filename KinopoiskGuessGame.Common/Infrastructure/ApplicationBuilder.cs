using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using KinopoiskGuessGame.Common.Logging;
using KinopoiskGuessGame.Common.Extensions;
using KinopoiskGuessGame.Data;

namespace KinopoiskGuessGame.Common.Infrastructure;

/// <summary>
/// ApplicationBuilder
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// Использование базовых сервисов 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="provider"></param>
    /// <param name="env"></param>
    public static void UseBaseServices(this IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        #region Middleware

        app.UseMiddleware<LoggingMiddleware>();

        #endregion

        #region Swagger

        app.UseSwaggerService(provider);

        #endregion

        app.UseCors(builder =>
        {
            builder.AllowAnyOrigin();
            builder.AllowAnyMethod();
            builder.AllowAnyHeader();
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller:slugify}/{action:slugify}/{id?}");
        });
    }

    /// <summary>
    /// Применение миграций БД
    /// </summary>
    /// <param name="app"></param>
    public static void MigrateDatabase(this IApplicationBuilder app,
        ILogger logger)
    {
        logger.LogInformation("Начало миграций");

        try
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<KinopoiskContext>().Database.Migrate();
            }

            logger.LogInformation("Миграции успешно завершены");
        }
        catch (Exception ex)
        {
            logger.LogError($"Во время миграций произошла ошибка: {ex}");
        }
    }

    /// <summary>
    /// Внедрнение автогенерируемой документации API - Swagger
    /// </summary>
    /// <param name="app"></param>
    /// <param name="provider"></param>
    private static void UseSwaggerService(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
    {
        var isDevelopment = app.ApplicationServices.GetService<IWebHostEnvironment>().IsDevelopment();

        var assemblyName = Assembly.GetEntryAssembly()?.GetName().GetNameDashCase();
        var prefixApi = isDevelopment ? "" : $"/api/{assemblyName}";

        app.UseSwagger();

        app.UseSwaggerUI(
            options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"{prefixApi}/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                options.DocExpansion(DocExpansion.None);
            });
    }
}