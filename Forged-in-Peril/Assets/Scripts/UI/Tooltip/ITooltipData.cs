using UnityEngine;

public interface ITooltipData
{
    string Title { get; }
    string Description { get; }
    Sprite Icon { get; }
}