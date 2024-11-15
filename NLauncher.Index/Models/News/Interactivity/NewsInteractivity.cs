using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.News.Interactivity;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ExploreNewsInteractivity), "explore")]
[JsonDerivedType(typeof(PlayNowNewsInteractivity), "play_now")]
public abstract class NewsInteractivity;
