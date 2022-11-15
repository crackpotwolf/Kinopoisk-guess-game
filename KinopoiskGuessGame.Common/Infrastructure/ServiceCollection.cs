using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Text.RegularExpressions;
using KinopoiskGuessGame.Common.Swagger;
using KinopoiskGuessGame.Data;
using KinopoiskGuessGame.Data.Interfaces.Repositories;
using KinopoiskGuessGame.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace KinopoiskGuessGame.Common.Infrastructure;

/// <summary>
/// ServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Базовая инициализация сервисов для модулей
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString">Строка подключения к БД</param>
    public static void AddBaseModuleDI(this IServiceCollection services, string connectionString)
    {
        // DB
        services.AddContext(connectionString);

        // CORS
        services.AddCors();

        // Routes
        services.AddControllersWithNewtonsoft();

        // Swagger docs
        services.AddSwagger();

        // Other services (Email, UserManager, etc.)
        services.AddServices();
    }

    /// <summary>
    /// Подключение Context
    /// </summary>s
    /// <param name="services"></param>
    /// <param name="connectionString">Строка подключения к БД</param>
    private static void AddContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<KinopoiskContext>(options =>
        {
            options.UseNpgsql(connectionString,
                b =>
                {
                    b.MigrationsAssembly("KinopoiskGuessGame.Data");
                });
        });
    }

    /// <summary>
    /// Внедрение контроллеров и сериализацию Newtonsoft
    /// </summary>
    /// <param name="services"></param>
    private static void AddControllersWithNewtonsoft(this IServiceCollection services)
    {
        services.AddControllers(o =>
            {
                o.Conventions.Add(new ControllerDocumentationConvention());
                o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            })
            .AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.Converters.Add(new StringEnumConverter
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                });
                o.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            });
    }

    /// <summary>
    /// Название Controller
    /// </summary>
    private class ControllerDocumentationConvention : IControllerModelConvention
    {
        /// <inheritdoc />
        void IControllerModelConvention.Apply(ControllerModel controller)
        {
            if (controller == null)
                return;

            foreach (var attribute in controller.Attributes)
            {
                if (attribute.GetType() != typeof(DisplayNameAttribute)) continue;
                var routeAttribute = (DisplayNameAttribute)attribute;
                if (!string.IsNullOrWhiteSpace(routeAttribute.DisplayName))
                    controller.ControllerName = routeAttribute.DisplayName;
            }
        }
    }

    /// <summary>
    /// Трансформатор пути запрос, пример: AccountSettings -> account-settings
    /// </summary>
    private class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        /// <inheritdoc />
        public string TransformOutbound(object value)
        {
            // Slugify value
            return value == null ? null : Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
    
    /// <summary>
    /// Внедрение автогенерируемой документации Swagger
    /// </summary>
    /// <param name="services"></param>
    public static void AddSwagger(this IServiceCollection services)
    {
        #region Swagger

        services.AddApiVersioning(
            options =>
            {
                // options.ReportApiVersions = true;
            });

        services.AddVersionedApiExplorer(
            options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddSwaggerGenNewtonsoftSupport();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile, true));
            xmlFiles.ForEach(xmlFile => c.SchemaFilter<EnumTypesSchemaFilter>(xmlFile));

            c.DocumentFilter<EnumTypesDocumentFilter>();

            c.EnableAnnotations();

            c.OperationFilter<SwaggerDefaultValues>();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
        });

        #endregion
    }

    /// <summary>
    /// Внедрение сервисов
    /// </summary>
    /// <param name="services"></param>
    private static void AddServices(this IServiceCollection services)
    {
        // Repositories
        services.AddTransient(typeof(IBaseEntityRepository<>), typeof(BaseEntityRepository<>));
    }
}