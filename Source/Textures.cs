using UnityEngine;
using Verse;

namespace CraftWithColor {
    [StaticConstructorOnStartup]
    public static class Textures {
        public static readonly Texture2D Random = ContentFinder<Texture2D>.Get("Random");
    }
}
