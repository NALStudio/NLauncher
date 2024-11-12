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
            // !! DANGEROUS !!
            // HTML is disabled since it was too limited (we couldn't have comments and/or colored text)
            // This is unsafe which doesn't matter currently as this is a hobby project and we download our markdown from a trusted source.
            // But consider changing this in the future.
            // .DisableHtml()

            // Strikethrough is supported by VSCode markdown preview so I guess that would be a nice to have
            // I don't care about the other emphasis extras since VSCode doesn't support them.
            .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
            .Build();
    }
}
