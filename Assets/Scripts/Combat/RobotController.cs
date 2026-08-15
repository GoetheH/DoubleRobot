using UnityEngine;
using UnityEngine.UI;

public class RobotController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int attackDamage = 20;
    public float attackRange = 1.5f;
    public float moveSpeed = 3f;
    public float defenseDuration = 1.5f;
    public float defenseReduction = 0.5f;

    [Header("Physics")]
    public float jumpForce = 8f;
    public float gravityScale = 3f;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer = 1;
    public Transform groundCheck;

    [Header("References")]
    public Transform attackPoint;
    public LayerMask enemyLayer;
    public Animator animator;
    public HealthBar healthBar;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = new Color(1f, 0.3f, 0.3f, 1f);
    public Color normalColor = Color.white;
    public Color attackColor = new Color(1f, 1f, 0.4f, 1f);
    public Color defendColor = new Color(0.3f, 0.6f, 1f, 1f);
    public Color bulletColor = new Color(1f, 0.9f, 0.2f, 1f);

    [HideInInspector]
    public bool isAttacking;
    [HideInInspector]
    public bool isDefending;
    [HideInInspector]
    public bool IsDead { get; private set; }

    public bool IsGrounded { get; private set; }

    private static Canvas _damageCanvas;
    private Rigidbody2D rb;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // 创建地面检测点（在机甲脚下）
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform, false);
            gc.transform.localPosition = new Vector3(0, -0.15f, 0);
            groundCheck = gc.transform;
        }
    }

    private void FixedUpdate()
    {
        // 地面检测
        IsGrounded = groundCheck != null && Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void InitHealthBar(HealthBar bar)
    {
        healthBar = bar;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    public void Move(Vector2 direction)
    {
        if (rb != null)
        {
            // 只控制X轴速度，Y轴由重力处理
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        }
        else
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }

    public void Jump()
    {
        if (IsGrounded && !IsDead && !isAttacking)
        {
            if (rb != null)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            SetTrigger("Jump");
        }
    }

    public void Attack()
    {
        if (isAttacking || isDefending || IsDead) return;

        isAttacking = true;
        SetTrigger("Attack");

        // 发射子弹
        FireBullet();

        Invoke(nameof(EndAttack), 0.3f);
    }

    private void FireBullet()
    {
        Transform atkPoint = attackPoint != null ? attackPoint : transform;
        float facingDir = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 bulletDir = new Vector2(facingDir, 0);

        // 创建子弹GameObject
        GameObject bulletObj = new GameObject("Bullet");
        bulletObj.transform.position = atkPoint.position + new Vector3(facingDir * 0.3f, 0, 0);
        bulletObj.layer = gameObject.layer; // 子弹与发射者同层

        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Initialize(bulletDir, attackDamage, this, bulletColor);

        // 攻击闪光
        if (spriteRenderer != null)
            StartCoroutine(FlashColor(attackColor, 0.1f));
    }

    public void Defend()
    {
        if (isAttacking || isDefending || IsDead) return;

        isDefending = true;
        SetTrigger("Defend");
        SetBool("Walk", false);
        SetBool("Idle", false);

        // 防御蓝色闪光
        if (spriteRenderer != null)
            StartCoroutine(FlashColor(defendColor, 0.15f));
    }

    public void StopDefend()
    {
        if (!isDefending) return;
        isDefending = false;
        SetBool("Idle", true);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        SetTrigger("Hit");

        // 受击红色闪烁（更强烈）
        if (spriteRenderer != null)
            StartCoroutine(FlashColor(hitColor, 0.15f));

        // 弹出伤害数字
        SpawnDamageNumber(damage);

        // 受击震动
        StartCoroutine(Shake());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        IsDead = true;
        SetTrigger("Die");
        SetBool("Walk", false);
        SetBool("Idle", false);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 死亡时变暗
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        if (GameManager.Instance != null)
            GameManager.Instance.OnRobotDeath(this);
    }

    protected void PlayAnimation(string animationName)
    {
        if (IsDead) return;

        bool walk = animationName == "Walk";
        bool idle = animationName == "Idle";

        SetBool("Walk", walk);
        SetBool("Idle", idle);
    }

    // ======== 视觉特效 ========

    private System.Collections.IEnumerator FlashColor(Color flash, float duration)
    {
        if (spriteRenderer == null) yield break;

        Color original = spriteRenderer.color;
        spriteRenderer.color = flash;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = original;
    }

    private System.Collections.IEnumerator FlashHit()
    {
        yield return FlashColor(hitColor, 0.15f);
    }

    private System.Collections.IEnumerator Shake()
    {
        if (rb == null) yield break;

        // 物理击退（不直接改position，避免与Rigidbody冲突）
        float facingDir = transform.localScale.x > 0 ? -1f : 1f; // 被击退方向=反朝向
        Vector2 knockback = new Vector2(facingDir * 2f, 1.5f);
        rb.AddForce(knockback, ForceMode2D.Impulse);

        // scale抖动替代position抖动
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            float s = 1f + Random.Range(-0.08f, 0.08f);
            transform.localScale = new Vector3(originalScale.x * s, originalScale.y * s, originalScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private void SpawnDamageNumber(int damage)
    {
        Canvas canvas = GetDamageCanvas();
        if (canvas == null) return;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject dmgObj = new GameObject("Damage_" + damage);
        dmgObj.transform.SetParent(canvas.transform, false);

        Text dmgText = dmgObj.AddComponent<Text>();
        dmgText.text = damage.ToString();
        dmgText.font = font;
        dmgText.fontSize = 28;
        dmgText.fontStyle = FontStyle.Bold;
        dmgText.alignment = TextAnchor.MiddleCenter;
        dmgText.color = damage >= 20 ? new Color(1f, 0.3f, 0.2f) : Color.yellow;
        dmgText.raycastTarget = false;

        RectTransform rt = dmgObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 40);

        // 将世界坐标转换为屏幕坐标
        Vector3 worldPos = transform.position + new Vector3(0, 1.2f, 0);
        Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPos) : worldPos;
        rt.position = screenPos;

        // 启动伤害数字飘动协程
        StartCoroutine(FloatDamageNumber(dmgObj, screenPos));
    }

    private System.Collections.IEnumerator FloatDamageNumber(GameObject dmgObj, Vector3 startPos)
    {
        if (dmgObj == null) yield break;

        float elapsed = 0f;
        float duration = 0.8f;
        Vector3 pos = startPos;

        while (elapsed < duration && dmgObj != null)
        {
            // 向上飘动
            pos.y += 60f * Time.deltaTime;
            if (dmgObj.TryGetComponent<RectTransform>(out RectTransform rt))
                rt.position = pos;

            // 渐隐
            if (dmgObj.TryGetComponent<Text>(out Text txt))
            {
                Color c = txt.color;
                c.a = 1f - (elapsed / duration);
                txt.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (dmgObj != null) Destroy(dmgObj);
    }

    private Canvas GetDamageCanvas()
    {
        if (_damageCanvas != null) return _damageCanvas;
        Canvas c = FindObjectOfType<Canvas>();
        if (c != null)
        {
            _damageCanvas = c;
            return c;
        }
        return null;
    }

    private void EndAttack()
    {
        isAttacking = false;
        SetBool("Idle", true);
    }

    private void SetBool(string name, bool value)
    {
        if (animator != null)
            animator.SetBool(name, value);
    }

    private void SetTrigger(string name)
    {
        if (animator != null)
            animator.SetTrigger(name);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}