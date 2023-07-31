using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using HarmonyLib;
using MelonLoader;
using VRCPlates.Reflection;

namespace VRCPlates.Patching;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class Patching
{
    private static readonly HarmonyLib.Harmony _instance = new HarmonyLib.Harmony("VRCPlates");
    
    public static void Init()
    {
        var _ReloadFriends =
            typeof(ViewManager).GetMethod("RequestFriendsListTask", BindingFlags.Public | BindingFlags.Instance);
        var _onReloadFriends =
            typeof(Patching).GetMethod(nameof(OnReloadFriends), BindingFlags.NonPublic | BindingFlags.Static);

        var _HandleRelations =
            typeof(ViewManager).GetMethod("HandleRelations", BindingFlags.NonPublic | BindingFlags.Instance);
        var _onRelations =
            typeof(Patching).GetMethod(nameof(OnRelations), BindingFlags.NonPublic | BindingFlags.Static);

        var _SettingsChanged =
            typeof(MetaPort).GetMethod("SettingsChangedHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        var _onSettingsChanged =
            typeof(Patching).GetMethod(nameof(OnSettingsChanged), BindingFlags.NonPublic | BindingFlags.Static);

        CVRPlayerManager.Instance.OnPlayerEntityCreated += OnPlayerJoin;
        CVRPlayerManager.Instance.OnPlayerEntityRecycled += OnPlayerLeave;

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
            VRCPlates.Error("Failed to patch HandleRelations\n" + new StackTrace());
        }

        if (_SettingsChanged != null && _onSettingsChanged != null)
        {
            _instance.Patch(_SettingsChanged, null, new HarmonyMethod(_onSettingsChanged));
        }
        else
        {
            VRCPlates.Error("Failed to patch SettingsChanged\n" + new StackTrace());
        }

        if (_AvatarInstantiated != null && _onAvatarInstantiated != null)
        {
            _instance.Patch(_AvatarInstantiated, null, new HarmonyMethod(_onAvatarInstantiated));
        }
        else
        {
            VRCPlates.Error("Failed to patch AvatarInstantiated\n" + new StackTrace());
        }

        if (_ReloadAllNameplates != null && _onReloadAllNameplates != null)
        {
            _instance.Patch(_ReloadAllNameplates, null, new HarmonyMethod(_onReloadAllNameplates));
        }
        else
        {
            VRCPlates.Error("Failed to patch ReloadAllNameplates\n" + new StackTrace());
        }

        if (_ReloadFriends != null && _onReloadFriends != null)
        {
            _instance.Patch(_ReloadFriends, null, new HarmonyMethod(_onReloadFriends));
        }
        else
        {
            VRCPlates.Error("Failed to patch ReloadFriends\n" + new StackTrace());
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
    
    private static void OnSettingsChanged(MetaPort __instance)
    {
        if (!__instance.settings.settingsHaveChanged) return;
        if (VRCPlates.NameplateManager == null) return;
        foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Where(nameplate => nameplate.Value != null))
        {
            nameplate.Value!.ApplySettings();
        }
    }

    private static void OnAvatarInstantiated(PuppetMaster __instance)
    {
        var descriptor = __instance.GetPlayerDescriptor();
        if (descriptor != null)
        {
            if (VRCPlates.NameplateManager != null)
                MelonCoroutines.Start(VRCPlates.NameplateManager.CreateNameplate(__instance));
        }
        else
        {
            VRCPlates.Error("Failed to get player descriptor for avatar " + __instance.name);
        }
    }

    private static void OnPlayerJoin(CVRPlayerEntity playerEntity)
    {
        if (VRCPlates.NameplateManager != null) MelonCoroutines.Start(VRCPlates.NameplateManager.CreateNameplate(playerEntity.PuppetMaster));
    }
    
    private static void OnPlayerLeave(CVRPlayerEntity playerEntity)
    {
        if (VRCPlates.NameplateManager != null) VRCPlates.NameplateManager.RemoveNameplate(playerEntity.Uuid);
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
    
    private static void OnReloadFriends(ViewManager __instance)
    {
        if (VRCPlates.NameplateManager == null) return;
        foreach (var nameplate in VRCPlates.NameplateManager.Nameplates.Select(pair => pair.Value).Where(nameplate => nameplate != null))
        {
            nameplate!.IsFriend = Friends.FriendsWith(nameplate.descriptor!.userName);
        }
    }
}