using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ESCMenuManager : MonoBehaviour
{
    public static ESCMenuManager Instance;

    public GameObject escMenuCanvas;
    public CanvasGroup canvasGroup;
    public Button resumeButton;
    public Button qualitySettingsButton;
    public Button exitButton;

    public GameObject optionsPanelPrefab;
    public GameObject exitPanelPrefab;

    public float fadeInDuration = 0.25f;
    public float fadeOutDuration = 0.25f;

    private bool isMenuActive = false;

    private GameObject currentOptionsPanel;
    private CanvasGroup canvasGroupOptionsPanel;

    private GameObject currentExitPanel;
    private CanvasGroup canvasGroupExitPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = escMenuCanvas.GetComponent<CanvasGroup>();
        }

        escMenuCanvas.SetActive(false);
        SetCanvasGroupAlpha(canvasGroup, 0);

        resumeButton.onClick.AddListener(OnResumeButtonPressed);
        qualitySettingsButton.onClick.AddListener(OnQualitySettingsButtonPressed);
        exitButton.onClick.AddListener(OnExitButtonPressed);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (isMenuActive)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    private void ShowMenu()
    {
        escMenuCanvas.SetActive(true);
        FadeInUI(canvasGroup, null);
        Time.timeScale = 0f;
        isMenuActive = true;
    }

    private void HideMenu()
    {
        FadeOutUI(canvasGroup, () =>
        {
            escMenuCanvas.SetActive(false);
            HideAllPanels();
            Time.timeScale = 1f;
        });
        isMenuActive = false;
    }

    private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
    {
        cg.alpha = alpha;
        cg.interactable = alpha > 0;
        cg.blocksRaycasts = alpha > 0;
    }

    private void OnResumeButtonPressed()
    {
        HideMenu();
    }

    private void OnQualitySettingsButtonPressed()
    {
        if (currentExitPanel != null && currentExitPanel.activeSelf)
        {
            HidePanel(currentExitPanel, canvasGroupExitPanel);
        }

        if (currentOptionsPanel == null)
        {
            currentOptionsPanel = Instantiate(optionsPanelPrefab, escMenuCanvas.transform);
            canvasGroupOptionsPanel = currentOptionsPanel.GetComponent<CanvasGroup>();
            if (canvasGroupOptionsPanel == null)
            {
                canvasGroupOptionsPanel = currentOptionsPanel.AddComponent<CanvasGroup>();
            }
            SetCanvasGroupAlpha(canvasGroupOptionsPanel, 0);
            currentOptionsPanel.SetActive(false);

            AssignPanelButtons(currentOptionsPanel, canvasGroupOptionsPanel);
        }

        if (currentOptionsPanel.activeSelf)
        {
            HidePanel(currentOptionsPanel, canvasGroupOptionsPanel);
        }
        else
        {
            ShowPanel(currentOptionsPanel, canvasGroupOptionsPanel);
        }
    }

    private void OnExitButtonPressed()
    {
        if (currentOptionsPanel != null && currentOptionsPanel.activeSelf)
        {
            HidePanel(currentOptionsPanel, canvasGroupOptionsPanel);
        }

        if (currentExitPanel == null)
        {
            currentExitPanel = Instantiate(exitPanelPrefab, escMenuCanvas.transform);
            canvasGroupExitPanel = currentExitPanel.GetComponent<CanvasGroup>();
            if (canvasGroupExitPanel == null)
            {
                canvasGroupExitPanel = currentExitPanel.AddComponent<CanvasGroup>();
            }
            SetCanvasGroupAlpha(canvasGroupExitPanel, 0);
            currentExitPanel.SetActive(false);

            AssignPanelButtons(currentExitPanel, canvasGroupExitPanel);
        }

        if (currentExitPanel.activeSelf)
        {
            HidePanel(currentExitPanel, canvasGroupExitPanel);
        }
        else
        {
            ShowPanel(currentExitPanel, canvasGroupExitPanel);
        }
    }

    private void AssignPanelButtons(GameObject panel, CanvasGroup cg)
    {
        Button[] buttons = panel.GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            string btnName = btn.gameObject.name.ToLower();

            if (btnName.Contains("close"))
            {
                btn.onClick.AddListener(() => HidePanel(panel, cg));
            }
            else if (btnName.Contains("cancel"))
            {
                btn.onClick.AddListener(() => HidePanel(panel, cg));
            }
            else if (btnName.Contains("continue"))
            {
                btn.onClick.AddListener(HideAllPanels);
            }
        }
    }

    private void ShowPanel(GameObject panel, CanvasGroup cg)
    {
        panel.SetActive(true);
        FadeInUI(cg, null);
    }

    private void HidePanel(GameObject panel, CanvasGroup cg)
    {
        FadeOutUI(cg, () =>
        {
            panel.SetActive(false);
        });
    }

    public void HideAllPanels()
    {
        if (currentOptionsPanel != null && currentOptionsPanel.activeSelf)
        {
            HidePanel(currentOptionsPanel, canvasGroupOptionsPanel);
        }

        if (currentExitPanel != null && currentExitPanel.activeSelf)
        {
            HidePanel(currentExitPanel, canvasGroupExitPanel);
        }
    }

    private void FadeInUI(CanvasGroup cg, Action onComplete = null)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.DOFade(1, fadeInDuration)
          .SetEase(Ease.InOutQuad)
          .SetUpdate(true)
          .OnStart(() =>
          {
              cg.interactable = true;
              cg.blocksRaycasts = true;
          }).OnComplete(() =>
          {
              onComplete?.Invoke();
          });
    }

    private void FadeOutUI(CanvasGroup cg, Action onComplete = null)
    {
        cg.DOFade(0, fadeOutDuration)
          .SetEase(Ease.InOutQuad)
          .SetUpdate(true)
          .OnComplete(() =>
          {
              cg.interactable = false;
              cg.blocksRaycasts = false;
              onComplete?.Invoke();
          });
    }
}
