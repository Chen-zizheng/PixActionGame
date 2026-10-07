using UnityEngine;

/// <summary>
/// 玩家攻击判定脚本，挂在玩家攻击盒子（attackCheck 子物体）上
/// 只负责把命中信息转发给 PlayerController
/// </summary>
public class PlayerAttackCheck : MonoBehaviour
{
    [Tooltip("玩家控制脚本")]
    public PlayerController playerController;

    private Collider2D selfCollider;

    private void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
        if (selfCollider != null)
        {
            selfCollider.isTrigger = true;
        }

        // 默认关闭��只有攻击时才打开
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerController == null || playerController.isDead) return;

        playerController.OnAttackHit(other);
    }
}