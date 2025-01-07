using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TooltipDataDatabase", menuName = "Tooltip/TooltipDataDatabase")]
public class TooltipDataDatabase : ScriptableObject
{
    public TooltipDataBase[] tooltipDataArray;
    private Dictionary<string, TooltipDataBase> tooltipDataDict;

    public void Initialize()
    {
        if (tooltipDataDict != null)
            return;

        tooltipDataDict = new Dictionary<string, TooltipDataBase>();

        foreach (var data in tooltipDataArray)
        {
            if (data != null && !tooltipDataDict.ContainsKey(data.Icon.name))
                tooltipDataDict.Add(data.Icon.name, data);
        }
    }

    public TooltipDataBase GetTooltipDataBySpriteName(string spriteName)
    {
        if (tooltipDataDict == null)
            Initialize();

        tooltipDataDict.TryGetValue(spriteName, out var data);
        return data;
    }
}