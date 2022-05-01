using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal class SelectColorDialog : Window, ITargetColor
    {
        private static Rect lastWindowRect;

        private readonly ITargetColor target;
        private readonly Color originalColor;
        private readonly List<Color> standardColors;

        private Color color;

        public SelectColorDialog(ITargetColor target, List<Color> standardColors = null)
        {
            this.target = target;
            originalColor = color = target.TargetColor;
            this.standardColors = standardColors;
            doCloseX = true;
            closeOnClickedOutside = true;
            draggable = true;
        }

        public Color TargetColor
        {
            get => color;
            set
            {
                color = value;
                if (target.Update)
                {
                    target.TargetColor = value;
                }
            }
        }

        public bool Update { get => false; }

        public void Cancel()
        {
            CancelColor();
            Close();
        }

        public void Accept()
        {
            AcceptColor();
            Close();
        }

        public void CancelColor()
        {
            if (target.Update)
            {
                target.TargetColor = originalColor;
                color = originalColor;
            }
        }

        public void AcceptColor()
        {
            target.TargetColor = color;
        }

        public override void OnCancelKeyPressed()
        {
            CancelColor();
            base.OnCancelKeyPressed();
        }

        public override void OnAcceptKeyPressed()
        {
            AcceptColor();
            base.OnAcceptKeyPressed();
        }

        public override void PostClose()
        {
            lastWindowRect = windowRect;
            base.PostClose();
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            if (lastWindowRect.width > 0)
            {
                windowRect = lastWindowRect;
            }
            else
            {
                windowRect.x += 200f;
            }
        }

        public List<Color> StandardColors { get => standardColors ?? DefaultColors; }

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
        private const float ColorWidth            = 164f;
        private const float Gap                   =  16f;
        private const float SmallGap              =   8f;
        private const float LabelCenterAdjustX    = 0.8f;
        private const float LabelCenterAdjustY    = 1.6f;
        private const float SlidersHeight         =  50f;
        private const float SliderLabel           =  15f;
        private const float TextBoxHeight         =  18f;
        private const float TextBoxMargin         =   6f;
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
        private const float ColorHeight           = ListSideHeight - 2 * SmallGap - SlidersHeight - TextBoxHeight;
        private const float ContentWidth          = ColorWidth + Gap + ColorListWidth;
        private const float ContentHeight         = ListSideHeight + ButtonsGap + ButtonsHeight;
        private const float Width                 = 2 * DialogMargin + ContentWidth;
        private const float Height                = 2 * DialogMargin + DialogLabelHeight + ContentHeight;

        private const string WideHexContent = "FFFFFF";
        private const string WideDecContent = "255";

        private static readonly Color dimmedMult = new Color(0.4f, 0.4f, 0.4f);
        private Color normalColor;
        private Color dimmedColor;
        private GUIStyle textBoxRightAlign;

        public static Color Dimmed(Color color) => color * dimmedMult;

        public const int SavedColorsMax = ColorListSavedRows * ColorListColumns;

        public override void DoWindowContents(Rect inRect)
        {
            if (target.Update && target.TargetColor != color)
            {
                color = target.TargetColor;
            }

            bool saved = State.SavedColors.FindIndex(c => c.IndistinguishableFrom(color)) > -1;
            bool savedMax = State.SavedColors.Count >= SavedColorsMax;

            normalColor = GUI.color;
            dimmedColor = Dimmed(normalColor);
            textBoxRightAlign = new GUIStyle(Text.CurTextFieldStyle) { alignment = TextAnchor.MiddleRight };

            Rect labelRect = inRect.TopPartPixels(DialogLabelHeight).ContractedBy(DialogMarginAdjust, 0f);
            inRect = inRect.BottomPartPixels(inRect.height - DialogLabelHeight).ContractedBy(DialogMarginAdjust);
            Rect colorRect = new Rect(inRect.x, inRect.y, ColorWidth, ColorHeight);
            Rect[] sliderRect = new Rect(inRect.x, colorRect.yMax + SmallGap, ColorWidth, SlidersHeight).SliceHorizontal(3);
            Rect hexRect = new Rect(inRect.x, sliderRect[2].yMax + SmallGap, ColorWidth, TextBoxHeight);
            float colorListsLeft = colorRect.xMax + Gap;
            Rect colorList = new Rect(colorListsLeft, inRect.y, ColorListWidth, 0f);
            Rect[] buttonRect = new Rect(inRect.x, inRect.yMax - ButtonsHeight, inRect.width, ButtonsHeight).SliceVertical(4, ButtonsGap);
            TextAnchor anchor = Text.Anchor;

            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, Strings.SelectColor);
            Text.Font = GameFont.Small;

            Widgets.DrawBoxSolid(colorRect, color);
            if (MySettings.WithIdeology && Widgets.ButtonInvisible(colorRect))
            {
                ColorMenu.Open(this);
            }

            if (!MySettings.OnlyStandard)
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                ColorSlider(sliderRect[0], Strings.R, ref color.r);
                ColorSlider(sliderRect[1], Strings.G, ref color.g);
                ColorSlider(sliderRect[2], Strings.B, ref color.b);

                ColorBoxes(hexRect, ref color);
            }

            Text.Anchor = TextAnchor.UpperLeft;
            if (MySettings.WithIdeology)
            {
                ColorSelector(ref colorList, Strings.Standard, DefaultColors, ColorListStandardRows);
            }
            if (!MySettings.OnlyStandard)
            {
                ColorSelector(ref colorList, Strings.Saved, State.SavedColors, ColorListSavedRows);
            }

            Text.Anchor = anchor;
            if (!MySettings.OnlyStandard)
            {
                DisablableButton(buttonRect[0], Strings.Delete, () => State.SavedColors.Remove(color), saved);
                DisablableButton(buttonRect[1], Strings.Save, () => State.SavedColors.Add(color), !saved && !savedMax);
            }
            DisablableButton(buttonRect[2], Strings.Cancel, Cancel);
            DisablableButton(buttonRect[3], Strings.Accept, Accept);

            if (target.Update && target.TargetColor != color)
            {
                target.TargetColor = color;
            }
        }

        private void ColorSlider(Rect rect, string label, ref float value)
        {
            rect.y -= LabelCenterAdjustY;
            Widgets.Label(rect, label);
            rect.y += LabelCenterAdjustY;
            rect.xMin += SliderLabel;
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
            Widgets_ColorSelector_Detour.Skip();
            Widgets.ColorSelector(innerRect.BottomPartPixels(colorsHeight), ref color, list);

            rect.y += rect.height + Gap;
        }

        private void ColorBoxes(Rect rect, ref Color color)
        {
            float hexWidth = Text.CalcSize(WideHexContent).x + TextBoxMargin;
            float decWidth = Text.CalcSize(WideDecContent).x + TextBoxMargin;
            Rect hexRect = rect.LeftPartPixels(hexWidth);
            Rect[] decRect = rect.RightPartPixels(3f * decWidth).SliceVertical(3, 1f);
            Rect slashRect = new Rect(hexRect.xMax + LabelCenterAdjustX, rect.y + LabelCenterAdjustY, decRect[0].x - hexRect.xMax, rect.height);

            Color32 col32 = color;
            uint colInt = (((uint)col32.r) << 16) | (((uint)col32.g) << 8) | ((uint)col32.b);
            string oldHex = Convert.ToString(colInt, 16).ToUpper().PadLeft(6, '0');
            string newHex = Widgets.TextField(hexRect, oldHex).ToUpper();
            if (oldHex != newHex)
            {
                if (oldHex.Length != newHex.Length)
                {
                    newHex = AdjustTo(newHex, oldHex, '0');
                }
                try   { colInt = Convert.ToUInt32(newHex, 16); }
                catch { return; }
                col32.r = (byte)(colInt >> 16);
                col32.g = (byte)((colInt >> 8) & 0xff);
                col32.b = (byte)(colInt & 0xff);
                color = col32;
            }

            if (slashRect.width >= Text.CalcSize("/").x)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(slashRect, "/");
            }

            ColorDecBox(decRect[0], ref color.r);
            ColorDecBox(decRect[1], ref color.g);
            ColorDecBox(decRect[2], ref color.b);
        }

        private void ColorDecBox(Rect rect, ref float value)
        {
            int intValue = Mathf.RoundToInt(255f * value);
            string oldText = intValue.ToString();
            string newText = GUI.TextField(rect, oldText, textBoxRightAlign);
            if (oldText != newText)
            {
                try
                {
                    uint newIntValue = Convert.ToUInt32(newText);
                    value = Mathf.Clamp(newIntValue / 255f, 0f, 1f);
                }
                catch { } // If it goes wrong, just don't update
            }
        }

        private static string AdjustTo(string changed, string original, char fill) 
        {
            int lenChanged = changed.Length;
            int lenOriginal = original.Length;
            StringBuilder buf = new StringBuilder(changed);
            int pos = 0; 
            int shorter = Math.Min(lenChanged, lenOriginal);
            int diff = Math.Abs(lenChanged - lenOriginal);
            // Find first difference
            for (; pos < shorter && changed[pos] == original[pos]; pos++);
            if (lenChanged > lenOriginal)
            {
                buf.Remove(pos + diff, diff);
            }
            else
            {
                int missingAt = lenChanged;
                // Find how long matching tail is
                for (int i = lenOriginal - 1; missingAt > pos && changed[missingAt - 1] == original[i]; i--, missingAt--);
                buf.Insert(missingAt, new string(fill, diff));
            }
            return buf.ToString();
        }

        private void DisablableButton(Rect rect, string label, Action action, bool active = true)
        {
            GUI.color = active ? normalColor : dimmedColor;
            bool pressed = Widgets.ButtonText(rect, label, active: active);
            GUI.color = normalColor;
            if (pressed) action();
        }

        internal static void Open(ITargetColor target, List<Color> standardColors = null)
        {
            Find.WindowStack.Add(new SelectColorDialog(target, standardColors));
        }
    }
}
