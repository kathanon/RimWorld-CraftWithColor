using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CraftWithColor
{
    internal class MySettings
    {
        public static SettingHandle<bool> OnlyStandard;
        public static SettingHandle<bool> RequireDye;

        private static SettingHandle<bool> styling;

        public static bool Styling { get => styling && !OnlyStandard; }

        public static MultiRange ConflictingCheckboxRange { get; private set; } = new MultiRange();
        public static List<string> ConflictingMods { get; private set; } = new List<string>();

        public static void Setup(ModSettingsPack pack)
        {
            OnlyStandard = pack.GetHandle("onlyStandard", Strings.OnlyStandard_title, Strings.OnlyStandard_desc, false);
            styling      = pack.GetHandle("styling",      Strings.Styling_title,      Strings.Styling_desc,      true);
            RequireDye   = pack.GetHandle("requireDye",   Strings.RequireDye_title,   Strings.RequireDye_desc,   true);

            styling.VisibilityPredicate = () => !OnlyStandard;
            RequireDye.ValueChanged += (_) => State.UpdateAll();

            var dict = Strings.MODS_CONFLICTING_WITH_CHECKBOX_POS;
            foreach (var mod in ModLister.AllInstalledMods.Where(m => dict.ContainsKey(m.PackageIdNonUnique) && m.Active))
            {
                ConflictingCheckboxRange.Merge(dict[mod.PackageIdNonUnique]);
                ConflictingMods.Add(mod.Name);
            }
        }
    }
}
