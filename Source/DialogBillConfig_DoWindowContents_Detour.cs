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

        private static void DrawWidgets(Rect inRect, BillAddition add)
        {
            float width = (int)((inRect.width - 34f) / 3f);
            float top = inRect.height - WidgetsHeight - Window.CloseButSize.y - Offset;
            Rect labelRect = new Rect(0f, top + (WidgetsHeight - LabelHeight) / 2, width - ColorSpace, LabelHeight);
            Rect colorRect = new Rect(width - ColorSize, top, ColorSize, ColorSize);
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

        private static bool ShouldApply(Bill_Production bill)
        {
            return bill?.recipe?.ProducedThingDef?.comps?.Find(c => c.compClass == typeof(CompColorable)) != null;
        }
    }
}
