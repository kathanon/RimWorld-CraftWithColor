using HarmonyLib;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Widgets))]
    public static class Widgets_Icon_Patch
    {
        private static Color? nextColor = null;
        private static ThingStyleDef nextStyle = null;

        [HarmonyPatch(nameof(Widgets.ThingIcon), 
            typeof(Rect), typeof(ThingDef), typeof(ThingDef), typeof(ThingStyleDef), typeof(float), typeof(Color?)
#if !VERSION_1_3
            , typeof(int?)
#endif
            )]
        [HarmonyPrefix]
        public static void ThingIcon(ref Color? color)
        {
            if (nextColor != null)
            {
                color = nextColor;
                nextColor = null;
            }
        }

        [HarmonyPatch(nameof(Widgets.GetIconFor),
            typeof(ThingDef), typeof(ThingDef), typeof(ThingStyleDef)
#if !VERSION_1_3
            , typeof(int?)
#endif
            )]
        [HarmonyPrefix]
        public static void GetIconFor(ref ThingStyleDef thingStyleDef) {
            if (nextStyle != null) {
                thingStyleDef = nextStyle;
                nextStyle = null;
            }
        }

        public static void Next(Color? color = null, ThingStyleDef style = null) {
            nextColor = color;
            nextStyle = style;
        }

        public static void Next(BillAddition add) {
            nextColor = add.ActiveColor;
            nextStyle = add.ActiveStyle;
        }
    }
}
