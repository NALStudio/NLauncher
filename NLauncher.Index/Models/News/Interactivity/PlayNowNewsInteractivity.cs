using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.News.Interactivity;
public class PlayNowNewsInteractivity : NewsInteractivity
{
    public required Guid AppId { get; set; }
}
