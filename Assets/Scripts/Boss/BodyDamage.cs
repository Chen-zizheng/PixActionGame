using UnityEngine;

/// <summary>
/// Boss身体碰撞伤害脚本（最终修复版）
/// </summary>
public class BodyDamageTrigger : MonoBehaviour
{
    [Header("===== 碰撞伤害 =====")]
    [Tooltip("身体碰撞伤害")] public int touchDamage = 1;
    [Tooltip("伤害冷却（避免重复掉血）")] public float damageCooldown = 1f;

    private BossController bossController;
    private float lastTouchTime;
    private int playerLayer;

    private void Awake()
    {
        bossController = GetComponentInParent<BossController>();
        lastTouchTime = -damageCooldown;
        playerLayer = LayerMask.NameToLayer("Character"); // 只检测玩家本体的图层
        Debug.Log("【身体碰撞】脚本初始化完成，玩家图层：" + playerLayer);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 基础校验：Boss死亡、冷却中，直接跳过
        if (bossController == null || bossController.isDead) return;  // 改回英文
        if (Time.time - lastTouchTime < damageCooldown) return;

        // 只检测玩家本体的图层，排除子物体
        if (other.gameObject.layer != playerLayer) return;

        // 从碰撞体的根物体拿PlayerController（确保拿到玩家本体的组件）
        PlayerController player = other.transform.root.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning($"【身体碰撞】没找到玩家控制器，碰撞物体：{other.gameObject.name}");
            return;
        }

        // 玩��没死、不在无敌帧，才造成伤害
        if (!player.isDead && !player.isInvincible)
        {
            player.GetHit(touchDamage);
            lastTouchTime = Time.time;
            Debug.Log($"【身体碰撞】对玩家造成{touchDamage}点伤害，玩家剩余血量：{player.currentHealth}");
        }
    }
}