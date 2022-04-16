using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
    public static class Dialog_BillConfig_DoWindowContents_Detour
    {
        private const float LabelHeight = 24f;
        private const float ColorSize = 28f;
        private const float ColorSpace = ColorSize + 10f;
        private const float WidgetsHeight = ColorSize;

        public static bool Prefix(Dialog_BillConfig __instance, Rect inRect, Bill_Production ___bill)
        {
            if (ShouldApply(___bill))
            {
                BillAddition add = State.GetAddition(___bill);
                DrawWidgets(inRect, add);
                UpdateBill(___bill, add);
            }

            return true;
        }

        private static void DrawWidgets(Rect inRect, BillAddition add)
        {
            float width = (int)((inRect.width - 34f) / 3f);
            float top = inRect.height - WidgetsHeight - Window.CloseButSize.y;
            Rect labelRect = new Rect(0f, top + WidgetsHeight - LabelHeight, width - ColorSpace, LabelHeight);
            Rect colorRect = new Rect(width - ColorSize, top, ColorSize, ColorSize);
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(labelRect, "Dye item", ref add.active, placeCheckboxNearText: true);
            if (add.active)
            {
                if (Widgets.ButtonInvisible(colorRect))
                {
                    ColorSelection(add);
                }
                Widgets.DrawBoxSolid(colorRect, add.color);
            }
        }

        private static void ColorSelection(BillAddition add)
        {
            // TODO Include all colonists, see ColonistBar.CheckRecacheEntries()
            List<FloatMenuOption> subSubMenu1 = new List<FloatMenuOption>
            {
                new FloatMenuOption("Option A.1", delegate { }),
                new FloatMenuOption("Option A.2", delegate { }),
                new FloatMenuOption("Option A.3", delegate { }),
            };
            List<FloatMenuOption> subSubMenu2 = new List<FloatMenuOption>
            {
                new FloatMenuOption("Option B.1", delegate { }),
                new FloatMenuOption("Option B.2", delegate { }),
                new FloatMenuOption("Option B.3", delegate { }),
                new FloatMenuOption("Option B.4", delegate { }),
            };
            List<FloatMenuOption> subMenu1 = new List<FloatMenuOption>
            {
                new FloatSubMenu("Option A", subSubMenu1),
                new FloatSubMenu("Option B", subSubMenu2),
                new FloatMenuOption("Option C", delegate { }),
                new FloatMenuOption("Option D", delegate { }),
                new FloatMenuOption("Option E", delegate { }),
            };
            List<FloatMenuOption> subMenu2 = new List<FloatMenuOption>
            {
                new FloatMenuOption("Option F", delegate { }),
                new FloatMenuOption("Option G", delegate { }),
                new FloatMenuOption("Option H", delegate { }),
            };
            List< FloatMenuOption> menu = new List<FloatMenuOption>
            {
                new FloatMenuOption("Select...", delegate { }),
                //new FloatSubMenu("Submenu 1", subMenu1),
                //new FloatSubMenu("Submenu 2", subMenu2),
                new FloatSubMenu("Favorite", FavoriteSubMenu(add)),
                new FloatSubMenu("Ideo",     IdeoSubMenu(add))
            };
            Find.WindowStack.Add(new FloatMenu(menu)
            {
                vanishIfMouseDistant = false
            });
        }

        private static List<FloatMenuOption> FavoriteSubMenu(BillAddition add) =>
            SubMenuItems<Pawn>(Find.CurrentMap.mapPawns.FreeColonists, p => p.story.favoriteColor.Value, add);
        private static List<FloatMenuOption> IdeoSubMenu(BillAddition add) =>
            SubMenuItems<Ideo>(Find.IdeoManager.IdeosInViewOrder, i => i.ApparelColor, add);

        private static List<FloatMenuOption> SubMenuItems<T>(IEnumerable<T> items, Func<T, Color> getColor, BillAddition add)
        {
            List<FloatMenuOption> menu = new List<FloatMenuOption>();
            foreach (T item in items)
            {
                Color color = getColor(item);
                menu.Add(new FloatMenuOption(
                    item.ToString(),
                    delegate { add.color = color; }, 
                    BaseContent.WhiteTex, 
                    color));
            }
            return menu;
        }

        private static void UpdateBill(Bill_Production bill, BillAddition add)
        {
        }

        private static bool ShouldApply(Bill_Production bill)
        {
            return bill.recipe.ProducedThingDef.comps.Find(c => c.compClass == typeof(CompColorable)) != null;
        }
    }
}
