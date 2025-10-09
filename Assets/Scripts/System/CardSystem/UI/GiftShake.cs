using UnityEngine;
using DG.Tweening;

public class GiftShake : MonoBehaviour
{
    [Header("抖动配置")]
    [Tooltip("每次抖动的持续时间")]
    public float shakeDuration = 0.5f;

    [Tooltip("两次抖动之间的间隔时间")]
    public float interval = 1f;

    [Tooltip("围绕 Z 轴左右抖动的角度")]
    public float shakeAngle = 15f;
    
    private Sequence _shakeSequence;
    
    public void CreateShakeSequence()
    {
        if (_shakeSequence != null) _shakeSequence.Kill();

        _shakeSequence = DOTween.Sequence();
        
        _shakeSequence.Append(transform.DORotate(new Vector3(0, 0, -shakeAngle), shakeDuration));
        _shakeSequence.Append(transform.DORotate(new Vector3(0, 0, shakeAngle), shakeDuration));
        _shakeSequence.Append(transform.DORotate(new Vector3(0, 0, 0), shakeDuration));

        _shakeSequence.AppendInterval(interval);

        _shakeSequence.SetLoops(-1, LoopType.Restart);
        _shakeSequence.Pause();
    }
    
    public void StartShaking()
    {
        // 确保序列存在且当前未播放
        if (_shakeSequence != null && !_shakeSequence.IsPlaying())
        {
            _shakeSequence.Play();
        }
    }
    
   
    
    public void StopShaking()
    {
        if (_shakeSequence != null && _shakeSequence.IsPlaying())
        {
            _shakeSequence.Pause();
            transform.localRotation = Quaternion.identity;
        }
    }

    void OnDestroy()
    {
        if (_shakeSequence != null)
        {
            _shakeSequence.Kill();
        }
    }
}