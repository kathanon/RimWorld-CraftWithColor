﻿using FloatSubMenus;
using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    [HarmonyPatch(typeof(Dialog_BillConfig))]
    public static class DialogBillConfig_Patches {
        private const float CheckSize     = 24f;
        private const float LabelHeight   = CheckSize;
        private const float IconSize      = 28f;
        private const float Offset        = 20f;
        private const float Gap           =  8f;
        private const float IconSpace     = IconSize + 10f;
        private const float WidgetsHeight = IconSize + Gap + LabelHeight;
        private const float CheckAdjust   = (IconSize - LabelHeight) / 2;

        private const float OpenAnimationMinHeight = 100f;

        private static float colorPos;
        private static float colorCheckPos;
        private static float stylePos;
        private static float styleCheckPos;
        private static bool posInitialized = false;
        private static bool disabled = false;
        private static float prevEventHeight = float.MaxValue;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Dialog_BillConfig.DoWindowContents))]
        public static void DoWindowContents_Pre(Rect inRect, Bill_Production ___bill) =>
            DoWindowContents(inRect, ___bill, 0f);

        public static void DoWindowContents(Rect inRect, Bill_Production bill, float widthAdj) {
            if (ShouldApply(bill)) {
                BillAddition add = State.GetAddition(bill);
                DrawWidgets(inRect, add, widthAdj);
                add.UpdateBill();
#if VERSION_1_3
                Widgets_Icon_Patch.Next(add);
#endif
            }
        }

#if !VERSION_1_3
        [HarmonyPrefix]
        [HarmonyPatch("LateWindowOnGUI")]
        public static void LateWindowOnGUI(Bill_Production ___bill) {
            if (ShouldApply(___bill)) {
                BillAddition add = State.GetAddition(___bill);
                Widgets_Icon_Patch.Next(add);
            }
        }
#endif

        public static void SetupPosition(float height) {
            if (!posInitialized) {
                bool openAnimation = height < 0f || height > prevEventHeight;
                prevEventHeight = height;
                if (openAnimation && height < OpenAnimationMinHeight) {
                    disabled = true;
                    return;
                }

                try {
                    float fromBottom = Window.CloseButSize.y + Offset;
                    Range preferred = new Range(fromBottom, WidgetsHeight);
                    preferred.Expand(Gap);
                    Range limit = new Range(0f, height / 2);
                    Range range = MySettings.ConflictingCheckboxRange.FindClosestFreeRange(preferred, limit);
                    range.Contract(Gap);
                    if (MySettings.HasStyleButton) {
                        range.start += CheckSize + Gap;
                    }
                    float top = -WidgetsHeight - range.start;
                    stylePos = top;
                    colorPos = top + LabelHeight + Gap;
                    styleCheckPos = stylePos + CheckAdjust;
                    colorCheckPos = colorPos + CheckAdjust;
                    disabled = false;
                    Map m = Find.CurrentMap;
                    var b = m.PlayerWealthForStoryteller;
                    var a = m.PlayerPawnsForStoryteller;
                } catch (RangeLimitException) {
                    if (!openAnimation) {
                        Main.Instance.Logger.Error(Strings.NoSpaceError + MySettings.ConflictingMods);
                    }
                    disabled = true;
                }
                posInitialized = !openAnimation;
            }
        }

        private static void DrawWidgets(Rect inRect, BillAddition add, float widthAdj) {
            SetupPosition(inRect.height);
            if (!disabled) {
                float width = Mathf.Floor((inRect.width - 34f) / 3f + widthAdj);
                Text.Font = GameFont.Small;
                Color old = GUI.color;
                Color dim = SelectColorDialog.Dimmed(old);

                if (add.CanColor) {
                    Rect checkRect = new Rect(0f, inRect.yMax + colorCheckPos, width - IconSpace, LabelHeight);
                    Rect colorRect = new Rect(width - IconSize, inRect.yMax + colorPos, IconSize, IconSize);
                    Widgets.CheckboxLabeled(checkRect, Strings.DyeItem, ref add.colorActive, placeCheckboxNearText: false);
                    if (add.colorActive) {
                        if (Widgets.ButtonInvisible(colorRect)) {
                            ColorMenu.Open(add);
                        }
                        Widgets.DrawBoxSolid(colorRect, add.TargetColor);
                        if (add.HasRandomColor) {
                            Widgets.DrawTextureFitted(colorRect, Textures.Random, 1f);
                            TooltipHandler.TipRegion(colorRect, add.RandomColorTip);
                        }
                        GUI.color = dim;
                        Widgets.DrawBox(colorRect);
                        GUI.color = old;
                    }
                }

                if (add.CanStyle) {
                    Rect checkRect = new Rect(0f, inRect.yMax + styleCheckPos, width - IconSpace, LabelHeight);
                    Rect styleRect = new Rect(width - IconSize, inRect.yMax + stylePos, IconSize, IconSize);
                    Widgets.CheckboxLabeled(checkRect, Strings.StyleItem, ref add.styleActive, placeCheckboxNearText: false);
                    if (add.styleActive) {
                        ThingStyleDef style = add.TargetStyle;
                        if (Widgets.ButtonInvisible(styleRect)) {
                            add.Styles.Select(s => s.MenuOption).OpenMenu();
                        }
                        GUI.color = dim;
                        Widgets.DrawBox(styleRect);
                        GUI.color = old;
                        if (add.HasRandomStyle) {
                            Widgets.DrawTextureFitted(styleRect, Textures.RandomMenu, 1f);
                        } else {
                            Widgets.DefIcon(styleRect, add.Thing, thingStyleDef: style);
                        }
                        TooltipHandler.TipRegion(styleRect, add.StyleSelection.Label);
                    }
                }
            }
        }

        private static bool ShouldApply(Bill_Production bill) => ShouldApply(bill?.recipe?.ProducedThingDef);

        private static bool ShouldApply(ThingDef thing) => 
            (thing?.HasComp(typeof(CompColorable)) ?? false) || State.StylesFor(thing).Any();
    }
}
