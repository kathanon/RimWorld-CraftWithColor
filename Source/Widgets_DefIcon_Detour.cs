using HarmonyLib;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Widgets), nameof(Widgets.DefIcon))]
    public static class Widgets_DefIcon_Detour
    {
        private static Color? next = null;

        public static void Prefix(ref Color? color)
        {
            if (next != null)
            {
                color = next;
                next = null;
            }
        }

        public static void Next(Color? color)
        {
            next = color;
        }
    }
}
