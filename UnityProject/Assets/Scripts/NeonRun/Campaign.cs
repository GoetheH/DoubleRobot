using UnityEngine;
using System.Collections.Generic;

public partial class NeonRun
{
    int profession, relics, sequence;
    int selectedDifficulty = 1, runDifficulty = 1;
    int selectedWeapon;

    public void SelectWeapon(int index)
    {
        if (State == Phase.Menu && index >= 0 && index < weaponNames.Length) selectedWeapon = index;
    }
    readonly string[] difficultyNames = { "简单", "中等", "困难" };
    readonly string[] difficultyDescriptions = { "敌人更脆弱，伤害更低，攻击更慢", "标准战斗体验", "敌人更坚韧，伤害更高，攻击更快" };
    float EnemyHealthScale => balance.difficulties[runDifficulty].health;
    float EnemyDamageScale => balance.difficulties[runDifficulty].damage;
    float EnemyAttackIntervalScale => balance.difficulties[runDifficulty].attackInterval;

    public void SelectDifficulty(int index)
    {
        if (State == Phase.Menu && index >= 0 && index < difficultyNames.Length) selectedDifficulty = index;
    }
    float cameraY, skillReady, skillUntil, heat, turretUntil, turretShot;
    Vector2 turret, exit;
    bool overheated;
    float mapWidth = 44, mapHeight = 12;
    string mission;
    readonly List<Vector2> missionItems = new List<Vector2>();
    readonly List<Vector2> switches = new List<Vector2>();
    readonly string[] jobNames = { "突击兵", "医疗兵", "工程师", "侦察兵" };
    readonly string[] gunTraits = { "精准稳定 / 持续输出", "7弹丸扇形 / 近战爆发 / 距离衰减", "高速穿透 / 单发重击 / 低射速", "连续压制 / 高射速 / 过热冷却", "极速连射 / 宽散布 / 短射程" };
    readonly string[] stageNames = { "", "01 / 霓雨下城", "02 / 熔炉生产线", "03 / 核心守卫", "04 / 天线高塔", "05 / 数据回收", "06 / 配电迷阵", "07 / 地下档案", "08 / 中枢停机" };

    void ConfigureSector()
    {
        relics = sequence = 0; missionItems.Clear(); switches.Clear();
        cameraY = 0; heat = 0; overheated = false; turretUntil = skillUntil = 0;
        skillFxUntil = flashUntil = 0;
        skillReady = dashReady = clock; dashUntil = 0;
        ReadAuthoredLevel(Sector);
    }
    bool ObjectiveComplete()
    {
        if(!activeLayout)return false;
        if(activeLayout.goal==StageLayout.Goal.CollectItems)return relics==missionItemTotal;
        if(activeLayout.goal==StageLayout.Goal.Circuit)return CircuitComplete;
        if(activeLayout.goal==StageLayout.Goal.BossAndCircuit)return CircuitComplete&&spawned>=quota&&enemies.Count==0;
        return spawned >= quota && enemies.Count == 0;
    }
    void CheckExit()
    {
        if (State != Phase.Playing || !ObjectiveComplete()) return;
        // The final objective ends the campaign at the player's current position.
        if (activeLayout.autoFinish)
        {
            State = Phase.Victory;
            shots.Clear(); turretUntil = 0;
            SaveBest(); Sound(upgradeSound, .25f);
            return;
        }
        if (Vector2.Distance(player, exit) <= 1.4f) OpenUpgrade();
    }
    void InteractSwitch(int index)
    {
        if (CircuitComplete) return;
        if (index == CircuitOrder[sequence]) { sequence++; Sound(upgradeSound, .12f); }
        else { sequence = 0; AddSpark(player + Vector2.up, Vector2.up, Pink, 2, "顺序错误，已重置"); }
    }
    void UpdateCampaign(float dt)
    {
        heat = Mathf.Max(0, heat - dt * (overheated ? .35f : .18f));
        if (overheated && heat <= .1f) overheated = false;
        for (int i = missionItems.Count - 1; i >= 0; i--)
            if (Vector2.Distance(player + Vector2.up * .5f, missionItems[i]) < 1)
            { missionItems.RemoveAt(i); relics++; Sound(upgradeSound, .18f); }
        if (Input.GetKeyDown(KeyCode.E))
            for (int i = 0; i < switches.Count; i++)
                if (Vector2.Distance(player + Vector2.up * .5f, switches[i]) < 1.7f) { InteractSwitch(i); break; }
        if (Input.GetKeyDown(KeyCode.Q)) UseSkill();
        if (clock < turretUntil && clock >= turretShot && enemies.Count > 0)
        {
            Foe target = enemies[0];
            foreach (Foe e in enemies) if (Vector2.Distance(e.p, turret) < Vector2.Distance(target.p, turret)) target = e;
            turretAim = (target.p + Vector2.up * .5f - turret).normalized;
            shots.Add(new Shot { p = turret, v = turretAim * 30, life = 2, damage = CurrentRole.turretDamage, color = Cyan });
            turretShot = clock + CurrentRole.turretInterval;
        }
        CheckExit();
    }
    void UseSkill()
    {
        if (State != Phase.Playing || clock < skillReady) return;
        skillFxKind = profession; skillFxStarted = clock;
        skillFxUntil = clock + CurrentRole.duration;
        skillReady=clock+CurrentRole.cooldown;
        if (profession == 0) { skillUntil = clock + CurrentRole.duration; }
        if (profession == 1) { Health = Mathf.Min(MaxHealth, Health + CurrentRole.healing); immuneUntil = clock + CurrentRole.duration; }
        if (profession == 2) { turret = player + Vector2.up * .5f; turretUntil = clock + CurrentRole.duration; turretShot = clock; }
        if (profession == 3)
        {
            skillUntil=clock+CurrentRole.duration;
            foreach(Foe foe in enemies)
                if(Vector2.Distance(player+Vector2.up*.5f,foe.p+Vector2.up*(foe.kind==3?0:.5f))<=CurrentRole.scanRadius)
                    foe.scannedUntil=skillUntil;
        }
        Burst(player + Vector2.up * .5f, ClassColors[profession], 20); Sound(upgradeSound, .15f);
    }
    bool IsScanned(Foe foe) { return profession==3 && clock<foe.scannedUntil; }
    void DrawMission()
    {
        foreach (Vector2 item in missionItems)
        {
            Vector2 p = ScreenPoint(item); if (p.y < 240 || p.y > 780) continue;
            DrawCampaignItem(item);
        }
        for (int i = 0; i < switches.Count; i++)
        {
            Vector2 v = switches[i]; Vector2 p = ScreenPoint(v); if (p.y < 250 || p.y > 780) continue;
            DrawCampaignTerminal(i);
        }
        if (State == Phase.Menu) return;
        string activeMission = mission;
        if (Sector == 8)
            activeMission = spawned >= quota && enemies.Count == 0
                ? "守卫已击败 / 关闭反应堆后立即结算："+OrderText()+"（靠近按E）"
                : CircuitComplete ? "反应堆已关闭 / 击败守卫后立即结算" : mission;
        Text(activeMission, 420, 108, 1050, 18, Orange);
        if (missionItems.Count > 0 || relics > 0) Text("已回收 " + relics + " / " + (relics + missionItems.Count), 420, 136, 440, 16, Cyan);
        if (switches.Count > 0) Text("线路 " + sequence + " / "+CircuitOrder.Length+" · "+OrderText(), 420, 136, 520, 16, Cyan);
        Vector2 delta = exit - player;
        if (Sector == 8) Text("终关 / 完成任务即结算", 1320, 112, 270, 16, Cyan);
        else Text("传送门 " + (delta.y > 3 ? "↑" : delta.y < -3 ? "↓" : delta.x > 0 ? "→" : "←") + " " + delta.magnitude.ToString("0") + "m", 1360, 112, 230, 18, ObjectiveComplete() ? Cyan : Muted);
    }

    public void ReturnToMenu()
    {
        State = Phase.Menu; Sector = 0;
        setupPage = SetupPage.Home; skillFxUntil = flashUntil = 0;
        enemies.Clear(); shots.Clear(); sparks.Clear(); pickups.Clear();
        missionItems.Clear(); switches.Clear(); ledges.Clear();
        turretUntil = 0; cameraX = cameraY = 0;
        player = new Vector2(5, Floor); velocity = Vector2.zero;
    }
}
