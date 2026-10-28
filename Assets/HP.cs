using UnityEngine;
using UnityEngine.UI;

public class HP : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private Image healthFill;
    [SerializeField] private bool showValueLabel;
    [SerializeField] private Text valueLabel;
    private float currentHealth;

    private void Awake()
    {
        if (healthFill == null) healthFill = GetComponent<Image>();
        currentHealth = maxHealth;
        EnsureValueLabel();
        RefreshBar();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        RefreshBar();
        if (currentHealth <= 0f)
            Debug.Log("Wizard defeated", this);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        RefreshBar();
    }

    private void RefreshBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (valueLabel != null)
        {
            string value = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
            if (valueLabel.text != value) valueLabel.text = value;
        }
    }

    private void EnsureValueLabel()
    {
        if (!showValueLabel) return;

        if (valueLabel == null)
        {
            Transform existingLabel = transform.Find("ValueLabel");
            if (existingLabel != null) valueLabel = existingLabel.GetComponent<Text>();
        }

        if (valueLabel != null) return;

        GameObject labelObject = new GameObject("ValueLabel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)labelObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        valueLabel = labelObject.GetComponent<Text>();
        valueLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueLabel.fontSize = 84;
        valueLabel.resizeTextForBestFit = true;
        valueLabel.resizeTextMinSize = 32;
        valueLabel.resizeTextMaxSize = 84;
        valueLabel.fontStyle = FontStyle.Bold;
        valueLabel.alignment = TextAnchor.MiddleCenter;
        valueLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        valueLabel.verticalOverflow = VerticalWrapMode.Overflow;
        valueLabel.color = Color.white;
        valueLabel.raycastTarget = false;

        Shadow shadow = labelObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);
    }
}
