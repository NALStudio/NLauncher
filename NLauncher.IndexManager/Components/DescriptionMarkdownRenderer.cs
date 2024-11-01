using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            .Build();
    }
}
