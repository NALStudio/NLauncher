using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components;

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

    public static void WriteError(string msg)
    {
        AnsiConsole.MarkupLine($"[red]{msg.EscapeMarkup()}[/]");
    }

    public static void WriteOperationCancelled() => WriteWarning("Operation was cancelled by the user.");
    public static void WriteWarning(string msg)
    {
        AnsiConsole.MarkupLine($"[yellow]{msg.EscapeMarkup()}[/]");
    }

    public static TextPath DeletedFile(string pathToFileOrDirectory) => ColoredPath(pathToFileOrDirectory, Color.Yellow).LeafColor(Color.Red);
    public static TextPath CreatedFile(string pathToFileOrDirectory) => ColoredPath(pathToFileOrDirectory, Color.Yellow).LeafColor(Color.Green);

    public static TextPath ColoredPath(string path, Color color)
    {
        return new TextPath(path)
            .LeafColor(color)
            .RootColor(color)
            .SeparatorColor(color)
            .StemColor(color);
    }
}
