using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Enemy: MonoBehaviour
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

    [Header("VFX")]
    [SerializeField] private GameObject normalDamageVFX;
    [SerializeField] private GameObject critDamageVFX;
    [SerializeField] private GameObject missVFX;
    [SerializeField] private GameObject dieVFX;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip dieSFX;
    [SerializeField] private AudioClip winSFX;
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
                             $"Weapon Effect: {_Weapon.effect} \n" +
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



    public IEnumerator Attack(_Character target)
    {
        double critChance = _Weapon.crit / 100;
        int critDamage = _Weapon.critDamage;

        bool isCrit = UnityEngine.Random.Range(0f, 1f) <= critChance;
        bool isMiss = false;

        float missChance = 0.1f;
        if (UnityEngine.Random.Range(0f, 1f) > missChance)
        {
            int finalDamage = UnityEngine.Random.Range(calculateMinDamage(_Weapon.minBaseDamage), calculateMaxDamage(_Weapon.maxBaseDamage));
            if (isCrit) finalDamage *= critDamage;

            if (finalDamage > 0)
            {
             _animator.SetTrigger("Attack");
             PlayAttackSFX();
            }

            target.TakeDamage(finalDamage, isCrit, isMiss);
            yield return new WaitForSeconds(3f);

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
                _GameManager.Instance.selectedCharacter._animator.SetTrigger("TakeDamage");
    }

    public void TakeDamage(int damage, bool isCrit, bool isMiss)
    {
        Health -= damage;

        if (isMiss)
        {
            PlayMissVFX();
            LogManager.Instance.AppendEnemyLog($"<b> PLAYER ATTACK MISSED !</b>");
        }
        else if (isCrit)
        {
            PlayCritDamageVFX();
            LogManager.Instance.AppendEnemyLog($"<color=yellow><b>CRITICAL HIT RECEIVED !: </b></color><color=red><b> {damage}</b></color>");
        }
        else
        {
            PlayNormalDamageVFX();
            LogManager.Instance.AppendEnemyLog($"<color=black><b>Damage Received: </b></color><color=red><b> {damage}</b></color>");

        }

        Debug.Log($"{characterName} took {damage} damage. Remaining health: {Health}");

        if (_GameManager.Instance != null)
        {
            _GameManager.Instance.UpdateEnemyHealth(Health);
        }

        if (Health <= 0)
        {
            LogManager.Instance.AppendEnemyLog("<color=red><b>ENEMY DIED !</b></color>");
            _animator.SetTrigger("Die");
            PlayDieSFX();
            StartCoroutine(Die());
            _GameManager.Instance.playerCharacter._animator.SetTrigger("Win");
        }
    }
    public IEnumerator Die()
    {
        PlayDieVFX();
        yield return new WaitForSeconds(3f);
        _GameManager.Instance.playerCharacter.PlayWinSFX();
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

    public void PlayDieSFX()
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
