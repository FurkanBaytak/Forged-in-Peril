using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string simpleTitle;
    public string simpleText;
    public ScriptableObject tooltipData;
    public bool useTooltipData = true;
    private Coroutine showCoroutine;

    public void SetTooltipData(ITooltipData data)
    {
        tooltipData = data as ScriptableObject;
        useTooltipData = data != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        showCoroutine = StartCoroutine(ShowTooltipAfterDelay(0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
        TooltipManager.Instance?.HideTooltip();
    }

    private IEnumerator ShowTooltipAfterDelay(float delay)
    {   
        yield return new WaitForSeconds(delay);
        if (useTooltipData && tooltipData is ITooltipData data)
        {
            TooltipManager.Instance?.ShowTooltip(data);
        }
        else if (!string.IsNullOrEmpty(simpleText))
        {
            TooltipManager.Instance?.ShowTooltip(simpleTitle, simpleText);
        }
    }
}