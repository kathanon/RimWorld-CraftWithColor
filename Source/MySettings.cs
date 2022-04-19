using HugsLib.Settings;
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

        public static void Setup(ModSettingsPack pack)
        {
            OnlyStandard = pack.GetHandle("onlyStandard", Strings.OnlyStandard_title, Strings.OnlyStandard_desc, false);
            styling      = pack.GetHandle("styling",      Strings.Styling_title,      Strings.Styling_desc,      true);
            RequireDye   = pack.GetHandle("requireDye",   Strings.RequireDye_title,   Strings.RequireDye_desc,   true);

            styling.VisibilityPredicate = () => !OnlyStandard;
            RequireDye.ValueChanged += (_) => State.UpdateAll();
        }
    }
}
