using System.Collections;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using Dissonance;
using UnityEngine;
using UnityEngine.UI;
using VRCPlates.MonoScripts;
using Object = UnityEngine.Object;

namespace VRCPlates;

public class NameplateManager
{
    public readonly Dictionary<string, OldNameplate?> Nameplates;
    private static Dictionary<string, Texture2D>? _cachedImage;

    public NameplateManager()
    {
        Nameplates = new Dictionary<string, OldNameplate?>();
        _cachedImage = new Dictionary<string, Texture2D>();
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

    public void RemoveNameplate(CVRPlayerEntity player)
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

        Nameplates.Remove(id);
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
        if (_cachedImage != null)
        {
            if (_cachedImage.TryGetValue(url, out var tex))
            {
                VRCPlates.Debug("Found Cached Image for: " + url);
            }
            else
            {
                //Dis Lily
                var http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0");

                var req = http.GetByteArrayAsync(url);
                while (!req.GetAwaiter().IsCompleted)
                {
                    yield return null;
                }

                if (!req.IsCanceled & !req.IsFaulted)
                {
                    var bytes = req.Result;
                    try
                    {
                        //I do Dis
                        VRCPlates.Debug($"Download Finished: {url}");
                        tex = new Texture2D(2, 2)
                        {
                            hideFlags = HideFlags.DontUnloadUnusedAsset,
                            wrapMode = TextureWrapMode.Clamp,
                            filterMode = FilterMode.Trilinear
                        };

                        // ReSharper disable once InvokeAsExtensionMethod
                        // Compiles incorrectly if called as an extension. Why? Who knows
                        if (ImageConversion.LoadImage(tex, bytes))
                        {
                            VRCPlates.Debug("Loading Using LoadImage...");
                        }
                        else
                        {
                            VRCPlates.Debug("Loading using LoadRawTextureData...");
                            tex.LoadRawTextureData(bytes);
                        }

                        _cachedImage.Add(url, tex);
                    }
                    catch (Exception e)
                    {
                        VRCPlates.Error(e.ToString());
                    }
                }
                else
                {
                    VRCPlates.Error("Image Request Failed");
                }

                http.Dispose();
            }

            if (tex != null && tex.isReadable)
            {
                image.texture = tex;
                if (Settings.ShowIcon != null) image.transform.parent.gameObject.SetActive(Settings.ShowIcon.Value);
                VRCPlates.Debug("Applying Image");
            }
            else
            {
                VRCPlates.Error("Texture is Unreadable: " + url);
                _cachedImage.Remove(url);
                image.transform.parent.gameObject.SetActive(false);
            }
        }
        else
        {
            VRCPlates.Error("Image Cache is Null");
        }
    }

    public void CreateNameplate(PlayerDescriptor playerDescriptor)
    {
        var oldNameplate = playerDescriptor.GetComponentInChildren<PlayerNameplate>();
        var position = oldNameplate.transform.position;
        if (Settings.Offset != null && Settings.Scale != null && Settings.Enabled != null)
        {
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