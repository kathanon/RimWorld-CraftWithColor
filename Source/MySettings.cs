using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;
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
        public enum ColorChangeMode { Keep, Switch, RequireDye }
        public enum ColorChangeModeNoIdeo { Keep, Switch }

        private static SettingHandle<bool> onlyStandard;
        private static SettingHandle<bool> requireDye;
        private static SettingHandle<bool> styling;
        private static SettingHandle<bool> setStyle;
        private static SettingHandle<ColorChangeMode> changeMode;
        private static SettingHandle<ColorChangeModeNoIdeo> changeModeNoIdeo;

        private static bool getOnlyStandard => onlyStandard ?? false;
        private static bool getRequireDye => requireDye ?? true;
        private static bool getStyling => styling ?? true;
        private static bool getSetStyle => setStyle ?? true;
        private static ColorChangeMode getChangeMode =>
            changeMode ?? ColorChangeMode.Keep;
        private static ColorChangeModeNoIdeo getChangeModeNoIdeo =>
            changeModeNoIdeo ?? ColorChangeModeNoIdeo.Keep;

        public static bool OnlyStandard => WithIdeology && getOnlyStandard;

        public static bool RequireDye => (WithIdeology || !DyeRequiresIdeology) && getRequireDye;

        public static bool Styling => WithIdeology && getStyling && !getOnlyStandard;

        public static bool IdeoSymbols => true; // TODO

        public static bool SetStyle => getSetStyle && !HasStyleButton;

        public static bool SwitchColor {
            get => WithIdeology
                ? getChangeMode != ColorChangeMode.Keep
                : getChangeModeNoIdeo != ColorChangeModeNoIdeo.Keep;
        }

        public static bool SwitchUseDye {
            get => WithIdeology && getRequireDye && getChangeMode == ColorChangeMode.RequireDye;
        }

        public static readonly bool WithIdeology =
            ModLister.GetActiveModWithIdentifier("Ludeon.RimWorld.Ideology") != null;

        public static MultiRange ConflictingCheckboxRange { get; private set; } = new MultiRange();
        public static List<string> ConflictingMods { get; private set; } = new List<string>();

        private static bool setupDone = false;

        public static void Setup(ModSettingsPack pack) {
            if (setupDone || Instance == null) return;
            setupDone = true;

            if (WithIdeology) {
                onlyStandard = pack.GetHandle(
                    "onlyStandard",
                    Strings.OnlyStandard_title,
                    Strings.OnlyStandard_desc,
                    false);
                styling = pack.GetHandle(
                    "styling",
                    Strings.Styling_title,
                    Strings.Styling_desc,
                    true);
            }
            if (WithIdeology || !DyeRequiresIdeology) {
                requireDye = pack.GetHandle(
                    "requireDye",
                    Strings.RequireDye_title,
                    Strings.RequireDye_desc,
                    true);
            }

            setStyle = pack.GetHandle(
                "setStyle",
                Strings.SetStyle_title,
                Strings.SetStyle_desc,
                true);

            if (WithIdeology) {
                changeMode = pack.GetHandle(
                    "changeMode",
                    Strings.ChangeMode_title,
                    Strings.ChangeMode_desc,
                    ColorChangeMode.Keep,
                    enumPrefix: Strings.ChangeMode_prefix);

                styling.VisibilityPredicate = () => !onlyStandard;
                requireDye.ValueChanged += _ => State.UpdateAll();
                // Disabled until ColorChangeMode.RequireDye is implemented
                changeMode.VisibilityPredicate = () => requireDye && false;
            }

            changeModeNoIdeo = pack.GetHandle(
                "changeModeNoIdeo",
                Strings.ChangeMode_title,
                Strings.ChangeMode_desc,
                ColorChangeModeNoIdeo.Keep,
                enumPrefix: Strings.ChangeMode_prefix);

            // Locked-in until ColorChangeMode.RequireDye is implemented
            changeModeNoIdeo.VisibilityPredicate = () => !RequireDye || true;
            if (WithIdeology) {
                changeMode.ValueChanged += (_) => {
                    if (changeMode != ColorChangeMode.RequireDye) {
                        changeModeNoIdeo.Value =
                            (changeMode == ColorChangeMode.Keep)
                            ? ColorChangeModeNoIdeo.Keep
                            : ColorChangeModeNoIdeo.Switch;
                    }
                };
                changeModeNoIdeo.ValueChanged += (_) => {
                    if (changeMode != ColorChangeMode.RequireDye) {
                        changeMode.Value =
                            (changeModeNoIdeo == ColorChangeModeNoIdeo.Keep)
                            ? ColorChangeMode.Keep
                            : ColorChangeMode.Switch;
                    }
                };
            }

            var dict = Strings.OverlapingMods;
            foreach (var mod in ModLister.AllInstalledMods.Where(m => dict.ContainsKey(m.PackageIdNonUnique) && m.Active)) {
                ConflictingCheckboxRange.Merge(dict[mod.PackageIdNonUnique]);
                ConflictingMods.Add(mod.Name);
            }

            Instance.Write();
        }

        public override void ExposeData() {
            if (Scribe.mode != LoadSaveMode.Saving) return;

            bool onlyStandard = MySettings.onlyStandard ?? false;
            bool requireDye   = MySettings.requireDye ?? false;
            bool styling      = MySettings.styling ?? false;
            bool setStyle     = MySettings.setStyle;
            var changeMode    = (ColorChangeMode) changeModeNoIdeo.Value;

            Scribe_Values.Look(ref onlyStandard, "onlyStandard", false);
            Scribe_Values.Look(ref requireDye,   "requireDye",   true);
            Scribe_Values.Look(ref styling,      "styling",      true);
            Scribe_Values.Look(ref setStyle,     "setStyle",     true);
            Scribe_Values.Look(ref changeMode,   "changeMode",   ColorChangeMode.Keep);
        }
    }
}
