using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal interface ITargetColor
    {
        Color TargetColor { get; set; }

        bool Update { get; }
    }

    internal class ColorMenu
    {
        public static void Open(ITargetColor add)
        {
            List<FloatMenuOption> menu = new List<FloatMenuOption>();
            if (add is BillAddition)
            {
                menu.Add(new FloatMenuOption(Strings.Select, () => SelectColorDialog.Open(add as BillAddition)));
                if (State.SavedColors.Count > 0 && !MySettings.OnlyStandard)
                {
                    menu.Add(new FloatSubMenu(Strings.SavedColors, SubMenuItems(State.SavedColors, c => c, c => " ", add)));
                }
            }
            menu.Add(new FloatSubMenu(Strings.Favorite, FavoriteSubMenu(add)));
            if (!Find.IdeoManager.classicMode)
            {
                menu.Add(new FloatSubMenu(Strings.Ideoligion, IdeoSubMenu(add)));
            }
            Find.WindowStack.Add(new FloatMenu(menu)
            {
                vanishIfMouseDistant = false
            });
        }

        private static List<FloatMenuOption> FavoriteSubMenu(ITargetColor add) =>
            SubMenuItems(Find.CurrentMap.mapPawns.FreeColonists, p => p.story.favoriteColor.Value, add);
        private static List<FloatMenuOption> IdeoSubMenu(ITargetColor add) =>
            SubMenuItems(Find.IdeoManager.IdeosInViewOrder, i => i.ApparelColor, add);

        private static List<FloatMenuOption> SubMenuItems<T>(IEnumerable<T> items, Func<T, Color> getColor, ITargetColor add) =>
            SubMenuItems(items, getColor, o => o.ToString(), add);
        private static List<FloatMenuOption> SubMenuItems<T>(IEnumerable<T> items, Func<T, Color> getColor, Func<T, string> toString, ITargetColor add)
        {
            List<FloatMenuOption> menu = new List<FloatMenuOption>();
            foreach (T item in items)
            {
                Color color = getColor(item);
                menu.Add(new FloatMenuOption(
                    toString(item),
                    delegate { add.TargetColor = color; },
                    BaseContent.WhiteTex,
                    color));
            }
            return menu;
        }
    }
}
