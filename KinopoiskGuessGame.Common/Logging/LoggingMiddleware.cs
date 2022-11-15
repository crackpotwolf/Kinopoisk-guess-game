using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;
using KinopoiskGuessGame.Common.Extensions;

namespace KinopoiskGuessGame.Common.Logging;

/// <summary>
/// Логирование начала выполнения запроса и его завершение
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public LoggingMiddleware(RequestDelegate next, ILoggerFactory logger)
    {
        _next = next;
        _logger = logger.CreateLogger<LoggingMiddleware>();
    }

    /// <summary>
    /// Запрос
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var req = context.GetInfoAboutRequest();
        _logger.LogInformation(req);

        try
        {
            await this._next(context);
        }
        catch (Exception ex)
        {
            // Произошло исключение на этапе выполнения запроса
            _logger.LogError($"{req}\n{GetInformationError(context, ex)}");
            ExceptionDispatchInfo.Capture(ex).Throw();
        }

        // Если не было исключения и запрос отработал нормально
        if (context.Response.StatusCode == 200)
        {
            _logger.LogInformation($"{req}\nЗапрос успешно завершен");
        }
        else
        {
            _logger.LogError($"{req}\n{GetInformationError(context)}");
        }
    }

    /// <summary>
    /// Ошибка получения информации
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    private string GetInformationError(HttpContext context, Exception ex = null)
    {
        var req = context.GetInfoAboutRequest();
        _logger.LogInformation(req);

        if (ex == null)
        {
            return $"{req}\nЗапрос завершился с [{context.Response.StatusCode}] кодом ошибки.";
        }
        else
        {
            return $"{req}\nЗапрос завершился с исключением. Информация об ошибке:\n{ex}";
        }
    }
}