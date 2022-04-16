using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    public class FloatSubMenu : FloatMenuOption
    {
        private readonly List<FloatMenuOption> subOptions;
        private readonly string subTitle;
        private readonly float extraPartWidthOuter;
        private readonly Func<Rect, bool> extraPartOnGUIOuter;

        private FloatSubMenuInner subMenu = null;
        private bool open = false;
        private bool subMenuExecuted = false;

        private static readonly Vector2 MenuOffset = new Vector2(-1f, 0f);
        //private static readonly Texture2D ArrowIcon = ContentFinder<Texture2D>.Get("Arrow");
        private const float ArrowExtraWidth = 16f;

        public FloatSubMenu(string label, List<FloatMenuOption> subOptions, string subTitle = null, MenuOptionPriority priority = MenuOptionPriority.Default, Thing revalidateClickTarget = null, float extraPartWidth = 0, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0) 
            : base(label, NoAction, priority, null, revalidateClickTarget, extraPartWidth + ArrowExtraWidth, null, revalidateWorldClickTarget, playSelectionSound, orderInPriority)
        {
            this.subOptions = subOptions;
            this.subTitle = subTitle;
            extraPartOnGUIOuter = extraPartOnGUI;
            extraPartWidthOuter = extraPartWidth;
            this.extraPartOnGUI = DrawExtra;
        }

        public FloatSubMenu(string label, List<FloatMenuOption> subOptions, ThingDef shownItemForIcon, string subTitle = null, MenuOptionPriority priority = MenuOptionPriority.Default, Thing revalidateClickTarget = null, float extraPartWidth = 0, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0) 
            : base(label, NoAction, shownItemForIcon, priority, null, revalidateClickTarget, extraPartWidth + ArrowExtraWidth, null, revalidateWorldClickTarget, playSelectionSound, orderInPriority)
        {
            this.subOptions = subOptions;
            this.subTitle = subTitle;
            extraPartOnGUIOuter = extraPartOnGUI;
            extraPartWidthOuter = extraPartWidth;
            this.extraPartOnGUI = DrawExtra;
        }

        public FloatSubMenu(string label, List<FloatMenuOption> subOptions, Texture2D itemIcon, Color iconColor, string subTitle = null, MenuOptionPriority priority = MenuOptionPriority.Default, Thing revalidateClickTarget = null, float extraPartWidth = 0, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0) 
            : base(label, NoAction, itemIcon, iconColor, priority, null, revalidateClickTarget, extraPartWidth + ArrowExtraWidth, null, revalidateWorldClickTarget, playSelectionSound, orderInPriority)
        {
            this.subOptions = subOptions;
            this.subTitle = subTitle;
            extraPartOnGUIOuter = extraPartOnGUI;
            extraPartWidthOuter = extraPartWidth;
            this.extraPartOnGUI = DrawExtra;
        }

        private static void NoAction() { }

        public bool DrawExtra(Rect rect)
        {
            extraPartOnGUIOuter?.Invoke(rect.LeftPartPixels(extraPartWidthOuter));
            DrawArrow(rect.RightPartPixels(ArrowExtraWidth));
            return false;
        }

        private static void DrawArrow(Rect arrow)
        {
            arrow.width /= 2f;
            arrow.x += arrow.width;
            arrow.height /= 2f;
            arrow.y += arrow.height / 2f;

            GameFont font = Text.Font;
            Color color = GUI.color;

            Text.Font = (font > GameFont.Tiny) ? font - 1 : GameFont.Tiny;
            GUI.color = new Color(color.r, color.g, color.b, color.a * 0.75f);
            Widgets.Label(arrow, ">");

            Text.Font = font;
            GUI.color = color;
        }

        public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
        {
            MouseArea mouseArea = FindMouseArea(rect, floatMenu);

            if (mouseArea == (open ? MouseArea.Menu : MouseArea.Option))
            {
                MouseAction(rect, !open);
            }

            base.DoGUI(rect, colonistOrdering, floatMenu);
            return subMenuExecuted;
        }

        private enum MouseArea { Option, Menu, Outside }

        private MouseArea FindMouseArea(Rect option, FloatMenu menu)
        {
            option.height--;
            if (Mouse.IsOver(option)) { return MouseArea.Option; }

            // As the current window being drawn, origo will be relative to menu
            Rect menuRect = new Rect(0f, 0f, menu.windowRect.width, menu.windowRect.height);
            return Mouse.IsOver(menuRect) ? MouseArea.Menu : MouseArea.Outside;
        }

        public void MouseAction(Rect rect, bool enter)
        {
            Vector2 localPos = new Vector2(rect.xMax, rect.yMin);
            if (enter || !MouseInSubMenu(localPos))
            {
                open = enter;
                if (open)
                {
                    Vector2 mouse = Event.current.mousePosition;
                    Main.Instance.Logger.Message($"Opening sub menu for {Label}, {rect}, mouse: {mouse}");
                    Vector2 offset = localPos - mouse + MenuOffset;
                    subMenu = new FloatSubMenuInner(subOptions, subTitle, offset);
                    Find.WindowStack.Add(subMenu);
                }
                else
                {
                    Main.Instance.Logger.Message($"Closing sub menu for {Label}");
                    Find.WindowStack.TryRemove(subMenu);
                    subMenu = null;
                }
            }
        }

        private bool MouseInSubMenu(Vector2 localPos)
        {
            if (!open) { return false; }
            Rect menu = subMenu.windowRect;
            menu.x = localPos.x;
            menu.y = localPos.y;
            //Main.Instance.Logger.Message($"MouseInSubMenu for {Label}, {menu}, mouse: {Event.current.mousePosition}");
            return menu.Contains(Event.current.mousePosition);
        }

        private class FloatSubMenuInner : FloatMenu
        {
            public Vector2 MouseOffset;

            public FloatSubMenuInner(List<FloatMenuOption> options, string title, Vector2 MouseOffset) : base(options, title, false) 
            {
                this.MouseOffset = MouseOffset;
                // TODO: support vanishIfMouseDistant = true
                vanishIfMouseDistant = false;
                onlyOneOfTypeAllowed = false;
            }

            protected override void SetInitialSizeAndPosition()
            {
                Vector2 pos = UI.MousePositionOnUIInverted + MouseOffset;
                float x = Mathf.Min(pos.x, UI.screenWidth - InitialSize.x);
                float y = Mathf.Min(pos.y, UI.screenHeight - InitialSize.y);

                windowRect = new Rect(x, y, InitialSize.x, InitialSize.y);
                Main.Instance.Logger.Message($"* SetInitialSizeAndPosition windowRect = {windowRect}, mouse = {UI.MousePositionOnUIInverted}");
            }
        }
    }
}
