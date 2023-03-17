using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.AvatarTracking.Remote;
using ABI_RC.Core.Savior;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRCPlates.Reflection;
using static VRCPlates.AssetManager;

namespace VRCPlates.MonoScripts;

public class OldNameplate : MonoBehaviour
{
    public CVRPlayerEntity Player = null!;
    public bool qmOpen;
    
    private bool _isFriend;
    private bool _isMuted;
    private bool _isHidden;
    private bool _isBlocked;
    private bool _isLocal;
    private string? _vipRank;

    private Color _plateColor;
    private Color _nameColor;
    private string? _name;
    private string? _rank;
    
    private string? _profilePicture;
    private string? _plateBackground;
    
    internal GameObject? Nameplate;
    private Transform _transform = null!;
    private RemoteHeadPoint _headPoint = null!;
    private PositionConstraint _constraint = null!;
    private Camera _camera = null!;

    private Image _mainPlate = null!;
    private Text _mainText = null!;
    private RawImage _mainBackground = null!;

    // private Image? _afkPlate;
    // private Text? _afkText;
    // private RawImage? _afkBackground;

    private Image _userPlate = null!;
    private RawImage _userIcon = null!;

    private Image _vipPlate = null!;
    private Text _vipText = null!;
    private RawImage _vipBackground = null!;

    private Image _voiceBubble = null!;

    private Image _badgeHidden = null!;
    public Image badgeCompat = null!;

    private Image _iconFriend = null!;
    private Text _rankText = null!;
    
    private bool _isVoiceBubbleNotNull;
    private bool _isNameplateNotNull;
    private bool _isMainPlateNotNull;
    private bool _isVipPlateNotNull;


    public bool IsLocal
    {
        get => _isLocal;
        set
        {
            _isLocal = value;
            Nameplate!.SetActive(!_isLocal);
        }
    }

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

    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            _isHidden = value;
            if (_badgeHidden == null) return;
            _badgeHidden.gameObject.SetActive(_isHidden);
        }
    }

    public string? VipRank
    {
        get => _vipRank;
        set
        {
            _vipRank = value;
            if (_vipText != null) _vipText.text = _vipRank;
            if (_vipPlate != null) _vipPlate.transform.parent.gameObject.SetActive(_vipRank != null);
        }
    }

    public Color PlateColor
    {
        get => _plateColor;
        set
        {
            // if (Settings.RainbowPlates == null || Settings.RainbowFriends == null) { return; }
                
            _plateColor = value;

            // if (Settings.RainbowPlates.Value | (Settings.RainbowFriends.Value && IsFriend))
            // {
            //     return;
            // }

            if (_mainPlate != null) _mainPlate.color = _plateColor;
                if (_vipPlate != null) _vipPlate.color = _plateColor;
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
    
    [SuppressMessage("ReSharper", "IteratorMethodResultIsIgnored")]
    public string? ProfilePicture
    {
        get => _profilePicture;
        set
        {
            _profilePicture = value;
            if (string.IsNullOrEmpty(_profilePicture)) return;
            if (_profilePicture is null || _userIcon == null) return;
            NameplateManager.AddImageToQueue(_profilePicture, new [] {_userIcon});
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
            if (_mainBackground != null && _vipBackground != null)
                NameplateManager.AddImageToQueue(_plateBackground, new [] {_mainBackground, _vipBackground});
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
        _mainBackground = Nameplate.transform.Find("Main/Mask/Background").GetComponent<RawImage>();

        _userPlate = Nameplate.transform.Find("VIP/Icon").GetComponent<Image>();
        _userIcon = Nameplate.transform.Find("VIP/Icon/Image").GetComponent<RawImage>();

        _vipPlate = Nameplate.transform.Find("VIP/Plate/Plate").GetComponent<Image>();
        _vipText = Nameplate.transform.Find("VIP/Plate/Text").GetComponent<Text>();
        _vipBackground = Nameplate.transform.Find("VIP/Plate/Text").GetComponent<RawImage>();

        _voiceBubble = Nameplate.transform.Find("Voice/Bubble").GetComponent<Image>();
        Nameplate.transform.Find("Voice/Volume").GetComponent<Text>();

        _badgeHidden = Nameplate.transform.Find("Badges/Hidden").GetComponent<Image>();
        badgeCompat = Nameplate.transform.Find("Badges/Compat").GetComponent<Image>();

        _iconFriend = Nameplate.transform.Find("FriendIcon").GetComponent<Image>();
        
        _rankText = Nameplate.transform.Find("Rank").GetComponent<Text>();

        if (VRCPlates.NameplateManager == null) return;
        
        var descriptor = Nameplate.GetComponentInParent<PlayerDescriptor>();
        if (descriptor == null) return;
        
        var player = PlayerUtils.GetPlayerEntity(descriptor.ownerId);
        if (player == null)
        {
            VRCPlates.NameplateManager.RemoveNameplate(descriptor.ownerId);
            Destroy(Nameplate);
            return;
        }
        
        _isNameplateNotNull = Nameplate != null;
        _isVoiceBubbleNotNull = _voiceBubble != null;
        _isVipPlateNotNull = _vipPlate != null;
        _isMainPlateNotNull = _mainPlate != null;

        NameplateManager.InitializePlate(this, descriptor);
                    
        StartCoroutine(SpeechManagement());
        StartCoroutine(PlateManagement());
        
        ApplySettings();
    }

    private IEnumerator SpeechManagement()
    {
        while (true)
        {
            if (_isNameplateNotNull && Nameplate!.activeInHierarchy && Player != null && SpriteDict != null)
            {
                if (Player.TalkerAmplitude > 0f)
                {
                    if (_isVoiceBubbleNotNull && !_voiceBubble.gameObject.activeInHierarchy)
                    {
                        if (Settings.ShowVoiceBubble != null)
                        {
                            _voiceBubble.gameObject.SetActive(Settings.ShowVoiceBubble.Value);
                        }

                        if (_isMainPlateNotNull && _mainPlate.gameObject.activeInHierarchy)
                        {
                            _mainPlate.sprite = SpriteDict["nameplatetalk"];
                        }

                        if (_isVipPlateNotNull && _vipPlate.gameObject.activeInHierarchy)
                        {
                            _vipPlate.sprite = SpriteDict["nameplatetalk"];
                        }
                    }
                }
                else
                {
                    if (_isVoiceBubbleNotNull)
                    {
                        _voiceBubble.gameObject.SetActive(IsMuted);

                        if (_isMainPlateNotNull && _mainPlate.gameObject.activeInHierarchy)
                        {
                            _mainPlate.sprite = SpriteDict["nameplate"];
                        }
                        
                        if (_isVipPlateNotNull && _vipPlate.gameObject.activeInHierarchy)
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

    private IEnumerator PlateManagement()
    {
        while (true)
        {
            if (_isNameplateNotNull && Nameplate!.activeInHierarchy && Player != null)
            {
                IsFriend = Friends.FriendsWith(Player.Uuid);
            }

            yield return new WaitForSeconds(3f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    /*private IEnumerator Rainbow()
    {
        while (true)
        {
            if (Settings.RainbowFriends != null && Settings.RainbowPlates != null &&
                Settings.RainbowPlates.Value | (Settings.RainbowFriends.Value && IsFriend) && _isNameplateNotNull &&
                Nameplate!.activeInHierarchy && Player != null)
            {
                if (_mainPlate != null && _mainPlate.gameObject.activeInHierarchy)
                {
                    _mainPlate.color = Color.Lerp(_mainPlate.color, Color.red, Time.deltaTime * 2);
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
    */
    
    public void Update()
    {
        transform.LookAt(2f * transform.position - _camera.transform.position);
    }

    public void ApplySettings()
    {
        try
        {
            Nameplate ??= gameObject;

            if (Player != null)
            {
                OnScaleUpdate();

                OnOffsetUpdate();

                OnModernMovementToggle();

                OnShowRankToggle();

                OnShowPfpToggle();

                OnVoiceBubbleToggle();

                OnPlateColorUpdate();

                OnNameColorUpdate();

                IsFriend = Friends.FriendsWith(Player.Uuid);

                OnVisibilityUpdate();
            }
            else
            {
                VRCPlates.Error("[0006] Player is null, cannot apply settings.");
            }
        }
        catch (Exception e)
        {
            VRCPlates.Error("[0003] Unable to Apply Nameplate Settings:\n" + e);
        }
    }

    public void OnVisibilityUpdate()
    {
        var nameplateSetting = MetaPort.Instance.settings.GetSettingInt("GeneralNameplatesVisibility");
        if (!MetaPort.Instance.worldEnableNameplates || nameplateSetting == 2 ||
            (nameplateSetting == 1 && !ViewManager.Instance.isGameMenuOpen()))
        {
            Nameplate!.SetActive(false);
        }
        else
        {
            Nameplate!.SetActive(true);
        }

        Nameplate!.SetActive(Settings.Enabled is {Value: true});
    }

    public void OnNameColorUpdate()
    {
        if (Settings.BtkColorNames is {Value: true})
        {
            NameColor = Utils.GetColourFromUserID(Player.Uuid ?? "00000000-0000-0000-0000-000000000000");
        }
        else
        {
            if (Settings.NameColorByRank is {Value: true})
            {
                NameColor = Utils.GetColorForSocialRank(Player.ApiUserRank ?? "User");
            }
            else
            {
                if (Settings.NameColor != null &&
                    ColorUtility.TryParseHtmlString(Settings.NameColor.Value, out var color))
                    NameColor = color;
                else
                {
                    NameColor = Color.white;
                    if (Settings.NameColor != null) Settings.NameColor.Value = "#FFFFFF";
                    VRCPlates.DebugError("Invalid color string for name color.");
                }
            }
        }
    }

    public void OnPlateColorUpdate()
    {
        if (Settings.BtkColorPlates is {Value: true})
        {
            PlateColor = Utils.GetColourFromUserID(Player.Uuid ?? "00000000-0000-0000-0000-000000000000");
        }
        else
        {
            if (Settings.PlateColorByRank is {Value: true})
            {
                PlateColor = Utils.GetColorForSocialRank(Player.ApiUserRank ?? "User");
            }
            else
            {
                if (Settings.PlateColor != null && ColorUtility.TryParseHtmlString(Settings.PlateColor.Value, out var color))
                    PlateColor = color;
                else
                {
                    PlateColor = Color.green;
                    if (Settings.PlateColor != null) Settings.PlateColor.Value = "#00FF00";
                    VRCPlates.DebugError("Invalid color string for nameplate color.");
                }
            }
        }
    }

    public void OnVoiceBubbleToggle()
    {
        if (Settings.ShowVoiceBubble == null) return;
        if (_voiceBubble != null)
            _voiceBubble.gameObject.SetActive(Settings.ShowVoiceBubble.Value &&
                                              Player.TalkerAmplitude > 0f);
    }

    public void OnShowPfpToggle()
    {
        var showPfp = MetaPort.Instance.settings.GetSettingInt("GeneralNameplatesImageVisibility");
        if ((showPfp == 0 || (showPfp == 1 && IsFriend)) && ProfilePicture != "" &&
            ProfilePicture != "https://files.abidata.io/user_images/00default.png")
        {
            if (_userPlate != null)
                _userPlate.gameObject.SetActive(true);
        }
        else
        {
            if (_userPlate != null)
                _userPlate.gameObject.SetActive(false);
        }
    }

    public void OnShowRankToggle()
    {
        if (Settings.ShowRank != null)
            if (_rankText != null && Player != null)
            {
                // ShowSocialRank = player.field_Private_APIUser_0.showSocialRank;
                Rank = Player.ApiUserRank;
            }
    }

    public void OnOffsetUpdate()
    {
        if (Settings.Offset == null) return;

        if (Player.PlayerDescriptor.TryGetComponent<PuppetMaster>(out var puppetMaster))
        {
            if (_transform == null)
            {
                VRCPlates.Error("Transform is null, cannot apply offset");
                return;
            }
            
            _headPoint = puppetMaster.GetViewPoint();

            var headPosition = _headPoint.GetPointPosition();
            _transform.position.Set(headPosition.x, headPosition.y + Settings.Offset.Value, headPosition.z);
        }
        else
        {
            VRCPlates.Error("PuppetMaster is null, cannot apply offset.");
        }
    }

    public void OnScaleUpdate()
    {
        if (Settings.Scale != null)
        {
            var scaleValue = Settings.Scale.Value * .001f;
            if (_transform != null)
            {
                _transform.localScale.Set(scaleValue, scaleValue, scaleValue);
            }
        }
    }

    public void OnModernMovementToggle()
    {
        if (Settings.ModernMovement is not {Value: true}) return;
        if (Settings.Offset == null) return;
        
        if (_constraint == null)
        {
            _constraint = Nameplate!.AddComponent<PositionConstraint>();
            VRCPlates.Error("[0000] Constraint is null, forcefully adding it.");
        }

        if (_constraint.sourceCount > 1)
        {
            VRCPlates.Error("[0001] Constraint.sourceCount is greater than 1, resetting...");
            _constraint.SetSources(null);
        }

        if (_constraint.sourceCount == 1)
        {
            if (_constraint.GetSource(0).sourceTransform == null)
            {
                VRCPlates.Debug("[0002] Removing Null Constraint Source");
                _constraint.RemoveSource(0);
            }
        }
        
        if (_constraint.sourceCount < 1)
        {
            try
            {
                _constraint.AddSource(new ConstraintSource
                {
                    sourceTransform = _headPoint.GetTransform(),
                    weight = 1
                });
            }
            catch
            {
                VRCPlates.DebugError("No Headpoint Found for: " + Player.Username);
            }
        }

        if (_constraint != null)
        {
            _constraint.translationOffset = new Vector3(0f, Settings.Offset.Value, 0f);
            _constraint.constraintActive = Settings.ModernMovement.Value;
        }
    }
}