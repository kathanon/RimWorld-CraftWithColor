using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CraftWithColor
{
    public class MySettings
    {
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
        private static bool getRequireDye   => requireDye   ?? true;
        private static bool getStyling      => styling      ?? true;
        private static bool getSetStyle     => setStyle     ?? true;
        private static ColorChangeMode getChangeMode => 
            changeMode ?? ColorChangeMode.Keep;
        private static ColorChangeModeNoIdeo getChangeModeNoIdeo => 
            changeModeNoIdeo ?? ColorChangeModeNoIdeo.Keep;

        public static bool OnlyStandard => WithIdeology && getOnlyStandard;

        public static bool RequireDye   => (WithIdeology || !DyeRequiresIdeology) && getRequireDye;

        public static bool Styling      => WithIdeology && getStyling && !getOnlyStandard;

        public static bool SetStyle     => getSetStyle && !HasStyleButton;

        public static bool SwitchColor
        {
            get => WithIdeology 
                ? getChangeMode != ColorChangeMode.Keep 
                : getChangeModeNoIdeo != ColorChangeModeNoIdeo.Keep;
        }

        public static bool SwitchUseDye
        {
            get => WithIdeology && getRequireDye && getChangeMode == ColorChangeMode.RequireDye;
        }

        public static readonly bool WithIdeology = 
            ModLister.GetActiveModWithIdentifier("Ludeon.RimWorld.Ideology") != null;

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

            var dict = Strings.OverlapingMods;
            foreach (var mod in ModLister.AllInstalledMods.Where(m => dict.ContainsKey(m.PackageIdNonUnique) && m.Active))
            {
                ConflictingCheckboxRange.Merge(dict[mod.PackageIdNonUnique]);
                ConflictingMods.Add(mod.Name);
            }
        }
    }
}
