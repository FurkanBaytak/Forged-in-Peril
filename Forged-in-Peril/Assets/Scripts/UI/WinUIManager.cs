using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WinUIManager : MonoBehaviour
{
    [Header("Canvas Group for Fade")]
    public CanvasGroup victoryCanvasGroup;
    public float fadeInDuration = 0.5f;

    [Header("Character Info")]
    public TMP_Text characterNameText;
    public TMP_Text hpText;
    public TMP_Text strText;
    public TMP_Text dexText;
    public TMP_Text talentText;
    public Image talentIcon;

    [Header("Resulting Sword Info")]
    public TMP_Text weightText;
    public TMP_Text baseDmgText;
    public TMP_Text totalDmgText;
    public TMP_Text critText;
    public TMP_Text materialEffectText;
    public Image materialEffectIcon;
    public TMP_Text swordEffectText;
    public Image swordEffectIcon;

    [Header("Fireworks Effect")]
    public GameObject fireworks;

    private void Awake()
    {
        SetCanvasGroupAlpha(victoryCanvasGroup, 0f);
        this.gameObject.SetActive(true); 
        victoryCanvasGroup.interactable = false;
        victoryCanvasGroup.blocksRaycasts = false;

        if (fireworks != null)
        {
            fireworks.SetActive(false);
        }
    }

    public void ShowVictoryScreen(_Character winnerCharacter)
    {

        FillCharacterInfo(winnerCharacter);
        FillWeaponInfo(winnerCharacter);

        victoryCanvasGroup.gameObject.SetActive(true);

        victoryCanvasGroup.interactable = true;
        victoryCanvasGroup.blocksRaycasts = true;

        victoryCanvasGroup.alpha = 0f;
        victoryCanvasGroup.DOFade(1, fadeInDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
            });

        if (fireworks != null)
        {
            fireworks.SetActive(true);
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(MusicManager.Instance.victoryClip, true);
        }
    }

    private void FillCharacterInfo(_Character winnerCharacter)
    {
        if (winnerCharacter == null) return;

        characterNameText.text = winnerCharacter.characterName;
        hpText.text = winnerCharacter.Health.ToString();
        strText.text = winnerCharacter.strength.ToString();
        dexText.text = winnerCharacter.dexterenity.ToString();

        if (winnerCharacter.talent != null)
        {
            talentText.text = winnerCharacter.talent.type.ToString();
            if (winnerCharacter.talent.icon != null)
            {
                talentIcon.gameObject.SetActive(true);
                talentIcon.sprite = winnerCharacter.talent.icon;
            }
            else
            {
                talentIcon.gameObject.SetActive(false);
            }
        }
        else
        {
            talentText.text = "No Talent";
            talentIcon.gameObject.SetActive(false);
        }
    }

    private void FillWeaponInfo(_Character winnerCharacter)
    {
        if (winnerCharacter._Weapon == null)
        {
            weightText.text = "---";
            baseDmgText.text = "---";
            totalDmgText.text = "---";
            critText.text = "---";
            materialEffectText.text = "";
            swordEffectText.text = "";
            materialEffectIcon.gameObject.SetActive(false);
            swordEffectIcon.gameObject.SetActive(false);
            return;
        }
        var wpn = winnerCharacter._Weapon;

        weightText.text = wpn.weight;
        baseDmgText.text = $"{wpn.minBaseDamage} - {wpn.maxBaseDamage}";

        int minDamage = winnerCharacter.calculateMinDamage(wpn.minBaseDamage);
        int maxDamage = winnerCharacter.calculateMaxDamage(wpn.maxBaseDamage);
        totalDmgText.text = $"{minDamage} - {maxDamage}";

        critText.text = $"(x{wpn.critDamage}): {wpn.crit}%";

        if (wpn.material != null && wpn.material.effect != null && wpn.material.effect.type.ToString() != "None")
        {
            materialEffectText.text = wpn.material.effect.type.ToString();
            if (wpn.material.effect.icon != null)
            {
                materialEffectIcon.sprite = wpn.material.effect.icon;
                materialEffectIcon.gameObject.SetActive(true);
            }
            else
            {
                materialEffectIcon.gameObject.SetActive(false);
            }
        }
        else
        {
            materialEffectText.text = "";
            materialEffectIcon.gameObject.SetActive(false);
        }

        if (wpn.effect != null && wpn.effect.type.ToString() != "None")
        {
            swordEffectText.text = wpn.effect.type.ToString();
            if (wpn.effect.icon != null)
            {
                swordEffectIcon.sprite = wpn.effect.icon;
                swordEffectIcon.gameObject.SetActive(true);
            }
            else
            {
                swordEffectIcon.gameObject.SetActive(false);
            }
        }
        else
        {
            swordEffectText.text = "";
            swordEffectIcon.gameObject.SetActive(false);
        }
    }

    private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
    {
        cg.alpha = alpha;
        cg.interactable = (alpha > 0f);
        cg.blocksRaycasts = (alpha > 0f);
    }
}
