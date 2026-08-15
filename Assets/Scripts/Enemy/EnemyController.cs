using UnityEngine;
using System.Collections;

public class EnemyController : RobotController
{
    [Header("AI Settings")]
    public float detectionRange = 7f;
    public float preferredDistance = 4f;        // 保持的理想射击距离
    public float attackCooldown = 1.2f;
    public float defenseChance = 0.4f;          // 玩家攻击时的防御概率
    public float retreatHealthPercent = 0.3f;   // 血量低于30%时后撤
    public float bulletDodgeRange = 3f;         // 子弹检测范围
    public float bulletDodgeChance = 0.7f;     // 躲避子弹概率

    private Transform player;
    private float lastAttackTime;
    private bool isRetreating;
    private float decisionTimer;
    private float strafeDirection = 1;
    private float strafeTimer;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (IsDead) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.TwoPlayer) return;

        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                PlayAnimation("Idle");
                return;
            }
        }

        // 优先级0：玩家站在头顶时，立刻移开
        if (IsPlayerOnHead())
        {
            EscapeFromHead();
            return;
        }

        // 优先级1：躲避 incoming 子弹
        if (TryDodgeBullet()) return;

        // 优先级2：玩家正在攻击时概率防御
        if (TryReactToPlayerAttack()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float playerHealthRatio = GetPlayerHealthRatio();

        FacePlayer();

        // 优先级3：血量低时后撤拉开距离
        if (playerHealthRatio > 0.6f && GetHealthRatio() < retreatHealthPercent)
        {
            RetreatFromPlayer();
            return;
        }

        // 优先级4：战斗逻辑
        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                HandleCombatAI();
            }
            else if (distanceToPlayer > preferredDistance + 1f)
            {
                // 太远了，靠近
                MoveTowardsPlayer();
            }
            else
            {
                // 在理想距离附近，左右走位 + 射击
                StrafeAndShoot();
            }
        }
        else
        {
            if (IsGrounded)
                PlayAnimation("Idle");
        }
    }

    private void FindPlayer()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    /// <summary>
    /// 检测来袭子弹并躲避（跳跃或防御）
    /// </summary>
    private bool TryDodgeBullet()
    {
        // 查找所有子弹
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            if (bullet.owner != null && bullet.owner.gameObject == gameObject) continue;

            float dist = Vector2.Distance(bullet.transform.position, transform.position);
            if (dist < bulletDodgeRange)
            {
                // 子弹朝向自己飞来
                Vector2 toMe = (transform.position - bullet.transform.position).normalized;
                Vector2 bulletDir = bullet.GetComponent<Rigidbody2D>().velocity.normalized;
                if (Vector2.Dot(toMe, bulletDir) > 0.5f)
                {
                    if (Random.value < bulletDodgeChance)
                    {
                        // 近距离用防御，远距离用跳跃躲避
                        if (dist < 1.5f)
                        {
                            Defend();
                            StartCoroutine(StopDefendAfter(0.5f));
                        }
                        else if (IsGrounded)
                        {
                            Jump();
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 检测玩家是否正在攻击，概率防御
    /// </summary>
    private bool TryReactToPlayerAttack()
    {
        RobotController playerRobot = GameManager.Instance?.player;
        if (playerRobot == null || playerRobot.IsDead) return false;

        // 玩家正在攻击 → 概率防御
        if (playerRobot.isAttacking && !isDefending && IsGrounded)
        {
            if (Random.value < defenseChance)
            {
                Defend();
                StartCoroutine(StopDefendAfter(0.6f));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检测玩家是否站在AI头顶上
    /// </summary>
    private bool IsPlayerOnHead()
    {
        if (player == null) return false;
        float verticalDiff = player.position.y - transform.position.y;
        float horizontalDiff = Mathf.Abs(player.position.x - transform.position.x);
        // 玩家在上方且水平距离很近
        return verticalDiff > 0.5f && horizontalDiff < 0.6f;
    }

    /// <summary>
    /// 玩家站在头顶时，快速向一侧移动 + 跳跃甩开
    /// </summary>
    private void EscapeFromHead()
    {
        // 向远离玩家的方向移动（其实玩家在头顶，随便选一个方向跑）
        float escapeDir = transform.localScale.x > 0 ? -1f : 1f;
        Move(new Vector2(escapeDir * 1.2f, 0));
        PlayAnimation("Walk");

        // 跳跃甩开头顶的玩家
        if (IsGrounded && Random.value < 0.08f)
        {
            Jump();
        }
    }

    private void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void MoveTowardsPlayer()
    {
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        Move(new Vector2(dirX, 0));
        PlayAnimation("Walk");

        // 玩家在高处时跳跃追击
        if (IsGrounded && player.position.y > transform.position.y + 1f)
        {
            if (Random.value < 0.03f)
                Jump();
        }
    }

    private void RetreatFromPlayer()
    {
        float dirX = -Mathf.Sign(player.position.x - transform.position.x);
        Move(new Vector2(dirX, 0));
        PlayAnimation("Walk");

        // 后撤时偶尔跳跃躲避
        if (IsGrounded && Random.value < 0.01f)
            Jump();
    }

    /// <summary>
    /// 在理想距离附近左右走位 + 射击
    /// </summary>
    private void StrafeAndShoot()
    {
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0)
        {
            strafeDirection *= -1;
            strafeTimer = Random.Range(0.8f, 1.5f);
        }

        Move(new Vector2(strafeDirection * 0.6f, 0));
        PlayAnimation("Walk");

        // 走位中射击
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void HandleCombatAI()
    {
        if (IsGrounded)
            PlayAnimation("Idle");

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 玩家正在防御 → 不浪费子弹，靠近逼防
            RobotController playerRobot = GameManager.Instance?.player;
            if (playerRobot != null && playerRobot.isDefending)
            {
                // 概率假装攻击消耗防御，然后绕到侧面
                if (Random.value < 0.3f)
                {
                    // 靠近逼迫玩家松开防御
                    MoveTowardsPlayer();
                }
                else
                {
                    // 等待防御结束
                    if (IsGrounded)
                        PlayAnimation("Idle");
                }
            }
            else
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    private float GetPlayerHealthRatio()
    {
        RobotController playerRobot = GameManager.Instance?.player;
        if (playerRobot == null) return 0f;
        return (float)playerRobot.currentHealth / playerRobot.maxHealth;
    }

    private float GetHealthRatio()
    {
        return (float)currentHealth / maxHealth;
    }

    private IEnumerator StopDefendAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopDefend();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bulletDodgeRange);
    }
}
