using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTalent", menuName = "CharacterTalents/CharacterTalent")]
public class _CharacterTalent : TooltipDataBase
{
    public Sprite icon;
    public talentType type;
    public string talentDesc;

    public override string Title => "Talent: " + type.ToString();
    public override string Description => talentDesc;
    public override Sprite Icon => icon;

    public enum talentType
    {
        Buffed,
        SelfHealer,
        AimHigher,
        Clumsy,
        Anger,
        Optimistic,
        FastHands
    }
}
