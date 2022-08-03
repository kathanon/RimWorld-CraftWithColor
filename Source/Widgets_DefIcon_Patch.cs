using HarmonyLib;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Widgets), nameof(Widgets.DefIcon))]
    public static class Widgets_DefIcon_Patch
    {
        private static Color? nextColor = null;
        private static ThingStyleDef nextStyle = null;

        public static void Prefix(Def def, ref Color? color, ref ThingStyleDef thingStyleDef)
        {
            if (nextColor != null)
            {
                color = nextColor;
                nextColor = null;
            }
        }

        [HarmonyPatch(nameof(Widgets.GetIconFor))]
        [HarmonyPrefix]
        public static void GetIconFor(ref ThingStyleDef thingStyleDef) {
            if (nextStyle != null) {
                thingStyleDef = nextStyle;
                nextStyle = null;
            }
        }

        public static void Next(Color? color, ThingStyleDef style)
        {
            nextColor = color;
            nextStyle = style;
        }
    }
}
