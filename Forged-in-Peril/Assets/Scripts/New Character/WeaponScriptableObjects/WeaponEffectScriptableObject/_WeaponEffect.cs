using UnityEngine;

[CreateAssetMenu(fileName = "WeaponEffect", menuName = "WeaponEffects/Effect")]
public class _WeaponEffect : TooltipDataBase
{
    public Sprite icon;
    public weaponEffectType type;
    public string effectDesc;

    public override string Title => "Effect: " + type.ToString();
    public override string Description => effectDesc;
    public override Sprite Icon => icon;

    public enum weaponEffectType
    {
        None = 0,
        Unwieldy,
        Momentum,
        CrescentGuard,
        DullBlade,
        Precision,
        BladeCounter
    }
}
