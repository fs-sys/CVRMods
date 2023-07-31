using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.AvatarTracking.Remote;
using ABI_RC.Core.Savior;
using NicoKuroKusagi.MemoryManagement;
using UnityEngine;

namespace VRCPlates.Reflection;

public static class PlayerUtils
{
    private static readonly FieldInfo AnimatorField =
        typeof(PuppetMaster).GetField("_animator", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo PlayerDescriptorField =
        typeof(PuppetMaster).GetField("_playerDescriptor", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo InUsePoolField =
        typeof(ObjectPool<CVRPlayerEntity>).GetField("inUse",BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo ViewPointField =
        typeof(PuppetMaster).GetField("_viewPoint", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo VisemeControllerField =
        typeof(PuppetMaster).GetField("_visemeController", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo ModerationIndexField =
        typeof(CVRSelfModerationManager).GetField("_moderationIndex", BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    private static readonly FieldInfo TrackerField = typeof(CVRVisemeController).GetField("_tracker", BindingFlags.Instance | BindingFlags.NonPublic)!;
        
    private static readonly PropertyInfo PipelineProperty = TrackerField.GetType().GetProperty("pipeline", BindingFlags.Instance | BindingFlags.NonPublic)!;
        
    private static readonly PropertyInfo IsActiveSmoothProperty = PipelineProperty.GetType().GetProperty("IsActiveSmooth", BindingFlags.Instance | BindingFlags.NonPublic)!;


    public static CVRSelfModerationIndex? GetModerationIndex(this CVRSelfModerationManager moderationManager)
    {
        return (CVRSelfModerationIndex)ModerationIndexField.GetValue(moderationManager);
    }
    
    public static CVRVisemeController? GetVisemeController(this PuppetMaster puppetMaster)
    {
        return (CVRVisemeController)VisemeControllerField.GetValue(puppetMaster);
    }
    
    public static PlayerDescriptor GetPlayerDescriptor(this PuppetMaster puppetMaster)
    {
        return (PlayerDescriptor)PlayerDescriptorField.GetValue(puppetMaster);
    }

    public static Animator GetAnimator(this PuppetMaster puppetMaster)
    {
        return (Animator)AnimatorField.GetValue(puppetMaster);
    }

    private static ConcurrentDictionary<CVRPlayerEntity, bool> GetEntityPool(this ObjectPool<CVRPlayerEntity> objectPool)
    {
        return (ConcurrentDictionary<CVRPlayerEntity, bool>)InUsePoolField.GetValue(objectPool);
    }

    public static RemoteHeadPoint GetViewPoint(this PuppetMaster puppetMaster)
    {
        var point = ViewPointField.GetValue(puppetMaster);
        var rmPoint = (RemoteHeadPoint)point;
        if (rmPoint != null) return rmPoint;
        VRCPlates.Error("Unable to process Viewpoint");
        return null!;
    }

    public static CVRPlayerEntity? GetPlayerEntity(string? userID)
    {
        foreach (var cvrPlayerEntity in CVRPlayerEntity.Pool.GetEntityPool().Keys)
        {
            if (cvrPlayerEntity.Uuid == userID)
            {
                return cvrPlayerEntity;
            }
        }

        VRCPlates.Error("Could not find player entity for user ID: " + userID + "\n" + new StackTrace());
        return null;
    }

    public static object? GetIsActiveSmooth(this CVRVisemeController? visemeController)
    {
        var tracker = TrackerField.GetValue(visemeController);
        var pipeline = PipelineProperty.GetValue(tracker);
        var isActive = IsActiveSmoothProperty.GetValue(pipeline);
        return isActive;
    }
}