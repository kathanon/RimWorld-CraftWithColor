﻿using HugsLib.Utils;

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

        public override string ModIdentifier => Strings.MOD_IDENTIFIER;

        public override void DefsLoaded()
        {
            MySettings.Setup(Settings);
        }
    }
}
