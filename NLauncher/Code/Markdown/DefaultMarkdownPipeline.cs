using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace NLauncher.Code.Markdown;

public static class DefaultMarkdownPipeline
{
    private static MarkdownPipeline? _pipeline = null;
    public static MarkdownPipeline Instance => _pipeline ??= BuildDefaultPipeline();

    private static MarkdownPipeline BuildDefaultPipeline()
    {
        return new MarkdownPipelineBuilder()
            // Disable HTML for safe parsing
            // This does limit our styling options, but MudBlazor.Markdown doesn't support them anyways...
            .DisableHtml()

            // Strikethrough is supported by VSCode markdown preview so I guess that would be a nice to have
            // I don't care about the other emphasis extras since VSCode doesn't support them.
            .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
            .Build();
    }
}
