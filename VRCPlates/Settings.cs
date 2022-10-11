using MelonLoader;

namespace VRCPlates;

internal static class Settings
{
    public static void Init()
    {
        var melonPreferencesCategory = MelonPreferences.CreateCategory("VRCPlates");

        Enabled = melonPreferencesCategory.CreateEntry("_enabled", true, "Enable Old Nameplates");
        Enabled.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                if (nameplate.Value!.Player != null && nameplate.Value.Player.PlayerNameplate != null)
                {
                    NameplateManager.OnEnableToggle(nameplate.Value.Player.PlayerNameplate, nameplate.Value);
                }

                nameplate.Value.OnVisibilityUpdate();
            }
        });

        Offset = melonPreferencesCategory.CreateEntry("_offset", .35f, "Height Offset");
        Offset.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnOffsetUpdate();
            }
        });

        Scale = melonPreferencesCategory.CreateEntry("_scale", 1f, "Plate Scale");
        Scale.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnScaleUpdate();
            }
        });

        PlateColor = melonPreferencesCategory.CreateEntry("_plateColor", "#00FF00", "Plate Color");
        PlateColor.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.ApplySettings();
            }
        });

        NameColor = melonPreferencesCategory.CreateEntry("_nameColor", "#FFFFFF", "Name Color");
        NameColor.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.ApplySettings();
            }
        });

        PlateColorByRank = melonPreferencesCategory.CreateEntry("_plateColorByRank", false, "Rank Color Plate");
        PlateColorByRank.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnPlateColorUpdate();
            }
        });

        NameColorByRank = melonPreferencesCategory.CreateEntry("_nameColorByRank", false, "Rank Color Name");
        NameColorByRank.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnNameColorUpdate();
            }
        });

        BtkColorPlates = melonPreferencesCategory.CreateEntry("_btkColorPlates", false, "Random Color Plates");
        BtkColorPlates.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnPlateColorUpdate();
            }
        });

        BtkColorNames = melonPreferencesCategory.CreateEntry("_btkColorNames", false, "Random Color Names");
        BtkColorNames.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnNameColorUpdate();
            }
        });

        ShowRank = melonPreferencesCategory.CreateEntry("_showRank", true, "Show Rank");
        ShowRank.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnShowRankToggle();
            }
        });

        ShowVoiceBubble = melonPreferencesCategory.CreateEntry("_showVoiceBubble", true, "Show Voice Bubble");
        ShowVoiceBubble.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnVoiceBubbleToggle();
            }
        });

        ModernMovement = melonPreferencesCategory.CreateEntry("_modernMovement", true, "Enable Head Follow [Buggy if Disabled]");
        ModernMovement.OnEntryValueChanged.Subscribe((_, _) =>
        {
            if (VRCPlates.NameplateManager == null) return;
            foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
            {
                nameplate.Value!.OnModernMovementToggle();
            }
        });
        
        RateLimit = melonPreferencesCategory.CreateEntry("_rateLimit", 2f, "Image Rate Limit");
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

    public static MelonPreferences_Entry<float>? RateLimit;

    //public static MelonPreferences_Entry<bool>? RainbowPlates;
    //public static MelonPreferences_Entry<bool>? RainbowFriends;
    //public static MelonPreferences_Entry<float>? RainbowDelay;
}