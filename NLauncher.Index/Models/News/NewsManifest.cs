using NLauncher.Index.Models.News.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.News;
public class NewsManifest
{
    public required string Title { get; init; }
    public required string Text { get; init; }
    public required NewsInteractivity Interactivity { get; init; }
}
