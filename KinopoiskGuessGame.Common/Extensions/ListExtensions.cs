namespace KinopoiskGuessGame.Common.Extensions;

/// <summary>
/// List расширения
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Словарь символов
    /// </summary>
    private static Dictionary<string, string> ServicSymbols = new Dictionary<string, string>()
    {
        { @"\", @"\\" },
        { "\0", @"\\0" },
        { "\a", @"\\a"  },
        { "\b", @"\\b"  },
        { "\f", @"\\f"  },
        { "\n", @"\\n"  },
        { "\r", @"\\r"  },
        { "\t", @"\\t"  },
        { "\v", @"\\v"  },
    };
    
    /// <summary>
    /// Объединяет список значений в строку
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string Join<T>(this IEnumerable<T> list, string separator)
    {
        // Замена спец-символов
        var newList = list.Escaping();
        return string.Join(separator, newList);
    }

    /// <summary>
    /// Объединяет список значений в строку
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string Join<T>(this IEnumerable<T> list, char separator)
    {
        // Замена спец-символов
        var newList = list.Escaping();
        return string.Join(separator, newList);
    }

    /// <summary>
    /// Экранирование спец-символов
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">Список объектов</param>
    /// <returns></returns>
    private static List<string?> Escaping<T>(this IEnumerable<T> list)
    {
        return list.Select(p =>
        {
            var s = p?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return s;

            foreach (var item in ServicSymbols)
            {
                s = s.Replace(item.Key, item.Value);
            }
            return s;
        }).ToList();
    }
}