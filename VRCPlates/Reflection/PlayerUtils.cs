using System.Diagnostics;
using System.Reflection;
using ABI_RC.Core.Player;
using UnityEngine;

namespace VRCPlates.Reflection;

public static class PlayerUtils
{
    private static readonly FieldInfo AnimatorField = typeof(PuppetMaster).GetField("_animator", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly FieldInfo PlayerDescriptorField = typeof(PuppetMaster).GetField("_playerDescriptor", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static PlayerDescriptor GetPlayerDescriptor(this PuppetMaster puppetMaster)
    {
        return (PlayerDescriptor) PlayerDescriptorField.GetValue(puppetMaster);
    }

    public static Animator GetAnimator(this PuppetMaster puppetMaster)
    {
        return (Animator) AnimatorField.GetValue(puppetMaster);
    }
    
    public static CVRPlayerEntity? GetPlayerEntity(string? userID)
    {
        var player = CVRPlayerManager.Instance.NetworkPlayers.Find(p=> p.Uuid == userID);
        if (player != null)
        {
            return player;
        }
        VRCPlates.Error("Could not find player entity for user ID: " + userID + "\n" + new StackTrace());
        return null;
    }
}