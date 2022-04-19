using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
    public static class DialogBillConfig_DoWindowContents_Detour
    {
        private const float LabelHeight = 24f;
        private const float ColorSize = 28f;
        private const float ColorSpace = ColorSize + 10f;
        private const float WidgetsHeight = ColorSize;
        private const float Offset = 20f;
        private const float Gap = 8f;

        private static float labelPos;
        private static float colorPos;
        private static bool posInitialized = false;
        private static bool disabled = false;

        public static bool Prefix(Rect inRect, Bill_Production ___bill)
        {
            if (ShouldApply(___bill))
            {
                BillAddition add = State.GetAddition(___bill);
                DrawWidgets(inRect, add);
                add.UpdateBill(___bill);
                Widgets_DefIcon_Detour.Next(add.ActiveColor);
            }
            return true;
        }

        public static void SetupPosition(float height)
        {
            if (!posInitialized)
            {
                try
                {
                    float fromBottom = Window.CloseButSize.y + Offset;
                    Range preferred = new Range(fromBottom, WidgetsHeight);
                    preferred.Expand(Gap);
                    Range limit = new Range(0f, height / 2);
                    Range range = MySettings.ConflictingCheckboxRange.FindClosestFreeRange(preferred, limit);
                    range.Contract(Gap);
                    float top = -WidgetsHeight - range.start;
                    labelPos = top + (WidgetsHeight - LabelHeight) / 2;
                    colorPos = top;
                }
                catch (RangeLimitException)
                {
                    Main.Instance.Logger.Error("Could not find valid place for dialog addition, conflicting mods:\n" + MySettings.ConflictingMods);
                    disabled = true;
                }
                posInitialized = true;
            }
        }

        private static void DrawWidgets(Rect inRect, BillAddition add)
        {
            SetupPosition(inRect.height);
            if (!disabled)
            {
                float width = Mathf.Floor((inRect.width - 34f) / 3f);
                Rect labelRect = new Rect(0f, inRect.yMax + labelPos, width - ColorSpace, LabelHeight);
                Rect colorRect = new Rect(width - ColorSize, inRect.yMax + colorPos, ColorSize, ColorSize); ;
                Text.Font = GameFont.Small;
                Widgets.CheckboxLabeled(labelRect, Strings.DyeItem, ref add.active, placeCheckboxNearText: false);
                if (add.active)
                {
                    if (Widgets.ButtonInvisible(colorRect))
                    {
                        ColorMenu.Open(add);
                    }
                    Widgets.DrawBoxSolid(colorRect, add.TargetColor);
                    Color old = GUI.color;
                    GUI.color = SelectColorDialog.Dimmed(old);
                    Widgets.DrawBox(colorRect);
                    GUI.color = old;
                }
            }
        }

        private static bool ShouldApply(Bill_Production bill)
        {
            return bill?.recipe?.ProducedThingDef?.comps?.Find(c => c.compClass == typeof(CompColorable)) != null;
        }
    }
}
