using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal class SelectColorDialog : Window, ITargetColor
    {
        private BillAddition add;
        private Color color;

        public SelectColorDialog(BillAddition add)
        {
            this.add = add;
            color = add.TargetColor;
            doCloseX = true;
            closeOnClickedOutside = true;
            draggable = true;
        }

        public Color TargetColor { set => color = value; }

        public void Cancel()
        {
            Close();
        }

        public void Accept()
        {
            AcceptColor();
            Close();
        }

        public void AcceptColor()
        {
            add.TargetColor = color;
        }

        public override void OnAcceptKeyPressed()
        {
            Accept();
            base.OnAcceptKeyPressed();
        }

        public List<Color> DefaultColors
        {
            get
            {
                if (defaultColorsCache == null)
                {
                    defaultColorsCache = (
                        from x in DefDatabase<ColorDef>.AllDefsListForReading 
                        where !x.hairOnly 
                        select x.color
                        ).ToList();
                }
                return defaultColorsCache;
            }
        }

        private static List<Color> defaultColorsCache = null;

        public override Vector2 InitialSize => new Vector2(Width, Height);

        protected override float Margin => DialogLabelMarginTop;

        private const float DialogMargin          =  24f;
        private const float DialogLabelHeight     =  32f;
        private const float DialogLabelMarginTop  =  16f;
        private const float ColorSize             = 130f;
        private const float Gap                   =  16f;
        private const float SlidersHeight         =  50f;
        private const float SliderLabel           =  15f;
        private const float ColorListLabelHeight  =  20f;
        private const float ColorListColorsAdjust =  -2f;
        private const float ColorListSquareSize   =  28f;
        private const float ColorListMargin       =   2f;
        private const int   ColorListColumns      =  10;
        private const int   ColorListStandardRows =   4;
        private const int   ColorListSavedRows    =   2;
        private const float ButtonsHeight         =  30f;
        private const float ButtonsGap            =  24f;
        private const int   ColorListRows         = ColorListStandardRows + ColorListSavedRows;
        private const float ColorListHeight       = ColorListColorsAdjust + 2 * ColorListMargin + ColorListLabelHeight;
        private const float ColorListWidth        = ColorListColumns * ColorListSquareSize + 2 * ColorListMargin;
        private const float DialogMarginAdjust    = DialogMargin - DialogLabelMarginTop;
        private const float ListSideHeight        = 2 * ColorListHeight + ColorListRows * ColorListSquareSize + Gap;
        private const float SliderSideHeight      = ColorSize + 2 * Gap + SlidersHeight + ButtonsHeight;
        private const float MaxSideHeight         = ListSideHeight > SliderSideHeight ? ListSideHeight : SliderSideHeight;
        private const float ContentWidth          = ColorSize + Gap + ColorListWidth;
        private const float ContentHeight         = MaxSideHeight + ButtonsGap + ButtonsHeight;
        private const float Width                 = 2 * DialogMargin + ContentWidth;
        private const float Height                = 2 * DialogMargin + DialogLabelHeight + ContentHeight;

        private static readonly Color dimmedMult = new Color(0.4f, 0.4f, 0.4f);
        private Color normalColor;
        private Color dimmedColor;

        public const int SavedColorsMax = ColorListSavedRows * ColorListColumns;

        public override void DoWindowContents(Rect inRect)
        {
            bool saved = State.SavedColors.Contains(color);
            bool savedMax = State.SavedColors.Count >= SavedColorsMax;

            normalColor = GUI.color;
            dimmedColor = normalColor * dimmedMult;

            Rect labelRect = inRect.TopPartPixels(DialogLabelHeight).ContractedBy(DialogMarginAdjust, 0f);
            inRect = inRect.BottomPartPixels(inRect.height - DialogLabelHeight).ContractedBy(DialogMarginAdjust);
            Rect colorRect    = new Rect(inRect.x, inRect.y, ColorSize, ColorSize);
            Rect[] sliderRect = new Rect(inRect.x, colorRect.yMax + Gap, ColorSize - SliderLabel, SlidersHeight).SliceHorizontal(3);
            float colorListsLeft = colorRect.xMax + Gap;
            Rect colorList    = new Rect(colorListsLeft, inRect.y, ColorListWidth, 0f);
            Rect[] buttonRect = new Rect(inRect.x, inRect.yMax - ButtonsHeight, inRect.width, ButtonsHeight).SliceVertical(4, ButtonsGap);
            TextAnchor anchor = Text.Anchor;

            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, "Select color");
            Text.Font = GameFont.Small;
            
            Widgets.DrawBoxSolid(colorRect, color);
            if (Widgets.ButtonInvisible(colorRect)) ColorMenu.Open(this);

            Text.Anchor = TextAnchor.MiddleLeft;
            ColorSlider(sliderRect[0], "R", ref color.r);
            ColorSlider(sliderRect[1], "G", ref color.g);
            ColorSlider(sliderRect[2], "B", ref color.b);

            Text.Anchor = TextAnchor.UpperLeft;
            ColorSelector(ref colorList, "Standard", DefaultColors, ColorListStandardRows);
            ColorSelector(ref colorList, "Saved", State.SavedColors, ColorListSavedRows);

            Text.Anchor = anchor;
            DisablableButton(buttonRect[0], "Delete", () => State.SavedColors.Remove(color), saved);
            DisablableButton(buttonRect[1], "Save",   () => State.SavedColors.Add(color),    !saved && !savedMax);
            DisablableButton(buttonRect[2], "Cancel", Cancel);
            DisablableButton(buttonRect[3], "Accept", Accept);
        }

        private void ColorSlider(Rect rect, string label, ref float value)
        {
            Widgets.Label(rect, label);
            rect.x += SliderLabel;
            value = Widgets.HorizontalSlider(rect, value, 0f, 1f);
        }

        private void ColorSelector(ref Rect rect, string label, List<Color> list, int rows)
        {
            rect.height = ColorListHeight + rows * ColorListSquareSize;
            float colorsHeight = rows * ColorListSquareSize - ColorListColorsAdjust;
            Rect innerRect = rect.ContractedBy(ColorListMargin);

            GUI.color = dimmedColor;
            Widgets.DrawBox(rect);
            GUI.color = normalColor;

            Widgets.Label(innerRect.TopPartPixels(ColorListLabelHeight), label);
            Widgets.ColorSelector(innerRect.BottomPartPixels(colorsHeight), ref color, list);

            rect.y += rect.height + Gap;
        }

        private void DisablableButton(Rect rect, string label, Action action, bool active = true)
        {
            GUI.color = active ? normalColor : dimmedColor;
            bool pressed = Widgets.ButtonText(rect, label, active: active);
            GUI.color = normalColor;
            if (pressed) action();
        }

        internal static void Open(BillAddition add)
        {
            Find.WindowStack.Add(new SelectColorDialog(add));
        }
    }
}
