using System.Reflection;
using ABI_RC.Core.Player;
using UnityEngine;

namespace VRCPlates.Reflection;

public static class PlayerUtils
{
    private static readonly FieldInfo? AnimatorField;
    private static readonly FieldInfo? PlayerDescriptorField;

    static PlayerUtils()
    {
        PlayerDescriptorField = typeof(PuppetMaster).GetField("_playerDescriptor", BindingFlags.Instance | BindingFlags.NonPublic);
        AnimatorField = typeof(PuppetMaster).GetField("_animator", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public static PlayerDescriptor? GetPlayerDescriptor(this PuppetMaster puppetMaster)
    {
        if (PlayerDescriptorField != null)
        {
            return (PlayerDescriptor) PlayerDescriptorField.GetValue(puppetMaster);
        }
        else
        {
            VRCPlates.Error("GetAnimator: PuppetMaster._playerDescriptor is null");
            return null;
        }
    }

    public static Animator? GetAnimator(this PuppetMaster puppetMaster)
    {
        if (AnimatorField != null)
        {
            return (Animator) AnimatorField.GetValue(puppetMaster);
        }
        else
        {
            VRCPlates.Error("GetAnimator: PuppetMaster._animator is null");
            return null;
        }
    }
}