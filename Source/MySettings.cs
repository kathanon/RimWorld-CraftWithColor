using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CraftWithColor
{
    public class MySettings
    {
        public enum ColorChangeMode { Keep, Switch, RequireDye }
        public enum ColorChangeModeNoIdeo { Keep, Switch }

        private static SettingHandle<bool> onlyStandard;
        private static SettingHandle<bool> requireDye;
        private static SettingHandle<bool> styling;
        private static SettingHandle<bool> setStyle;
        private static SettingHandle<ColorChangeMode> changeMode;
        private static SettingHandle<ColorChangeModeNoIdeo> changeModeNoIdeo;

        public static bool OnlyStandard => WithIdeology && onlyStandard;

        public static bool RequireDye   => WithIdeology && requireDye;

        public static bool Styling      => WithIdeology && styling && !onlyStandard;

        public static bool SetStyle     => setStyle;

        public static bool SwitchColor
        {
            get => WithIdeology 
                ? changeMode != ColorChangeMode.Keep 
                : changeModeNoIdeo != ColorChangeModeNoIdeo.Keep;
        }

        public static bool SwitchUseDye
        {
            get => WithIdeology && requireDye && changeMode == ColorChangeMode.RequireDye;
        }

        public static readonly bool WithIdeology = ModLister.IdeologyInstalled;

        public static MultiRange ConflictingCheckboxRange { get; private set; } = new MultiRange();
        public static List<string> ConflictingMods { get; private set; } = new List<string>();

        public static void Setup(ModSettingsPack pack)
        {
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
                requireDye.ValueChanged += (_) => State.UpdateAll();
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
            if (WithIdeology)
            {
                changeMode.ValueChanged += (_) =>
                {
                    if (changeMode != ColorChangeMode.RequireDye)
                    {
                        changeModeNoIdeo.Value =
                            (changeMode == ColorChangeMode.Keep)
                            ? ColorChangeModeNoIdeo.Keep
                            : ColorChangeModeNoIdeo.Switch;
                    }
                };
                changeModeNoIdeo.ValueChanged += (_) => { 
                    if (changeMode != ColorChangeMode.RequireDye)
                    {
                        changeMode.Value = 
                            (changeModeNoIdeo == ColorChangeModeNoIdeo.Keep) 
                            ? ColorChangeMode.Keep 
                            : ColorChangeMode.Switch;
                    }
                };
            }

            var dict = Strings.MODS_CONFLICTING_WITH_CHECKBOX_POS;
            foreach (var mod in ModLister.AllInstalledMods.Where(m => dict.ContainsKey(m.PackageIdNonUnique) && m.Active))
            {
                ConflictingCheckboxRange.Merge(dict[mod.PackageIdNonUnique]);
                ConflictingMods.Add(mod.Name);
            }
        }
    }
}
