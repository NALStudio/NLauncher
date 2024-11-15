using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.Paths;
internal sealed class NewsPaths : DirectoryPathProvider
{
    public NewsPaths(string directory) : base(directory)
    {
        BackgroundImageFile = Path.Join(directory, "background.png");
        LogoImageFile = Path.Join(directory, "logo.png");
        NewsFile = Path.Join(directory, "news.json");
    }

    public string BackgroundImageFile { get; }
    public string LogoImageFile { get; }
    public string NewsFile { get; }

    /// <summary>
    /// Returns <see langword="true"/> if slot is reserved or <see langword="false"/> otherwise.
    /// </summary>
    public bool Exists()
    {
        return System.IO.Directory.Exists(Directory);
    }
}