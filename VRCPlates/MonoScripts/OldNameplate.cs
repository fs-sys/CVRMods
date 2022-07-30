using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using MelonLoader;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRCPlates.MonoScripts;
using VRCPlates.Reflection;
using static VRCPlates.AssetManager;

namespace VRCPlates.MonoScripts;

[RegisterTypeInIl2Cpp]
public class OldNameplate : MonoBehaviour
{
    public CVRPlayerEntity? Player;
    
    public bool qmOpen;
    
    private bool _isFriend;
    private bool _isMuted;
    private bool _isBlocked;
    private string? _vipRank;
    
    private Color _plateColor;
    private Color _nameColor;
    private string? _name;
    private string? _rank;
    
    private string? _profilePicture;
    private string? _plateBackground;
    
    internal GameObject? Nameplate;
    private Transform? _transform;
    private PositionConstraint? _constraint;
    private Camera? _camera;

    private Image? _mainPlate;
    private Text? _mainText;
    private Text? _mainStatus;
    private RawImage? _mainBackground;

    private Image? _afkPlate;
    private Text? _afkText;
    private RawImage? _afkBackground;

    private Image? _userPlate;
    private RawImage? _userIcon;

    private Image? _vipPlate;
    private Text? _vipText;
    private RawImage? _vipBackground;

    private Image? _voiceBubble;
    private Image? _voiceStatus;
    private Text? _voiceVolume;

    private Image? _badgeMaster;
    private Image? _badgeFallback;
    private Image? _badgePerformance;
    private Image? _badgeQuest;
    public Image? badgeCompat;

    private Image? _iconFriend;
    private Image? _iconInteract;

    private Text? _rankText;


    public bool IsLocal { get; set; }

    public bool IsFriend
    {
        get => _isFriend;
        set
        {
            _isFriend = value;
            if (_iconFriend != null) _iconFriend.gameObject.SetActive(_isFriend);
            if (_isFriend)
            {
                if (_rankText == null) return;
                var text = _rankText.text;
                if (text.Contains("Friend")) return;
                text = $"Friend ({text})";
                _rankText.text = text;
            }
            else
            {
                if (_rankText == null) return;
                var text = _rankText.text;
                if (!text.StartsWith("Friend (")) return;
                text = text.Remove(0, 8).Replace(')', ' ').Trim();
                _rankText.text = text;
            }
        }
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;

            if (_voiceBubble == null || _voiceBubble.gameObject == null) return;

            var component = _voiceBubble.gameObject.GetComponent<SpriteSwapAnimation>();
            if (component == null)
            {
                component = _voiceBubble.gameObject.AddComponent<SpriteSwapAnimation>();
                component.image = _voiceBubble;
                component.sprites = _isMuted ? MutedSprites : SpeakingSprites;
            }

            if (component == null) return;
            component.sprites = _isMuted ? MutedSprites : SpeakingSprites;

            _voiceBubble.gameObject.SetActive(_isMuted);
        }
    }
    
    public string? VipRank
    {
        get => _vipRank;
        set
        {
            _vipRank = value;
            if (_vipPlate != null) _vipPlate.transform.parent.gameObject.SetActive(_vipRank != string.Empty);
        }
    }

    public Color PlateColor
    {
        get => _plateColor;
        set
        {
            if (Settings.RainbowPlates == null || Settings.RainbowFriends == null) { return; }
                
            _plateColor = value;

            if (Settings.RainbowPlates.Value | (Settings.RainbowFriends.Value && IsFriend))
            {
                return;
            }

            if (_mainPlate != null) _mainPlate.color = _plateColor;
                if (_vipPlate != null) _vipPlate.color = _plateColor;
                if (_afkPlate != null) _afkPlate.color = _plateColor;
                if (_userPlate != null) _userPlate.color = _plateColor;
            
        }
    }

    public Color NameColor
    {
        get => _nameColor;
        set
        {
            _nameColor = value;

            if (_mainText != null) _mainText.color = _nameColor;

            // Didn't really like how this looked, so I'm disabling it
            // if (_mainStatus != null) _mainStatus.color = _nameColor;
            // if (_rankText != null) _rankText.color = _nameColor;
            // if (_vipText != null) _vipText.color = _nameColor;
            // if (_afkText != null) _afkText.color = _nameColor;
        }
    }

    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            if (_mainText == null) return;
            var displayName = _name;
            if (displayName is {Length: > 16})
            {
                displayName = displayName.Remove(15) + "...";
            }

            _mainText.text = displayName;
            _mainText.gameObject.SetActive(true);
        }
    }

    public string? Rank
    {
        get => _rank;
        set
        {
            if (_rankText == null) return;
            _rank = value;
            _rankText.text = _rank;
            IsFriend = _isFriend;
            if (Settings.ShowRank != null) _rankText.gameObject.SetActive(Settings.ShowRank.Value);
        }
    }
    
    public string? ProfilePicture
    {
        get => _profilePicture;
        set
        {
            _profilePicture = value;
            if (string.IsNullOrEmpty(_profilePicture)) return;
            if (_profilePicture != null && _userIcon != null)
                MelonCoroutines.Start(NameplateManager.SetRawImage(_profilePicture, _userIcon));
        }
    }

    [SuppressMessage("ReSharper", "IteratorMethodResultIsIgnored")]
    public string? PlateBackground
    {
        get => _plateBackground;
        set
        {
            _plateBackground = value;

            if (_plateBackground == null)
                return;
            if (_mainBackground != null)
                MelonCoroutines.Start(NameplateManager.SetRawImage(_plateBackground, _mainBackground));
            if (_afkBackground != null)
                MelonCoroutines.Start(NameplateManager.SetRawImage(_plateBackground, _afkBackground));
            if (_vipBackground != null)
                MelonCoroutines.Start(NameplateManager.SetRawImage(_plateBackground, _vipBackground));
        }
    }

    public string? VipText
    {
        get => _vipRank;
        set
        {
            if (value == null) return;
            _vipRank = value;
            if (_vipText == null) return;
            _vipText.text = _vipRank;
            if (_vipPlate != null) _vipPlate.gameObject.SetActive(true);
        }
    }

    public bool IsBlocked
    {
        get => _isBlocked;
        set
        {
            _isBlocked = value;
            if (Nameplate != null) Nameplate.SetActive(!_isBlocked);
        }
    }

    public void Awake()
    {
        if (Camera.main != null) _camera = Camera.main;

        Nameplate = gameObject;
        _transform = Nameplate.transform;
        _constraint = Nameplate.AddComponent<PositionConstraint>();
        _constraint.constraintActive = false;

        _mainPlate = Nameplate.transform.Find("Main/Plate").GetComponent<Image>();
        _mainText = Nameplate.transform.Find("Main/Name").GetComponent<Text>();
        _mainStatus = Nameplate.transform.Find("Main/Status").GetComponent<Text>();
        _mainBackground = Nameplate.transform.Find("Main/Mask/Background").GetComponent<RawImage>();

        _afkPlate = Nameplate.transform.Find("AFK/Plate").GetComponent<Image>();
        _afkText = Nameplate.transform.Find("AFK/Text").GetComponent<Text>();
        _afkBackground = Nameplate.transform.Find("AFK/Mask/Background").GetComponent<RawImage>();

        _userPlate = Nameplate.transform.Find("VIP/Icon").GetComponent<Image>();
        _userIcon = Nameplate.transform.Find("VIP/Icon/Image").GetComponent<RawImage>();

        _vipPlate = Nameplate.transform.Find("VIP/Plate/Plate").GetComponent<Image>();
        _vipText = Nameplate.transform.Find("VIP/Plate/Text").GetComponent<Text>();
        _vipBackground = Nameplate.transform.Find("VIP/Plate/Text").GetComponent<RawImage>();

        _voiceBubble = Nameplate.transform.Find("Voice/Bubble").GetComponent<Image>();

        _voiceStatus = Nameplate.transform.Find("Voice/Status").GetComponent<Image>();
        _voiceVolume = Nameplate.transform.Find("Voice/Volume").GetComponent<Text>();

        _badgeMaster = Nameplate.transform.Find("Badges/Master").GetComponent<Image>();
        _badgeFallback = Nameplate.transform.Find("Badges/Fallback").GetComponent<Image>();
        _badgePerformance = Nameplate.transform.Find("Badges/Performance").GetComponent<Image>();
        _badgeQuest = Nameplate.transform.Find("Badges/Quest").GetComponent<Image>();
        badgeCompat = Nameplate.transform.Find("Badges/Compat").GetComponent<Image>();

        _iconFriend = Nameplate.transform.Find("Icons/Friend").GetComponent<Image>();
        _iconInteract = Nameplate.transform.Find("Icons/Interact").GetComponent<Image>();

        _rankText = Nameplate.transform.Find("Rank").GetComponent<Text>();

        if (VRCPlates.NameplateManager != null)
            NameplateManager.InitializePlate(this,
                Nameplate.GetComponentInParent<CVRPlayerEntity>());

        MelonCoroutines.Start(SpeechManagement());
        MelonCoroutines.Start(Rainbow());

        ApplySettings();
    }
    
    
    private IEnumerator SpeechManagement()
    {
        while (true)
        {
            if (Nameplate != null && Nameplate.activeInHierarchy && Player != null && SpriteDict != null)
            {
                if (Player.TalkerAmplitude > 0f)
                {
                    if (_voiceBubble != null && !_voiceBubble.gameObject.activeInHierarchy)
                    {
                        if (Settings.ShowVoiceBubble != null)
                        {
                            _voiceBubble.gameObject.SetActive(Settings.ShowVoiceBubble.Value);
                        }

                        if (_mainPlate != null && _mainPlate.gameObject.activeInHierarchy)
                        {
                            _mainPlate.sprite = SpriteDict["nameplatetalk"];
                        }

                        if (_afkPlate != null && _afkPlate.gameObject.activeInHierarchy)
                        {
                            _afkPlate.sprite = SpriteDict["nameplatetalk"];
                        }

                        if (_vipPlate != null && _vipPlate.gameObject.activeInHierarchy)
                        {
                            _vipPlate.sprite = SpriteDict["nameplatetalk"];
                        }
                    }
                }
                else
                {
                    if (_voiceBubble != null)
                    {
                        _voiceBubble.gameObject.SetActive(IsMuted);

                        if (_mainPlate != null && _mainPlate.gameObject.activeInHierarchy)
                        {
                            _mainPlate.sprite = SpriteDict["nameplate"];
                        }

                        if (_afkPlate != null && _afkPlate.gameObject.activeInHierarchy)
                        {
                            _afkPlate.sprite = SpriteDict["nameplate"];
                        }

                        if (_vipPlate != null && _vipPlate.gameObject.activeInHierarchy)
                        {
                            _vipPlate.sprite = SpriteDict["nameplate"];
                        }
                    }
                }
            }

            yield return new WaitForSeconds(.5f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator Rainbow()
    {
        while (true)
        {
            if (Settings.RainbowFriends != null && Settings.RainbowPlates != null &&
                Settings.RainbowPlates.Value | (Settings.RainbowFriends.Value && IsFriend) && Nameplate != null &&
                Nameplate.activeInHierarchy && Player != null)
            {
                if (_mainPlate != null && _mainPlate.gameObject.activeInHierarchy)
                {
                    _mainPlate.color = Color.Lerp(_mainPlate.color, Color.red, Time.deltaTime * 2);
                }

                if (_afkPlate != null && _afkPlate.gameObject.activeInHierarchy)
                {
                    _afkPlate.color = Color.Lerp(_afkPlate.color, Color.red, Time.deltaTime * 2);
                }

                if (_vipPlate != null && _vipPlate.gameObject.activeInHierarchy)
                {
                    _vipPlate.color = Color.Lerp(_vipPlate.color, Color.red, Time.deltaTime * 2);
                }
            }

            if (Settings.RainbowDelay != null)
            {
                yield return new WaitForSeconds(Settings.RainbowDelay.Value);
            }
           
        }
        // ReSharper disable once IteratorNeverReturns
    }
    
    public void Update()
    {
        transform.LookAt(2f * transform.position - _camera!.transform.position);
    }
    
   
    public void ApplySettings(Vector3 position, float scaleValue, float offsetValue)
    {
        if (_transform != null)
        {
            _transform.position = new(position.x, position.y + offsetValue, position.z);
            _transform.localScale = new(scaleValue, scaleValue, scaleValue);
        }
        ApplySettings();
    }

   
    public void ApplySettings()
    {
        try
        {
            if (Player == null)
            {
                if (Nameplate != null) Player = Nameplate.GetComponentInParent<CVRPlayerEntity>();
            }

            if (Settings.ModernMovement is {Value: true})
            {
                if (_constraint == null)
                {
                    _constraint = Nameplate!.AddComponent<PositionConstraint>();
                    VRCPlates.Error("Constraint is null, forcefully adding it.");
                }
                
                if (_constraint.sourceCount > 1)
                {
                    VRCPlates.Error("Constraint.sourceCount is greater than 1, resetting...");
                    _constraint.SetSources(null);
                }

                if (_constraint.sourceCount == 1)
                {
                    if (_constraint.GetSource(0).sourceTransform == null)
                    {
                        VRCPlates.Debug("Removing Null Constraint Source");
                        _constraint.RemoveSource(0);
                    }
                }

                if (_constraint.sourceCount < 1)
                {
                    if (Player != null)
                    {
                        if (Player.PlayerDescriptor.TryGetComponent<PuppetMaster>(out var master))
                        {
                            var animator = master.GetAnimator();

                            if (animator != null)
                            {
                                var headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                                if (headBone != null)
                                {
                                    _constraint.AddSource(new ConstraintSource
                                    {
                                        sourceTransform = headBone,
                                        weight = 1
                                    });
                                }
                            }
                            else
                            {
                                VRCPlates.DebugError("VRCAvatarManager is null, cannot add constraint source.");
                            }
                        }
                        else
                        {
                            VRCPlates.DebugError("PuppetMaster is null, cannot add constraint source.");
                        }
                    }
                    else
                    {
                        VRCPlates.Error("Could not create constraint, player is null.");
                    }
                }
            }

            if (Settings.Offset != null)
            {
                if (_constraint != null)
                {
                    _constraint.translationOffset = new Vector3(0f, Settings.Offset.Value, 0f);
                    if (Settings.ModernMovement != null) _constraint.constraintActive = Settings.ModernMovement.Value;
                }
            }

            if (Settings.ShowRank != null)
                if (_rankText != null && Player != null)
                {
                    // ShowSocialRank = player.field_Private_APIUser_0.showSocialRank;
                    Rank = Player.ApiUserRank;
                }
            
            if (Settings.ShowIcon != null)
                if (_userPlate != null)
                    _userPlate.gameObject.SetActive(Settings.ShowIcon.Value && ProfilePicture != "");

            if (Settings.ShowVoiceBubble != null)
                if (_voiceBubble != null && Player != null)
                    _voiceBubble.gameObject.SetActive(Settings.ShowVoiceBubble.Value && Player.TalkerAmplitude > 0f);

            if (Settings.PlateColor != null && Settings.PlateColorByRank != null && Settings.BtkColorPlates != null)
            {
                if (Settings.BtkColorPlates.Value)
                {
                    if (Player != null) PlateColor = Utils.GetColourFromUserID(Player.Uuid);
                }
                else
                {
                    if (Settings.PlateColorByRank.Value)
                    {
                        if (Player != null) PlateColor = Utils.GetColorForSocialRank(Player.ApiUserRank);
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString(Settings.PlateColor.Value, out var color))
                            PlateColor = color;
                        else
                        {
                            PlateColor = Color.green;
                            Settings.PlateColor.Value = "#00FF00";
                            VRCPlates.DebugError("Invalid color string for nameplate color.");
                        }
                    }
                }
            }

            if (Settings.NameColor != null && Settings.NameColorByRank != null && Settings.BtkColorNames != null)
            {
                if (Settings.BtkColorNames.Value)
                {
                    if (Player != null) NameColor = Utils.GetColourFromUserID(Player.Uuid);
                }
                else
                {
                    if (Settings.NameColorByRank.Value)
                    {
                        if (Player != null) NameColor = Utils.GetColorForSocialRank(Player.ApiUserRank);
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString(Settings.NameColor.Value, out var color))
                            NameColor = color;
                        else
                        {
                            NameColor = Color.white;
                            Settings.NameColor.Value = "#FFFFFF";
                            VRCPlates.DebugError("Invalid color string for name color.");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            VRCPlates.Error("Unable to Apply Nameplate Settings: " + e);
        }
    }
    
    public void OnNameplateModeChanged()
    {
        if (Nameplate != null)
        {
            if (!MetaPort.Instance.settings.GetSettingsBool("GeneralShowNameplates") ||
                !MetaPort.Instance.worldEnableNameplates)
            {
                Nameplate.SetActive(false);
                return;
            }

            Nameplate.SetActive(true);
        }
    }
}