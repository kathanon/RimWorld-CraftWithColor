using HugsLib.Utils;
using RimWorld;

namespace CraftWithColor
{
    public class Main : HugsLib.ModBase
    {
        public Main()
        {
            Instance = this;
        }

        internal new ModLogger Logger => base.Logger;

        internal static Main Instance { get; private set; }

        public const string ModId = "kathanon.CraftWithColor";

        public override string ModIdentifier => ModId;

        public override void DefsLoaded()
        {
            // TODO: load settings
        }
    }
}
