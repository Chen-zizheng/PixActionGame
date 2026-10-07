using UnityEngine;

/// <summary>
/// BOSS 普通攻击判定（每次攻击只打一次）
/// 挂在 Boss 的攻击判定子物体上，子物体带 BoxCollider2D（IsTrigger = true）
/// </summary>
public class BossAttackCheck : MonoBehaviour
{
    [Tooltip("BOSS 普通攻击伤害")]
    public int attackDamage = 1;

    [Tooltip("BOSS 控制器本体")]
    public BossController bossController;

    private Collider2D selfCollider;

    /// <summary>
    /// 标记这一轮攻击是否已经命中过玩家（避免一轮攻击多次伤害）
    /// </summary>
    private bool hasHitThisAttack = false;

    private void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
        if (selfCollider != null)
        {
            selfCollider.isTrigger = true;
        }

        // 默认关闭，只有 BossAttack 打开时才启用
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 每次 BossAttack 打开判定时，重置为“这一轮还没打中过”
        hasHitThisAttack = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 已经在本轮攻击中命中过一次，不再继续伤害
        if (hasHitThisAttack) return;

        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }

        if (player != null)
        {
            hasHitThisAttack = true;
            player.GetHit(attackDamage);
            Debug.Log($"=== [BossAttackCheck] 普通攻击命中玩家，造成 {attackDamage} 点伤害 ===");
        }
    }

    /// <summary>
    /// 玩家弹反成功时调用（如果你有弹反系统，可先不用）
    /// </summary>
    public void OnParried()
    {
        if (bossController == null || bossController.isDead) return;

        bossController.OnParried();
        gameObject.SetActive(false);
    }
}