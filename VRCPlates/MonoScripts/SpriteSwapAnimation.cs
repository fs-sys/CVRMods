using UnityEngine;
using UnityEngine.UI;

namespace VRCPlates.MonoScripts;

public class SpriteSwapAnimation : MonoBehaviour
{
    private void Start()
    {
        _isimageNull = image == null;
    }

    private void Awake()
    {
        _currentFrameTime = 0f;
        _framePeriod = 1f / frameRate;
    }

    private void OnEnable()
    {
        _currentFrameTime = 0f;
        _currentFrame = 0;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || sprites == null || _isimageNull || !image!.enabled || sprites.Length == 0)
        {
            return;
        }
        _currentFrameTime += Time.deltaTime;
        
        if (!(_currentFrameTime > _framePeriod)) return;
        
        _currentFrame++;
        if (_currentFrame >= sprites.Length)
        {
            _currentFrame = 0;
        }
        image.sprite = sprites[_currentFrame];
        _currentFrameTime = 0f;
    }

    public Image? image;
    public Sprite[]? sprites;
    public int frameRate = 4;
    private int _currentFrame;
    private float _currentFrameTime;
    private float _framePeriod = 1f;
    private bool _isimageNull;
}