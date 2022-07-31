using System.Collections;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using Dissonance;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRCPlates.MonoScripts;
using Object = UnityEngine.Object;

namespace VRCPlates;

public class NameplateManager
{
    public readonly Dictionary<string, OldNameplate?> Nameplates;

    public NameplateManager()
    {
        Nameplates = new Dictionary<string, OldNameplate?>();
    }

    private void AddNameplate(OldNameplate nameplate, PlayerDescriptor player)
    {
        string id;
        try
        {
            id = player.ownerId;
        }
        catch
        {
            return;
        }
        
        if (id != null && nameplate != null)
            Nameplates.Add(id, nameplate);
    }

    public void RemoveNameplate(string player)
    {
        if (Nameplates.ContainsKey(player))
        {
            Nameplates.Remove(player);
        }
        else
        {
            VRCPlates.Error("NameplateManager: RemoveNameplate: Player not found: " + player);
        }
    }

    public OldNameplate? GetNameplate(CVRPlayerEntity player)
    {
        if (Nameplates.TryGetValue(player.Uuid, out var nameplate))
        {
            return nameplate;
        }

        VRCPlates.DebugError($"Nameplate does not exist in Dictionary for player: {player.Username}");
        return null;
    }

    public OldNameplate? GetNameplate(string id)
    {
        if (Nameplates.TryGetValue(id, out OldNameplate? nameplate))
        {
            return nameplate;
        }

        VRCPlates.DebugError($"Nameplate does not exist in Dictionary for player: {id}");
        return null;
    }

    public void ClearNameplates()
    {
        Nameplates.Clear();
    }

    //Nameplates can support 5 compatibility badges total
    public GameObject? AddBadge(OldNameplate plate, string id, Texture2D? icon)
    {
        if (plate.badgeCompat == null) return null;
        var gameObject = plate.badgeCompat.gameObject;
        var badge = Object.Instantiate(gameObject, gameObject.transform.parent);
        badge.name = id;
        badge.transform.localPosition = gameObject.transform.localPosition;
        badge.transform.localRotation = gameObject.transform.localRotation;
        badge.transform.localScale = gameObject.transform.localScale;
        badge.SetActive(true);
        var image = badge.GetComponent<Image>();
        if (icon != null)
        {
            image.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height),
                new Vector2(0.5f, 0.5f));
        }
        else
        {
            image.enabled = false;
        }
        return badge;
    }

    public static void InitializePlate(OldNameplate oldNameplate, CVRPlayerEntity? player)
    {
        try
        {
            if (player != null)
            {
                oldNameplate.Player = player;

                if (oldNameplate.Player != null)
                {
                    oldNameplate.Name = player.Username;

                    // oldNameplate.Status = player.field_Private_APIUser_0.statusDescriptionDisplayString;

                    oldNameplate.Rank = player.ApiUserRank;

                    // Literally broken no matter what I try.
                    //oldNameplate.ShowSocialRank = player.field_Private_APIUser_0.showSocialRank;

                    oldNameplate.IsFriend = Friends.FriendsWith(player.Uuid);

                    // oldNameplate.IsMaster = player.field_Private_VRCPlayerApi_0.isMaster;

                    // VRCPlates.NameplateManager!._masterClient = player.field_Private_APIUser_0.id;

                    //Getting if this value has changed.
                    //uSpeaker.NativeMethodInfoPtr_Method_Public_Single_1
                    //Have fun future me, it's your favorite thing, native patching :D
                    // oldNameplate.UserVolume = player.prop_USpeaker_0.field_Private_Single_1;

                    oldNameplate.ProfilePicture = player.ApiProfileImageUrl;

                    // oldNameplate.IsQuest = player.field_Private_APIUser_0._last_platform.ToLower() == "android";



                    oldNameplate.VipText = Utils.GetAbbreviation(player.ApiUserRank);

                    oldNameplate.IsLocal = player.DarkRift2Player.Type == NetworkPlayerType.Local;

                    oldNameplate.IsMuted = player.PlayerDescriptor.voiceMuted;
                }
                else
                {
                    oldNameplate.Name = "||Error||";
                }
            }

            else
            {
                oldNameplate.Name = "||Error||";
                VRCPlates.Error("Unable to Initialize Nameplate: Player is null");
            }
        }
        catch (Exception e)
        {
            oldNameplate.Name = "||Error||";
            VRCPlates.Error("Unable to Initialize Nameplate: " + e);
        }
    }

    internal static IEnumerator SetRawImage(string url, RawImage image)
    {
        using var uwr = UnityWebRequest.Get(url);
        uwr.downloadHandler = new DownloadHandlerTexture();
        yield return uwr.SendWebRequest();
        yield return image.texture = DownloadHandlerTexture.GetContent(uwr);
        if (Settings.ShowIcon != null)
            image.transform.parent.gameObject.SetActive(Settings.ShowIcon.Value);
        VRCPlates.Debug("Applying Image");
    }
    
    public void CreateNameplate(PlayerDescriptor playerDescriptor)
    {
        var oldNameplate = playerDescriptor.GetComponentInChildren<PlayerNameplate>();

        if (oldNameplate != null && oldNameplate.gameObject != null && oldNameplate.gameObject.transform != null)
        {

            var position = oldNameplate.gameObject.transform.position;

            if (Settings.Offset == null || Settings.Scale == null || Settings.Enabled == null) return;
            var scaleValue = Settings.Scale.Value * .001f;
            var offsetValue = Settings.Offset.Value;

            // Hopefully fixes ID null issues
            if (playerDescriptor != null)
            {
                var id = playerDescriptor.ownerId;
                if (id is {Length: > 0})
                {
                    if (Nameplates.TryGetValue(id, out var nameplate))
                    {
                        if (nameplate != null)
                        {
                            nameplate.ApplySettings(position, scaleValue, offsetValue);
                        }
                        else
                        {
                            VRCPlates.Error("Unable to Update Nameplate: Nameplate is Null");
                        }
                    }
                    else
                    {
                        var plate = Object.Instantiate(AssetManager.Nameplate,
                            new(position.x, position.y + offsetValue, position.z),
                            new(0, 0, 0, 0), oldNameplate.transform.parent);

                        if (plate != null)
                        {
                            plate.transform.localScale = new(scaleValue, scaleValue, scaleValue);
                            plate.name = "OldNameplate";
                            nameplate = plate.AddComponent<OldNameplate>();
                            AddNameplate(nameplate, playerDescriptor);
                        }
                        else
                        {
                            VRCPlates.Error("Unable to Instantiate Nameplate: Nameplate is Null");
                        }
                    }

                    if (Settings.Enabled.Value)
                    {
                        oldNameplate.gameObject.SetActive(false);
                        if (nameplate != null && !nameplate.IsLocal &&
                            nameplate.Nameplate != null)
                            nameplate.Nameplate.SetActive(Settings.Enabled.Value);
                    }
                    else
                    {
                        oldNameplate.gameObject.SetActive(true);
                        if (nameplate != null && nameplate.Nameplate != null)
                            nameplate.Nameplate.SetActive(false);
                    }
                }
                else
                {
                    VRCPlates.Error("Unable to Instantiate Nameplate: Player is Null");
                }
            }
            else
            {
                VRCPlates.Error("Unable to Instantiate Nameplate: Player is Null");
            }
        }
        else
        {
            VRCPlates.Error("Unable to Initialize Nameplate: Settings are null");
        }
    }
}