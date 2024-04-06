using HarmonyLib;
using HugsLib.Utils;
using RimWorld;
using Verse;


namespace CraftWithColor
{
    public class Main : HugsLib.ModBase {
        public Main() 
            => Instance = this;

        internal new ModLogger Logger => base.Logger;

        internal static Main Instance { get; private set; }

        public override string ModIdentifier => Strings.ID;

        public override void DefsLoaded() 
            => MySettings.Setup(Settings);

        public override void SettingsChanged() 
            => MySettings.Instance.Write();
    }

    public class ModMain : Mod {
        public ModMain(ModContentPack content) : base(content) {
            MySettings.Instance = GetSettings<MySettings>();
            Main.Instance?.DefsLoaded();
        }
    }
}
