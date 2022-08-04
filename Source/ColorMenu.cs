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
        public static void Open(ITargetColor target)
        {
            List<FloatMenuOption> menu = new List<FloatMenuOption>();
            if (target is BillAddition)
            {
                menu.Add(new FloatMenuOption(Strings.Select, () => SelectColorDialog.Open(target as BillAddition)));
                if (State.SavedColors.Count > 0 && !MySettings.OnlyStandard)
                {
                    menu.Add(new FloatSubMenu(Strings.SavedColors, SubMenuItems(State.SavedColors, c => c, c => " ", target)));
                }
            }
            menu.Add(new FloatSubMenu(Strings.Favorite, FavoriteSubMenu(target)));
            if (!Find.IdeoManager.classicMode)
            {
                menu.Add(new FloatSubMenu(Strings.Ideoligion, IdeoSubMenu(target)));
            }
            // TODO: Change back when FloatSubMenu has completed VUIE support
            FloatSubMenu.NoVUIEMenu(menu);
            //Find.WindowStack.Add(new FloatMenu(menu)
            //{
            //    vanishIfMouseDistant = false
            //});
        }

        private static List<FloatMenuOption> FavoriteSubMenu(ITargetColor target) =>
            SubMenuItems(Find.CurrentMap.mapPawns.FreeColonists, p => p.story.favoriteColor, target);
        private static List<FloatMenuOption> IdeoSubMenu(ITargetColor target) =>
            SubMenuItems(Find.IdeoManager.IdeosInViewOrder, i => i.ApparelColor, target);

        private static List<FloatMenuOption> SubMenuItems<T>(IEnumerable<T> items, Func<T, Color?> getColor, ITargetColor target) =>
            SubMenuItems(items, getColor, o => o.ToString(), target);
        private static List<FloatMenuOption> SubMenuItems<T>(IEnumerable<T> items, Func<T, Color?> getColor, Func<T, string> toString, ITargetColor target)
        {
            List<FloatMenuOption> menu = new List<FloatMenuOption>();
            foreach (T item in items)
            {
                Color? color = getColor(item);
                if (color.HasValue)
                {
                    menu.Add(new FloatMenuOption(
                        toString(item),
                        delegate { target.TargetColor = color.Value; },
                        BaseContent.WhiteTex,
                        color.Value));
                }
            }
            return menu;
        }
    }
}
