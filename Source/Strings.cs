﻿using System;
using System.Collections.Generic;
using Verse;

namespace CraftWithColor
{
    internal static class Strings
    {
        // Non-translated constants
        public const string ID = "kathanon.CraftWithColor";
        public const string PREFIX = ID + ".";
        public static readonly string DEF = PREFIX.Replace('.', '_');

        public const string BWM_ID      = "falconne.bwm";
        public const string BWM_TEMP_ID = "falconne.bwm.tempupdate";
        public const string MATH_ID     = "crunchyduck.math";
        public static bool IsBwmId(string id) => id == BWM_ID || id == BWM_TEMP_ID;

        public static readonly Dictionary<string, Range> OverlapingMods = new Dictionary<string, Range>
        {
            // Range is distance from bottom margin of dialog
            { BWM_ID,      new Range(60f, 60f) },
            { BWM_TEMP_ID, new Range(60f, 60f) },
        };

        // Menus and dialogs
        public static readonly string Select       = (PREFIX + "Select"      ).Translate();
        public static readonly string SavedColors  = (PREFIX + "SavedColors" ).Translate();
        public static readonly string Favorite     = (PREFIX + "Favorite"    ).Translate();
        public static readonly string Ideoligion   = (PREFIX + "Ideoligion"  ).Translate();
        public static readonly string Random       = (PREFIX + "Random"      ).Translate();
        public static readonly string DyeItem      = (PREFIX + "DyeItem"     ).Translate();
        public static readonly string StyleItem    = (PREFIX + "StyleItem"   ).Translate();
        public static readonly string RandomAny    = (PREFIX + "RandomAny"   ).Translate();
        public static readonly string RandomIdeo   = (PREFIX + "RandomIdeo"  ).Translate();
        public static readonly string RandomFavo   = (PREFIX + "RandomFavo"  ).Translate();
        public static readonly string RandomSaved  = (PREFIX + "RandomSaved" ).Translate();
        public static readonly string RandomStd    = (PREFIX + "RandomStd"   ).Translate();
        public static readonly string BasicStyle   = (PREFIX + "BasicStyle"  ).Translate();
        public static readonly string RandomStyle  = (PREFIX + "RandomStyle" ).Translate();
        public static readonly string SelectColor  = (PREFIX + "SelectColor" ).Translate();
        public static readonly string R            = (PREFIX + "R"           ).Translate();
        public static readonly string G            = (PREFIX + "G"           ).Translate();
        public static readonly string B            = (PREFIX + "B"           ).Translate();
        public static readonly string Standard     = (PREFIX + "Standard"    ).Translate();
        public static readonly string Saved        = (PREFIX + "Saved"       ).Translate();
        public static readonly string Delete       = (PREFIX + "Delete"      ).Translate();
        public static readonly string Save         = (PREFIX + "Save"        ).Translate();
        public static readonly string Cancel       = (PREFIX + "Cancel"      ).Translate();
        public static readonly string Accept       = (PREFIX + "Accept"      ).Translate();
        public static readonly string More         = (PREFIX + "More"        ).Translate();
        public static readonly string NoSpaceError = (PREFIX + "NoSpaceError").Translate();

        // Settings
        public static readonly string OnlyStandard_title = (PREFIX + "OnlyStandard.title").Translate();
        public static readonly string OnlyStandard_desc  = (PREFIX + "OnlyStandard.desc" ).Translate();
        public static readonly string Styling_title      = (PREFIX + "Styling.title"     ).Translate();
        public static readonly string Styling_desc       = (PREFIX + "Styling.desc"      ).Translate();
        public static readonly string SetStyle_title     = (PREFIX + "SetStyle.title"    ).Translate();
        public static readonly string SetStyle_desc      = (PREFIX + "SetStyle.desc"     ).Translate();
        public static readonly string RequireDye_title   = (PREFIX + "RequireDye.title"  ).Translate();
        public static readonly string RequireDye_desc    = (PREFIX + "RequireDye.desc"   ).Translate();
        public static readonly string ChangeMode_title   = (PREFIX + "ChangeMode.title"  ).Translate();
        public static readonly string ChangeMode_desc    = (PREFIX + "ChangeMode.desc"   ).Translate();
        public static readonly string ChangeMode_prefix  =  PREFIX + "ChangeMode.";
    }
}
