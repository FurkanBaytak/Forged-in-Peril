using UnityEngine;

using System;
using System.Collections;


public class _Character : MonoBehaviour
{
    [SerializeField] public string characterName { get; set; }
    [SerializeField] public int Health { get; set; }
    [SerializeField] public int MaxHealth { get; set; }
    [SerializeField] public int dexterenity { get; set; }
    [SerializeField] public int strength { get; set; }

    public _CharacterTalent talent = null;
    public _weapon _Weapon = null;
    public _WeaponMaterial _material = null;

    public int characterPrefabIndex = -1;


    public Animator _animator;

    public int momentumStack = 0;
    public int dullBladeStack = 0;
    public int NumbingColdStack = 0;

    public bool bladeCounter = false;
    public int prevTakingDamage = 1000;

    [Header("VFX")]
    public GameObject normalDamageVFX;
    public GameObject critDamageVFX;
    public GameObject missVFX;
    public GameObject dieVFX;

    [Header("Audio Clips")]
    public AudioClip attackSFX;
    public AudioClip dieSFX;
    public AudioClip winSFX;
    public AudioSource audioSource;

    public void DisplayCharacterStats()
    {
        string characterInfo = $"--- {characterName.ToUpper()} INFO ---\n" +
                                $"Name: {characterName}\n" +
                                $"Health: {Health}/{MaxHealth}\n" +
                                $"Strength: {strength}\n" +
                                $"Dexterity: {dexterenity}\n" +
                                $"Talent: {talent?.name ?? "None"}\n" +
                                $"Talent Type: {talent?.type.ToString() ?? "None"}\n";

        if (_Weapon != null)
        {
            characterInfo += $"--- WEAPON INFO ---\n" +
                             $"Weapon Name: {_Weapon.type}\n" +
                             $"Weight: {_Weapon.weight}\n" +
                             $"Base Damage: {_Weapon.minBaseDamage} - {_Weapon.maxBaseDamage} \n" +
                             $"Total Damage: {calculateMinDamage(_Weapon.minBaseDamage)} - {calculateMaxDamage(_Weapon.maxBaseDamage)} \n" +
                             $"Weapon Effect: {_Weapon.effect} \n"+
                             $"Crit (x{_Weapon.critDamage}): {_Weapon.crit}";
        }
        else
        {
            characterInfo += $"--- WEAPON INFO ---\nNo Weapon Equipped\n";
        }

        if (_material != null)
        {
            characterInfo += $"--- MATERIAL INFO ---\n" +
                             $"Material Name: {_material.type}\n" +
                             $"Effect Type: {_material.effect.type}\n" +
                             $"Effect Description: {_material.effect.effectDesc}\n";
        }
        else
        {
            characterInfo += $"--- MATERIAL INFO ---\nNo Material Equipped\n";
        }

        Debug.Log(characterInfo);
    }

    public int calculateMinDamage(int minDamage)
    {
        float weightVal = weightValue(_Weapon.weight);
        int damage = minDamage + (int)Mathf.Round(weightVal * strength) + (int)Mathf.Round((1 - weightVal) * dexterenity);
        return damage;
    }

    public int calculateMaxDamage(int maxDamage)
    {
        float weightVal = weightValue(_Weapon.weight);
        int damage = maxDamage + (int)Mathf.Round(weightVal * strength) + (int)Mathf.Round((1 - weightVal) * dexterenity);
        return damage;
    }

    public float weightValue(string weight)
    {
        float value = 0f;
        switch (weight.ToLower())
        {
            case "lightless":
                value = 0f;
                break;
            case "light":
                value = 0.25f;
                break;
            case "medium":
                value = 0.50f;
                break;
            case "heavy":
                value = 0.75f;
                break;
            case "heavier":
                value = 1.0f;
                break;
            default:
                throw new ArgumentException("Invalid weight value provided.");
        }

        return value;
    }



    public IEnumerator Attack(_Enemy target)
    {
        double critChance = _Weapon.crit / 100 + (momentumStack * 0.1f);
        if (bladeCounter) { critChance += 0.2f; bladeCounter = false; }
        int critDamage = _Weapon.critDamage;

        bool isCrit = UnityEngine.Random.Range(0f, 1f) <= critChance;
        bool isMiss = false;

        float missChance = 0.1f;
        if (_Weapon.effect.type == _WeaponEffect.weaponEffectType.Unwieldy) { missChance = 0.2f;}

        if (UnityEngine.Random.Range(0f, 1f) > missChance)
        {
            int finalDamage = UnityEngine.Random.Range(calculateMinDamage(_Weapon.minBaseDamage), calculateMaxDamage(_Weapon.maxBaseDamage));
            if (isCrit) finalDamage *= critDamage;

            if (talent != null)
            {
                if (talent.type == _CharacterTalent.talentType.Clumsy && UnityEngine.Random.Range(0f, 1f) < 0.05f)
                {
                    if (_material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.BloodSacrifice)
                    {
                        finalDamage += 5;
                        Health -= 1;
                        _GameManager.Instance.UpdateCharacterHealth(Health);
                        LogManager.Instance.AppendSpecialLog("Material Effect", "BloodSacrifice", "+5 Damage, -1 Health");
                    }
                    if (talent.type == _CharacterTalent.talentType.Anger)
                    {
                        float healthLostPercentage = ((float)(MaxHealth - Health) / (float)MaxHealth) * 100f;
                        int extraDamageAnger = Mathf.FloorToInt(healthLostPercentage / 20f);
                        finalDamage += extraDamageAnger;
                        LogManager.Instance.AppendSpecialLog("Talent Effect", "Anger", $"+{extraDamageAnger} Damage");
                    }

                    if (talent.type == _CharacterTalent.talentType.Optimistic)
                    {
                        float healthPercentage = ((float)Health / (float)MaxHealth) * 100f;
                        int extraDamageOptimistic = Mathf.FloorToInt(healthPercentage / 20f);
                        finalDamage += extraDamageOptimistic;
                        LogManager.Instance.AppendSpecialLog("Talent Effect", "Optimistic", $"+{extraDamageOptimistic} Damage");
                    }
                    TakeDamage(finalDamage, isCrit, isMiss);
                    _GameManager.Instance.selectedCharacter._animator.SetTrigger("TakeDamage");
                    yield return new WaitForSeconds(3f);
                }
                else
                {
                    if (talent.type == _CharacterTalent.talentType.Anger)
                    {
                        float healthLostPercentage = ((float)(MaxHealth - Health) / (float)MaxHealth) * 100f;
                        int extraDamageAnger = Mathf.FloorToInt(healthLostPercentage / 20f);
                        finalDamage += extraDamageAnger;
                        LogManager.Instance.AppendSpecialLog("Talent Effect", "Anger", $"+{extraDamageAnger} Damage");
                    }

                    if (talent.type == _CharacterTalent.talentType.Optimistic)
                    {
                        float healthPercentage = ((float)Health / (float)MaxHealth) * 100f;
                        int extraDamageOptimistic = Mathf.FloorToInt(healthPercentage / 20f);
                        finalDamage += extraDamageOptimistic;
                        LogManager.Instance.AppendSpecialLog("Talent Effect", "Optimistic", $"+{extraDamageOptimistic} Damage");
                    }
                    if (_material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.BloodSacrifice)
                    {
                        finalDamage += 5;
                        Health -= 1;
                        LogManager.Instance.AppendSpecialLog("Material Effect", "BloodSacrifice", "+5 Damage, -1 Health");
                    }
                    if (finalDamage > 0)
                    {
                        _animator.SetTrigger("Attack");
                        PlayAttackSFX();
                    }
                    target.TakeDamage(finalDamage, isCrit, isMiss);
                    yield return new WaitForSeconds(3f);

                    if (_Weapon.effect.type == _WeaponEffect.weaponEffectType.Momentum)
                    {
                        momentumStack += 1;
                        LogManager.Instance.AppendSpecialLog("Weapon Effect", "Momentum", $"+%{momentumStack}0 Crit Chance");
                    }
                    if (_Weapon.effect.type == _WeaponEffect.weaponEffectType.DullBlade)
                    {
                        dullBladeStack += 1;
                        LogManager.Instance.AppendSpecialLog("Weapon Effect", "Dull Blade", "+1 Dull Blade Stack");
                    }
                    if (_material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.NumbingCold)
                    {
                        NumbingColdStack += 1;
                        LogManager.Instance.AppendSpecialLog("Material Effect", "Numbing Cold", "+1 Numbing Cold Stack");
                    }
                }
            }
        }
        else
        {
            _animator.SetTrigger("Attack");
            PlayAttackSFX();
            isMiss = true;
            target.TakeDamage(0, isCrit, isMiss);
            yield return new WaitForSeconds(3f);
        }
    }

    public void TakeDamageAnim()
    {
        if (characterName != "Enemy")
            _GameManager.Instance.enemyCharacter._animator.SetTrigger("TakeDamage");
        else
        {
            _GameManager.Instance.selectedCharacter._animator.SetTrigger("TakeDamage");
        }
    }

    public void TakeDamage(int damage, bool isCrit, bool isMiss)
    {
        if (_Weapon.effect.type == _WeaponEffect.weaponEffectType.CrescentGuard)
        {
            damage = Mathf.Clamp(damage-2, 0, MaxHealth);
            LogManager.Instance.AppendSpecialLog("Weapon Effect", "Crescent Guard", "-2 Damage Reduction");
        }
        if (_Weapon.effect.type == _WeaponEffect.weaponEffectType.DullBlade)
        {
            damage = Mathf.Clamp(damage-dullBladeStack, 0, MaxHealth);
            LogManager.Instance.AppendSpecialLog("Weapon Effect", "Dull Blade", $"-{dullBladeStack} Damage Reduction");
        }
        if (_material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.NumbingCold)
        {
            damage = Mathf.Clamp(damage-NumbingColdStack, 0, MaxHealth);
            LogManager.Instance.AppendSpecialLog("Material Effect", "Numbing Cold", $"-{NumbingColdStack} Damage Reduction");
        }
        if (_Weapon.effect.type == _WeaponEffect.weaponEffectType.BladeCounter)
        {
            if (prevTakingDamage < damage) { bladeCounter = true; }
        }

        Health -= damage;

        if (isMiss)
        {
            PlayMissVFX();
            LogManager.Instance.AppendPlayerLog("<b>ENEMY ATTACK MISSED !</b>");
        }
        else if (isCrit)
        {
            PlayCritDamageVFX();
            LogManager.Instance.AppendPlayerLog($"<color=yellow><b>CRITICAL HIT RECEIVED !: </b></color><color=red><b> {damage}</b></color>");
        }
        else{
            PlayNormalDamageVFX();
            LogManager.Instance.AppendPlayerLog($"<color=black><b>Damage Received: </b></color><color=red><b> {damage}</b></color>");
        }

        Debug.Log($"{characterName} took {damage} damage. Remaining health: {Health}");

        if (_GameManager.Instance != null)
        {
            _GameManager.Instance.UpdateCharacterHealth(Health);
        }

        if (Health <= 0)
        {
            LogManager.Instance.AppendPlayerLog("<color=red><b>PLAYER DIED !</b></color>");
            _animator.SetTrigger("Die");
            PlayDieSFX();
            StartCoroutine(Die());
            _GameManager.Instance.enemyCharacter._animator.SetTrigger("Win");
        }
        prevTakingDamage = damage;
    }
    public IEnumerator Die()
    {
        PlayDieVFX();
        yield return new WaitForSeconds(3f);
        _GameManager.Instance.enemyCharacter.PlayWinSFX();
        yield return new WaitForSeconds(4f);
    }

    private void PlayNormalDamageVFX()
    {
        if (normalDamageVFX != null)
        {
            StartCoroutine(PlayVFXCoroutine(normalDamageVFX));
        }
        else
        {
            Debug.LogWarning("Normal Damage VFX null!");
        }
    }

    private void PlayCritDamageVFX()
    {
        if (critDamageVFX != null)
        {
            StartCoroutine(PlayVFXCoroutine(critDamageVFX));
        }
        else
        {
            Debug.LogWarning("Crit Damage VFX null!");
        }
    }

    private void PlayMissVFX()
    {
        if (missVFX != null)
        {
            StartCoroutine(PlayVFXCoroutine(missVFX));
        }
        else
        {
            Debug.LogWarning("Miss VFX null!");
        }
    }

    private void PlayDieVFX()
    {
        if (dieVFX != null)
        {
            StartCoroutine(PlayVFXCoroutine(dieVFX));
        }
        else
        {
            Debug.LogWarning("Die VFX null!");
        }
    }
    private void PlayAttackSFX()
    {
        if (attackSFX != null)
        {
            audioSource.PlayOneShot(attackSFX);
        }
    }

    private void PlayDieSFX()
    {
        if (dieSFX != null)
        {
            audioSource.PlayOneShot(dieSFX);
        }
    }
    public void PlayWinSFX()
    {
        if (winSFX != null)
        {
            audioSource.PlayOneShot(winSFX);
        }
    }

    private IEnumerator PlayVFXCoroutine(GameObject vfx)
    {
        vfx.SetActive(true);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            yield return new WaitForSeconds(.5f);
            ps.Play();
            yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
        vfx.SetActive(false);
    }
}
