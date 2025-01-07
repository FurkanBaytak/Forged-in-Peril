using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI simpleTooltipText;
    public TextMeshProUGUI simpleTooltipTitleText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;
    public TooltipDataDatabase tooltipDataDatabase;

    public float fadeDuration = 0.2f;
    public float scaleDuration = 0.2f;
    public Vector3 targetScale = Vector3.one;
    public Vector2 offset = new Vector2(10f, 10f);
    public float screenEdgeMargin = 300f;

    private Vector3 initialScale;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private bool isVisible = false;

    void Awake()
    {
        Instance = this;
        canvasGroup = tooltipPanel.GetComponent<CanvasGroup>() ?? tooltipPanel.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        initialScale = tooltipPanel.transform.localScale;
        tooltipPanel.transform.localScale = initialScale * 0;
        canvas = tooltipPanel.GetComponentInParent<Canvas>();
        tooltipDataDatabase?.Initialize();
        HideTooltipInstant();
    }

    void Update()
    {
        if (isVisible)
            UpdateTooltipPosition();
    }

    private void UpdateTooltipPosition()
    {
        Vector3 mousePos = Input.mousePosition + new Vector3(offset.x, offset.y, 0);
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Vector2 tooltipSize = tooltipRect.sizeDelta;

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out anchoredPos
        );

        Vector2 pivot = Vector2.zero;

        bool isLeft = mousePos.x > Screen.width / 2;
        bool isTop = mousePos.y < Screen.height / 2;

        if (isLeft && isTop)
        {
            pivot = new Vector2(1, 0);
        }
        else if (isLeft && !isTop)
        {
            pivot = new Vector2(1, 1);
        }
        else if (!isLeft && isTop)
        {
            pivot = new Vector2(0, 0);
        }
        else if (!isLeft && !isTop)
        {
            pivot = new Vector2(0, 1);
        }

        tooltipRect.pivot = pivot;

        tooltipPanel.transform.position = mousePos;

        Vector3[] worldCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(worldCorners);

        float minX = worldCorners[0].x;
        float maxX = worldCorners[2].x;
        float minY = worldCorners[0].y;
        float maxY = worldCorners[2].y;

        Vector3 finalPosition = tooltipPanel.transform.position;

        if (minX < 0)
            finalPosition.x -= minX;
        if (maxX > Screen.width)
            finalPosition.x -= (maxX - Screen.width);
        if (minY < 0)
            finalPosition.y -= minY;
        if (maxY > Screen.height)
            finalPosition.y -= (maxY - Screen.height);

        tooltipPanel.transform.position = finalPosition;
    }


    public void ShowTooltip(string title, string content)
    {
        if (isVisible)
        {
            simpleTooltipText.text = content;
            simpleTooltipTitleText.text = title;
            simpleTooltipText.gameObject.SetActive(true);
            simpleTooltipTitleText.gameObject.SetActive(true);
            titleText.gameObject.SetActive(false);
            descriptionText.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
            return;
        }

        isVisible = true;
        simpleTooltipText.text = content;
        simpleTooltipTitleText.text = title;
        simpleTooltipText.gameObject.SetActive(true);
        simpleTooltipTitleText.gameObject.SetActive(true);
        titleText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        iconImage.gameObject.SetActive(false);
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.DOKill();
        tooltipPanel.transform.localScale = initialScale * 0;
        tooltipPanel.transform.DOScale(targetScale, scaleDuration).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.Linear);
    }

    public void ShowTooltip(ITooltipData data)
    {
        if (isVisible)
        {
            titleText.text = data.Title;
            descriptionText.text = data.Description;
            iconImage.sprite = data.Icon;
            simpleTooltipText.gameObject.SetActive(false);
            simpleTooltipTitleText.gameObject.SetActive(false);
            titleText.gameObject.SetActive(true);
            descriptionText.gameObject.SetActive(true);
            iconImage.gameObject.SetActive(true);
            return;
        }

        isVisible = true;
        titleText.text = data.Title;
        descriptionText.text = data.Description;
        iconImage.sprite = data.Icon;
        simpleTooltipText.gameObject.SetActive(false);
        simpleTooltipTitleText.gameObject.SetActive(false);
        titleText.gameObject.SetActive(true);
        descriptionText.gameObject.SetActive(true);
        iconImage.gameObject.SetActive(true);
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.DOKill();
        tooltipPanel.transform.localScale = initialScale * 0;
        tooltipPanel.transform.DOScale(targetScale, scaleDuration).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.Linear);
    }

    public void HideTooltip()
    {
        if (!isVisible)
            return;

        isVisible = false;
        tooltipPanel.transform.DOScale(initialScale, scaleDuration).SetEase(Ease.InBack);
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() => tooltipPanel.SetActive(false));
    }

    private void HideTooltipInstant()
    {
        isVisible = false;
        tooltipPanel.SetActive(false);
        canvasGroup.alpha = 0f;
        tooltipPanel.transform.localScale = initialScale * 0;
    }
}
