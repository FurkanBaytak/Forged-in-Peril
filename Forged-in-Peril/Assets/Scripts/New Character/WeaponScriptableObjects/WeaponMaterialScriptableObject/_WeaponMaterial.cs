using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponMaterial", menuName = "WeaponMaterials/WeaponMaterial")]
public class _WeaponMaterial : TooltipDataBase
{
    public Sprite icon;
    public Sprite pressedIcon;
    public weaponMaterialType type;
    public string materialDesc;
    public weaponMaterialEffect effect;

    public Boolean isLocked = true;

    public override string Title => "Material: " + type.ToString();
    public override string Description => materialDesc;
    public override Sprite Icon => icon;

    public enum weaponMaterialType
    {
        Iron,
        Mithril,
        Adamantine,
        RoyalGold,
        TrueIron,
        EnchantedIce,
        MoonSilver,
        Vampirium
    }
}