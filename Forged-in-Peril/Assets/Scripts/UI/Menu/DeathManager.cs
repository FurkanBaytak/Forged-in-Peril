using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public class DeathManager : MonoBehaviour
{
    [Header("Canvas Settings")]
    public GameObject deathNewItemCanvas;
    public GameObject deathNoItemCanvas;
    public CanvasGroup newItemcanvasGroup;
    public CanvasGroup noItemcanvasGroup;
    public Button newItemToShopButton;
    public Button noItemToShopButton;

    [Header("UI Settings")]
    public TMP_Text newItemCharacterName;
    public TMP_Text noItemCharacterName;
    public GameObject ItemParent;
    public GameObject ItemPrefab;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.25f;
    public float fadeOutDuration = 0.25f;

    public Vector3 itemParentStartPosition = new Vector3(0, -500, 0);
    public Vector3 itemParentEndPosition = Vector3.zero;
    public float itemParentMoveDuration = 0.5f;
    public float itemParentScaleDuration = 0.5f;

    void Awake()
    {
        if (newItemcanvasGroup == null)
        {
            newItemcanvasGroup = deathNewItemCanvas.GetComponent<CanvasGroup>();
        }

        if (noItemcanvasGroup == null)
        {
            noItemcanvasGroup = deathNoItemCanvas.GetComponent<CanvasGroup>();
        }

        deathNewItemCanvas.SetActive(false);
        deathNoItemCanvas.SetActive(false);
        SetCanvasGroupAlpha(newItemcanvasGroup, 0);
        SetCanvasGroupAlpha(noItemcanvasGroup, 0);

        newItemToShopButton.onClick.AddListener(OnToShopButtonPressed);
        noItemToShopButton.onClick.AddListener(OnToShopButtonPressed);
    }

    public void ShowDeathPanels()
    {
        bool hasNewWeapon = false;
        bool hasNewMaterial = false;

        if (_GameManager.Instance.unlockedWeapon != null && _GameManager.Instance.unlockedWeapon.Count > 0)
        {
            foreach (var weapon in _GameManager.Instance.unlockedWeapon)
            {
                Debug.Log($"Weapon Name: {weapon.name}");
            }
            hasNewWeapon = true;
        }
        else
        {
            Debug.Log("newWeapon listesi boþ.");
        }

        if (_GameManager.Instance.unlockedMaterials != null && _GameManager.Instance.unlockedMaterials.Count > 0)
        {
            foreach (var material in _GameManager.Instance.unlockedMaterials)
            {
                Debug.Log($"Material Name: {material.name}");
            }
            hasNewMaterial = true;
        }
        else
        {
            Debug.Log("newWeaponMaterials listesi boþ.");
        }

        if (hasNewWeapon || hasNewMaterial)
        {
            DeathNewItemMenu();
        }
        else
        {
            DeathNoItemMenu();
        }
    }

    private void DeathNewItemMenu()
    {
        newItemCharacterName.text = _GameManager.Instance.selectedCharacter.characterName + " Has Fallen...";

        foreach (Transform child in ItemParent.transform)
        {
            Destroy(child.gameObject);
        }

        if (_GameManager.Instance.unlockedWeapon != null && _GameManager.Instance.unlockedWeapon.Count > 0)
        {
            foreach (var weapon in _GameManager.Instance.unlockedWeapon)
            {
                GameObject newItem = Instantiate(ItemPrefab, ItemParent.transform);

                Image iconImage = newItem.GetComponentInChildren<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = weapon.icon;
                }

                TMP_Text itemNameText = newItem.GetComponentInChildren<TMP_Text>();
                if (itemNameText != null)
                {
                    itemNameText.text = weapon.name;
                }
            }
        }

        if (_GameManager.Instance.unlockedMaterials != null && _GameManager.Instance.unlockedMaterials.Count > 0)
        {
            foreach (var material in _GameManager.Instance.unlockedMaterials)
            {
                GameObject newItem = Instantiate(ItemPrefab, ItemParent.transform);

                Image iconImage = newItem.GetComponentInChildren<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = material.icon;
                }

                TMP_Text itemNameText = newItem.GetComponentInChildren<TMP_Text>();
                if (itemNameText != null)
                {
                    itemNameText.text = material.name;
                }
            }
        }

        ItemParent.transform.localPosition = itemParentStartPosition;
        ItemParent.transform.localScale = Vector3.zero;

        deathNewItemCanvas.SetActive(true);
        FadeInUI(newItemcanvasGroup, () =>
        {
            AnimateItemParent();
        });
    }

    private void DeathNoItemMenu()
    {
        noItemCharacterName.text = _GameManager.Instance.selectedCharacter.characterName + " Has Fallen...";
        deathNoItemCanvas.SetActive(true);
        FadeInUI(noItemcanvasGroup, () =>
        {
        });
    }

    private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
    {
        cg.alpha = alpha;
        cg.interactable = alpha > 0;
        cg.blocksRaycasts = alpha > 0;
    }

    private void OnToShopButtonPressed()
    {
        _GameManager.Instance.isReturning = true;
        MapManager.Instance.ResetMap();
        UnityEngine.SceneManagement.SceneManager.LoadScene("PreperationScene");
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

    private void AnimateItemParent()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(ItemParent.transform.DOLocalMove(itemParentEndPosition, itemParentMoveDuration).SetEase(Ease.OutBack));
        sequence.Join(ItemParent.transform.DOScale(1f, itemParentScaleDuration).SetEase(Ease.OutBack));
        sequence.OnComplete(() =>
        {
            ApplyShakeToItems();
        });
    }

    private void ApplyShakeToItems()
    {
        foreach (Transform child in ItemParent.transform)
        {
            child.DOShakeScale(0.5f, new Vector3(0.1f, 0.1f, 0), 10, 90, false)
                 .SetLoops(2, LoopType.Yoyo)
                 .SetEase(Ease.OutBounce);

            child.DOScale(child.localScale * 1.1f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }
}
