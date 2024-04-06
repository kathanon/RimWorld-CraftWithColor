using UnityEngine;
using Verse;

namespace CraftWithColor {
    [StaticConstructorOnStartup]
    public static class Textures {
        public const string Prefix = Strings.ID + "/";

        public static readonly Texture2D Random     = ContentFinder<Texture2D>.Get(Prefix + "Random");
        public static readonly Texture2D RandomMenu = ContentFinder<Texture2D>.Get(Prefix + "RandomMenu");
    }
}
