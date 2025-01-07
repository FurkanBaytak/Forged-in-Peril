using DG.Tweening;
using System.Collections;
using UnityEngine;

public class MapSceneLoading : MonoBehaviour
{
    public Camera Camera;
    public GameObject cloud;
    public ParticleSystemRenderer _particleSystem;
    [SerializeField] private AudioSource windSource;
    [SerializeField] private AudioClip windClip;
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystemRenderer>();
        _particleSystem.sortingOrder = 20;
        StartCoroutine(AnimateCameraProperties(100f, 5f, 0f, 6f, 1.5f));
        StartCoroutine(AnimateCloud(new Vector3(0.1f, 0.1f, 0.1f), 1.5f, _particleSystem));
    }

    IEnumerator AnimateCameraProperties(float startSize, float endSize, float startNear, float endNear, float duration)
    {
        Camera.orthographicSize = startSize;
        Camera.nearClipPlane = startNear;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            Camera.orthographicSize = Mathf.Lerp(startSize, endSize, easedT);
            Camera.nearClipPlane = Mathf.Lerp(startNear, endNear, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.orthographicSize = endSize;
        Camera.nearClipPlane = endNear;
    }

    IEnumerator AnimateCloud(Vector3 targetScale, float duration, ParticleSystemRenderer particleSystem)
    {
        Vector3 startScale = cloud.transform.localScale;
        Vector3 endScale = targetScale;
        PlayWindSFX(1.5f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);

            float easedT = Mathf.SmoothStep(0f, 1f, t);
            cloud.transform.localScale = Vector3.Lerp(startScale, endScale, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        particleSystem.sortingOrder = -1;
        cloud.transform.localScale = endScale;
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
