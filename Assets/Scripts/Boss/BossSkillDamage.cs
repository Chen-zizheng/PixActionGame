using UnityEngine;

/// <summary>
/// Boss 技能伤害判定
/// 挂在 Boss 的技能判定子物体上（你图里的绿色矩形）
/// 子物体需要带 BoxCollider2D (IsTrigger = true)
/// </summary>
public class BossSkillDamage : MonoBehaviour
{
    [Tooltip("技能伤害数值")]
    public int skillDamage = 2;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // 默认关闭，由 BossController 在技能动画中途开启
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.GetHit(skillDamage);
            Debug.Log($"[BossSkillDamage] 技能命中玩家，造成 {skillDamage} 点���害");
        }
    }
}