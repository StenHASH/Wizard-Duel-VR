using UnityEngine;
using UnityEngine.UI;

public class MP : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxMana = 100f;
    [SerializeField, Min(0f)] private float regenerationPerSecond = 10f;
    [SerializeField] private Image manaFill;
    [SerializeField] private bool showValueLabel;
    [SerializeField] private Text valueLabel;
    private float currentMana;

    private void Awake()
    {
        if (manaFill == null) manaFill = GetComponent<Image>();
        currentMana = maxMana;
        EnsureValueLabel();
        RefreshBar();
    }

    private void Start()
    {
        currentMana = 0;
    }

    private void Update()
    {
        if (currentMana >= maxMana) return;
        currentMana = Mathf.Min(maxMana,
            currentMana + regenerationPerSecond * Time.deltaTime);
        RefreshBar();
    }

    public bool TrySpend(float cost)
    {
        if (cost < 0f || currentMana < cost) return false;
        currentMana -= cost;
        RefreshBar();
        return true;
    }

    private void RefreshBar()
    {
        if (manaFill != null)
            manaFill.fillAmount = currentMana / maxMana;

        if (valueLabel != null)
        {
            string value = $"{Mathf.RoundToInt(currentMana)}/{Mathf.RoundToInt(maxMana)}";
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
