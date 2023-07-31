using System.Collections;
using System.Diagnostics;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRCPlates.MonoScripts;
using VRCPlates.Reflection;
using Object = UnityEngine.Object;

namespace VRCPlates;

public class NameplateManager
{
    public readonly Dictionary<string, OldNameplate?> Nameplates;
    private static Dictionary<string, Texture>? _imageCache;
    private static Dictionary<string, RawImage[]>? _imageQueue;

    public NameplateManager()
    {
        Nameplates = new Dictionary<string, OldNameplate?>();
        _imageCache = new Dictionary<string, Texture>();
        _imageQueue = new Dictionary<string, RawImage[]>();

        MelonCoroutines.Start(ImageRequestLoop());
    }

    public static void AddImageToQueue(string id, RawImage[] image)
    {
        if (_imageQueue != null && _imageCache != null)
        {
            if (id is "" or "https://files.abidata.io/user_images/00default.png") return;
            if (_imageCache.TryGetValue(id, out var cachedImage))
            {
                foreach (var im in image)
                {
                    im.texture = cachedImage;
                }
            }
            else
            {
                _imageQueue.TryAdd(id, image);
            }
        }
        else
        {
            VRCPlates.Error("Image Queue is Null");
        }
    }

    private static IEnumerator ImageRequestLoop()
    {
        while (true)
        {
            var rateLimit = Settings.RateLimit == null ? 1f : Settings.RateLimit.Value;
            _imageQueue = (_imageQueue ?? new Dictionary<string, RawImage[]>()).Where(w => w.Key != null).ToDictionary(w => w.Key, w => w.Value);
            if (_imageQueue is { Count: > 0 })
            {
                var pair = _imageQueue.First(w => w.Key != null);
                if (pair.Key != null)
                {
                    using var uwr = UnityWebRequest.Get(pair.Key);
                    uwr.downloadHandler = new DownloadHandlerTexture();
                    yield return uwr.SendWebRequest();
                    if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                    {
                        VRCPlates.Warning("Unable to set profile picture: " + uwr.error + "\n" + new StackTrace());
                        _imageQueue.Remove(pair.Key);
                    }
                    else
                    {
                        var tex = DownloadHandlerTexture.GetContent(uwr);
                        _imageCache?.Add(pair.Key, tex);

                        foreach (var im in pair.Value)
                        {
                            yield return (im.texture = tex);
                        }
                    }
                    _imageQueue.Remove(pair.Key);
                    uwr.Dispose();
                }
            }
            yield return new WaitForSeconds(rateLimit);
        }
        // ReSharper disable once IteratorNeverReturns
    }
    
    private void AddNameplate(OldNameplate nameplate, string id)
    {
        if (nameplate != null)
            Nameplates.Add(id, nameplate);
    }

    public void RemoveNameplate(string player)
    {
        if (Nameplates.ContainsKey(player))
        {
            if (Nameplates.TryGetValue(player, out var nameplate))
            {
                Object.Destroy(nameplate!.gameObject);
            }
            Nameplates.Remove(player);
        }
        else
        {
            VRCPlates.Error("NameplateManager: RemoveNameplate: Player not found: " + player + "\n" + new StackTrace());
        }
    }

    public OldNameplate? GetNameplate(PuppetMaster puppetMaster)
    {
        var descriptor = puppetMaster.GetPlayerDescriptor();
        if (Nameplates.TryGetValue(descriptor.ownerId, out var nameplate))
        {
            return nameplate;
        }

        MelonCoroutines.Start(CreateNameplate(puppetMaster));

        VRCPlates.DebugError($"Nameplate does not exist in Dictionary for player: {descriptor.userName}");
        return null;
    }

    public OldNameplate? GetNameplate(string id)
    {
        if (Nameplates.TryGetValue(id, out var nameplate))
        {
            return nameplate;
        }

        if (CVRPlayerManager.Instance.GetPlayerPuppetMaster(id, out var pm))
        {
            if (pm != null)
            {
                MelonCoroutines.Start(CreateNameplate(pm));
            }
        }
        else
        {
            VRCPlates.DebugError($"Player does not exist in Dictionary for id: {id}");
        }
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

    public static void InitializePlate(OldNameplate oldNameplate, PlayerDescriptor playerDescriptor)
    {
        try
        {
            if (playerDescriptor != null)
            {
                oldNameplate.descriptor = playerDescriptor;

                if (oldNameplate.descriptor != null)
                {
                    var descriptor = oldNameplate.descriptor;

                    oldNameplate.Name = descriptor.userName;

                    oldNameplate.Rank = descriptor.userRank;

                    oldNameplate.VipRank = Utils.GetAbbreviation(descriptor.userRank);

                    oldNameplate.IsFriend = Friends.FriendsWith(descriptor.ownerId);

                    oldNameplate.ProfilePicture = descriptor.profileImageUrl;

                    oldNameplate.IsMuted = Utils.IsUserModerated(descriptor.ownerId, ModerationType.Mute);

                    oldNameplate.IsLocal = descriptor.ownerId.Equals(MetaPort.Instance.ownerId);
                }
                else
                {
                    VRCPlates.NameplateManager?.RemoveNameplate(playerDescriptor.ownerId);
                }
            }
            else
            {
                oldNameplate.Name = "||Error||";
                VRCPlates.Error("Unable to Initialize Nameplate: Player Descriptor is null\n" + new StackTrace());
            }
        }
        catch (Exception e)
        {
            oldNameplate.Name = "||Error||";
            VRCPlates.Error("Unable to Initialize Nameplate: " + e + "\n" + new StackTrace());
        }
    }
    
    public static void OnEnableToggle(Component playerNameplate, OldNameplate? oldNameplate)
    {
        if (Settings.Enabled == null) return;
        if (Settings.Enabled.Value)
        {
            playerNameplate.gameObject.SetActive(false);
            if (oldNameplate != null && !oldNameplate.IsLocal &&
                oldNameplate.Nameplate != null)
                oldNameplate.Nameplate.SetActive(Settings.Enabled.Value);
        }
        else
        {
            playerNameplate.gameObject.SetActive(true);
            if (oldNameplate != null && oldNameplate.Nameplate != null)
                oldNameplate.Nameplate.SetActive(false);
        }
    }

    public IEnumerator CreateNameplate(PuppetMaster puppetMaster)
    {
        yield return new WaitForSeconds(0.25f);

        var oldNameplate = puppetMaster.GetComponentInChildren<PlayerNameplate>(true);
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
                    var id = puppetMaster.GetPlayerDescriptor().ownerId;
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
                                AddNameplate(nameplate, id);
                            }
                            else
                            {
                                VRCPlates.Error("Unable to Instantiate Nameplate: Nameplate is Null\n" +
                                                new StackTrace());
                            }
                        }

                        OnEnableToggle(oldNameplate, nameplate);
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
            VRCPlates.Error("Unable to Initialize Nameplate: Nameplate GameObject is null\n" + new StackTrace());
        }
    }
}