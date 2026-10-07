using System.Collections;
using UnityEngine;

/// <summary>
/// 控制 Boss 受击时自身闪红（直接改 SpriteRenderer 颜色）
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BossFlashEffect : MonoBehaviour
{
    [Header("闪红设置")]
    public Color flashColor = Color.red;   // 受伤时的颜色
    public float flashDuration = 0.1f;     // 持续时间（秒）

    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashCo;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    /// <summary>
    /// 对外调用：触发一次闪红
    /// </summary>
    public void Flash()
    {
        if (flashCo != null)
        {
            StopCoroutine(flashCo);
        }
        flashCo = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    private void OnDisable()
    {
        // 确保被关掉/销毁前，颜色还原
        if (sr != null)
        {
            sr.color = originalColor;
        }
    }
}