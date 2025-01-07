using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public _SelectionManager selectionManager;
    [Header("Character UI")]
    public Button characterButton1;
    public Button characterButton2;
    public Button characterButton3;

    public TMP_Text characterButtonText1;
    public TMP_Text characterButtonText2;
    public TMP_Text characterButtonText3;

    public TMP_Text characterNameText;
    public TMP_Text hpText;
    public TMP_Text strText;
    public TMP_Text dexText;
    public TMP_Text talentText;
    public Image talentIcon;

    private int currentSelectedCharacterIndex = 0;

    [Header("Material UI")]
    public List<Image> materialSlots;
    public List<Image> materialSlotIcons;
    public TMP_Text selectedMaterialName;

    [Header("Weapon UI")]
    public List<Image> weaponSlots;
    public List<Image> weaponSlotIcons;
    public TMP_Text selectedWeaponName;

    [Header("Resulting Sword Panel")]
    public TMP_Text weightText;
    public TMP_Text baseDmgText;
    public TMP_Text totalDmgText;
    public TMP_Text critText;
    public TMP_Text materialEffectText;
    public Image materialEffectIcon;
    public TMP_Text swordEffectText;
    public Image swordEffectIcon;

    [Header("Sprites")]
    public Sprite normalSlotSprite;
    public Sprite selectedSlotSprite;

    public Sprite normalButtonSprite;
    public Sprite selectedButtonSprite;

    [Header("Weapon Models")]
    public List<GameObject> weaponModels;

    [Header("Material Models")]
    public List<GameObject> materialModels;

    [Header("Blacksmith Animator")]
    public Animator blacksmithAnimator;

    private int currentSelectedMaterialIndex = 0;
    private int currentSelectedWeaponIndex = -1;
    private bool isOnCooldown = false;
    private bool isMaterialAnimating = false;

    private Coroutine animationDelayCoroutine;

    private int currentlySelectedWeaponModelIndex = -1;
    private int currentlySelectedMaterialModelIndex = -1;

    private bool isWeaponAnimating = false;

    public List<GameObject> newWeapons;
    public List<GameObject> newMaterials;



    void Start()
    {
        if (selectionManager != null)
        {
            characterButtonText1.text = selectionManager.getCharacterName(0);
            characterButtonText2.text = selectionManager.getCharacterName(1);
            characterButtonText3.text = selectionManager.getCharacterName(2);
        }

        characterButton1.onClick.AddListener(() => OnCharacterButtonClicked(0));
        characterButton2.onClick.AddListener(() => OnCharacterButtonClicked(1));
        characterButton3.onClick.AddListener(() => OnCharacterButtonClicked(2));

        OnCharacterButtonClicked(0);

        SetupMaterialSlots();
        SetupWeaponSlots();

        if(_GameManager.Instance.newWeapon != null)
        {
            foreach(var wpn in _GameManager.Instance.unlockedWeapon)
            {
                int newWeaponIndex = selectionManager.AvailableWeapons.IndexOf(wpn);
                Debug.Log(newWeaponIndex);
                newWeapons[newWeaponIndex].SetActive(true);
            }
            _GameManager.Instance.newWeapon.Clear();
        }
        if (_GameManager.Instance.newWeaponMaterials != null)
        {
            foreach (var mat in _GameManager.Instance.unlockedMaterials)
            {
                int newMaterialIndex = selectionManager.AvailableMaterials.IndexOf(mat);
                Debug.Log(newMaterialIndex);
                newMaterials[newMaterialIndex].SetActive(true);
            }
            _GameManager.Instance.newWeaponMaterials.Clear();
        }
        if (weaponModels.Count != weaponSlots.Count)
        {
            Debug.LogError("weaponModels listesi, weaponSlots sayýsýyla ayný olmalýdýr.");
        }

        if (materialModels.Count != materialSlots.Count)
        {
            Debug.LogError("materialModels listesi, materialSlots sayýsýyla ayný olmalýdýr.");
        }

        if (selectionManager.AvailableWeapons.Count > 0)
        {
            HandleWeaponModelSelection(0);
            HandleMaterialSelectionCoroutine(0);
        }
    }

    void OnCharacterButtonClicked(int index)
    {
        if (isOnCooldown) return;
        currentSelectedCharacterIndex = index;

        characterButton1.GetComponent<Image>().sprite = normalButtonSprite;
        characterButton2.GetComponent<Image>().sprite = normalButtonSprite;
        characterButton3.GetComponent<Image>().sprite = normalButtonSprite;

        if (index == 0) characterButton1.GetComponent<Image>().sprite = selectedButtonSprite;
        else if (index == 1) characterButton2.GetComponent<Image>().sprite = selectedButtonSprite;
        else if (index == 2) characterButton3.GetComponent<Image>().sprite = selectedButtonSprite;

        _Character selectedCharacter = selectionManager.GetCharacterByIndex(index);
        selectedCharacter.characterPrefabIndex = index;
        if (selectedCharacter != null)
        {
            characterNameText.text = selectedCharacter.characterName;
            hpText.text = selectedCharacter.Health.ToString();
            strText.text = selectedCharacter.strength.ToString();
            dexText.text = selectedCharacter.dexterenity.ToString();

            if (selectedCharacter.talent != null)
            {
                talentText.text = $"{selectedCharacter.talent.type}";
                if (selectedCharacter.talent.icon != null)
                {
                    talentIcon.sprite = selectedCharacter.talent.icon;
                    talentIcon.gameObject.SetActive(true);
                }
                else
                {
                    talentIcon.gameObject.SetActive(false);
                }
            }
            else
            {
                talentText.text = "Talent: None";
                talentIcon.gameObject.SetActive(false);
            }

            selectionManager.selectCharacter(selectedCharacter);

            UpdateResultingSwordPanel();

            if (animationDelayCoroutine != null)
            {
                StopCoroutine(animationDelayCoroutine);
            }

            animationDelayCoroutine = StartCoroutine(AnimationDelayCoroutine(5f));

            StartCoroutine(ButtonCooldown());
        }
    }

    IEnumerator ButtonCooldown()
    {
        isOnCooldown = true;

        characterButton1.interactable = false;
        characterButton2.interactable = false;
        characterButton3.interactable = false;

        yield return new WaitForSeconds(0.5f);

        characterButton1.interactable = true;
        characterButton2.interactable = true;
        characterButton3.interactable = true;

        isOnCooldown = false;
    }

    IEnumerator AnimationDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        selectionManager.TriggerAnimationForSelectedCharacter();
    }

    void SetupMaterialSlots()
    {
        if (selectionManager.AvailableMaterials.Count > 0)
        {
            OnMaterialSlotClicked(0);
        }
    }

    public void OnMaterialSlotClicked(int index)
    {
        if (isMaterialAnimating) return;

        StartCoroutine(HandleMaterialSelectionCoroutine(index));
    }

    IEnumerator HandleMaterialSelectionCoroutine(int index)
    {
        isMaterialAnimating = true;

        if (index < 0 || index >= selectionManager.AvailableMaterials.Count)
        {
            Debug.LogWarning($"OnMaterialSlotClicked: Index {index} is out of bounds for AvailableMaterials.");
            isMaterialAnimating = false;
            yield break;
        }

        if (currentSelectedMaterialIndex >= 0 && currentSelectedMaterialIndex < materialSlots.Count)
        {
            _WeaponMaterial oldMat = selectionManager.AvailableMaterials[currentSelectedMaterialIndex];
            materialSlots[currentSelectedMaterialIndex].sprite = normalSlotSprite;
            if (materialSlotIcons.Count > currentSelectedMaterialIndex && oldMat.icon != null)
            {
                materialSlotIcons[currentSelectedMaterialIndex].sprite = oldMat.icon;
            }
        }

        currentSelectedMaterialIndex = index;
        _WeaponMaterial mat = selectionManager.AvailableMaterials[index];
        selectionManager.Selectmaterial(mat);
        newMaterials[index].SetActive(false);
        selectedMaterialName.text = $"{mat.type}";

        if (materialSlots.Count > index)
        {
            materialSlots[index].sprite = selectedSlotSprite;
        }

        if (materialSlotIcons.Count > index && mat.pressedIcon != null)
        {
            materialSlotIcons[index].sprite = mat.pressedIcon;
        }

        if (mat.effect != null && mat.effect.icon != null)
        {
            materialEffectIcon.sprite = mat.effect.icon;
            materialEffectIcon.gameObject.SetActive(true);
        }
        else
        {
            materialEffectIcon.gameObject.SetActive(false);
        }

        UpdateResultingSwordPanel();

        if (currentlySelectedMaterialModelIndex != index)
        {
            if (currentlySelectedMaterialModelIndex != -1 && currentlySelectedMaterialModelIndex < materialModels.Count)
            {
                var previousModel = materialModels[currentlySelectedMaterialModelIndex];
                var previousAnimator = previousModel.GetComponent<MaterialAnimator>();
                if (previousAnimator != null)
                {
                    previousAnimator.Deselect(() =>
                    {
                        previousModel.SetActive(false);
                    });

                    yield return new WaitForSeconds(previousAnimator.dropDuration);
                }
                else
                {
                    previousModel.SetActive(false);
                }
            }

            if (index >= 0 && index < materialModels.Count)
            {
                var selectedModel = materialModels[index];
                selectedModel.SetActive(true);

                var selectedAnimator = selectedModel.GetComponent<MaterialAnimator>();
                if (selectedAnimator != null)
                {
                    selectedAnimator.characterAnimator = blacksmithAnimator;
                    selectedAnimator.Select();
                }

                currentlySelectedMaterialModelIndex = index;
            }
            else
            {
                currentlySelectedMaterialModelIndex = -1;
            }
        }

        isMaterialAnimating = false;
    }

    void SetupWeaponSlots()
    {
        if (selectionManager.AvailableWeapons.Count > 0)
        {
            OnWeaponSlotClicked(0);
        }
        else
        {
            Debug.LogWarning("SetupWeaponSlots: AvailableWeapons listesi boþ.");
        }
    }

    public void OnWeaponSlotClicked(int index)
    {
        if (isWeaponAnimating)
        {
            return;
        }

        if (index < 0 || index >= selectionManager.AvailableWeapons.Count)
        {
            Debug.LogWarning($"OnWeaponSlotClicked: Index {index} AvailableWeapons listesi sýnýrlarý dýþýnda.");
            return;
        }

        if (index >= weaponSlots.Count)
        {
            Debug.LogWarning($"OnWeaponSlotClicked: Index {index} weaponSlots listesi sýnýrlarý dýþýnda.");
            return;
        }

        isWeaponAnimating = true;

        if (currentSelectedWeaponIndex != -1 && currentSelectedWeaponIndex < selectionManager.AvailableWeapons.Count && currentSelectedWeaponIndex < weaponSlots.Count)
        {
            _weapon oldWpn = selectionManager.AvailableWeapons[currentSelectedWeaponIndex];
            weaponSlots[currentSelectedWeaponIndex].sprite = normalSlotSprite;
            if (weaponSlotIcons.Count > currentSelectedWeaponIndex && oldWpn.icon != null)
            {
                weaponSlotIcons[currentSelectedWeaponIndex].sprite = oldWpn.icon;
            }
        }

        currentSelectedWeaponIndex = index;
        _weapon wpn = selectionManager.AvailableWeapons[index];
        selectionManager.SelectWeapon(wpn);
        newWeapons[index].SetActive(false);
        selectedWeaponName.text = $"{wpn.type}";

        weaponSlots[index].sprite = selectedSlotSprite;

        if (weaponSlotIcons.Count > index && wpn.pressedIcon != null)
        {
            weaponSlotIcons[index].sprite = wpn.pressedIcon;
        }

        if (wpn.effect != null && wpn.effect.icon != null)
        {
            swordEffectIcon.sprite = wpn.effect.icon;
            swordEffectIcon.gameObject.SetActive(true);
        }
        else
        {
            swordEffectIcon.gameObject.SetActive(false);
        }

        UpdateResultingSwordPanel();

        HandleWeaponModelSelection(index);

        StartCoroutine(AnimationCooldown(0.6f));
    }

    private IEnumerator AnimationCooldown(float delay)
    {
        yield return new WaitForSeconds(delay);
        isWeaponAnimating = false;
    }

    private void HandleWeaponModelSelection(int index)
    {
        if (currentlySelectedWeaponModelIndex == index)
        {
            return;
        }

        if (currentlySelectedWeaponModelIndex != -1 && currentlySelectedWeaponModelIndex < weaponModels.Count)
        {
            var previousModel = weaponModels[currentlySelectedWeaponModelIndex];
            var previousAnimator = previousModel.GetComponent<WeaponAnimator>();
            if (previousAnimator != null)
            {
                previousAnimator.Deselect();
            }
        }

        if (index >= 0 && index < weaponModels.Count)
        {
            var selectedModel = weaponModels[index];
            var selectedAnimator = selectedModel.GetComponent<WeaponAnimator>();
            if (selectedAnimator != null)
            {
                selectedAnimator.Select();
            }
            currentlySelectedWeaponModelIndex = index;
        }
        else
        {
            currentlySelectedWeaponModelIndex = -1;
        }
    }

    void UpdateResultingSwordPanel()
    {
        _Character currentChar = selectionManager.selectedCharacter;
        if (currentChar == null || currentChar._Weapon == null) return;

        int minDamage = currentChar.calculateMinDamage(currentChar._Weapon.minBaseDamage);
        int maxDamage = currentChar.calculateMaxDamage(currentChar._Weapon.maxBaseDamage);

        weightText.text = $"{currentChar._Weapon.weight}";
        baseDmgText.text = $"{currentChar._Weapon.minBaseDamage} - {currentChar._Weapon.maxBaseDamage}";
        totalDmgText.text = $"{minDamage} - {maxDamage}";
        critText.text = $" (x{currentChar._Weapon.critDamage}): {currentChar._Weapon.crit}%";

        if (currentChar._Weapon.material != null && currentChar._Weapon.material.effect != null && currentChar._Weapon.material.effect.type.ToString() != "None")
        {
            materialEffectText.text = currentChar._Weapon.material.effect.type.ToString();
        }
        else
        {
            materialEffectText.text = " ";
        }

        if (currentChar._Weapon.effect != null && currentChar._Weapon.effect.type.ToString() != "None")
        {
            swordEffectText.text = currentChar._Weapon.effect.type.ToString();
        }
        else
        {
            swordEffectText.text = " ";
        }
    }
}
