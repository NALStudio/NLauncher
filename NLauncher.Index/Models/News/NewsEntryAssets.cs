using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.News;
public class NewsEntryAssets
{
    public required Uri Background { get; init; }
    public required double BackgroundBrightness { get; init; }
    // ^^ Background Brightness requires a new index rebuild after every news asset change

    public required Uri Logo { get; init; }
}
