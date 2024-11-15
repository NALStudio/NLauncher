using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Index.Models.News;
using NLauncher.IndexManager.Components.Paths;
using NLauncher.IndexManager.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.PromptUtils;
internal static class NewsSlotPrompt
{
    private readonly record struct NewsSlot(int Index, bool IsEmpty, NewsManifest? Manifest);


    public const string DefaultTitle = "Choose News Slot";

    /// <summary>
    /// Returns the index of the chosen slot or <c>-1</c> if no slots are available with the given filters.
    /// </summary>
    public static async Task<int> AskNewsSlot(IndexPaths paths, string title = DefaultTitle, bool allowEmpty = true, bool allowReserved = true)
    {
        SelectionPrompt<NewsSlot> prompt =
            new SelectionPrompt<NewsSlot>()
                .Title(title)
                .UseConverter(static s => $"{s.Index}: " + (s.IsEmpty ? "<empty>" : ManifestDisplayName(s.Manifest)));

        int choiceCount = 0;
        await foreach (NewsSlot slot in LoadSlots(paths))
        {
            bool allowSelect = true;
            if (slot.IsEmpty && !allowEmpty)
                allowSelect = false;
            if (!slot.IsEmpty && !allowReserved)
                allowSelect = false;

            if (allowSelect)
            {
                prompt.AddChoice(slot);
                choiceCount++;
            }
            else
            {
                prompt.AddChoiceGroup(slot);
            }
        }

        // Only show prompt if there are actually choices that we can pick.
        if (choiceCount > 0)
            return AnsiConsole.Prompt(prompt).Index;
        else
            return -1;
    }

    /// <summary>
    /// Returns the path to the chosen slot or <see langword="null"/> if no slots are available with the given filters.
    /// </summary>
    public static async Task<NewsPaths?> AskNewsPath(IndexPaths paths, string title = DefaultTitle, bool allowEmpty = true, bool allowReserved = true)
    {
        int slot = await AskNewsSlot(paths: paths, title: title, allowEmpty: allowEmpty, allowReserved: allowReserved);
        if (slot == -1)
            return null;
        else
            return NewsSlots.GetPath(paths, slot);
    }

    public static void PrintNoSlotsAvailable()
    {
        AnsiFormatter.WriteError("No slots available.");
    }

    private static async IAsyncEnumerable<NewsSlot> LoadSlots(IndexPaths index)
    {
        foreach ((int i, NewsPaths paths) in NewsSlots.EnumeratePathIndexes(index))
        {
            bool isEmpty = !paths.Exists();

            NewsManifest? manifest = null;
            if (!isEmpty)
            {
                string manifestJson = await File.ReadAllTextAsync(paths.NewsFile);
                manifest = IndexJsonSerializer.Deserialize<NewsManifest>(manifestJson);
            }

            yield return new NewsSlot()
            {
                Index = i,
                IsEmpty = isEmpty,
                Manifest = manifest
            };
        }
    }

    private static string ManifestDisplayName(NewsManifest? manifest)
    {
        if (manifest is null)
            return string.Empty;

        const int maxTextLength = 64;

        string text = manifest.Text;
        if (text.Length > maxTextLength)
            text = text[..maxTextLength] + "...";

        return $"{manifest.Title} ({text})";
    }
}
