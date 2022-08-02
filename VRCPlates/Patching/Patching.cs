
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using HarmonyLib;
using MelonLoader;
using VRCPlates.Reflection;

namespace VRCPlates.Patching;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class Patching
{
    private static readonly HarmonyLib.Harmony _instance = new HarmonyLib.Harmony("VRCPlates");

    //Many patches are based on VRChat Utility Kit. Thank you Sleepers and loukylor.
    public static void Init()
    {
        var _ReloadFriends = typeof(Friends).GetMethod("ReloadFriends", BindingFlags.Public | BindingFlags.Static);
        var _onReloadFriends = typeof(Patching).GetMethod(nameof(OnReloadFriends), BindingFlags.NonPublic | BindingFlags.Static);

        var _HandleRelations =
            typeof(ViewManager).GetMethod("HandleRelations", BindingFlags.NonPublic | BindingFlags.Instance);
        var _onRelations =
            typeof(Patching).GetMethod(nameof(OnRelations), BindingFlags.NonPublic | BindingFlags.Static);

        var _PlayerJoin =
            AccessTools.Constructor(typeof(PlayerDescriptor));
        var _onPlayerJoin =
            typeof(Patching).GetMethod(nameof(OnPlayerJoin), BindingFlags.NonPublic | BindingFlags.Static);

        var _AvatarInstantiated =
            typeof(PuppetMaster).GetMethod("AvatarInstantiated", BindingFlags.Public | BindingFlags.Instance);
        var _onAvatarInstantiated =
            typeof(Patching).GetMethod(nameof(OnAvatarInstantiated), BindingFlags.NonPublic | BindingFlags.Static);

        var _ReloadAllNameplates =
            typeof(CVRPlayerManager).GetMethod("ReloadAllNameplates", BindingFlags.Public | BindingFlags.Instance);
        var _onReloadAllNameplates =
            typeof(Patching).GetMethod(nameof(OnReloadAllNameplates), BindingFlags.NonPublic | BindingFlags.Static);

        if (_HandleRelations != null && _onRelations != null)
        {
            _instance.Patch(_HandleRelations, null, new HarmonyMethod(_onRelations));
        }
        else
        {
            VRCPlates.Error("[0004] Failed to patch HandleRelations");
        }

        if (_PlayerJoin != null && _onPlayerJoin != null)
        {
            _instance.Patch(_PlayerJoin, null, new HarmonyMethod(_onPlayerJoin));
        }
        else
        {
            VRCPlates.Error("[0005] Failed to patch PlayerJoin");
        }

        if (_AvatarInstantiated != null && _onAvatarInstantiated != null)
        {
            _instance.Patch(_AvatarInstantiated, null, new HarmonyMethod(_onAvatarInstantiated));
        }
        else
        {
            VRCPlates.Error("[0006] Failed to patch AvatarInstantiated");
        }

        if (_ReloadAllNameplates != null && _onReloadAllNameplates != null)
        {
            _instance.Patch(_ReloadAllNameplates, null, new HarmonyMethod(_onReloadAllNameplates));
        }
        else
        {
            VRCPlates.Error("[0007] Failed to patch ReloadAllNameplates");
        }
        
        if (_ReloadFriends != null && _onReloadFriends != null)
        {
            _instance.Patch(_ReloadFriends, null, new HarmonyMethod(_onReloadFriends));
        }
        else
        {
            VRCPlates.Error("[0008] Failed to patch ReloadFriends");
        }
    }

    private static void OnRelations(ViewManager __instance, string __0, string __1)
    {
        switch (__1)
        {
            case "Add":
            {
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
            if (VRCPlates.NameplateManager != null) MelonCoroutines.Start(VRCPlates.NameplateManager.CreateNameplate(descriptor)); 
        }
        else
        {
            VRCPlates.Error("[0008] Failed to get player descriptor for avatar " + __instance.name);
        }
    }
    
    private static void OnPlayerJoin(PlayerDescriptor __instance)
    {
        if (VRCPlates.NameplateManager != null) MelonCoroutines.Start(VRCPlates.NameplateManager.CreateNameplate(__instance));
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
    
    private static void OnReloadFriends()
    {
        if (VRCPlates.NameplateManager == null) return;
        foreach (var pair in VRCPlates.NameplateManager.Nameplates)
        {
            var nameplate = pair.Value;
            if (nameplate != null)
            {
                nameplate.IsFriend = Friends.FriendsWith(pair.Key);
            }
        }
    }
}