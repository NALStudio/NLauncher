using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace NLauncher.IndexManager.Components;
internal static class DescriptionMarkdownRenderer
{
    private static MarkdownPipeline? pipeline;

    public static string RenderDescription(string markdown)
    {
        pipeline ??= CreateDefaultPipeline();
        return Markdown.ToHtml(markdown, pipeline);
    }

    private static MarkdownPipeline CreateDefaultPipeline()
    {
        return new MarkdownPipelineBuilder()
            .DisableHtml()
            // Strikethrough is supported by VSCode markdown preview so I guess that would be a nice to have
            // I don't care about the other emphasis extras since VSCode doesn't support them.
            .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
            .Build();
    }
}
