using System;
using System.Collections.Generic;
using UnityEngine;

// Eight-sector campaign; current runtime only.
public partial class NeonRun : MonoBehaviour
{
    public enum Phase { Menu, Playing, Upgrade, Paused, Victory, Defeat }
    public Phase State { get; private set; }
    public int Sector { get; private set; }
    public int Health { get; private set; }
    public int Kills { get; private set; }
    public int MaxHealth { get; private set; }
    public Vector2 PlayerPosition { get { return player; } }
    public int EnemyCount { get { return enemies.Count; } }
    Vector2 player, velocity, aim = Vector2.right;
    float cameraX, clock, nextShot, dashUntil, dashReady, immuneUntil, spawnAt, comboUntil;
    float coyoteUntil, jumpBufferedUntil, shake, bannerUntil;
    int jumps, weapon, combo, bestCombo, score, spawned, quota, damageLevel, rateLevel, spreadLevel;
    int dashFacing = 1;
    float previousBottom;
    bool grounded, muted;
    readonly List<Foe> enemies = new List<Foe>();
    readonly List<Shot> shots = new List<Shot>();
    readonly List<Spark> sparks = new List<Spark>();
    readonly List<Pickup> pickups = new List<Pickup>();
    readonly List<Rect> ledges = new List<Rect>();
    readonly int[] choices = new int[3];
    readonly string[] upgradeNames = { "强化弹芯", "高速供弹", "分裂弹道", "装甲修复", "机动冷却" };
    readonly string[] upgradeDescriptions = { "携带枪械伤害 +25%", "携带枪械射速 +18%", "额外两发侧向弹 · 可叠加", "最大装甲 +25，并回复 50", "冲刺冷却缩短 20% · 回复 20" };
    float dashCooldown = 1.35f;
    AudioSource audioSource;
    AudioClip fireSound, hitSound, jumpSound, upgradeSound;
    class Foe { public Vector2 p; public float hp, max, fire, phase, scannedUntil; public int kind; }
    class Shot { public Vector2 p, v; public float life, traveled; public int gun = -1, barrel; public int damage; public bool hostile, pierce; public Color color; public readonly HashSet<Foe> hit = new HashSet<Foe>(); }
    class Spark { public Vector2 p, v; public float life, max; public Color color; public string text; }
    class Pickup { public Vector2 p; public float life = 12; }

    void Awake()
    {
        Application.targetFrameRate = 120;
        Application.runInBackground = true;
        Camera cam = Camera.main;
        if (cam == null) { var go = new GameObject("Main Camera"); go.tag = "MainCamera"; cam = go.AddComponent<Camera>(); }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.025f, .035f, .075f);
        if (FindObjectOfType<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
        audioSource = gameObject.AddComponent<AudioSource>();
        fireSound = Tone(340, .055f); hitSound = Tone(95, .13f);
        jumpSound = Tone(520, .08f); upgradeSound = Tone(780, .24f);
        LoadAuthoring();
        MakeArt();
        ApplyArtOverrides();
        State = Phase.Menu;
        player = new Vector2(5, Floor);
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--neon-smoke") >= 0) StartCoroutine(RunSmoke());
        else if (Array.IndexOf(Environment.GetCommandLineArgs(), "--neon-aim-check") >= 0) StartCoroutine(RunAimCheck());
    }

    public void StartRun()
    {
        runDifficulty = selectedDifficulty;
        enemies.Clear(); shots.Clear(); sparks.Clear(); pickups.Clear();
        Health = MaxHealth = balance.startingHealth; Kills = score = combo = bestCombo = 0;
        damageLevel = rateLevel = spreadLevel = 0;
        weapon = selectedWeapon; dashCooldown = balance.dashCooldown;
        clock = 0; nextShot = dashUntil = dashReady = immuneUntil = 0;
        BeginSector(InitialSector());
    }

    void BeginSector(int sector)
    {
        Sector = sector; State = Phase.Playing;
        player = new Vector2(4, Floor); velocity = Vector2.zero; cameraX = 0;
        shots.Clear(); enemies.Clear(); pickups.Clear(); ledges.Clear();
        jumps = 0; spawned = 0; quota = 0;
        spawnAt = clock + 1.2f; bannerUntil = clock + 3;
        coyoteUntil = jumpBufferedUntil = -1;
        ConfigureSector();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) muted = !muted;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == Phase.Playing) State = Phase.Paused;
            else if (State == Phase.Paused) State = Phase.Playing;
            else if (State == Phase.Victory || State == Phase.Defeat) ReturnToMenu();
            else if (State == Phase.Menu) BackSetup();
        }
        if (State == Phase.Menu && Input.GetKeyDown(KeyCode.Return)) AdvanceSetup();
        if ((State == Phase.Defeat || State == Phase.Victory) && Input.GetKeyDown(KeyCode.R)) StartRun();
        if (State == Phase.Upgrade)
            for (int i = 0; i < 3; i++) if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) ChooseUpgrade(i);
        if (State != Phase.Playing) return;
        float dt = Mathf.Min(Time.deltaTime, .033f);
        clock += dt; shake = Mathf.Max(0, shake - dt * 2);
        if (clock > comboUntil) combo = 0;
        float move = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
        Vector2 mouse = Input.mousePosition;
        float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
        mouse = (mouse - new Vector2((Screen.width - 1600 * scale) / 2, (Screen.height - 900 * scale) / 2)) / scale;
        Vector2 worldMouse = new Vector2(mouse.x / 60 + cameraX, (mouse.y - 95) / 60 + cameraY);
        aim = (worldMouse - (player + Vector2.up * .98f)).normalized;
        if (aim.sqrMagnitude < .1f) aim = Vector2.right;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) jumpBufferedUntil = clock + .12f;
        if (jumpBufferedUntil >= clock && (jumps < 2 || coyoteUntil >= clock))
        {
            if (!grounded && coyoteUntil < clock && jumps == 0) jumps = 1;
            velocity.y = balance.jumpVelocity; jumps++; grounded = false; coyoteUntil = -1; jumpBufferedUntil = -1;
            Burst(player, Cyan, 8); Sound(jumpSound, .12f);
        }
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && clock >= dashReady)
        {
            dashFacing = move != 0 ? (int)Mathf.Sign(move) : (aim.x >= 0 ? 1 : -1);
            dashUntil = clock + balance.dashDuration; dashReady = clock + dashCooldown; immuneUntil = Mathf.Max(immuneUntil, dashUntil);
            Burst(player + Vector2.up * .4f, Cyan, 10);
        }
        if (Input.GetMouseButton(0) && clock >= nextShot) FirePlayer();
        previousBottom = player.y;
        velocity.x = clock < dashUntil ? dashFacing * balance.dashSpeed : move * CurrentRole.moveSpeed * (CurrentGun.usesHeat?balance.machinegunMoveScale:1);
        velocity.y = clock < dashUntil ? 0 : velocity.y - balance.gravity * dt;
        player += velocity * dt; player.x = Mathf.Clamp(player.x, .6f, mapWidth - .5f);
        grounded = false;
        if (player.y <= Floor) Land(Floor);
        foreach (Rect p in ledges)
            if (!Input.GetKey(KeyCode.S) && velocity.y <= 0 && previousBottom >= p.yMax && player.y <= p.yMax && player.x + .28f > p.xMin && player.x - .28f < p.xMax) Land(p.yMax);
        if (grounded) coyoteUntil = clock + .1f;
        if (clock < dashUntil) AddSpark(player + Vector2.up * .5f, Vector2.zero, Cyan, .2f);
        cameraX = Mathf.Lerp(cameraX, Mathf.Clamp(player.x - 9, 0, Mathf.Max(0, mapWidth - 1600f / 60)), dt * 5);
        cameraY = Mathf.Lerp(cameraY, Mathf.Clamp(player.y - 5, 0, Mathf.Max(0, mapHeight - 10)), dt * 6); SpawnWave(); UpdateEnemies(dt); UpdateShots(dt); UpdateEffects(dt);
        if (State != Phase.Playing) return;
        UpdateCampaign(dt);
    }

    void Land(float y) { player.y = y; velocity.y = 0; grounded = true; jumps = 0; }
    void SpawnWave()
    {
        if (spawned >= quota || clock < spawnAt || enemies.Count >= 5) return;
        int kind = spawned % 3;
        float x = Mathf.Clamp(player.x + (spawned % 4 == 3 ? -9 : 13), 1, mapWidth - 2);
        float hp = ((kind == 2 ? 95 : 48) + Sector * 12) * EnemyHealthScale;
        enemies.Add(new Foe { p = new Vector2(x, Sector >= 4 ? Mathf.Clamp(player.y + (kind == 1 ? 3 : 0), Floor, mapHeight - 3) : kind == 1 ? 5 : Floor), kind = kind, hp = hp, max = hp, fire = clock + 1.3f * EnemyAttackIntervalScale, phase = spawned });
        Burst(new Vector2(x, Floor + .5f), Pink, 10);
        spawned++; spawnAt = clock + activeLayout.waveInterval;
    }

    void FirePlayer()
    {
        var gun=CurrentGun;
        if (gun.usesHeat && overheated) return;
        nextShot = clock + gun.interval / (1 + rateLevel * .18f);
        int count = gun.pellets + spreadLevel * 2;
        int barrels = IsDualWield ? 2 : 1;
        for (int barrel = 0; barrel < barrels; barrel++)
        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) / 2f) * gun.pelletAngle;
            if (gun.randomSpread>0) angle += UnityEngine.Random.Range(-gun.randomSpread,gun.randomSpread);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * aim;
            shots.Add(new Shot { p = GunMuzzle(barrel), v = dir * gun.speed, life = gun.lifetime, damage = Mathf.RoundToInt(gun.damage * (1 + damageLevel * .25f)), color = GunColors[weapon], pierce = gun.penetration, gun = weapon, barrel = barrel });
        }
        if (gun.usesHeat) { heat = Mathf.Min(1, heat + gun.heatPerShot * barrels); if (heat >= .99f) overheated = true; }
        flashUntil = clock + .075f;
        for (int barrel = 0; barrel < barrels; barrel++) Burst(GunMuzzle(barrel), GunColors[weapon], weapon == 1 ? 8 : 3);
        Sound(fireSound, weapon == 1 ? .13f : .08f);
    }
    void UpdateEnemies(float dt)
    {
        foreach (Foe e in enemies)
        {
            Vector2 target = player + Vector2.up * .5f;
            float dx = player.x - e.p.x;
            if (e.kind == 1) { e.p.x += Mathf.Sign(dx) * 1.2f * dt; e.p.y = (Sector >= 4 ? Mathf.Clamp(player.y + 3, Floor + 2, mapHeight - 3) : 5) + Mathf.Sin(clock * 2 + e.phase) * 1.3f; }
            else if (e.kind == 0 && Mathf.Abs(dx) > 4) e.p.x += Mathf.Sign(dx) * 2.1f * dt;
            else if (e.kind == 3) { e.p.x = Mathf.Clamp(player.x + 10, 18, 38); e.p.y = 5.2f + Mathf.Sin(clock * 1.4f) * 1.5f; }
            if (clock < e.fire) continue;
            Vector2 origin = e.p + Vector2.up * (e.kind == 3 ? 0 : .5f);
            Vector2 dir = (target - origin).normalized;
            int count = e.kind == 3 ? (e.hp < e.max / 2 ? 9 : 5) : e.kind == 2 ? 3 : 1;
            for (int i = 0; i < count; i++)
            {
                Vector2 d = Quaternion.Euler(0, 0, (i - (count - 1) / 2f) * 13) * dir;
                shots.Add(new Shot { p = origin, v = d * (e.kind == 3 ? 6.5f : 7), life = 5, damage = Mathf.RoundToInt((e.kind == 3 ? 16 : 10) * EnemyDamageScale), hostile = true, color = Orange });
            }
            e.fire = clock + (e.kind == 3 ? (e.hp < e.max / 2 ? .85f : 1.3f) : 1.8f) * EnemyAttackIntervalScale;
            Burst(origin, Orange, 3);
        }
    }

    void UpdateShots(float dt)
    {
        for (int i = shots.Count - 1; i >= 0; i--)
        {
            Shot s = shots[i]; Vector2 from = s.p; s.p += s.v * dt; s.traveled += s.v.magnitude * dt; s.life -= dt;
            if (s.hostile)
            {
                if (SegmentDistance(player + Vector2.up * .5f, from, s.p) < .5f)
                { Hurt(s.damage); s.life = 0; }
            }
            else
            {
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    Foe e = enemies[j];
                    if (s.hit.Contains(e) || SegmentDistance(e.p + Vector2.up * (e.kind == 3 ? 0 : .5f), from, s.p) > (e.kind == 3 ? 1.45f : .6f)) continue;
                    int hitDamage = s.gun>=0&&balance.guns[s.gun].distanceFalloff ? Mathf.RoundToInt(s.damage * Mathf.Lerp(1, .25f, Mathf.Clamp01(s.traveled / 12))) : s.damage;
                    bool weakpoint=IsScanned(e)&&s.gun>=0;
                    if(weakpoint) hitDamage=Mathf.RoundToInt(hitDamage*balance.roles[3].scanMultiplier);
                    s.hit.Add(e); e.hp -= hitDamage; Burst(s.p, s.color, 5);
                    AddSpark(s.p, Vector2.up, weakpoint?ClassColors[3]:Color.white, .6f, hitDamage.ToString());
                    if (e.hp <= 0)
                    {
                        Kills++; combo++; bestCombo = Mathf.Max(bestCombo, combo); comboUntil = clock + 3.5f;
                        score += 100 * Mathf.Min(combo, 10); shake = .12f;
                        Burst(e.p + Vector2.up * .5f, Orange, 22); Sound(hitSound, .24f);
                        if (Kills % 3 == 0) pickups.Add(new Pickup { p = new Vector2(e.p.x, Floor + .3f) });
                        enemies.RemoveAt(j);
                    }
                    if (!s.pierce) { s.life = 0; break; }
                }
            }
            if (s.life <= 0 || s.p.y < Floor - .2f || s.p.x < -2 || s.p.x > mapWidth + 4) shots.RemoveAt(i);
        }
    }

    public static float SegmentDistance(Vector2 point, Vector2 a, Vector2 b)
    { Vector2 ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / Mathf.Max(.0001f, ab.sqrMagnitude)); return Vector2.Distance(point, a + t * ab); }
    public void Hurt(int damage)
    {
        if (State != Phase.Playing || clock < immuneUntil) return;
        Health = Mathf.Max(0, Health - damage); immuneUntil = clock + .65f; combo = 0; shake = .3f;
        Burst(player + Vector2.up * .5f, Pink, 12); Sound(hitSound, .25f);
        if (Health == 0) { State = Phase.Defeat; SaveBest(); }
    }
    void UpdateEffects(float dt)
    {
        for (int i = sparks.Count - 1; i >= 0; i--) { var s = sparks[i]; s.life -= dt; s.p += s.v * dt; s.v *= 1 - dt * 2; if (s.life <= 0) sparks.RemoveAt(i); }
        for (int i = pickups.Count - 1; i >= 0; i--)
        {
            Pickup p = pickups[i]; p.life -= dt;
            if (Vector2.Distance(player + Vector2.up * .4f, p.p) < 2.2f) p.p = Vector2.MoveTowards(p.p, player + Vector2.up * .4f, dt * 8);
            if (Vector2.Distance(player + Vector2.up * .4f, p.p) < .6f) { Health = Mathf.Min(MaxHealth, Health + 15); p.life = 0; AddSpark(player + Vector2.up, Vector2.up, Cyan, 1, "+15 HP"); }
            if (p.life <= 0) pickups.RemoveAt(i);
        }
    }
    void OpenUpgrade()
    {
        State = Phase.Upgrade;
        List<int> pool = new List<int> { 0, 1, 2, 3, 4 };
        for (int i = 0; i < 3; i++) { int n = UnityEngine.Random.Range(0, pool.Count); choices[i] = pool[n]; pool.RemoveAt(n); }
    }
    public void ChooseUpgrade(int index)
    {
        if (State != Phase.Upgrade || index < 0 || index > 2) return;
        switch (choices[index])
        {
            case 0: damageLevel++; break;
            case 1: rateLevel++; break;
            case 2: spreadLevel++; break;
            case 3: MaxHealth += 25; Health = Mathf.Min(MaxHealth, Health + 50); break;
            case 4: dashCooldown *= .8f; Health = Mathf.Min(MaxHealth, Health + 20); break;
        }
        Sound(upgradeSound, .2f); BeginSector(Sector + 1);
    }
    void SaveBest() { PlayerPrefs.SetInt("NeonBreakout.Best", Mathf.Max(score, PlayerPrefs.GetInt("NeonBreakout.Best", 0))); PlayerPrefs.Save(); }
    void Burst(Vector2 p, Color color, int count) { for (int i = 0; i < count; i++) AddSpark(p, UnityEngine.Random.insideUnitCircle * 5, color, UnityEngine.Random.Range(.18f, .5f)); }
    void AddSpark(Vector2 p, Vector2 v, Color color, float life, string text = null) { if (sparks.Count < 400) sparks.Add(new Spark { p = p, v = v, color = color, life = life, max = life, text = text }); }
    void Sound(AudioClip clip, float volume) { if (!muted) audioSource.PlayOneShot(clip, volume); }
    AudioClip Tone(float frequency, float duration)
    {
        int count = (int)(22050 * duration); float[] samples = new float[count];
        for (int i = 0; i < count; i++) { float t = i / 22050f; samples[i] = Mathf.Sin(t * frequency * (1 - t / duration * .5f) * Mathf.PI * 2) * (1 - i / (float)count) * .4f; }
        var clip = AudioClip.Create("Synth", count, 1, 22050, false); clip.SetData(samples, 0); return clip;
    }
    void OnApplicationFocus(bool focused) { if (!focused && State == Phase.Playing) State = Phase.Paused; }
    void OnDestroy()
    {
        foreach (Texture2D t in ownedTextures) if (t != null) {if(Application.isPlaying)Destroy(t);else DestroyImmediate(t);}
        foreach (AudioClip clip in new[] { fireSound, hitSound, jumpSound, upgradeSound }) if (clip != null) {if(Application.isPlaying)Destroy(clip);else DestroyImmediate(clip);}
        // Resources-owned fonts are shared assets and must not be destroyed.
        if (uiFont != null && uiFont != Resources.Load<Font>("Fonts/ChineseUI")) {if(Application.isPlaying)Destroy(uiFont);else DestroyImmediate(uiFont);}
    }
}
