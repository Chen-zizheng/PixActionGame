using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("“˝”√")]
    public BossController boss;         // Inspector ¿ÔÕœ Boss
    public Image bossHealthFill;        // BossHealthBar/BossHealthFill
    public TextMeshProUGUI bossHealthText; // BossHealthBar/BossHealthText

    private float maxWidth;

    private void Awake()
    {
        if (boss == null)
        {
            var b = GameObject.FindObjectOfType<BossController>();
            if (b != null) boss = b;
        }

        if (bossHealthFill != null)
        {
            maxWidth = bossHealthFill.rectTransform.sizeDelta.x;
        }
    }

    private void OnEnable()
    {
        if (boss != null)
        {
            boss.OnBossHealthChanged += OnBossHealthChanged;
            OnBossHealthChanged(boss.currentHealth, boss.maxHealth);
        }
    }

    private void OnDisable()
    {
        if (boss != null)
            boss.OnBossHealthChanged -= OnBossHealthChanged;
    }

    private void OnBossHealthChanged(int current, int max)
    {
        if (bossHealthFill != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            ratio = Mathf.Clamp01(ratio);

            var rt = bossHealthFill.rectTransform;
            var size = rt.sizeDelta;
            size.x = maxWidth * ratio;
            rt.sizeDelta = size;
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"{current} / {max}";
        }
    }
}