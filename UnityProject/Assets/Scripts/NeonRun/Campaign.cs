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
    float EnemyHealthScale => runDifficulty == 0 ? .75f : runDifficulty == 2 ? 1.35f : 1f;
    float EnemyDamageScale => runDifficulty == 0 ? .7f : runDifficulty == 2 ? 1.35f : 1f;
    float EnemyAttackIntervalScale => runDifficulty == 0 ? 1.25f : runDifficulty == 2 ? .8f : 1f;

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
    readonly string[] jobSkills = { "双枪过载：幻化副枪，同步开火5秒 / 冷却12秒", "急救：回复40装甲、无敌2秒 / 冷却14秒", "炮台：自动索敌8秒 / 冷却15秒", "弱点扫描：标记14米内敌人6秒，枪械伤害+50% / 冷却14秒" };
    readonly string[] gunTraits = { "精准稳定 / 持续输出", "7弹丸扇形 / 近战爆发 / 距离衰减", "高速穿透 / 单发重击 / 低射速", "连续压制 / 高射速 / 过热冷却", "极速连射 / 宽散布 / 短射程" };
    readonly string[] stageNames = { "", "01 / 霓雨下城", "02 / 熔炉生产线", "03 / 核心守卫", "04 / 天线高塔", "05 / 数据回收", "06 / 配电迷阵", "07 / 地下档案", "08 / 中枢停机" };

    void ConfigureSector()
    {
        relics = sequence = 0; missionItems.Clear(); switches.Clear();
        cameraY = 0; heat = 0; overheated = false; turretUntil = skillUntil = 0;
        skillFxUntil = flashUntil = 0;
        skillReady = dashReady = clock; dashUntil = 0;
        mapWidth = 44; mapHeight = 12; exit = new Vector2(42, Floor);
        mission = "清除敌军后进入传送门";
        if (Sector == 4)
        {
            mapWidth = 24; mapHeight = 32; ledges.Clear();
            for (int i = 0; i < 10; i++) ledges.Add(new Rect(i % 2 == 0 ? 7 : 12, 3.5f + i * 2.5f, 7, .32f));
            exit = new Vector2(15, 26.32f); mission = "攀登高塔，领取天线钥匙后到达顶部";
            missionItems.Add(new Vector2(9, 24.4f));
        }
        if (Sector == 5)
        {
            exit = new Vector2(3, Floor); mission = "取得3份数据，再返回左侧传送门";
            missionItems.Add(new Vector2(10, 4.2f)); missionItems.Add(new Vector2(18, 5.8f)); missionItems.Add(new Vector2(37, 5.9f));
        }
        if (Sector == 6 || Sector == 8)
        {
            exit = new Vector2(22, Sector == 8 ? 5.22f : Floor);
            mission = Sector == 8 ? "击败守卫＋按 II → I → III 关闭反应堆，即可通关（靠近按E）" : "按 II → I → III 接通配电（靠近按E，错误会重置）";
            switches.Add(new Vector2(10, 4.1f)); switches.Add(new Vector2(18, 5.7f)); switches.Add(new Vector2(37, 5.8f));
            if (Sector == 8) ledges.Add(new Rect(20, 4.9f, 5, .32f));
        }
        if (Sector == 7)
        {
            mapWidth = 26; mapHeight = 30; ledges.Clear();
            for (int i = 0; i < 9; i++) ledges.Add(new Rect(i % 2 == 0 ? 4 : 13, 3.5f + i * 2.5f, 8, .32f));
            player = new Vector2(8, 23.82f); exit = new Vector2(5, Floor);
            mission = "下行收集2份档案，进入底层左侧传送门（S穿过平台）";
            missionItems.Add(new Vector2(7, 14.1f)); missionItems.Add(new Vector2(17, 6.6f));
        }
    }
    bool ObjectiveComplete()
    {
        if (Sector == 4) return relics == 1;
        if (Sector == 5) return relics == 3;
        if (Sector == 6) return sequence == 3;
        if (Sector == 7) return relics == 2;
        if (Sector == 8) return sequence == 3 && spawned >= quota && enemies.Count == 0;
        return spawned >= quota && enemies.Count == 0;
    }
    void CheckExit()
    {
        if (State != Phase.Playing || !ObjectiveComplete()) return;
        // The final objective ends the campaign at the player's current position.
        if (Sector == 8)
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
        if (sequence >= 3) return;
        int[] order = { 1, 0, 2 };
        if (index == order[sequence]) { sequence++; Sound(upgradeSound, .12f); }
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
            shots.Add(new Shot { p = turret, v = turretAim * 30, life = 2, damage = 22, color = Cyan });
            turretShot = clock + .22f;
        }
        CheckExit();
    }
    void UseSkill()
    {
        if (State != Phase.Playing || clock < skillReady) return;
        skillFxKind = profession; skillFxStarted = clock;
        skillFxUntil = clock + (profession == 0 ? 5 : profession == 1 ? 2 : profession == 2 ? 8 : 6);
        if (profession == 0) { skillUntil = clock + 5; skillReady = clock + 12; }
        if (profession == 1) { Health = Mathf.Min(MaxHealth, Health + 40); immuneUntil = clock + 2; skillReady = clock + 14; }
        if (profession == 2) { turret = player + Vector2.up * .5f; turretUntil = clock + 8; turretShot = clock; skillReady = clock + 15; }
        if (profession == 3)
        {
            skillUntil=clock+6;skillReady=clock+14;
            foreach(Foe foe in enemies)
                if(Vector2.Distance(player+Vector2.up*.5f,foe.p+Vector2.up*(foe.kind==3?0:.5f))<=14)
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
                ? "守卫已击败 / 关闭反应堆后立即结算：II → I → III（靠近按E）"
                : sequence == 3 ? "反应堆已关闭 / 击败守卫后立即结算" : mission;
        Text(activeMission, 420, 108, 1050, 18, Orange);
        if (missionItems.Count > 0 || relics > 0) Text("已回收 " + relics + " / " + (relics + missionItems.Count), 420, 136, 440, 16, Cyan);
        if (switches.Count > 0) Text("线路 " + sequence + " / 3 · II → I → III", 420, 136, 520, 16, Cyan);
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
