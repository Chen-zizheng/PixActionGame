using UnityEngine;

/// <summary>
/// 空洞骑士风格简化版相机：死区+边界+丝滑跟随
/// </summary>
public class HollowKnightCamera : MonoBehaviour
{
    [Header("核心跟随设置")]
    [Tooltip("拖入你的Player角色")]
    public Transform player;
    [Tooltip("丝滑度，推荐2-5，越大跟得越快")]
    public float smoothTime = 0.2f;

    [Header("死区设置（角色在这个范围内相机不动）")]
    [Tooltip("死区宽度，推荐2-4")]
    public float deadZoneWidth = 3f;
    [Tooltip("死区高度，推荐1-2")]
    public float deadZoneHeight = 1.5f;

    [Header("边界限制（解决露空白的核心）")]
    public bool useBounds = true;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -2f;
    public float maxY = 3f;

    private Vector3 velocity;
    private Vector3 targetPos;

    void LateUpdate()
    {
        if (player == null) return;

        // 1. 先算相机的目标位置
        targetPos = transform.position;

        // 2. 死区逻辑：只有角色跑出死区，相机才会动
        float playerDistX = player.position.x - transform.position.x;
        float playerDistY = player.position.y - transform.position.y;

        // 水平死区
        if (Mathf.Abs(playerDistX) > deadZoneWidth / 2f)
        {
            targetPos.x = player.position.x - Mathf.Sign(playerDistX) * deadZoneWidth / 2f;
        }

        // 垂直死区
        if (Mathf.Abs(playerDistY) > deadZoneHeight / 2f)
        {
            targetPos.y = player.position.y - Mathf.Sign(playerDistY) * deadZoneHeight / 2f;
        }

        // 3. 锁定边界，绝对不会露空白
        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        // 4. 丝滑平滑，和空洞骑士一样的质感
        targetPos.z = -10; // Z轴固定，别改
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    // 辅助：在场景窗口里画出死区和边界，方便你调
    private void OnDrawGizmosSelected()
    {
        // 画死区（蓝色）
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneWidth, deadZoneHeight, 0));

        // 画边界（红色）
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 boundsCenter = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
            Vector3 boundsSize = new Vector3(maxX - minX, maxY - minY, 0);
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}