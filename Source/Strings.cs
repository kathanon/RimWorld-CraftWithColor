using System.Collections.Generic;
using Verse;

namespace CraftWithColor
{
    internal static class Strings
    {
        // Non-translated constants
        public const string MOD_IDENTIFIER = "kathanon.CraftWithColor";
        public const string PREFIX = MOD_IDENTIFIER + ".";
        public static readonly Dictionary<string, Range> MODS_CONFLICTING_WITH_CHECKBOX_POS = new Dictionary<string, Range>
        {
            // Range is distance from bottom margin of dialog
            { "falconne.bwm", new Range(67f, 30f) }
        };

        // Menus and dialogs
        public static readonly string Select      = (PREFIX + "Select"     ).Translate();
        public static readonly string SavedColors = (PREFIX + "SavedColors").Translate();
        public static readonly string Favorite    = (PREFIX + "Favorite"   ).Translate();
        public static readonly string Ideoligion  = (PREFIX + "Ideoligion" ).Translate();
        public static readonly string DyeItem     = (PREFIX + "DyeItem"    ).Translate();
        public static readonly string SelectColor = (PREFIX + "SelectColor").Translate();
        public static readonly string R           = (PREFIX + "R"          ).Translate();
        public static readonly string G           = (PREFIX + "G"          ).Translate();
        public static readonly string B           = (PREFIX + "B"          ).Translate();
        public static readonly string Standard    = (PREFIX + "Standard"   ).Translate();
        public static readonly string Saved       = (PREFIX + "Saved"      ).Translate();
        public static readonly string Delete      = (PREFIX + "Delete"     ).Translate();
        public static readonly string Save        = (PREFIX + "Save"       ).Translate();
        public static readonly string Cancel      = (PREFIX + "Cancel"     ).Translate();
        public static readonly string Accept      = (PREFIX + "Accept"     ).Translate();
        public static readonly string More        = (PREFIX + "More"       ).Translate();

        // Settings
        public static readonly string OnlyStandard_title = (PREFIX + "OnlyStandard.title").Translate();
        public static readonly string OnlyStandard_desc  = (PREFIX + "OnlyStandard.desc" ).Translate();
        public static readonly string Styling_title      = (PREFIX + "Styling.title"     ).Translate();
        public static readonly string Styling_desc       = (PREFIX + "Styling.desc"      ).Translate();
        public static readonly string RequireDye_title   = (PREFIX + "RequireDye.title"  ).Translate();
        public static readonly string RequireDye_desc    = (PREFIX + "RequireDye.desc"   ).Translate();
    }
}
