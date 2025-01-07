using UnityEngine;

[CreateAssetMenu(fileName = "WeaponMaterialEffect", menuName = "WeaponMaterialEffects/WeaponMaterialEffect")]
public class weaponMaterialEffect : TooltipDataBase
{
    public Sprite icon;
    public weaponMaterialEffectType type;
    public string effectDesc;

    public override string Title => "Effect: " + type.ToString();
    public override string Description => effectDesc;
    public override Sprite Icon => icon;

    public enum weaponMaterialEffectType
    {
        None = 0,
        LightAsAir,
        WeightOfEarth,
        Extravagance,
        TrueStrike,
        NumbingCold,
        GuidingMoonlight,
        BloodSacrifice,
    }
}
