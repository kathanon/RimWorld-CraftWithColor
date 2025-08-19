using FloatSubMenus;
using MoreWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    public class MySettings : ModSettings {
        public static MySettings Instance;

#if VERSION_1_3
        public const bool DyeRequiresIdeology = true;
        public const bool HasStyleButton = false;
#else
        public const bool DyeRequiresIdeology = false;
        public static bool HasStyleButton => WithIdeology && Find.IdeoManager.classicMode;
#endif
        public enum ColorChangeMode { 
            Keep, 
            Switch, 
            //RequireDye,
        }

        private readonly BoolOption onlyStandard = new BoolOption(
            "onlyStandard",
            Strings.OnlyStandard_title,
            Strings.OnlyStandard_desc,
            false,
            HasIdeology);
        private readonly BoolOption styling = new BoolOption(
            "styling",
            Strings.Styling_title,
            Strings.Styling_desc,
            true, 
            ShowStyling);
        private readonly BoolOption requireDye = new BoolOption(
            "requireDye",
            Strings.RequireDye_title,
            Strings.RequireDye_desc, 
            true, 
            () => WithIdeology || !DyeRequiresIdeology, 
            State.UpdateAll);
        private readonly BoolOption setStyle = new BoolOption(
            "setStyle",
            Strings.SetStyle_title,
            Strings.SetStyle_desc, 
            true,
            HasIdeology);
        private readonly EnumOption<ColorChangeMode> changeMode = new EnumOption<ColorChangeMode>(
            "changeMode",
            Strings.ChangeMode_title,
            Strings.ChangeMode_desc,
            Strings.ChangeMode_prefix, 
            ColorChangeMode.Keep);
        private readonly IOption[] options;
        private float? width;

        public static bool OnlyStandard => Instance?.IntOnlyStandard ?? false;
        public static bool RequireDye   => Instance?.IntRequireDye   ?? false;
        public static bool Styling      => Instance?.IntStyling      ?? false;
        public static bool IdeoSymbols  => Instance?.IntIdeoSymbols  ?? false;
        public static bool SetStyle     => Instance?.IntSetStyle     ?? false;
        public static bool SwitchColor  => Instance?.IntSwitchColor  ?? false;
        public static bool SwitchUseDye => Instance?.IntSwitchUseDye ?? false;

        private bool IntOnlyStandard => WithIdeology && onlyStandard.Value;
        private bool IntRequireDye   => (WithIdeology || !DyeRequiresIdeology) && requireDye.Value;
        private bool IntStyling      => WithIdeology && styling.Value && !onlyStandard.Value;
        private bool IntIdeoSymbols  => true; // TODO
        private bool IntSetStyle     => setStyle.Value && !HasStyleButton;
        private bool IntSwitchColor  => changeMode.Value != ColorChangeMode.Keep;
        private bool IntSwitchUseDye => false;  // Not implemented yet

        public static readonly bool WithIdeology =
            ModLister.GetActiveModWithIdentifier("Ludeon.RimWorld.Ideology") != null;

        private static bool HasIdeology() 
            => WithIdeology;

        private static bool ShowStyling() 
            => !Instance.onlyStandard.Value;

        public static MultiRange ConflictingCheckboxRange { get; private set; } = new MultiRange();
        public static List<string> ConflictingMods { get; private set; } = new List<string>();

        public MySettings() {
            options = new IOption[] { onlyStandard, requireDye, styling, setStyle, changeMode };

            var dict = StaticStrings.OverlappingMods;
            foreach (var mod in ModLister.AllInstalledMods.Where(m => dict.ContainsKey(m.PackageIdNonUnique) && m.Active)) {
                ConflictingCheckboxRange.Merge(dict[mod.PackageIdNonUnique]);
                ConflictingMods.Add(mod.Name);
            }
        }

        public void DoGUI(Rect r) {
            if (width == null) width = options.Max(x => x.Width);
            var row = new Rect(r.x, r.y, width.Value, Widgets.CheckboxSize);
            foreach (var option in options) {
                option.DoGUI(ref row);
            }
        }


        public override void ExposeData() {
            foreach (var option in options) {
                option.ExposeData();
            }
        }

        private interface IOption : IExposable {
            void DoGUI(ref Rect row);
            float Width { get; }
        }

        private abstract class Option<T> : IOption {
            protected readonly string name;
            protected readonly string label;
            protected readonly string tip;
            protected readonly T defaultValue;
            protected readonly Func<bool> visible;
            protected readonly Action onChange;
            protected T value;

            protected Option(string name,
                             string label,
                             string tip,
                             T defaultValue,
                             Func<bool> visible,
                             Action onChange) {
                this.name = name;
                this.label = label;
                this.tip = tip;
                this.defaultValue = defaultValue;
                this.visible = visible;
                this.onChange = onChange;
                value = defaultValue;
            }

            public T Value
                => value;

            public float Width 
                => Text.CalcSize(label).x + 8f + ControlWidth;

            protected abstract float ControlWidth { get; }

            protected abstract void DoControl(Rect row);

            public void DoGUI(ref Rect row) {
                if (visible != null && !visible()) return;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(row, label);
                GenUI.ResetLabelAlign();
                DoControl(row);
                TooltipHandler.TipRegion(row, tip);
                row.StepY(4f);
            }

            public void ExposeData() 
                => Scribe_Values.Look(ref value, name, defaultValue);
        }

        private class BoolOption : Option<bool> {
            public BoolOption(string name,
                              string label,
                              string tip,
                              bool defaultValue,
                              Func<bool> visible = null,
                              Action onChange = null) 
                : base(name, label, tip, defaultValue, visible, onChange) {}

            protected override float ControlWidth => Widgets.CheckboxSize;

            protected override void DoControl(Rect row) {
                bool old = value;
                Widgets.Checkbox(new Vector2(row.xMax - Widgets.CheckboxSize, row.y), ref value);
                if (old != value && onChange != null) onChange(); 
            }
        }

        private class EnumOption<T> : Option<T> where T : Enum {
            private readonly T[] values;
            private readonly Dictionary<T, string> labels;
            private float controlWidth = 0f;

            public EnumOption(string name,
                              string label,
                              string tip,
                              string prefix,
                              T defaultValue,
                              Func<bool> visible = null,
                              Action onChange = null)
                : base(name, label, tip, defaultValue, visible, onChange) {
                values = (T[]) Enum.GetValues(typeof(T));
                labels = values
                    .ToDictionary(x => x, 
                                  x => Strings.EnumLabel(x, prefix));
            }

            protected override float ControlWidth {
                get {
                    if (controlWidth == 0f) {
                        controlWidth = 16f + labels.Values
                            .Max(l => Text.CalcSize(l).x);
                    }
                    return controlWidth;
                }
            }

            protected override void DoControl(Rect row) {
                if (Widgets.ButtonText(row.RightPartPixels(controlWidth), labels[value])) {
                    values.Select(x => new FloatMenuOption(labels[x], () => value = x)).OpenMenu();
                }
            }
        }
    }
}
