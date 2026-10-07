using UnityEngine;
using System;

public class BossController : MonoBehaviour
{
    [Header("===== Boss基础属性 =====")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public float moveSpeed = 1.5f;

    [Tooltip("Boss 停止往前挤的最小距离（几乎贴脸）")]
    public float stopDistance = 0.2f;

    [Tooltip("在这个距离内 Boss 才会考虑发动攻击/技能")]
    public float attackTriggerDistance = 5f;

    [Tooltip("普通攻击冷却（秒）")]
    public float attackInterval = 1f;

    [Tooltip("技能冷却（秒）")]
    public float skillInterval = 5f;

    [Header("===== 状态（只读）=====")]
    public bool isWeak;
    public bool isDead;
    public bool isAttacking;
    public bool isUsingSkill;
    public bool isWalking;

    [Header("===== 引用 =====")]
    public Transform player;
    public Animator anim;
    public BossAttackCheck attackCheck;      // 普攻判定子物体
    public BossSkillDamage skillCheck;       // 技能判定子物体（绿色矩形）
    public BossFlashEffect flashEffect;      // 受伤闪红脚本
    public SpriteRenderer spriteRenderer;    // Boss 本体 SpriteRenderer

    private Rigidbody2D rb;
    private float lastAttackTime;
    private float lastSkillTime;

    // 攻击模式：三次普攻 → 技能 → 普攻 → 技能 → 循环
    private int attackPatternStep = 0;

    public event Action<int, int> OnBossHealthChanged;
    public event Action OnBossDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.LogError("【自动修复】Boss 添加了缺失的 Rigidbody2D 组件");
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        currentHealth = maxHealth;
        isWeak = false;
        isDead = false;
        isAttacking = false;
        isUsingSkill = false;
        isWalking = false;

        // 自动抓组件
        if (flashEffect == null)
            flashEffect = GetComponent<BossFlashEffect>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

        float t = Time.time;
        lastAttackTime = t - attackInterval;
        lastSkillTime = t;
    }

    private void Update()
    {
        if (isDead || isWeak)
        {
            StopMove();
            return;
        }

        if (player == null)
        {
            Debug.LogError("【Boss错误】player 引用为空，请在 Inspector 里拖入 Player 对象！");
            StopMove();
            return;
        }

        float distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);
        FlipToPlayer();

        if (distanceToPlayer > stopDistance)
        {
            MoveToPlayer();
        }
        else
        {
            StopMove();
        }

        if (distanceToPlayer <= attackTriggerDistance &&
            !isAttacking && !isUsingSkill)
        {
            TryDoNextPatternAttack();
        }

        anim?.SetBool("IsWalking", isWalking);
    }

    #region 移动/朝向

    private void MoveToPlayer()
    {
        float moveDir = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
        isWalking = true;
    }

    private void StopMove()
    {
        if (rb != null)
            rb.velocity = new Vector2(0, rb.velocity.y);
        isWalking = false;
    }

    private void FlipToPlayer()
    {
        float dir = player.position.x - transform.position.x;
        transform.localScale = new Vector3(Mathf.Sign(dir) * Mathf.Abs(transform.localScale.x),
                                           transform.localScale.y,
                                           transform.localScale.z);
    }

    #endregion

    #region 攻击模式调度

    private void TryDoNextPatternAttack()
    {
        float now = Time.time;

        switch (attackPatternStep)
        {
            case 0:
            case 1:
            case 2:
                if (now >= lastAttackTime + attackInterval)
                {
                    BossAttack();
                    attackPatternStep++;
                }
                break;

            case 3:
                if (now >= lastSkillTime + skillInterval)
                {
                    BossSkill();
                    attackPatternStep++;
                }
                break;

            case 4:
                if (now >= lastAttackTime + attackInterval)
                {
                    BossAttack();
                    attackPatternStep++;
                }
                break;

            case 5:
                if (now >= lastSkillTime + skillInterval)
                {
                    BossSkill();
                    attackPatternStep = 0;
                }
                break;

            default:
                attackPatternStep = 0;
                break;
        }
    }

    #endregion

    #region 普通攻击

    private void BossAttack()
    {
        Debug.Log("=== [BossController] BossAttack() 普通攻击开始 ===");

        isAttacking = true;
        lastAttackTime = Time.time;

        if (anim != null)
        {
            anim.ResetTrigger("Skill");
            anim.SetTrigger("Attack");
        }

        CancelInvoke();

        Invoke(nameof(OpenAttackCheck), 0.40f);
        Invoke(nameof(CloseAttackCheck), 0.45f);
        Invoke(nameof(EndAttack), 0.60f);
    }

    private void OpenAttackCheck()
    {
        if (attackCheck != null)
            attackCheck.gameObject.SetActive(true);
    }

    private void CloseAttackCheck()
    {
        if (attackCheck != null)
            attackCheck.gameObject.SetActive(false);
    }

    private void EndAttack()
    {
        isAttacking = false;
        CloseAttackCheck();
    }

    #endregion

    #region 技能攻击

    private void BossSkill()
    {
        Debug.Log("[BossController] BossSkill() 被调用");

        isUsingSkill = true;
        lastSkillTime = Time.time;

        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.SetTrigger("Skill");
        }

        CancelInvoke();

        Invoke(nameof(OpenSkillCheck), 0.30f);
        Invoke(nameof(CloseSkillCheck), 0.35f);
        Invoke(nameof(EndSkill), 1.0f);
    }

    private void OpenSkillCheck()
    {
        if (skillCheck != null)
            skillCheck.gameObject.SetActive(true);
    }

    private void CloseSkillCheck()
    {
        if (skillCheck != null)
            skillCheck.gameObject.SetActive(false);
    }

    private void EndSkill()
    {
        isUsingSkill = false;
        CloseSkillCheck();
    }

    #endregion

    #region 受伤 / 死亡

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

        if (anim != null)
            anim.SetTrigger("Hit");

        if (flashEffect != null)
            flashEffect.Flash();

        if (currentHealth <= 0)
            BossDead();
    }

    public void OnParried()
    {
        if (isDead || isWeak) return;

        isAttacking = false;
        isUsingSkill = false;
        CloseAttackCheck();
        CloseSkillCheck();
        CancelInvoke();

        if (anim != null)
            anim.SetTrigger("ParryHit");

        lastAttackTime = Time.time;
        lastSkillTime = Time.time;
    }

    private void BossDead()
    {
        if (isDead) return;

        isDead = true;
        isAttacking = false;
        isUsingSkill = false;
        isWeak = false;
        isWalking = false;

        CancelInvoke();

        // 播放死亡动画（如果有）
        if (anim != null)
            anim.SetTrigger("Dead");

        var col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        CloseAttackCheck();
        CloseSkillCheck();

        // 让刚体自由下落，同时做“变黑+掉落”的演出
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 8f;                        // 掉落得快一点
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        OnBossDead?.Invoke();

        // 启动死亡演出协程：渐变成黑色 + 向下掉出屏幕
        StartCoroutine(BossDeathVisualRoutine());
    }

    private System.Collections.IEnumerator BossDeathVisualRoutine()
    {
        // 渐渐变黑的时间
        float fadeTime = 0.6f;
        float t = 0f;

        Color original = spriteRenderer != null ? spriteRenderer.color : Color.white;
        Color target = Color.black;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeTime);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(original, target, k);

            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = target;

        // 再额外给一点时间让 Boss 掉出画面
        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }

    #endregion
}