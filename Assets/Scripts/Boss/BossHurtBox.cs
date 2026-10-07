using UnityEngine;

/// <summary>
/// Boss受伤判定区域（玩家攻击打到这里就算命中）
/// </summary>
public class BossHurtBox : MonoBehaviour
{
    [Tooltip("Boss控制器")] public BossController bossController;
    [Tooltip("一次受击的伤害")] public int damagePerHit = 1;

    private void Awake()
    {
        if (bossController == null)
        {
            bossController = GetComponentInParent<BossController>();
        }

        // 确保这是触发器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测玩家的攻击判定（AttackCheck）
        if (other.CompareTag("PlayerAttack") || other.name.Contains("AttackCheck"))
        {
            // 调用 Boss 受伤方法
            if (bossController != null && !bossController.isDead)
            {
                bossController.TakeDamage(damagePerHit);
                Debug.Log($"【Boss受击】玩家攻击命中，造成{damagePerHit}点伤害");
            }
        }
    }
}