using FloatSubMenus;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static CraftWithColor.BillAddition;

namespace CraftWithColor
{
    internal interface ITargetColor
    {
        Color TargetColor { get; set; }

        RandomType RandomColorType { set; }

        bool Update { get; }
    }

    internal class ColorMenu {
        public static void Open(ITargetColor target) {
            List<FloatMenuOption> menu = new List<FloatMenuOption>();
            if (target is BillAddition) {
                menu.Add(new FloatMenuOption(Strings.Select, () => SelectColorDialog.Open(target as BillAddition)));
            }
            menu.Add(RandomOption(target, RandomType.Any));
            if (target is BillAddition && State.SavedColors.Count > 0 && !MySettings.OnlyStandard) {
                menu.Add(new FloatSubMenu(Strings.SavedColors, SavedSubMenu(target)));
            }
            if (MySettings.WithIdeology) {
                menu.Add(new FloatSubMenu(Strings.Favorite, FavoriteSubMenu(target)));
                if (!Find.IdeoManager.classicMode) {
                    menu.Add(new FloatSubMenu(Strings.Ideoligion, IdeoSubMenu(target)));
                }
            }
            Find.WindowStack.Add(new FloatMenu(menu));
        }

        private static FloatMenuOption RandomOption(ITargetColor target, RandomType type) =>
            new FloatMenuOption(Strings.Random, () => target.RandomColorType = type, Textures.RandomMenu, Color.white);

        private static List<FloatMenuOption> SavedSubMenu(ITargetColor target) =>
            SubMenuItems(target, State.SavedColors, RandomType.Saved, c => c, c => " ");
        private static List<FloatMenuOption> FavoriteSubMenu(ITargetColor target) =>
            SubMenuItems(target,
                         Find.CurrentMap.mapPawns.FreeColonists,
                         RandomType.Favorite,
                         p => p.story.favoriteColor
#if VERSION_GE_1_6
                         ?.color
#endif
                         );
        private static List<FloatMenuOption> IdeoSubMenu(ITargetColor target) =>
            SubMenuItems(target, Find.IdeoManager.IdeosInViewOrder, RandomType.Ideo, i => i.ApparelColor);

        private static List<FloatMenuOption> SubMenuItems<T>(
            ITargetColor target, IEnumerable<T> items, RandomType random, 
            Func<T, Color?> getColor, Func<T, string> toString = null)
        {
            if (toString is null) toString = o => o.ToString();
            List<FloatMenuOption> menu = new List<FloatMenuOption>();
            foreach (T item in items)
            {
                Color? color = getColor(item);
                if (color.HasValue)
                {
                    menu.Add(new FloatMenuOption(
                        toString(item),
                        () => target.TargetColor = color.Value,
                        BaseContent.WhiteTex,
                        color.Value));
                }
            }
            menu.Add(RandomOption(target, random));
            return menu;
        }
    }
}
