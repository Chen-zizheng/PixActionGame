using UnityEngine;

/// <summary>
/// 判定传递脚本【完善版】
/// 作用：把子物体（AttackCheck/ParryCheck）的触发器事件，传递给父物体的PlayerController
/// 挂载在：AttackCheck和ParryCheck子物体上
/// </summary>
public class TriggerSender : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        // 自动找到父物体的玩家控制器
        playerController = GetComponentInParent<PlayerController>();

        // 空引用安全检查
        if (playerController == null)
        {
            Debug.LogWarning($"【TriggerSender】在{gameObject.name}上没找到父物体的PlayerController！");
        }
    }

    // 有物体进入触发器时调用
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 安全检查：没找到玩家控制器就不执行
        if (playerController == null) return;

        // 根据当前物体的名字，判断是攻击判定还是弹反判定
        if (gameObject.name == "AttackCheck")
        {
            // 攻击判定命中，告诉玩家
            playerController.OnAttackHit(other);
        }
        else if (gameObject.name == "ParryCheck")
        {
        }
    }
}