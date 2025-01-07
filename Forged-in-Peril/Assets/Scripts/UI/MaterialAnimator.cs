using UnityEngine;
using DG.Tweening;
using System;

public class MaterialAnimator : MonoBehaviour
{
    private bool isSelected = false;
    private bool isFirstSelect = true;
    public float dropHeight = 2f;
    public float dropDuration = 0.5f;
    public float bounceHeight = 0.1f;
    public float bounceDuration = 0.2f;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip selectClip;

    [Header("Character Animator")]
    public Animator characterAnimator;

    private Vector3 originalPosition;

    void OnEnable()
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

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("Material");
        }

        DropAnimation();
    }

    public void Deselect(Action onComplete = null)
    {
        if (!isSelected) return;
        isSelected = false;

        transform.DOMove(originalPosition, dropDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }

    private void DropAnimation()
    {
        transform.position = originalPosition + Vector3.up * dropHeight;

        Sequence dropSequence = DOTween.Sequence();

        dropSequence.Append(transform.DOMove(originalPosition, dropDuration).SetEase(Ease.OutCubic));
        dropSequence.Append(transform.DOMove(originalPosition + Vector3.up * bounceHeight, bounceDuration).SetEase(Ease.OutSine));
        dropSequence.Append(transform.DOMove(originalPosition, bounceDuration).SetEase(Ease.InSine));

        dropSequence.OnComplete(() => { });
    }
}
