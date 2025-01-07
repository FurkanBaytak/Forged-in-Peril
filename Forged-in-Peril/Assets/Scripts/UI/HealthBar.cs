using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;

    public void Initialize(int currentHealth, int maxHealth, string name)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (nameText != null)
        {
            nameText.text = name;
        }

        UpdateHealthText(currentHealth, maxHealth);
    }
    public void UpdateHealth(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (healthSlider != null)
        {
            UpdateHealthText(currentHealth, (int)healthSlider.maxValue);
        }
    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
}
