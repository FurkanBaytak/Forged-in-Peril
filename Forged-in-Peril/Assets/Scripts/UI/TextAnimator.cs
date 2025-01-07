using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextAnimator : MonoBehaviour
{
    [Header("Text Settings")]
    public TextMeshProUGUI textTMP;  
    public Vector3 targetPosition;     
    public Vector3 startPosition;       

    [Header("Animation Settings")]
    public float moveDuration = 1f;     
    public float zoomDuration = 0.5f;      
    public int zoomLoops = 3;         
    public float zoomScale = 1.1f;   
    public float hideDuration = 1f;   
    public float displayTime = 2f;  

    private Vector3 originalScale;  

    void Start()
    {
        if (textTMP == null)
        {
            textTMP = GetComponent<TextMeshProUGUI>();
            if (textTMP == null)
            {
                Debug.LogError("TextMeshProUGUI component not found!");
                return;
            }
        }

        originalScale = textTMP.transform.localScale;
        textTMP.transform.position = startPosition;
        textTMP.transform.localScale = Vector3.zero;

        AnimateText();
    }

    void AnimateText()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(textTMP.transform.DOMove(targetPosition, moveDuration).SetEase(Ease.OutBack));
        sequence.Join(textTMP.transform.DOScale(originalScale, moveDuration).SetEase(Ease.OutBack));

        sequence.AppendInterval(2f);
        sequence.Append(textTMP.transform.DOScale(originalScale * zoomScale, zoomDuration).SetLoops(zoomLoops * 2, LoopType.Yoyo).SetEase(Ease.InOutSine));

        sequence.AppendInterval(displayTime);

        sequence.Append(textTMP.transform.DOMove(startPosition, hideDuration).SetEase(Ease.InBack));
        sequence.Join(textTMP.transform.DOScale(0, hideDuration).SetEase(Ease.InBack));

        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    [ContextMenu("Play Animation")]
    public void PlayAnimation()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            textTMP.transform.position = startPosition;
            textTMP.transform.localScale = Vector3.zero;
            AnimateText();
        }
    }
}
