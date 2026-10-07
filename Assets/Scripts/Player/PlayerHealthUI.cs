using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("引用")]
    public PlayerController player;     // Inspector 里拖 Player
    public Image healthFill;           // PlayerHealthBar/HealthFill
    public TextMeshProUGUI healthText; // PlayerHealthBar/HealthText （如果用的是 TMP）

    private float maxWidth;            // 血条满血时的宽度

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.GetComponent<PlayerController>();
        }

        if (healthFill != null)
        {
            // 记录初始宽度作为满血宽度
            maxWidth = (healthFill.rectTransform.sizeDelta.x);
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnHealthChanged += OnPlayerHealthChanged;
            // 主动同步一次
            OnPlayerHealthChanged(player.currentHealth, player.maxHealth);
        }
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnHealthChanged -= OnPlayerHealthChanged;
    }

    private void OnPlayerHealthChanged(int current, int max)
    {
        if (healthFill != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            ratio = Mathf.Clamp01(ratio);

            // 按比例改变子矩形宽度
            var rt = healthFill.rectTransform;
            var size = rt.sizeDelta;
            size.x = maxWidth * ratio;
            rt.sizeDelta = size;
        }

        if (healthText != null)
        {
            healthText.text = $"{current} / {max}";
        }
    }
}