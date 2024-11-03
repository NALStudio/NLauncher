using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.AnsiFormatter;

/// <summary>
/// <see cref="AnsiConsole"/> formatters for IndexManager commands.
/// </summary>
internal static class AnsiFormatter
{
    public static void WriteHeader(string text)
    {
        AnsiConsole.Write(new Rule(text).HeavyBorder());
    }

    public static void WriteSectionTitle(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule(text).LeftJustified());
    }
}
