using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static RimWorld.Dialog_StylingStation;
using static CraftWithColor.Widgets_ColorSelector_Patch;
using System.Linq;
using System;
using System.Reflection.Emit;

namespace CraftWithColor {
    [HarmonyPatch]
    public static class AddMoreColorsButton_Patch {
        private static readonly Target hairTarget = new Target();
        private static readonly Dictionary<Apparel, Target> apparelTargets = new Dictionary<Apparel, Target>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dialog_StylingStation), nameof(Dialog_StylingStation.DoWindowContents))]
        public static void Styling_Pre(ref Color ___desiredHairColor,
                                       Dictionary<Apparel, Color> ___apparelColors) {
            if (MySettings.Styling) {
                hairTarget.Writeback(ref ___desiredHairColor);
                foreach (var target in apparelTargets) {
                    target.Value.Writeback(___apparelColors, target.Key);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dialog_StylingStation), nameof(Dialog_StylingStation.DoWindowContents))]
        public static void Styling_Post(StylingTab ___curTab,
                                        Pawn ___pawn,
                                        Color ___desiredHairColor,
                                        Dictionary<Apparel, Color> ___apparelColors) {
            if (MySettings.Styling) {
                switch (___curTab) {
                    case StylingTab.Hair:
                        hairTarget.Reset(___desiredHairColor);
                        Add(hairTarget);
                        break;
                    case StylingTab.ApparelColor:
                        if (___pawn != null && ___apparelColors != null) {
                            // Remove any stale entries
                            apparelTargets.RemoveAll(p => !___apparelColors.ContainsKey(p.Key));
                            // Add any missing and reset
                            foreach (var target in ___apparelColors) {
                                if (!apparelTargets.ContainsKey(target.Key)) {
                                    apparelTargets.Add(target.Key, new Target());
                                }
                                apparelTargets[target.Key].Reset(target.Value);
                            }
                            // Register in same order as in dialog
                            foreach (Apparel item in ___pawn.apparel.WornApparel) {
                                if (!___pawn.apparel.IsLocked(item)) {
                                    Add(apparelTargets[item]);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        private static readonly Target ideoTarget = new Target();
        private static List<Color> allColors;
        private static ColorDef dummyDef = new ColorDef();
        private static ColorDef lastDef = null;
        private static float ideoPreY;
        private static bool ideoOn = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols), MethodType.Constructor, typeof(Ideo))]
        public static void IdeoSymbols_Constr(Ideo ideo) {
            SetIdeoColor(ideo.colorDef);
            ideoOn = MySettings.IdeoSymbols;
        }

        private static void SetIdeoColor(ColorDef color) {
            dummyDef.color = color.color;
            ideoTarget.Reset(color.color);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols), "DoColorSelector")]
        public static void IdeoSymbols_Color_Pre(float curY, ColorDef ___newColorDef) {
            if (ideoOn) {
                lastDef = ___newColorDef;
                ideoPreY = curY;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols), "DoColorSelector")]
        public static void IdeoSymbols_Color_Post(Rect mainRect,
                                                  ref float curY,
                                                  List<ColorDef> ___allColors,
                                                  ref ColorDef ___newColorDef) {
            if (ideoOn && ___allColors != null) {
                if (allColors == null) {
                    allColors = ___allColors.Select(x => x.color).ToList();
                }
                Add(ideoTarget);
                mainRect.yMin = ideoPreY;
                float y = ColorSelector(mainRect, allColors, BaseContent.ClearTex, 22, 2, 88) + 4f;
                if (y > curY) curY = y;
                ideoTarget.Writeback(ref dummyDef.color);

                if (ReferenceEquals(___newColorDef, lastDef)) {
                    ___newColorDef = ___allColors.Find(Selected) ?? dummyDef;
                } else {
                    SetIdeoColor(___newColorDef);
                }

                bool Selected(ColorDef def) 
                    => def.color.IndistinguishableFromFast(dummyDef.color);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols), "TryAccept")]
        public static void IdeoSymbols_TryAccept_Pre(ref ColorDef ___newColorDef) {
            if (ideoOn && ReferenceEquals(___newColorDef, dummyDef)) {
                 ___newColorDef = State.CustomColorDef(dummyDef.color, ColorType.Ideo);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ideo), nameof(Ideo.ExposeData))]
        public static void Ideo_ExposeData_Pre() {
            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                var node = Scribe.loader.curXmlParent["colorDef"];
                State.CustomColorDef(node?.InnerText, ColorType.Ideo);
            }
        }


        private class Target : ITargetColor {
            private Color color;
            private bool updated = false;

            public void Reset(Color initialColor) {
                color = initialColor;
                updated = false;
            }

            public Color TargetColor {
                get => color;
                set {
                    updated = true;
                    color = value;
                }
            }

            public bool Update { get => true; }
            public BillAddition.RandomType RandomColorType { set => throw new System.NotImplementedException(); }

            public void Writeback(ref Color color) {
                if (updated) {
                    color = this.color;
                    updated = false;
                }
            }

            public void Writeback<T>(Dictionary<T, Color> dict, T key) {
                if (updated) {
                    if (dict != null && key != null && dict.ContainsKey(key)) {
                        dict[key] = color;
                    }
                    updated = false;
                }
            }
        }
    }
}
