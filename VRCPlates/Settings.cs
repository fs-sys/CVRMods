using MelonLoader;

namespace VRCPlates;

internal static class Settings
{
    public static void Init()
    {
        var melonPreferencesCategory = MelonPreferences.CreateCategory("ClassicNameplates");
        Enabled = melonPreferencesCategory.CreateEntry("_enabled", true, "Enable Old Nameplates");
        ModernMovement = melonPreferencesCategory.CreateEntry("_modernMovement", true, "Enable Modern Nameplates Movement");

        Offset = melonPreferencesCategory.CreateEntry("_offset", .3f, "Height Offset");
        Scale = melonPreferencesCategory.CreateEntry("_scale", 1f, "Plate Scale");

        PlateColor = melonPreferencesCategory.CreateEntry("_plateColor", "#00FF00", "Plate Color");
        NameColor = melonPreferencesCategory.CreateEntry("_nameColor", "#FFFFFF", "Name Color");

        PlateColorByRank = melonPreferencesCategory.CreateEntry("_plateColorByRank", false, "Rank Color Plate");
        NameColorByRank = melonPreferencesCategory.CreateEntry("_nameColorByRank", false, "Rank Color Name");
        
        BtkColorPlates = melonPreferencesCategory.CreateEntry("_btkColorPlates", false, "Random Color Plates");
        BtkColorNames = melonPreferencesCategory.CreateEntry("_btkColorNames", false, "Random Color Names");

        ShowRank = melonPreferencesCategory.CreateEntry("_showRank", true, "Show Rank");
        ShowVoiceBubble = melonPreferencesCategory.CreateEntry("_showVoiceBubble", true, "Show Voice Bubble");
        ShowIcon = melonPreferencesCategory.CreateEntry("_showIcon", true, "Show User Icon");

        RainbowPlates = melonPreferencesCategory.CreateEntry("_rainbowPlates", false, "owo","Hidden Rainbows~", true);
        RainbowFriends = melonPreferencesCategory.CreateEntry("_rainbowFriends", false, "fren", "Fren only rainbows~", true);
        RainbowDelay = melonPreferencesCategory.CreateEntry("_rainbowSpeed", .5f, "owodelay", "Delay between rainbow colors", true);
    }


    public static MelonPreferences_Entry<bool>? Enabled;
    public static MelonPreferences_Entry<bool>? ModernMovement;

    public static MelonPreferences_Entry<float>? Offset;
    public static MelonPreferences_Entry<float>? Scale;

    public static MelonPreferences_Entry<string>? PlateColor;
    public static MelonPreferences_Entry<string>? NameColor;

    public static MelonPreferences_Entry<bool>? PlateColorByRank;
    public static MelonPreferences_Entry<bool>? NameColorByRank;

    public static MelonPreferences_Entry<bool>? BtkColorPlates;
    public static MelonPreferences_Entry<bool>? BtkColorNames;
    
    public static MelonPreferences_Entry<bool>? ShowRank;
    public static MelonPreferences_Entry<bool>? ShowVoiceBubble;
    public static MelonPreferences_Entry<bool>? ShowIcon;

    public static MelonPreferences_Entry<bool>? RainbowPlates;
    public static MelonPreferences_Entry<bool>? RainbowFriends;
    public static MelonPreferences_Entry<float>? RainbowDelay;
}