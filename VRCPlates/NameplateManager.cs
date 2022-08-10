using System.Collections;
using System.Diagnostics;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using Dissonance;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRCPlates.MonoScripts;
using Object = UnityEngine.Object;

namespace VRCPlates;

public class NameplateManager
{
    public readonly Dictionary<string, OldNameplate?> Nameplates;
    public static Dictionary<string, RawImage>? ImageQueue;

    public NameplateManager()
    {
        Nameplates = new Dictionary<string, OldNameplate?>();
        ImageQueue = new Dictionary<string, RawImage>();

        MelonCoroutines.Start(ImageRequestLoop());
    }

    private static IEnumerator ImageRequestLoop()
    {
        while (true)
        {
            var rateLimit = Settings.RateLimit == null ? 1f : Settings.RateLimit.Value;
            if (ImageQueue is {Count: > 0})
            {
                var pair = ImageQueue.First();
                if (pair.Key is not null or "" or "https://files.abidata.io/user_images/00default.png")
                {
                    using var uwr = UnityWebRequest.Get(pair.Key);
                    uwr.downloadHandler = new DownloadHandlerTexture();

                    var request = uwr.SendWebRequest();
                    while (!request.isDone)
                    {
                        yield return null;
                    }

                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        VRCPlates.Warning("Unable to set profile picture: " + uwr.error + "\n" + new StackTrace());
                        ImageQueue.Remove(pair.Key);
                    }
                    else
                    {
                        pair.Value.texture = ((DownloadHandlerTexture) uwr.downloadHandler).texture;
                        pair.Value.transform.parent.gameObject.SetActive(true);
                    }
                    uwr.Dispose();
                }
                else
                {
                    pair.Value.transform.parent.gameObject.SetActive(false);
                }

                if (pair.Key != null) ImageQueue.Remove(pair.Key);
            }

            yield return new WaitForSeconds(rateLimit);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void AddNameplate(OldNameplate nameplate, CVRPlayerEntity player)
    {
        string id;
        try
        {
            id = player.Uuid;
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
            VRCPlates.Error("NameplateManager: RemoveNameplate: Player not found: " + player + "\n" + new StackTrace());
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

    public static void InitializePlate(OldNameplate oldNameplate, PlayerDescriptor? playerDescriptor)
    {
        try
        {
            if (playerDescriptor != null)
            {
                oldNameplate.Player = Utils.GetPlayerEntity(playerDescriptor.ownerId);

                if (oldNameplate.Player != null)
                {
                    var player = oldNameplate.Player;
                    
                    oldNameplate.Name = player.Username;

                    // oldNameplate.Status = player.field_Private_APIUser_0.statusDescriptionDisplayString;

                    oldNameplate.Rank = player.ApiUserRank;

                    oldNameplate.VipRank = Utils.GetAbbreviation(player.ApiUserRank);
                    
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
                VRCPlates.Error("Unable to Initialize Nameplate: Player is null\n" + new StackTrace());
            }
        }
        catch (Exception e)
        {
            oldNameplate.Name = "||Error||";
            VRCPlates.Error("Unable to Initialize Nameplate: " + e + "\n" + new StackTrace());
        }
    }

    public IEnumerator CreateNameplate(CVRPlayerEntity playerEntity)
    {
        yield return new WaitForSeconds(0.25f);

        var oldNameplate = playerEntity.PlayerNameplate;
        if (oldNameplate == null) yield break;
        if (oldNameplate.gameObject != null)
        {
            if (oldNameplate.gameObject.transform != null)
            {
                var position = oldNameplate.gameObject.transform.position;

                if (Settings.Offset != null && Settings.Scale != null && Settings.Enabled != null)
                {
                    var scaleValue = Settings.Scale.Value * .001f;
                    var offsetValue = Settings.Offset.Value;
                    var id = playerEntity.Uuid;
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
                                VRCPlates.Debug("Nameplate is null, removing from dictionary\n" + new StackTrace());
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
                                AddNameplate(nameplate, playerEntity);
                            }
                            else
                            {
                                VRCPlates.Error("Unable to Instantiate Nameplate: Nameplate is Null\n" +
                                                new StackTrace());
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
                        VRCPlates.Error("Unable to Instantiate Nameplate: Player is Null\n" + new StackTrace());
                    }
                }
                else
                {
                    VRCPlates.Error("Unable to Initialize Nameplate: Settings are null\n" + new StackTrace());
                }
            }
            else
            {
                VRCPlates.Error("Unable to Initialize Nameplate: Nameplate Transform is null\n" + new StackTrace());
            }
        }
        else
        {
            VRCPlates.Error("Unable to Initialize Nameplate: Nameplate Gameobject is null\n" + new StackTrace());
        }
    }
}