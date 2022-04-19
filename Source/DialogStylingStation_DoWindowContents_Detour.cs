using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static RimWorld.Dialog_StylingStation;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Dialog_StylingStation), nameof(Dialog_StylingStation.DoWindowContents))]
    public static class DialogStylingStation_DoWindowContents_Detour
    {
        private static readonly Target hairTarget = new Target();
        private static readonly Dictionary<Apparel, Target> apparelTargets = new Dictionary<Apparel, Target>();

        public static void Prefix(
            ref Color ___desiredHairColor, 
            Dictionary<Apparel, Color> ___apparelColors)
        {
            if (MySettings.Styling)
            {
                hairTarget.Writeback(ref ___desiredHairColor);
                foreach (var target in apparelTargets)
                {
                    target.Value.Writeback(___apparelColors, target.Key);
                }
            }
        }

        public static void Postfix(
            StylingTab ___curTab,
            Pawn ___pawn,
            Color ___desiredHairColor,
            Dictionary<Apparel, Color> ___apparelColors)
        {
            if (MySettings.Styling)
            {
                switch (___curTab)
                {
                    case StylingTab.Hair:
                        hairTarget.Reset(___desiredHairColor);
                        Widgets_ColorSelector_Detour.Add(hairTarget);
                        break;
                    case StylingTab.ApparelColor:
                        if (___pawn != null && ___apparelColors != null)
                        {
                            // Remove any stale entries
                            apparelTargets.RemoveAll(p => !___apparelColors.ContainsKey(p.Key));
                            // Add any missing and reset
                            foreach (var target in ___apparelColors)
                            {
                                if (!apparelTargets.ContainsKey(target.Key))
                                {
                                    apparelTargets.Add(target.Key, new Target());
                                }
                                apparelTargets[target.Key].Reset(target.Value);
                            }
                            // Register in same order as in dialog
                            foreach (Apparel item in ___pawn.apparel.WornApparel)
                            {
                                if (!___pawn.apparel.IsLocked(item))
                                {
                                    Widgets_ColorSelector_Detour.Add(apparelTargets[item]);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private class Target : ITargetColor
        {
            private Color color;
            private bool updated = false;

            public void Reset(Color initialColor)
            {
                color = initialColor;
                updated = false;
        }

            public Color TargetColor
            {
                get => color;
                set 
                {
                    updated = true;
                    color = value;
                }
            }

            public bool Update { get => true; }

            public void Writeback(ref Color color)
            {
                if (updated)
                {
                    color = this.color;
                    updated = false;
                }
            }

            public void Writeback<T>(Dictionary<T, Color> dict, T key)
            {
                if (updated)
                {
                    if (dict != null && key != null && dict.ContainsKey(key))
                    {
                        dict[key] = color;
                    }
                    updated = false;
                }
            }
        }
    }
}
