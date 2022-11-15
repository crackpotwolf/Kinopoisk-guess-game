using System.Reflection;

namespace KinopoiskGuessGame.Common.Extensions;

public static class AssemblyExtensions
{
    /// <summary>
    /// Получение имя сборки, разделенной тире
    /// Например: 
    /// AssemblyExtensions -> assembly-extensions
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetNameDashCase(this AssemblyName assembly)
    {
        var assemblyName = assembly.Name;

        assemblyName = assemblyName.ToLower().Replace(".", "-");

        return assemblyName;
    }
}