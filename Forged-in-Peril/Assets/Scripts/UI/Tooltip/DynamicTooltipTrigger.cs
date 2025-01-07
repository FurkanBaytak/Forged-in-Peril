using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DynamicTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TooltipManager tooltipManager;
    private Image targetImage;
    private Coroutine showCoroutine;

    void Awake()
    {
        targetImage = GetComponent<Image>();
        if (tooltipManager == null)
            tooltipManager = TooltipManager.Instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage && tooltipManager && tooltipManager.tooltipDataDatabase)
        {
            showCoroutine = StartCoroutine(ShowTooltipAfterDelay(0.5f));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
        tooltipManager.HideTooltip();
    }

    private IEnumerator ShowTooltipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        string spriteName = targetImage.sprite?.name;
        if (!string.IsNullOrEmpty(spriteName))
        {
            var data = tooltipManager.tooltipDataDatabase.GetTooltipDataBySpriteName(spriteName);
            if (data != null)
                tooltipManager.ShowTooltip(data);
        }
    }
}