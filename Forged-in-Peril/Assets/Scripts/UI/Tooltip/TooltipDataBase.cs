using UnityEngine;

public abstract class TooltipDataBase : ScriptableObject, ITooltipData
{
    public abstract string Title { get; }
    public abstract string Description { get; }
    public abstract Sprite Icon { get; }
}