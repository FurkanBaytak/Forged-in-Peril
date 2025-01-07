using System;
using UnityEngine;

[CreateAssetMenu(fileName = "_NewWeapon", menuName = "Weapons/Weapon")]
public class _weapon : TooltipDataBase
{
    public Sprite icon;
    public Sprite pressedIcon;
    public WeaponType type;
    public int minBaseDamage;
    public int maxBaseDamage;
    public string weight;
    public double crit;
    public int critDamage;
    public _WeaponEffect effect;
    public string WeaponDesc;
    public _WeaponMaterial material;
    public Boolean isLocked = true; 

    public override string Title => "Weapon: " + type.ToString();
    public override string Description => WeaponDesc;
    public override Sprite Icon => icon;

    public enum WeaponType
    {
        Longsword,
        Dagger,
        Claymore,
        Scimitar,
        HookSword,
        Cleaver,
        Rapier,
        Katana,
        enemyWeapon
    }

    
}