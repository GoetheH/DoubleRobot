using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 20;
    public float speed = 8f;
    public float lifeTime = 3f;
    public Vector2 direction = Vector2.right;
    public RobotController owner;
    public Color bulletColor = Color.yellow;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private bool initialized = false;

    public void Initialize(Vector2 dir, int dmg, RobotController shooter, Color color)
    {
        direction = dir.normalized;
        damage = dmg;
        owner = shooter;
        bulletColor = color;

        if (sr != null) sr.color = color;

        if (rb != null)
        {
            rb.velocity = direction * speed;
            // 子弹不受重力
            rb.gravityScale = 0;
        }

        // 面向方向旋转子弹
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        initialized = true;
    }

    private void Awake()
    {
        // 创建子弹精灵 — 体积为人物的1/8 (0.1 x 0.15)
        sr = gameObject.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(4, 4);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                tex.SetPixel(x, y, Color.white);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        // PPU=40 → 4px/40 = 0.1单位宽
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 40);
        sr.sortingOrder = 8;
        sr.color = bulletColor;
        // 缩放为0.1 x 0.15
        transform.localScale = new Vector3(1f, 1.5f, 1f);

        // 物理体
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 触发器碰撞体
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.1f, 0.15f);
        col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (initialized && rb != null)
        {
            // 确保速度恒定
            rb.velocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 不命中自己
        if (owner != null && other.gameObject == owner.gameObject) return;

        RobotController target = other.GetComponent<RobotController>();
        if (target != null && !target.IsDead)
        {
            target.TakeDamage(damage);
            SpawnHitEffect();
            Destroy(gameObject);
            return;
        }

        // 碰到墙/地面也消失
        if (other.CompareTag("Wall") || other.name.Contains("Wall") || other.name.Contains("Ground"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        GameObject fx = new GameObject("BulletHit");
        fx.transform.position = transform.position;
        SpriteRenderer fxR = fx.AddComponent<SpriteRenderer>();

        // 创建小爆炸纹理
        Texture2D tex = new Texture2D(16, 16);
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                float dx = x - 8f;
                float dy = y - 8f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 6f)
                    tex.SetPixel(x, y, new Color(1f, 0.8f, 0.2f, 1f - dist / 6f));
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        fxR.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        fxR.sortingOrder = 12;

        // 快速放大+消失
        StartCoroutine(HitFade(fx, fxR));
    }

    private System.Collections.IEnumerator HitFade(GameObject fx, SpriteRenderer fxR)
    {
        float elapsed = 0f;
        float dur = 0.25f;
        Vector3 startScale = new Vector3(0.5f, 0.5f, 1);

        while (elapsed < dur && fx != null)
        {
            float t = elapsed / dur;
            fx.transform.localScale = startScale * (1f + t * 2f);
            if (fxR != null)
            {
                Color c = fxR.color;
                c.a = 1f - t;
                fxR.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (fx != null) Destroy(fx);
    }
}
