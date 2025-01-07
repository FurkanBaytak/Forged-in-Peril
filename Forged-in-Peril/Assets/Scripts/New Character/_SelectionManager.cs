using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class _SelectionManager : MonoBehaviour
{
    public _CharacterTalent _characterTalent;
    public List<Animator> animator;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip selectClip;
    public List<_CharacterTalent> availableTalents = new List<_CharacterTalent>();
    public List<_weapon> AvailableWeapons = new List<_weapon>();
    public List<_WeaponMaterial> AvailableMaterials = new List<_WeaponMaterial>();

    private List<_Character> characters = new List<_Character>();

    public _weapon selectedWeapon;
    public _WeaponMaterial selectedMaterial;

    public _Character selectedCharacter;
    public _GameManager gameManager;

    public string baseWeight = "Medium";

    void Start()
    {
            sfxSource = SFXManager.Instance.sfxSource;

        restoreWeapon();
        generateCharacters();
    }

    public void generateCharacters()
    {
        List<_CharacterTalent> originalTalentList = new List<_CharacterTalent>(availableTalents);
        List<_CharacterTalent> tempTalentList = new List<_CharacterTalent>(originalTalentList);

        List<string> characterNames = new List<string> {
        "Furkan", "Ekmek", "Max", "Zoe", "Leo", "Mia", "Kai", "Eli", "Ivy",
        "Nox", "Jax", "Pip", "Rex", "Tia", "Zig", "Nova", "Finn",
        "Buzz", "Nina", "Ollie", "Kiki", "Vik", "Milo", "Gus",
        "Lily", "Dex", "Bree", "Juno", "Zara", "Echo", "Kade",
        "Maggie", "Toby", "Sage", "Rory", "Faye", "Axel", "Puck",
        "Nora", "Beau", "Cleo", "Rexy", "Mona", "Otis", "Fritz",
        "Gigi", "Zane", "Lulu", "Baxter", "Penny", "Spike", "Daisy",
        "Benny", "Misty", "Rocco", "Tess", "Wally", "Pixie", "Dash"
    };

        for (int i = 0; i < 3; i++)
        {
            characters.Add(gameObject.AddComponent<_Character>());
        }

        int[] healts = { 80, 85, 90, 95, 100, 105, 110, 120 };

        for (int i = 0; i < 3; i++)
        {
            if (characterNames.Count == 0)
            {
                Debug.LogError("Name List is Empty.");
                break;
            }

            int nameIndex = Random.Range(0, characterNames.Count);
            characters[i].characterName = characterNames[nameIndex];
            characterNames.RemoveAt(nameIndex);

            int healtIndex = Random.Range(0, healts.Length);
            characters[i].Health = healts[healtIndex];
            characters[i].MaxHealth = healts[healtIndex];
            characters[i].dexterenity = Random.Range(4, 14);
            characters[i].strength = Random.Range(4, 14);

            if (tempTalentList.Count == 0)
            {
                Debug.LogError("Talent List is Empty.");
                break;
            }

            int randomTalentIndex = Random.Range(0, tempTalentList.Count);
            characters[i].talent = tempTalentList[randomTalentIndex];
            tempTalentList.RemoveAt(randomTalentIndex);

            if (characters[i].talent.type == _CharacterTalent.talentType.Clumsy)
            {
                characters[i].Health += 10;
                characters[i].MaxHealth += 10;
                characters[i].dexterenity += 2;
                characters[i].strength += 2;
            }

            characters[i]._Weapon = selectedWeapon;
            if (selectedWeapon != null) selectedWeapon.material = selectedMaterial;
            characters[i]._material = selectedMaterial;
            characters[i]._animator = animator[i];
        }

        for (int i = 0; i < 3; i++)
        {
            characters[i].DisplayCharacterStats();
        }
    }


    public void setCharacters()
    {
        for (int i = 0; i < 3; i++)
        {
            characters[i]._Weapon = selectedWeapon;
            characters[i]._material = selectedMaterial;
            if (selectedWeapon != null) selectedWeapon.material = selectedMaterial;
        }
    }

    public void SelectWeapon(_weapon Weapon)
    {
        restoreWeapon();
        baseWeight = Weapon.weight;
        selectedWeapon = Weapon;

        if (selectedMaterial.effect.type == weaponMaterialEffect.weaponMaterialEffectType.LightAsAir)
        {
            LightAsAir();
            LogManager.Instance.AppendSpecialLog("Material Effect", "Light As Air", "Weight reduced by one level");
        }

        if (selectedMaterial.effect.type == weaponMaterialEffect.weaponMaterialEffectType.WeightOfEarth)
        {
            WeightOfEarth();
            LogManager.Instance.AppendSpecialLog("Material Effect", "Weight Of Earth", "Weight increased by one level");
        }

        if (selectedMaterial.effect.type == weaponMaterialEffect.weaponMaterialEffectType.Extravagance)
        {
            selectedWeapon.maxBaseDamage += 3;
            selectedWeapon.crit += 15;
            LogManager.Instance.AppendSpecialLog("Material Effect", "Extravagance", "+3 Max Damage, +15 Crit");
        }

        if (selectedMaterial.effect.type == weaponMaterialEffect.weaponMaterialEffectType.TrueStrike)
        {
            selectedWeapon.maxBaseDamage += 2*((int)selectedWeapon.crit / 5);
            selectedWeapon.minBaseDamage += 2*((int)selectedWeapon.crit / 5);
            selectedWeapon.crit = 0.0f;
            LogManager.Instance.AppendSpecialLog("Material Effect", "True Strike", $"+{(int)selectedWeapon.crit / 5} Damage per Crit");
        }

        if (selectedMaterial.effect.type == weaponMaterialEffect.weaponMaterialEffectType.GuidingMoonlight)
        {
            selectedWeapon.crit = 30;
            LogManager.Instance.AppendSpecialLog("Material Effect", "Guiding Moonlight", "Crit set to 30");
        }

        setCharacters();
    }

    public void Selectmaterial(_WeaponMaterial material)
    {
        restoreWeapon();

        if (material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.LightAsAir)
        {
            LightAsAir();
            LogManager.Instance.AppendSpecialLog("Material Effect", "Light As Air", "Weight reduced by one level");
        }

        if (material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.WeightOfEarth)
        {
            WeightOfEarth();
            LogManager.Instance.AppendSpecialLog("Material Effect", "Weight Of Earth", "Weight increased by one level");
        }

        if (material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.Extravagance)
        {
            selectedWeapon.maxBaseDamage += 3;
            selectedWeapon.crit += 15;
            LogManager.Instance.AppendSpecialLog("Material Effect", "Extravagance", "+3 Max Damage, +15 Crit");
        }

        if (material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.TrueStrike)
        {
            selectedWeapon.maxBaseDamage += (int)selectedWeapon.crit / 5;
            selectedWeapon.minBaseDamage += (int)selectedWeapon.crit / 5;
            selectedWeapon.crit = 0.0f;
            LogManager.Instance.AppendSpecialLog("Material Effect", "True Strike", $"+{(int)selectedWeapon.crit / 5} Damage per Crit");
        }

        if (material.effect.type == weaponMaterialEffect.weaponMaterialEffectType.GuidingMoonlight)
        {
            selectedWeapon.crit = 30;
            LogManager.Instance.AppendSpecialLog("Material Effect", "Guiding Moonlight", "Crit set to 30");
        }

        selectedMaterial = material;
        setCharacters();
    }

    public string getCharacterName(int index)
    {
        return characters[index].characterName;
    }

    public _Character GetCharacterByIndex(int index)
    {
        if (index < 0 || index >= characters.Count) return null;
        return characters[index];
    }

    public void selectCharacter(_Character character)
    {
        _Character previousSelected = selectedCharacter;

        if (previousSelected != null && previousSelected != character && previousSelected._animator != null)
        {
            previousSelected._animator.SetTrigger("Cancelled");
        }

        selectedCharacter = character;
        _characterTalent = character.talent;
        _GameManager.Instance.selectedCharacter = character;
    }

    public void LockCharacter()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("No character selected to lock.");
            return;
        }

        if (sfxSource != null && selectClip != null)
            sfxSource.PlayOneShot(selectClip);

        if (selectedCharacter._animator != null)
        {
            selectedCharacter._animator.SetTrigger("Locked");
        }
    }


    public void TriggerAnimationForSelectedCharacter()
    {
        if (selectedCharacter != null && selectedCharacter._animator != null)
        {
            selectedCharacter._animator.ResetTrigger("Cancelled");
            selectedCharacter._animator.SetTrigger("Selected");
        }
    }

    public void LightAsAir()
    {
        _weapon.WeaponType type = selectedWeapon.type;
        if (selectedWeapon.weight == baseWeight)
        {
            switch (selectedWeapon.weight.ToLower())
            {
                case "heavy":
                    selectedWeapon.weight = "Medium";
                    return;
                case "medium":
                    selectedWeapon.weight = "Light";
                    return;
                case "light":
                    selectedWeapon.weight = "Lightless";
                    return;


            }
        }
    }
    public void WeightOfEarth()
    {
        _weapon.WeaponType type = selectedWeapon.type;
        if (selectedWeapon.weight == baseWeight)
        {
            switch (selectedWeapon.weight.ToLower())
            {
                
                case "light":
                    selectedWeapon.weight = "Medium";
                    return;
                case "medium":
                    selectedWeapon.weight = "Heavy";
                    return;
                case "heavy":
                    selectedWeapon.weight = "Heavier";
                    return;
                
            }
        }
    }
    public void restoreWeapon()
    {
        switch (selectedWeapon.type)
        {
            case _weapon.WeaponType.Longsword:
                // Longsword
                selectedWeapon.weight = "Medium";
                selectedWeapon.minBaseDamage = 5;
                selectedWeapon.maxBaseDamage = 7;
                selectedWeapon.critDamage = 2;   
                selectedWeapon.crit = 10;
                selectedWeapon.material = null;
                break;

            case _weapon.WeaponType.Dagger:
                // Dagger
                selectedWeapon.weight = "Light";
                selectedWeapon.minBaseDamage = 3;
                selectedWeapon.maxBaseDamage = 8;
                selectedWeapon.critDamage = 2;  
                selectedWeapon.crit = 30;
                selectedWeapon.material = null;
                break;

            case _weapon.WeaponType.Claymore:
                // Claymore
                selectedWeapon.weight = "Heavy";
                selectedWeapon.minBaseDamage = 10;
                selectedWeapon.maxBaseDamage = 12;
                selectedWeapon.critDamage = 2;   
                selectedWeapon.crit = 00;
                selectedWeapon.material = null;
                break;

            case _weapon.WeaponType.Scimitar:
                // Scimitar
                selectedWeapon.weight = "Medium";
                selectedWeapon.minBaseDamage = 5;
                selectedWeapon.maxBaseDamage = 7;
                selectedWeapon.critDamage = 2;    
                selectedWeapon.crit = 15;
                selectedWeapon.material = null;
                break;

            case _weapon.WeaponType.HookSword:
                // Hook Sword
                selectedWeapon.weight = "Medium";
                selectedWeapon.minBaseDamage = 4;
                selectedWeapon.maxBaseDamage = 6;
                selectedWeapon.critDamage = 2;    
                selectedWeapon.crit = 10;
                selectedWeapon.material = null;
                break;

            case _weapon.WeaponType.Cleaver:
                // Cleaver
                selectedWeapon.weight = "Heavy";
                selectedWeapon.minBaseDamage = 8;
                selectedWeapon.maxBaseDamage = 10;
                selectedWeapon.critDamage = 2;    
                selectedWeapon.crit = 10;
                selectedWeapon.material = null;
                break;

            case _weapon.WeaponType.Rapier:
                // Rapier
                selectedWeapon.weight = "Light";
                selectedWeapon.minBaseDamage = 3;
                selectedWeapon.maxBaseDamage = 4;
                selectedWeapon.critDamage = 3;    
                selectedWeapon.crit = 30;
                selectedWeapon.material = null;
                break;

            case _weapon.WeaponType.Katana:
                // Katana
                selectedWeapon.weight = "Medium";
                selectedWeapon.minBaseDamage = 4;
                selectedWeapon.maxBaseDamage = 6;
                selectedWeapon.critDamage = 2;    
                selectedWeapon.crit = 15;
                selectedWeapon.material = null;
                break;

        }
    }
    public void selfDestroy()
    {
        Destroy(gameObject);
    }

}
