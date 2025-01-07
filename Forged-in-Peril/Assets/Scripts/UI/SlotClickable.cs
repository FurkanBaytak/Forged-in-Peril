using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotClickable : MonoBehaviour, IPointerClickHandler
{
    public UIManager uiManager;
    public bool isMaterialSlot;
    public int slotIndex;
    public _SelectionManager selectionManager;
    public TooltipTrigger trigger;
    public Image icon;
    void Start()
    {
        if (isMaterialSlot)
        {
            // Image bileþenini al
            

            // Materyali al
            var mat = selectionManager.AvailableMaterials[slotIndex];

            // Kilitli olup olmadýðýný kontrol et
            if (mat.isLocked)
            {
                trigger.useTooltipData = false;
                icon.color = Color.black; // Renk siyah olarak deðiþtirildi
            }
            else
            {
                trigger.useTooltipData = true;
                icon.color = Color.white; // Kilitli deðilse rengi beyaz yapabilirsiniz
            }

            //Debug.Log($"{mat.name} is {(mat.isLocked ? "Locked" : "Unlocked")}");
        }
        else
        {
            var wpn = selectionManager.AvailableWeapons[slotIndex];
            if (wpn.isLocked) { 
            
                trigger.useTooltipData = false;
                icon.color = Color.black;
            }
            else
            {
                trigger.useTooltipData = true;
                icon.color = Color.white;
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (uiManager == null) return;

        if (isMaterialSlot)
        {
            if (selectionManager.AvailableMaterials[slotIndex].isLocked) {return; }
            uiManager.OnMaterialSlotClicked(slotIndex);
        }
        else
        {
            if (selectionManager.AvailableWeapons[slotIndex].isLocked) { return; }
            uiManager.OnWeaponSlotClicked(slotIndex);
        }
    }
}
