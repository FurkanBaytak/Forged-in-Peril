using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;


public class GraphicsOptions : MonoBehaviour
{
    [Header("Render Pipeline Assets")]
    public RenderPipelineAsset ultraQualityAsset;
    public RenderPipelineAsset veryHighQualityAsset;
    public RenderPipelineAsset highQualityAsset;
    public RenderPipelineAsset mediumQualityAsset;
    public RenderPipelineAsset lowQualityAsset;
    public RenderPipelineAsset veryLowQualityAsset;

    [Header("UI Elements")]
    public TMP_Dropdown graphicsDropdown;

    private void Start()
    {
        graphicsDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string> { "Ultra", "Very High", "High", "Medium", "Low", "Very Low" };
        graphicsDropdown.AddOptions(options);

        graphicsDropdown.onValueChanged.AddListener(OnGraphicsDropdownChanged);

        LoadGraphicsSettings();
    }

    private void OnGraphicsDropdownChanged(int index)
    {
        switch (index)
        {
            case 0:
                QualitySettings.SetQualityLevel(5, true);
                GraphicsSettings.renderPipelineAsset = ultraQualityAsset;
                break;
            case 1:
                QualitySettings.SetQualityLevel(4, true);
                GraphicsSettings.renderPipelineAsset = veryHighQualityAsset;
                break;
            case 2:
                QualitySettings.SetQualityLevel(3, true);
                GraphicsSettings.renderPipelineAsset = highQualityAsset;
                break;
            case 3:
                QualitySettings.SetQualityLevel(2, true);
                GraphicsSettings.renderPipelineAsset = mediumQualityAsset;
                break;
            case 4:
                QualitySettings.SetQualityLevel(1, true);
                GraphicsSettings.renderPipelineAsset = lowQualityAsset;
                break;
            case 5:
                QualitySettings.SetQualityLevel(0, true);
                GraphicsSettings.renderPipelineAsset = veryLowQualityAsset;
                break;
            default:
                Debug.LogWarning("Error.");
                break;
        }

        SaveGraphicsSettings();
    }

    private void SaveGraphicsSettings()
    {
        int selectedIndex = graphicsDropdown.value;
        PlayerPrefs.SetInt("GraphicsQuality", selectedIndex);
        PlayerPrefs.Save();
    }

    private void LoadGraphicsSettings()
    {
        if (PlayerPrefs.HasKey("GraphicsQuality"))
        {
            int savedIndex = PlayerPrefs.GetInt("GraphicsQuality");
            graphicsDropdown.value = savedIndex;
            OnGraphicsDropdownChanged(savedIndex);
        }
        else
        {
            graphicsDropdown.value = 5;
            OnGraphicsDropdownChanged(5);
        }
    }
}
