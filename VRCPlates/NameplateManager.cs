using System;
using System.Collections;
using System.Collections.Generic;
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
            VRCPlates.Error("[0017] NameplateManager: RemoveNameplate: Player not found: " + player);
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
        if (Nameplates.TryGetValue(id, out var nameplate))
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
                    
                    oldNameplate.IsMuted = player.PlayerDescriptor.voiceMuted;

                    oldNameplate.VipText = Utils.GetAbbreviation(player.ApiUserRank);

                    oldNameplate.IsLocal = player.DarkRift2Player.Type == NetworkPlayerType.Local;
                }
                else
                {
                    oldNameplate.Name = "||Error||";
                }
            }

            else
            {
                oldNameplate.Name = "||Error||";
                VRCPlates.Error("[0018] Unable to Initialize Nameplate: Player is null");
            }
        }
        catch (Exception e)
        {
            oldNameplate.Name = "||Error||";
            VRCPlates.Error("[0018] Unable to Initialize Nameplate: " + e);
        }
    }

    internal static IEnumerator SetRawImage(string url, RawImage image)
    {
        if (url is null or "" or "https://files.abidata.io/user_images/00default.png")
        {
            image.transform.parent.gameObject.SetActive(false);
            yield break;
        }
        using var uwr = UnityWebRequest.Get(url);
        uwr.downloadHandler = new DownloadHandlerTexture();
        var request = uwr.SendWebRequest();
        while (!request.isDone)
        {
            yield return null;
        }
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            VRCPlates.Error("[0019] Unable to set profile picture: " + uwr.error);
        }
        else
        {
            image.texture = ((DownloadHandlerTexture) uwr.downloadHandler).texture;
            image.transform.parent.gameObject.SetActive(true);
        }
    }

    public IEnumerator CreateNameplate(PlayerDescriptor playerDescriptor)
    {
        yield return new WaitForSeconds(0.25f);
        
        var oldNameplate = playerDescriptor.GetComponentInChildren<PlayerNameplate>();
        if (oldNameplate != null)
        {
            if (oldNameplate.gameObject != null)
            {
                if (oldNameplate.gameObject.transform != null)
                {
                    var position = oldNameplate.gameObject.transform.position;

                    if (Settings.Offset != null && Settings.Scale != null && Settings.Enabled != null)
                    {
                        var scaleValue = Settings.Scale.Value * .001f;
                        var offsetValue = Settings.Offset.Value;

                        if (playerDescriptor != null)
                        {
                            var id = playerDescriptor.ownerId;
                            if (id is {Length: > 0})
                            {
                                if (Nameplates.TryGetValue(id, out var nameplate))
                                {
                                    if (nameplate != null)
                                    {
                                        nameplate.ApplySettings();
                                    }
                                    else
                                    {
                                        VRCPlates.Debug("[0019] Nameplate is null, removing from dictionary");
                                        RemoveNameplate(id);
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
                                        VRCPlates.Error("[0020] Unable to Instantiate Nameplate: Nameplate is Null");
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
                                VRCPlates.Error("[0021] Unable to Instantiate Nameplate: Player is Null");
                            }
                        }
                        else
                        {
                            VRCPlates.Error("[0022] Unable to Instantiate Nameplate: Player is Null");
                        }
                    }
                    else
                    {
                        VRCPlates.Error("[0023] Unable to Initialize Nameplate: Settings are null");
                    }
                }
                else
                {
                    VRCPlates.Error("[0024] Unable to Initialize Nameplate: Nameplate Transform is null");
                }
            }
            else
            {
                VRCPlates.Error("[0025] Unable to Initialize Nameplate: Nameplate Gameobject is null");
            }
        }
        else
        {
            //Throws Harmlessly.
            VRCPlates.DebugError("[0026] Unable to Initialize Nameplate: Nameplate is null");
        }
    }
}