using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    public _SelectionManager selectionManager;
    public CPC_CameraPath cameraPath;
    public GameObject UI_1;
    public GameObject UI_2;
    public GameObject OptionsPanel;
    public GameObject ExitPanel;
    public GameObject cloudLoading;
    [SerializeField] private AudioSource windSource;
    [SerializeField] private AudioClip windClip;

    private CanvasGroup canvasGroupUI1;
    private CanvasGroup canvasGroupUI2;
    private CanvasGroup canvasGroupOptionsPanel;
    private CanvasGroup canvasGroupExitPanel;
    private int currentStep = 1;
    private bool isMoving = false;

    public float fadeInDuration = 1f;
    public float fadeOutDuration = 0.5f;

    void Start()
    {
        canvasGroupUI1 = UI_1.GetComponent<CanvasGroup>();
        if (canvasGroupUI1 == null)
            canvasGroupUI1 = UI_1.AddComponent<CanvasGroup>();

        canvasGroupUI2 = UI_2.GetComponent<CanvasGroup>();
        if (canvasGroupUI2 == null)
            canvasGroupUI2 = UI_2.AddComponent<CanvasGroup>();

        canvasGroupOptionsPanel = OptionsPanel.GetComponent<CanvasGroup>();
        if (canvasGroupOptionsPanel == null)
            canvasGroupOptionsPanel = OptionsPanel.AddComponent<CanvasGroup>();

        canvasGroupExitPanel = ExitPanel.GetComponent<CanvasGroup>();
        if (canvasGroupExitPanel == null)
            canvasGroupExitPanel = ExitPanel.AddComponent<CanvasGroup>();

        SetCanvasGroupAlpha(canvasGroupUI1, 0);
        SetCanvasGroupAlpha(canvasGroupUI2, 0);
        SetCanvasGroupAlpha(canvasGroupOptionsPanel, 0);
        SetCanvasGroupAlpha(canvasGroupExitPanel, 0);

        UI_1.SetActive(false);
        UI_2.SetActive(false);

        if (_GameManager.Instance.isReturning)
        {
            currentStep = 3;
            cameraPath.SetCurrentWayPoint(1);
            cameraPath.PlayPath(5f);
            PlayWindSFX(2f);
            StartCoroutine(WaitForPathCompletion(() =>
            {
                UI_2.SetActive(true);
                FadeInUI(canvasGroupUI2, () =>
                {
                    _GameManager.Instance.isReturning = false;
                });
            }));
        }
        else
        {
            MoveCameraToNextPoint();
        }
    }

    private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
    {
        cg.alpha = alpha;
        cg.interactable = alpha > 0;
        cg.blocksRaycasts = alpha > 0;
    }

    public void MoveCameraToNextPoint()
    {
        if (isMoving) return;
        isMoving = true;

        switch (currentStep)
        {
            case 1:
                cameraPath.SetCurrentWayPoint(0);
                cameraPath.PlayPath(15f);
                PlayWindSFX(5f);
                StartCoroutine(WaitForPathCompletion(() =>
                {
                    isMoving = false;
                    UI_1.SetActive(true);
                    FadeInUI(canvasGroupUI1, () => { currentStep = 2; });
                }));
                break;

            case 2:
                FadeOutUI(canvasGroupUI1, () =>
                {
                    UI_1.SetActive(false);
                    cameraPath.SetCurrentWayPoint(1);
                    cameraPath.PlayPath(5f);
                    PlayWindSFX(2f);
                    StartCoroutine(WaitForPathCompletion(() =>
                    {
                        isMoving = false;
                        UI_2.SetActive(true);
                        FadeInUI(canvasGroupUI2, () => { currentStep = 3; });
                    }));
                });
                break;

            case 3:
                FadeOutUI(canvasGroupUI2, () =>
                {
                    UI_2.SetActive(false);
                    cameraPath.PlayPathReverseTo(0, 5f);
                    PlayWindSFX(2f);
                    StartCoroutine(WaitForPathCompletion(() =>
                    {
                        isMoving = false;
                        UI_1.SetActive(true);
                        FadeInUI(canvasGroupUI1, () => { currentStep = 2; });
                    }));
                });
                break;

            default:
                isMoving = false;
                break;
        }
    }

    private void FadeInUI(CanvasGroup cg, Action onComplete = null)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.DOFade(1, fadeInDuration).SetEase(Ease.InOutQuad).OnStart(() =>
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
        cg.DOFade(0, fadeOutDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
            onComplete?.Invoke();
        });
    }

    IEnumerator WaitForPathCompletion(Action onComplete)
    {
        while (cameraPath.IsPlaying()) yield return null;
        onComplete?.Invoke();
    }

    public void OnOptionsButtonPressed()
    {
        if (currentStep == 2)
        {
            ExitPanel.SetActive(false);
            OptionsPanel.SetActive(true);
            FadeInUI(canvasGroupOptionsPanel);
        }
    }

    public void OnOptionsCloseButtonPressed()
    {
        if (currentStep == 2)
        {
            FadeOutUI(canvasGroupOptionsPanel, () =>
            {
                OptionsPanel.SetActive(false);
            });
        }
    }

    public void OnExitButtonPressed()
    {
        if (currentStep == 2)
        {
            OptionsPanel.SetActive(false);
            ExitPanel.SetActive(true);
            FadeInUI(canvasGroupExitPanel);
        }
    }

    public void OnExitCloseButtonPressed()
    {
        if (currentStep == 2)
        {
            FadeOutUI(canvasGroupExitPanel, () =>
            {
                ExitPanel.SetActive(false);
            });
        }
    }

    public void OnStartButtonPressed()
    {
        if (currentStep == 2)
        {
            OptionsPanel.SetActive(false);
            ExitPanel.SetActive(false);
            FadeOutUI(canvasGroupUI1, () =>
            {
                UI_1.SetActive(false);
                MoveCameraToNextPoint();
            });
        }
    }

    public void OnBackButtonPressed()
    {
        if (currentStep == 3)
        {
            FadeOutUI(canvasGroupUI2, () =>
            {
                UI_2.SetActive(false);
                MoveCameraToNextPoint();
            });
        }
    }

    public void OnCraftButtonPressed()
    {
        cloudLoading.SetActive(true);
        if (currentStep == 3)
        {
            FadeOutUI(canvasGroupUI2, () =>
            {
                UI_2.SetActive(false);
                selectionManager.LockCharacter();
                StartCoroutine(WaitAndProceedToNextScene());
            });
        }
        _GameManager.Instance.unlockedWeapon.Clear();
        _GameManager.Instance.unlockedMaterials.Clear();
    }

    private IEnumerator WaitAndProceedToNextScene()
    {
        DontDestroyOnLoad(_GameManager.Instance.gameObject);
        DontDestroyOnLoad(_GameManager.Instance.selectedCharacter.gameObject);
        yield return new WaitForSeconds(2f);
        cameraPath.selectedCamera.farClipPlane = 200f;
        cameraPath.SetCurrentWayPoint(2);
        cameraPath.PlayPath(15f);
        DOTween.To(() => cameraPath.selectedCamera.farClipPlane,
                   x => cameraPath.selectedCamera.farClipPlane = x, 60f, 5f);
        PlayWindSFX(5f);
        yield return StartCoroutine(WaitForPathCompletion(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
        }));
    }

    private void PlayWindSFX(float totalDuration)
    {
        if (windClip == null || windSource == null) return;
        float fadeInTime = totalDuration * 0.2f;
        float fadeOutTime = totalDuration * 0.2f;
        float midTime = totalDuration - fadeInTime - fadeOutTime;
        if (midTime < 0f) midTime = 0f;
        windSource.volume = 0f;
        windSource.clip = windClip;
        windSource.Play();
        windSource.DOFade(1f, fadeInTime).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            StartCoroutine(HoldAndFade(midTime, fadeOutTime));
        });
    }

    private IEnumerator HoldAndFade(float holdTime, float fadeOutTime)
    {
        yield return new WaitForSeconds(holdTime);
        windSource.DOFade(0f, fadeOutTime).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            windSource.Stop();
        });
    }
}
