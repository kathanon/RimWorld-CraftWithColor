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
        private static bool doNextStyle = false;

        [HarmonyPatch(nameof(Widgets.ThingIcon), 
            typeof(Rect), typeof(ThingDef), typeof(ThingDef), typeof(ThingStyleDef), typeof(float), typeof(Color?)
#if VERSION_GE_1_4
            , typeof(int?)
#endif
#if VERSION_GE_1_6
            , typeof(float)
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
#if VERSION_GE_1_4
            , typeof(int?)
#endif
            )]
        [HarmonyPrefix]
        public static void GetIconFor(ref ThingStyleDef thingStyleDef) {
            if (doNextStyle) {
                thingStyleDef = nextStyle;
                doNextStyle = false;
            }
        }

        public static void Next(BillAddition add) {
            nextColor = add.ActiveColor;
            nextStyle = add.TargetStyle;
            doNextStyle = add.styleActive;
        }
    }
}
