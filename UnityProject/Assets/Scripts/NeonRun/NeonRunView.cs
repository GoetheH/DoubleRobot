using System.Collections.Generic;
using UnityEngine;

public partial class NeonRun
{
    static readonly Color Cyan = new Color(.22f, .95f, .88f);
    static readonly Color Pink = new Color(1, .25f, .48f);
    static readonly Color Orange = new Color(1, .65f, .25f);
    static readonly Color Ink = new Color(.025f, .045f, .08f);
    static readonly Color Muted = new Color(.44f, .58f, .66f);
    readonly List<Texture2D> ownedTextures = new List<Texture2D>();
    Texture2D robotArt;
    Font uiFont;
    GUIStyle labelStyle;
    float renderCamera;
    string[] weaponNames = { "脉冲步枪", "裂片霰弹", "磁轨狙击枪", "重型机枪", "蜂群冲锋枪" };

    void MakeArt()
    {
        uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 24);
        LoadEditableArt();
    }
    static Matrix4x4 CanvasMatrix(float width, float height)
    {
        float scale = Mathf.Min(width / 1600f, height / 900f);
        return Matrix4x4.TRS(new Vector3((width - 1600 * scale) / 2, (height - 900 * scale) / 2, 0), Quaternion.identity, new Vector3(scale,scale,1));
    }
    void OnGUI()
    {
        if (robotArt == null) return;
        Matrix4x4 original = GUI.matrix;
        GUI.matrix = CanvasMatrix(Screen.width, Screen.height);
        if (labelStyle == null) labelStyle = new GUIStyle(GUI.skin.label) { font = uiFont, wordWrap = true, richText = false };
        renderCamera = cameraX;
        if (State == Phase.Playing && shake > 0) renderCamera += Mathf.Sin(clock * 100) * shake;
        DrawWorld();
        if (State == Phase.Menu) DrawMenu();
        else
        {
            DrawHUD();
            if (State == Phase.Upgrade) DrawUpgrade();
            else if (State == Phase.Paused && !smokeCapture) DrawPause();
            else if (State == Phase.Victory || State == Phase.Defeat) DrawEnd();
        }
        GUI.matrix = original;
        // World textures extend past the logical canvas for scrolling. Keep those
        // pixels out of the letterbox margins on 4:3 and ultrawide windows.
        float canvasScale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
        float marginX=(Screen.width-1600*canvasScale)/2,marginY=(Screen.height-900*canvasScale)/2;
        if(marginY>0) {Box(0,0,Screen.width,marginY,Ink);Box(0,Screen.height-marginY,Screen.width,marginY,Ink);}
        if(marginX>0) {Box(0,0,marginX,Screen.height,Ink);Box(Screen.width-marginX,0,marginX,Screen.height,Ink);}
    }
    void Box(float x, float y, float w, float h, Color color)
    { GUI.color = color; GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture); GUI.color = Color.white; }
    void Text(string text, float x, float y, float width, int size, Color color, bool bold = false)
    { labelStyle.fontSize = size; labelStyle.normal.textColor = color; labelStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; labelStyle.alignment = TextAnchor.UpperLeft; GUI.Label(new Rect(x, y, width, size * 3 + 20), text, labelStyle); }
    bool Button(string text, Rect r, Color color)
    {
        bool hover = r.Contains(Event.current.mousePosition);
        Box(r.x, r.y, r.width, r.height, hover ? color : new Color(color.r * .18f, color.g * .18f, color.b * .18f, .98f));
        Box(r.x, r.y, 3, r.height, color);
        Text(text, r.x + 20, r.y + 15, r.width - 32, 20, hover ? Ink : color, true);
        return GUI.Button(r, GUIContent.none, GUIStyle.none);
    }
    Vector2 ScreenPoint(Vector2 p) { return new Vector2((p.x - renderCamera) * 60, 805 - (p.y - cameraY) * 60); }
    void WorldBox(float x, float y, float w, float h, Color color)
    { Vector2 p = ScreenPoint(new Vector2(x, y + h)); Box(p.x, p.y, w * 60, h * 60, color); }
    void Art(Texture2D art, Vector2 feet, float width, float height, bool flip = false, float alpha = 1)
    {
        Vector2 p = ScreenPoint(feet + Vector2.up * height);
        GUI.color = new Color(1, 1, 1, alpha);
        GUI.DrawTextureWithTexCoords(new Rect(p.x - width * 30, p.y, width * 60, height * 60), art, new Rect(flip ? 1 : 0, 0, flip ? -1 : 1, 1));
        GUI.color = Color.white;
    }
    void DrawWorld()
    {
        Box(0, 0, 1600, 900, new Color(.025f, .04f, .085f));
        if(Sector==1 || Sector==0) DrawSliceEnvironment();
        else DrawCampaignEnvironment();
        DrawAuthoredProps();
        bool open = ObjectiveComplete() && State != Phase.Menu;
        if (Sector != 8 && State != Phase.Menu) DrawSliceGate(open);
        foreach (Pickup p in pickups)
        {
            Vector2 a = ScreenPoint(p.p); Box(a.x - 11, a.y - 11, 22, 22, Ink); Box(a.x - 7, a.y - 2, 14, 4, Cyan); Box(a.x - 2, a.y - 7, 4, 14, Cyan);
        }
        foreach (Foe e in enemies)
        {
            DrawSliceFoe(e);
            if (e.kind != 3) { Vector2 p = ScreenPoint(e.p + Vector2.up * (e.kind==1?1.1f:2.1f)); Box(p.x - 25, p.y, 50, 4, Ink); Box(p.x - 25, p.y, 50 * Mathf.Clamp01(e.hp / e.max), 4, Pink); }
        }
        bool blink = clock < immuneUntil && (int)(clock * 22) % 2 == 0;
        DrawPlayerModel(blink);
        foreach (Shot b in shots)
        {
            Vector2 p = ScreenPoint(b.p);
            Box(p.x - 9, p.y - 7, 18, 14, new Color(b.color.r, b.color.g, b.color.b, .12f));
            Vector2 direction = new Vector2(b.v.x,-b.v.y).normalized;
            Line(p-direction*(b.hostile?5:b.gun==2?32:b.gun==4?10:17),p,b.color,b.hostile?5:b.gun==1?4:3);
        }
        foreach (Spark p in sparks)
        {
            Vector2 pos = ScreenPoint(p.p); Color c = p.color; c.a *= p.life / p.max;
            if (p.text != null) Text(p.text, pos.x, pos.y - 25, 120, 19, c, true);
            else Box(pos.x, pos.y, 4, 4, c);
        }
        Box(0, 805, 1600, 95, Ink);
        if (State == Phase.Playing)
        {
            Vector2 cursor = Event.current.mousePosition;
            Box(cursor.x - 12, cursor.y, 8, 2, Cyan); Box(cursor.x + 5, cursor.y, 8, 2, Cyan);
            Box(cursor.x, cursor.y - 12, 2, 8, Cyan); Box(cursor.x, cursor.y + 5, 2, 8, Cyan);
        }
    }
    void DrawHUD()
    {
        Box(0, 0, 1600, 104, new Color(.025f, .045f, .08f, .97f));
        Box(35, 24, 4, 54, Cyan); Text("NB / " + difficultyNames[runDifficulty], 52, 21, 180, 22, Color.white, true);
        Text("装甲完整度", 52, 53, 160, 14, Muted);
        Box(226, 31, 240, 12, new Color(.12f, .2f, .25f)); Box(226, 31, 240 * Health / Mathf.Max(1f, MaxHealth), 12, Health < 30 ? Pink : Cyan);
        Text(Health + " / " + MaxHealth, 226, 51, 240, 18, Color.white);
        Text(stageNames[Mathf.Clamp(Sector, 0, 8)], 620, 24, 400, 23, Color.white, true);
        Text("已清除 " + Kills.ToString("00") + "    /    本区 " + Mathf.Max(0, quota - spawned + enemies.Count).ToString("00") + " 敌人", 620, 60, 450, 14, Muted);
        Text("积分  " + score.ToString("000000"), 1240, 28, 320, 22, Cyan, true);
        Text("ESC 暂停    M " + (muted ? "声音关" : "声音开"), 1240, 62, 310, 13, Muted);
        Text("A D 移动    SPACE 二段跳    S 下落    SHIFT 冲刺", 36, 825, 580, 17, Muted);
        Text("Q 职业技能    E 机关交互    枪械在出击前选择", 36, 851, 580, 13, Muted);
        Box(620, 825, 610, 52, new Color(.07f, .24f, .25f));
        Box(620, 825, 610, 2, Cyan);
        Text("携带枪械 / " + weaponNames[weapon], 642, 839, 560, 20, Cyan, true);
        Text("本局固定\n返回主菜单可更换", 1270, 830, 280, 16, Muted);
        Text(jobNames[profession] + " [Q] " + (clock >= skillReady ? "就绪" : (skillReady - clock).ToString("0.0") + "s") + "  /  " + (weapon == 3 ? (overheated ? "过热冷却" : "热量 " + (heat * 100).ToString("0") + "%") : gunTraits[weapon]), 620, 881, 960, 13, Cyan);
        DrawMission();
        if(IsDualWield) {Text("双枪过载",46,240,330,30,Orange,true);Text("同步火力 / "+(skillUntil-clock).ToString("0.0")+"s",48,282,330,18,Color.white);}
        if(profession==3&&clock<skillUntil) {int marked=0;foreach(Foe foe in enemies)if(IsScanned(foe))marked++;Text("弱点扫描",46,240,330,30,ClassColors[3],true);Text("标记 "+marked+" / 枪械增伤"+((CurrentRole.scanMultiplier-1)*100).ToString("0")+"% / "+(skillUntil-clock).ToString("0.0")+"s",48,282,420,17,Color.white);}
        if (combo > 1) { Text(combo + " 连击", 46, 140, 300, 40, Orange, true); Text("连续击破提升积分倍率", 49, 193, 300, 15, Muted); }
        if (clock < bannerUntil) { Box(550, 155, 500, 76, new Color(.025f, .07f, .11f, .9f)); Text(stageNames[Sector], 590, 174, 440, 28, Cyan, true); }
        if ((Sector == 3 || Sector == 8) && enemies.Count > 0)
        {
            Foe boss = enemies[0]; Text(boss.hp < boss.max / 2 ? "核心守卫 / 狂暴协议" : "核心守卫 / SENTINEL", 570, 245, 600, 18, Pink, true);
            Box(460, 276, 680, 7, Ink); Box(460, 276, 680 * Mathf.Clamp01(boss.hp / boss.max), 7, Pink);
        }
        if (Sector != 8 && ObjectiveComplete()) Text("任务完成 / 按指引进入传送门", 520, 725, 700, 24, Cyan, true);
    }
    void DrawMenu() { DrawDeployment(); }
    void Overlay(string eyebrow, string title, string description)
    {
        Box(0, 104, 1600, 701, new Color(.015f, .025f, .055f, .95f));
        Text(eyebrow, 130, 175, 1300, 17, Cyan, true);
        Text(title, 125, 220, 1350, 54, Color.white, true);
        Text(description, 130, 310, 1300, 20, Muted);
    }
    void DrawUpgrade()
    {
        Overlay("FIELD UPGRADE / 区域补给", "选择你的进化方向", "选择一个模块，进入下一区域。模块效果可叠加，装甲与武器升级会保留至本局结束。");
        for (int i = 0; i < 3; i++)
        {
            float x = 130 + i * 450; int type = choices[i];
            Box(x, 392, 420, 280, new Color(.055f, .105f, .15f)); Box(x, 392, 420, 3, i == 1 ? Pink : Cyan);
            Text("MODULE / 0" + (i + 1), x + 26, 419, 360, 15, Muted);
            Text(upgradeNames[type], x + 26, 462, 360, 30, Color.white, true);
            Text(upgradeDescriptions[type], x + 26, 520, 360, 19, Muted);
            if (Button("选择模块  [" + (i + 1) + "] →", new Rect(x + 24, 586, 372, 60), i == 1 ? Pink : Cyan)) ChooseUpgrade(i);
        }
    }
    void DrawPause()
    {
        Overlay("SYSTEM / 战术暂停", "喘口气，重新锁定目标。", "切换窗口会自动暂停。按 ESC 或点击下方按钮继续。");
        if (Button("继续突围 →", new Rect(130, 415, 400, 64), Cyan)) State = Phase.Playing;
        if (Button("返回主菜单", new Rect(130, 505, 400, 64), Muted)) ReturnToMenu();
    }
    void DrawEnd()
    {
        bool win = State == Phase.Victory;
        Overlay(win ? "CAMPAIGN COMPLETE / 全部 8 个区域已完成" : "SIGNAL LOST / 信号中断", win ? "通关成功 · 中枢已关闭" : "机体损毁。再启动一次。", win ? "核心守卫已击败，反应堆已停机。你的突围任务完成。" : "装甲耗尽。本次突围结束，可以重新出击或更换职业。");
        int seconds = Mathf.FloorToInt(clock);
        Text("最终积分  " + score.ToString("000000") + "     击破 " + Kills + "     最高连击 " + bestCombo, 130, 372, 1250, 25, Cyan, true);
        Text("职业 " + jobNames[profession] + "     枪械 " + weaponNames[weapon] + "     难度 " + difficultyNames[runDifficulty] + "     用时 " + (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") + "     最高纪录 " + PlayerPrefs.GetInt("NeonBreakout.Best", 0).ToString("000000"), 130, 422, 1250, 19, Muted);
        if (Button("重新出击   [R] →", new Rect(130, 490, 440, 66), Cyan)) StartRun();
        if (Button("返回主菜单   [ESC]", new Rect(130, 585, 440, 64), Muted)) ReturnToMenu();
    }
}
