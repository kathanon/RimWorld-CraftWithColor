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

        public override string ModIdentifier => "kathanon.CraftWithColor";

        public override void DefsLoaded()
        {
            // TODO: load settings
        }
    }
}
