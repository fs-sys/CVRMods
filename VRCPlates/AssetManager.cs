using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace VRCPlates;

internal static class AssetManager
{
    internal static GameObject? Nameplate;
    private static AssetBundle? _bundle;
    public static readonly Dictionary<string, Sprite>? SpriteDict = new();
    public static Sprite[]? SpeakingSprites;
    public static Sprite[]? MutedSprites;

    private static GameObject LoadPrefab(string @object)
    {
        if (_bundle is null)
        {
            VRCPlates.Error($"[0011]Failed to load Prefab: {@object}");
            throw new FileLoadException();
        }
        var go = _bundle.LoadAsset(@object, typeof(GameObject)) as GameObject;
        if (go != null)
        {
            go.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            go.hideFlags = HideFlags.HideAndDontSave;
            VRCPlates.Debug($"Loaded Prefab: {@object}");
            return go;
        }

        VRCPlates.Error("[0012]Failed to load Prefab: " + @object);
        return new GameObject();
    }

    private static Sprite LoadSprite(string sprite)
    {
        if (_bundle is null)
        {
            VRCPlates.Error($"[0013] Failed to load Sprite: {sprite}");
            throw new FileLoadException();
        }
        var sprite2 = _bundle.LoadAsset(sprite, typeof(Sprite)) as Sprite;
        if (sprite2 != null)
        {
            sprite2.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            sprite2.hideFlags = HideFlags.HideAndDontSave;
            VRCPlates.Debug($"Loaded Sprite: {sprite}");
            return sprite2;
        }
        
        VRCPlates.Error("[0014] Failed to load Sprite: " + sprite);
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    private static IEnumerator LoadResources()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCPlates.Resources.vrcplates");
        if (stream != null)
        {
            using var memoryStream = new MemoryStream((int) stream.Length);
            stream.CopyTo(memoryStream);
            _bundle = AssetBundle.LoadFromMemory(memoryStream.ToArray(), 0);
            try
            {
                Nameplate = LoadPrefab("Nameplate.prefab");

                SpriteDict?.Add("bubble0", LoadSprite("bubble_0.png"));
                SpriteDict?.Add("bubble1", LoadSprite("bubble_1.png"));
                SpriteDict?.Add("bubble2", LoadSprite("bubble_2.png"));
                SpriteDict?.Add("bubble3", LoadSprite("bubble_3.png"));
                SpriteDict?.Add("bubblemute", LoadSprite("bubble_mute.png"));

                SpriteDict?.Add("ear", LoadSprite("ear.png"));

                SpriteDict?.Add("defaulticon", LoadSprite("icon_default.png"));
                SpriteDict?.Add("iconborder", LoadSprite("IconBorder.png"));
                SpriteDict?.Add("friend", LoadSprite("friend_icon.png"));
                
                SpriteDict?.Add("hidden", LoadSprite("Hidden.png"));
                
                SpriteDict?.Add("nameplate", LoadSprite("NameplateSilent.png"));
                SpriteDict?.Add("nameplatetalk", LoadSprite("NameplateTalk.png"));
                SpriteDict?.Add("nameplatemask", LoadSprite("NameplateMask.png"));
                SpriteDict?.Add("logo", LoadSprite("Logo.png"));

                CreateSpriteArrays();
            }
            catch (Exception e)
            {
                VRCPlates.Error($"[0015] Nameplate Assets failed to load\n\n{e}");
            }
        }
        else
        {
            VRCPlates.Error("[0016] Stream is null, Nameplates cannot load");
        }

        yield break;
    }
    public static void Init() => MelonCoroutines.Start(LoadResources());
 
        
    private static void CreateSpriteArrays()
    {
        if (SpriteDict == null) return;
        
        SpeakingSprites = new[]
        {
            SpriteDict["bubble0"],
            SpriteDict["bubble1"],
            SpriteDict["bubble2"],
            SpriteDict["bubble3"]
        };
            
        MutedSprites = new[]
        {
            SpriteDict["bubblemute"]
        };
    }
}