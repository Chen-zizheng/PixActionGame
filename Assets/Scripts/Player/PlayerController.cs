using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 玩家控制器（移动 / 跳跃 / 攻击 / 受伤 / 虚化冲刺）
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("===== 基础移动 =====")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("===== 战斗核心 =====")]
    public int maxHealth = 50;
    public int currentHealth;
    public float hitInvincibleTime = 1f;
    public float knockbackSpeed = 10f;
    public float knockbackDuration = 0.25f;     // 击退硬直时间
    public Transform boss;                      // 在 Inspector 拖 Boss
    public LayerMask enemyLayer;

    [Header("===== 冲刺（虚化） =====")]
    public float dashSpeed = 18f;        // 冲刺速度（横向）
    public float dashTime = 0.2f;        // 冲刺持续时间
    public float dashCooldown = 0.5f;    // 冲刺冷却时间
    public bool isDashing;               // 是否正在冲刺

    [Header("===== 攻击判定 =====")]
    public Transform attackCheck;        // 攻击判定子物体
    public int attackDamage = 1;

    [Header("===== 动画 =====")]
    public Animator anim;

    [Header("===== 调试状态（只读）=====")]
    public bool isDead;
    public bool isInvincible;
    public bool isAttacking;
    public bool isGrounded;
    public bool isKnockback;             // 正在被击退

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Collider2D bodyCollider;
    private float horizontalInput;
    private float lastHitTime;
    private float lastDashTime;

    // 攻击命中标记（每次攻击只命中一次）
    private bool hasAttackHit;

    // 图层缓存：普通玩家层 + 冲刺层
    private int originalLayer;
    private int dashLayer;

    public event Action<int, int> OnHealthChanged;
    public event Action OnPlayerDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.LogError("【自动修复】添加缺失的 Rigidbody2D 组件");
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (anim == null) anim = GetComponent<Animator>();

        // 记录普通玩家图层和 Dash 图层
        originalLayer = gameObject.layer;               // 一般是 Player
        dashLayer = LayerMask.NameToLayer("Dash");      // 你在 Project Settings 里建的 Dash 图层
        if (dashLayer == -1)
        {
            Debug.LogWarning("未找到名为 'Dash' 的图层，请在 Tags and Layers 里创建一个 Dash 图层。");
        }

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 攻击判定一开始必须是关闭的
        if (attackCheck != null)
            attackCheck.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isDead) return;

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer);

        horizontalInput = Input.GetAxisRaw("Horizontal");

        HandleMove();
        HandleJump();
        HandleDashInput();
        HandleAttackInput();
        UpdateAnimParams();
    }

    #region 移动 / 跳跃 / 冲刺 / 攻击

    private void HandleMove()
    {
        // 正在被击退或冲刺时，不处理普通移动输入，避免覆盖击退/冲刺速度
        if (isKnockback) return;
        if (isDashing) return;
        // if (isAttacking) return;   // 不再禁止攻击时移动

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(horizontalInput) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            isGrounded &&
            !isAttacking &&
            !isKnockback &&
            !isDashing)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim?.SetTrigger("Jump");
        }
    }

    private void HandleDashInput()
    {
        // 冲刺键：LeftShift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // 条件：未死亡、不在冲刺、不在击退、冷却结束
            if (!isDead && !isDashing && !isKnockback && Time.time >= lastDashTime + dashCooldown)
            {
                StartCoroutine(DashCoroutine());
            }
        }
    }

    private System.Collections.IEnumerator DashCoroutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // 冲刺期间视为无敌（虚化）
        isInvincible = true;

        // 冲刺时不认为是攻击
        bool prevIsAttacking = isAttacking;
        isAttacking = false;

        // 切换到 Dash 图层，避免与 Enemy / BodyDamage 碰撞
        if (dashLayer != -1)
        {
            gameObject.layer = dashLayer;
        }

        // 根据朝向确定冲刺方向（localScale.x 正右负左）
        float dir = Mathf.Sign(transform.localScale.x);
        if (dir == 0) dir = 1; // 万一出现 0，就默认向右

        float startTime = Time.time;

        // 虚化冲刺期间：持续 dashTime 秒，强制给一个固定的水平速度
        while (Time.time < startTime + dashTime)
        {
            if (rb != null)
            {
                rb.velocity = new Vector2(dir * dashSpeed, 0f); // 纯水平冲刺
            }
            yield return null;
        }

        // 冲刺结束：还原图层和冲刺状态
        gameObject.layer = originalLayer;
        isDashing = false;

        // 冲刺结束后再给一点点无敌缓冲（防止刚结束的一帧被判定到）
        float invincibleExtra = 0.08f;
        yield return new WaitForSeconds(invincibleExtra);

        // 确保无敌最终被关闭
        isInvincible = false;
        isAttacking = prevIsAttacking;
    }

    /// <summary>
    /// 攻击输入：只负责触发动画，真正的命中时机会由动画事件控制
    /// </summary>
    private void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0) &&
            !isAttacking &&
            !isDead &&
            !isKnockback &&
            !isDashing)
        {
            isAttacking = true;
            hasAttackHit = false;

            anim?.SetTrigger("Attack");
            // 攻击时停下水平移动
            rb.velocity = new Vector2(0, rb.velocity.y);

            // 不再用 Invoke 控制攻击判定时间，完全交给动画事件
        }
    }

    /// <summary>
    /// 由 Attack 动画第 N 帧的动画事件调用：
    /// 在动画上添加事件，函数名填 OnAttackHitStart
    /// </summary>
    public void OnAttackHitStart()
    {
        OpenAttackCheck();
    }

    /// <summary>
    /// 由 Attack 动画稍后的一帧动画事件调用：
    /// 在动画上添加事件，函数名填 OnAttackHitEnd
    /// </summary>
    public void OnAttackHitEnd()
    {
        CloseAttackCheck();
    }

    private void OpenAttackCheck()
    {
        hasAttackHit = false;
        if (attackCheck != null)
        {
            attackCheck.gameObject.SetActive(true);
            Debug.Log("【PlayerAttack】OpenAttackCheck (from animation event) at " + Time.time);
        }
    }

    private void CloseAttackCheck()
    {
        if (attackCheck != null)
        {
            attackCheck.gameObject.SetActive(false);
            Debug.Log("【PlayerAttack】CloseAttackCheck (from animation event) at " + Time.time);
        }
        // 攻击窗口结束时，可以允许进入收招阶段
        // 真正把 isAttacking 关掉在动画结束的某一帧更自然，这里保持不动
    }

    /// <summary>
    /// 如果你想在攻击动画结束的某一帧彻底结束攻击状态，
    /// 可以在动画最后再挂一个事件调用这个函数。
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        EndAttack();
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    #endregion

    #region 受击 / 击退 / 无敌 / 死亡

    public void GetHit(int damage)
    {
        Debug.Log("=== [PlayerController] GetHit 被调用，damage = " + damage);

        lastHitTime = Time.time;

        if (isDead || isInvincible) return;

        // 一旦受击，立刻打断冲刺和攻击
        isDashing = false;
        isAttacking = false;
        gameObject.layer = originalLayer;

        // 1. 扣血
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"【玩家受击】剩余血量：{currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 2. 受伤动画
        anim?.SetTrigger("Hit");

        // 3. 一段时间无敌，避免连续多段伤害秒杀
        StartCoroutine(InvincibleCoroutine(hitInvincibleTime));

        // 4. 击退逻辑
        if (rb != null)
        {
            // 4.1 计算击退方向：从 Boss 指向玩家的反方向
            Vector2 dir;

            if (boss != null)
            {
                // Boss 在玩家左边，就往右弹；反之往左
                dir = (transform.position - boss.position).normalized;
                dir.y = 0f;
            }
            else
            {
                // 没有 boss 引用时，根据玩家面朝方向反向弹开
                dir = new Vector2(-Mathf.Sign(transform.localScale.x), 0f);
            }

            dir = dir.normalized;
            if (Mathf.Abs(dir.x) < 0.1f)
            {
                // 极端情况：方向接近 0，就按面朝方向来
                dir.x = transform.localScale.x > 0 ? 1f : -1f;
            }

            // 4.2 主要是水平击退，几乎不往上
            Vector2 knockDir = new Vector2(dir.x, 0.05f).normalized;

            // 4.3 设置刚体速度：横向很大，纵向很小（你前面已经调成合适的系数）
            float horizontalPower = knockbackSpeed * 1.4f;
            float verticalPower = knockbackSpeed * 0.15f;

            rb.velocity = new Vector2(
                knockDir.x * horizontalPower,
                knockDir.y * verticalPower
            );

            Debug.Log($"[PlayerController] 击退速度设置为: {rb.velocity}");
        }

        // 5. 进入击退硬直状态
        isKnockback = true;
        CancelInvoke(nameof(EndKnockback));
        Invoke(nameof(EndKnockback), knockbackDuration);

        // 6. 死亡判断
        if (currentHealth <= 0)
        {
            Dead();
        }
    }

    private void EndKnockback()
    {
        isKnockback = false;
    }

    private IEnumerator InvincibleCoroutine(float time)
    {
        isInvincible = true;
        yield return new WaitForSeconds(time);
        isInvincible = false;
    }

    private void Dead()
    {
        isDead = true;
        isKnockback = false;
        isDashing = false;
        isInvincible = false;   // 死亡后一定不是无敌状态

        CancelInvoke();
        anim?.SetTrigger("Dead");

        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        if (bodyCollider != null) bodyCollider.enabled = false;

        CloseAttackCheck();

        OnPlayerDead?.Invoke();
        Debug.Log("【玩家】已死亡！");
    }

    #endregion

    #region 动画参数 & 攻击命中

    private void UpdateAnimParams()
    {
        if (anim == null) return;

        float speed = Mathf.Abs(rb.velocity.x);
        if (speed < 0.1f) speed = 0f;

        anim.SetFloat("Speed", speed);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsAttacking", isAttacking);
        // 如果以后在 Animator 里加了 IsDashing 参数，可以再打开这一行：
        // anim.SetBool("IsDashing", isDashing);
    }

    /// <summary>
    /// 被 PlayerAttackCheck 调用，处理命中 Boss。
    /// </summary>
    public void OnAttackHit(Collider2D other)
    {
        if (hasAttackHit || isDead || !isAttacking) return;

        // 只处理敌人图层
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            BossController bossCtrl = other.GetComponentInParent<BossController>();
            if (bossCtrl != null)
            {
                hasAttackHit = true;
                Debug.Log($"【玩家攻击】命中 BOSS，造成 {attackDamage} 点伤害");
                bossCtrl.TakeDamage(attackDamage);
            }
        }
    }

    #endregion
}