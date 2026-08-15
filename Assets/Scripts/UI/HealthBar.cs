using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fill;
    public Text hpText;
    public string label;

    private int maxHealth;
    private int currentHealth;
    private Color baseFillColor;

    public void SetFillColor(Color c)
    {
        baseFillColor = c;
        if (fill != null) fill.color = c;
    }

    public void SetMaxHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
        UpdateBar();
    }

    public void SetHealth(int health)
    {
        currentHealth = health;
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (fill != null)
        {
            float ratio = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;
            // 用anchorMax.x控制fill宽度，比Filled类型更可靠
            fill.rectTransform.anchorMax = new Vector2(ratio, 1f);
            fill.rectTransform.offsetMax = Vector2.zero;

            // 低血量变红
            if (ratio < 0.25f)
                fill.color = Color.red;
            else if (ratio < 0.5f)
                fill.color = Color.yellow;
            else
                fill.color = baseFillColor;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHealth}/{maxHealth}";
        }
    }
}