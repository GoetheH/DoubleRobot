using UnityEngine;

public partial class NeonRun
{
    enum SetupPage { Home, Difficulty, Profession, Weapon }
    SetupPage setupPage;
    float previewStarted;
    readonly string[] roleDescriptions = {
        "前线义体突击手。过载时幻化当前枪械的副本，两处枪口同步发射；结束后恢复单枪。",
        "白绿救援机体。医疗背包、十字标识与轻型护甲；急救同时修复装甲并提供短暂保护。",
        "黄铜施工机体。工程头盔、机械工具臂与能源背包；部署炮台形成交叉火力。",
        "紫色侦察机体。移动更快，扫描施放时14米内的敌人并追踪其弱点，持续6秒；对标记目标的枪械伤害提高50%，可标记Boss。" };
    readonly string[] roleEffects = { "幻化副枪 / 双枪口火光 / 过载能量环", "绿色治疗波 / 上升医疗十字", "蓝色部署网格 / 自动索敌炮台", "紫色雷达扫描 / 弱点锁定框 / 增伤数字" };
    readonly string[] weaponDescriptions = {
        "适合中距离稳定输出。紧凑机匣、能量匣与标准枪托，脉冲弹道清晰可控。",
        "适合近距离爆发。双管枪口与前置泵柄，一次射出七枚弹丸，距离越远伤害越低。",
        "适合远距离点杀。加长磁轨、瞄准镜与支架，重弹可连续穿透多个目标。",
        "适合持续火力压制。厚重散热护罩与大容量弹鼓，连续射击会过热，持枪移动较慢。",
        "适合机动近战。短枪管、折叠枪托与加长弹匣，连射极快，散布较大。" };
    readonly string[] weaponStats = { "基础伤害 16 / 间隔 0.145秒", "每枚伤害 13 / 7弹丸 / 间隔 0.65秒", "基础伤害 100 / 穿透 / 间隔 1.05秒", "基础伤害 11 / 间隔 0.085秒 / 过热冷却", "基础伤害 8 / 间隔 0.065秒 / 短射程" };

    void AdvanceSetup()
    {
        if (State != Phase.Menu) return;
        if (setupPage == SetupPage.Weapon) StartRun();
        else { setupPage++; previewStarted = Time.realtimeSinceStartup; }
    }
    void BackSetup()
    {
        if (State == Phase.Menu && setupPage != SetupPage.Home) setupPage--;
    }
    public void SelectProfession(int index)
    {
        if (State != Phase.Menu || index < 0 || index >= jobNames.Length) return;
        profession = index; previewStarted = Time.realtimeSinceStartup;
    }
    bool SelectCard(Rect r, bool selected, Color accent)
    {
        bool hover = r.Contains(Event.current.mousePosition);
        Box(r.x, r.y, r.width, r.height, selected ? new Color(accent.r * .13f, accent.g * .13f, accent.b * .13f, 1) : new Color(.045f, .07f, .105f));
        Box(r.x, r.y, selected ? 4 : 2, r.height, selected || hover ? accent : Muted * .45f);
        if (selected) Text("已选择", r.x + r.width - 90, r.y + 16, 85, 15, accent);
        return GUI.Button(r, GUIContent.none, GUIStyle.none);
    }
    void DrawDeployment()
    {
        Box(0, 0, 1600, 900, new Color(.018f, .03f, .055f, .98f));
        for (int i = 0; i < 20; i++) Box(40 + i * 84, 0, 1, 900, new Color(.1f, .2f, .25f, .12f));
        if (setupPage == SetupPage.Home)
        {
            Text("NEON BREAKOUT / 重启计划", 80, 83, 800, 20, Cyan, true);
            Text("NEON", 74, 163, 800, 112, Color.white, true);
            Text("BREAKOUT", 74, 280, 900, 112, Cyan, true);
            Text("霓 虹 突 围", 84, 428, 800, 34, Color.white, true);
            Text("雨夜废城，核心失控。\n选择你的机体与武装，穿过八个区域的封锁。", 84, 512, 790, 23, Muted);
            if (Button("开始游戏  /  ENTER →", new Rect(84, 640, 450, 68), Cyan)) AdvanceSetup();
            Text("难度选择 → 职业选择 → 枪械选择 → 出击", 84, 742, 850, 19, Muted);
            Box(1000, 130, 490, 590, new Color(.04f, .075f, .1f));
            DrawModelPreview(profession, new Rect(1090, 225, 290, 360));
            Text(jobNames[profession] + " / " + weaponNames[selectedWeapon], 1040, 624, 440, 23, ClassColors[profession], true);
            Text("原创像素机体 / 五类独立武装", 1040, 672, 440, 17, Muted);
            Text("最高纪录  " + PlayerPrefs.GetInt("NeonBreakout.Best", 0).ToString("000000"), 84, 818, 800, 18, Orange);
            return;
        }
        string[] titles = { "", "选择作战难度", "选择出击职业", "选择携带枪械" };
        Text("DEPLOYMENT / 出击准备", 80, 55, 900, 18, Cyan, true);
        Text(titles[(int)setupPage], 76, 95, 1200, 48, Color.white, true);
        for (int i = 1; i <= 3; i++) Text("0" + i + "  " + titles[i], 80 + (i - 1) * 470, 167, 450, 18, (int)setupPage == i ? Cyan : Muted);
        if (setupPage == SetupPage.Difficulty) DrawDifficultyPage();
        else if (setupPage == SetupPage.Profession) DrawProfessionPage();
        else DrawWeaponPage();
        if (Button("← 返回  /  ESC", new Rect(80, 790, 300, 64), Muted)) BackSetup();
        Text(difficultyNames[selectedDifficulty] + " / " + jobNames[profession] + " / " + weaponNames[selectedWeapon], 430, 808, 700, 20, Cyan);
        if (Button(setupPage == SetupPage.Weapon ? "确认配置并出击 →" : "下一步  /  ENTER →", new Rect(1180, 790, 340, 64), Cyan)) AdvanceSetup();
    }
    void DrawDifficultyPage()
    {
        string[] suggestions = { "推荐初次驾驶 / 更宽容的战斗节奏", "推荐熟悉操作后体验 / 标准挑战", "推荐熟练驾驶员 / 高频火力压迫" };
        string[] health = { "75%", "100%", "135%" }, damage = { "70%", "100%", "135%" }, cadence = { "125% · 更慢", "100% · 标准", "80% · 更快" };
        for (int i = 0; i < 3; i++)
        {
            float x = 80 + i * 490; Color c = i == 2 ? Pink : i == 0 ? Cyan : Orange;
            if (SelectCard(new Rect(x, 240, 460, 450), selectedDifficulty == i, c)) SelectDifficulty(i);
            Text(difficultyNames[i], x + 28, 286, 380, 44, c, true);
            Text(difficultyDescriptions[i], x + 28, 370, 395, 21, Color.white);
            Text("敌人生命   " + health[i], x + 28, 450, 395, 22, Muted);
            Text("敌人伤害   " + damage[i], x + 28, 493, 395, 22, Muted);
            Text("攻击间隔   " + cadence[i], x + 28, 536, 395, 22, Muted);
            Text(suggestions[i], x + 28, 616, 395, 17, c);
        }
        Text("普通敌军与Boss均受难度影响。职业、枪械和任务路线在三档难度中相同。", 84, 718, 1400, 18, Muted);
    }
    void DrawProfessionPage()
    {
        for (int i = 0; i < 4; i++)
        {
            Rect r = new Rect(80 + i % 2 * 260, 230 + i / 2 * 235, 240, 210);
            if (SelectCard(r, profession == i, ClassColors[i])) SelectProfession(i);
            DrawModelPreview(i, new Rect(r.x + 80, r.y + 35, 80, 110));
            Text(jobNames[i], r.x + 23, r.y + 158, 195, 24, ClassColors[i], true);
        }
        Box(650, 230, 870, 680 - 230, new Color(.035f, .065f, .095f));
        DrawModelPreview(profession, new Rect(682, 290, 228, 300));
        float previewAge = Time.realtimeSinceStartup - previewStarted;
        if (previewAge < 2) DrawSkillScreen(profession, new Vector2(800, 465), previewAge / 2, 1);
        Text(jobNames[profession], 955, 260, 520, 38, ClassColors[profession], true);
        Text(roleDescriptions[profession], 955, 328, 515, 20, Color.white);
        Text("主动技能 / Q", 955, 433, 515, 18, Muted, true);
        Text(jobSkills[profession], 955, 467, 515, 21, ClassColors[profession]);
        Text("视觉识别 / " + roleEffects[profession], 955, 551, 515, 17, Muted);
        if (Button("预览技能特效", new Rect(955, 610, 480, 56), ClassColors[profession])) previewStarted = Time.realtimeSinceStartup;
        Text("每种职业均可携带任意一类枪械。职业技能与携带枪械在本局中固定。", 84, 718, 1400, 18, Muted);
    }
    void DrawWeaponPage()
    {
        for (int i = 0; i < weaponNames.Length; i++)
        {
            Rect r = new Rect(80, 228 + i * 96, 510, 82);
            if (SelectCard(r, selectedWeapon == i, GunColors[i])) SelectWeapon(i);
            GUI.DrawTexture(new Rect(r.x + 18, r.y + 18, 120, 43), gunModels[i], ScaleMode.ScaleToFit);
            Text(weaponNames[i], r.x + 160, r.y + 23, 255, 24, selectedWeapon == i ? GunColors[i] : Color.white, true);
        }
        Box(650, 228, 870, 475, new Color(.035f, .065f, .095f));
        Text(weaponNames[selectedWeapon], 700, 260, 760, 38, GunColors[selectedWeapon], true);
        GUI.DrawTexture(new Rect(735, 346, 680, 170), gunModels[selectedWeapon], ScaleMode.ScaleToFit);
        Text(gunTraits[selectedWeapon], 700, 535, 760, 22, GunColors[selectedWeapon], true);
        Text(weaponStats[selectedWeapon], 700, 580, 760, 18, Muted);
        Text(weaponDescriptions[selectedWeapon], 700, 623, 760, 19, Color.white);
        Text("确认后，本局八关只能使用所选枪械；返回主菜单后才可更换。", 84, 729, 1400, 18, Orange);
    }
}
