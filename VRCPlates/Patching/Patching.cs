
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using HarmonyLib;
using VRCPlates.Reflection;

namespace VRCPlates.Patching;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class Patching
{
    private static readonly HarmonyLib.Harmony _instance = new HarmonyLib.Harmony("VRCPlates");

    //Many patches are based on VRChat Utility Kit. Thank you Sleepers and loukylor.
    public static void Init()
    {
        var _HandleRelations =
            typeof(ViewManager).GetMethod("HandleRelations", BindingFlags.NonPublic | BindingFlags.Instance);
        var _onRelations =
            typeof(Patching).GetMethod(nameof(OnRelations), BindingFlags.NonPublic | BindingFlags.Static);

        var _AvatarInstantiated =
            typeof(PuppetMaster).GetMethod("AvatarInstantiated", BindingFlags.Public | BindingFlags.Instance);
        var _onAvatarInstantiated =
            typeof(Patching).GetMethod(nameof(OnAvatarInstantiated), BindingFlags.NonPublic | BindingFlags.Static);

        var _ReloadAllNameplates = typeof(CVRPlayerManager).GetMethod("ReloadAllNameplates", BindingFlags.NonPublic | BindingFlags.Static);
        var _onReloadAllNameplates = typeof(Patching).GetMethod(nameof(OnReloadAllNameplates), BindingFlags.NonPublic | BindingFlags.Static);
        
        if (_HandleRelations != null && _onRelations != null)
        {
            _instance.Patch(_HandleRelations, null, new HarmonyMethod(_onRelations));
        }
        else
        {
            VRCPlates.Error("Failed to patch HandleRelations");
        }

        if (_AvatarInstantiated != null && _onAvatarInstantiated != null)
        {
            _instance.Patch(_AvatarInstantiated, null, new HarmonyMethod(_onAvatarInstantiated));
        }
        else
        {
            VRCPlates.Error("Failed to patch AvatarInstantiated");
        }

        if (_ReloadAllNameplates != null && _onReloadAllNameplates != null)
        {
            _instance.Patch(_ReloadAllNameplates, null, new HarmonyMethod(_onReloadAllNameplates));
        }
        else
        {
            VRCPlates.Error("Failed to patch ReloadAllNameplates");
        }
    }

    private static void OnRelations(ViewManager __instance, string __0, string __1)
    {
        switch (__1)
        {
            case "Add":
            {
                var nameplate = VRCPlates.NameplateManager?.GetNameplate(__0);
                if (nameplate != null)
                {
                    nameplate.IsFriend = true;
                }

                return;
            }
            case "Deny":
            {
                return;
            }
            case "Unfriend":
            {
                var nameplate = VRCPlates.NameplateManager?.GetNameplate(__0);
                if (nameplate != null)
                {
                    nameplate.IsFriend = false;
                }

                return;
            }
            case "Block":
            {
                var nameplate = VRCPlates.NameplateManager?.GetNameplate(__0);
                if (nameplate != null)
                {
                    nameplate.IsBlocked = true;
                }

                return;
            }
            case "Unblock":
            {
                var nameplate = VRCPlates.NameplateManager?.GetNameplate(__0);
                if (nameplate != null)
                {
                    nameplate.IsBlocked = false;
                }

                return;
            }
            case "RequestInvite":
            {
                return;
            }
        }
    }
    
    private static void OnAvatarInstantiated(PuppetMaster __instance)
    {
        var descriptor = __instance.GetPlayerDescriptor();
        if (descriptor != null)
        {
            if (VRCPlates.NameplateManager != null) VRCPlates.NameplateManager.CreateNameplate(descriptor); 
        }
        else
        {
            VRCPlates.Error("Failed to get player descriptor for avatar " + __instance.name);
        }
    }
    
    private static void OnReloadAllNameplates()
    {
        if (VRCPlates.NameplateManager == null) return;
        foreach (var pair in VRCPlates.NameplateManager.Nameplates)
        {
            VRCPlates.Debug("Reloading Nameplate: " + pair.Key);
            if (pair.Value != null) pair.Value.ApplySettings();
        }
    }
}