using UnityEngine;
using DG.Tweening; 

public class WeaponAnimator : MonoBehaviour
{
    private bool isSelected = false;
    private bool isFirstSelect = true;
    private Tween oscillationTween;
    private Vector3 originalPosition;

    [SerializeField] private AudioSource sfxSource;  
    [SerializeField] private AudioClip selectClip;   

    [SerializeField]
    private float moveUpAmount = 0.6f;
    [SerializeField]
    private float oscillateAmplitude = 0.1f;
    [SerializeField]
    private float oscillateDuration = 0.5f;

    void Start()
    {
        originalPosition = transform.position;
    }

    public void Select()
    {
        if (isSelected) return;
        isSelected = true;


        if (!isFirstSelect && sfxSource != null && selectClip != null)
        {
            sfxSource.PlayOneShot(selectClip);
        }
        else
        {
            isFirstSelect = false;
        }

        transform.DOMoveY(originalPosition.y + moveUpAmount, 0.5f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                StartOscillation();
            });
    }

    public void Deselect()
    {
        if (!isSelected) return;
        isSelected = false;

        if (oscillationTween != null && oscillationTween.IsActive())
        {
            oscillationTween.Kill();
        }

        transform.DOMoveY(originalPosition.y, 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
            });
    }

    private void StartOscillation()
    {
        oscillationTween = transform.DOMoveY(oscillateAmplitude, oscillateDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative(true);
    }
}
