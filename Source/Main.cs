using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    [StaticConstructorOnStartup]
    public class ModMain : Mod {
        private static ModMain Instance;

        static ModMain() 
            => new Harmony(StaticStrings.ID).PatchAll();

        public ModMain(ModContentPack content) : base(content) 
            => Instance = this;

        public override string SettingsCategory() 
            => StaticStrings.Name;

        public override void DoSettingsWindowContents(Rect inRect) 
            => MySettings.Instance.DoGUI(inRect);

        public static void OnInit() 
            => MySettings.Instance = Instance.GetSettings<MySettings>();
    }

    [HarmonyPatch]
    public static class InitHook {
        [HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.Init))]
        [HarmonyPostfix]
        public static void Init() 
            => ModMain.OnInit();
    }
}
