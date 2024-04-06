using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Widgets), nameof(Widgets.ColorSelector))]
    public static class Widgets_ColorSelector_Patch
    {
        private static readonly Queue<ITargetColor> queue = new Queue<ITargetColor>();
        private static bool skip = false;

        private const int ButtonSize = 3;

        [HarmonyPrefix]
        public static void ColorSelector_Pre(Rect rect, List<Color> colors, Texture icon, int colorSize, int colorPadding)
            => ColorSelector(rect, colors, icon, colorSize, colorPadding, 4 * colorSize);

        public static float ColorSelector(Rect rect, List<Color> colors, Texture icon, int colorSize, int colorPadding, int iconSize) {
            if (skip) {
                skip = false;
                return 0f;
            }

            if (queue.Count == 0) return 0f;

            ITargetColor target = queue.Dequeue();
            int iconSpace = (icon == null) ? 0 : iconSize + 10;
            float colorWidth = rect.width - iconSpace;
            int squareSpace = colorSize + 3 * colorPadding;
            int cols = (int) (colorWidth / squareSpace);
            int rows = (colors.Count + cols - 1) / cols;
            // This should use squareSpace without subtracting colorPadding, but Rimworld has a bug in this calculation
            float yAdjust = (icon == null) ? 0f : (iconSpace - (squareSpace - colorPadding) * rows - colorPadding) / 2f;
            int remainder = colors.Count % cols;
            // If there is space, place button *in* last row
            if (remainder > 0 && remainder < cols - ButtonSize) rows--;
            int width = cols * squareSpace - colorPadding;
            int height = rows * squareSpace;
            int buttonWidth = ButtonSize * squareSpace - colorPadding;
            Rect button = new Rect(rect.x + iconSpace + width - buttonWidth, rect.y + height + yAdjust, buttonWidth, squareSpace - colorPadding);
            if (Widgets.ButtonText(button, Strings.More)) {
                SelectColorDialog.Open(target, colors);
            }
            return button.yMax;
        }

        internal static void Skip() { skip = true; }

        internal static void Add(ITargetColor target) { queue.Enqueue(target); }
    }
}
