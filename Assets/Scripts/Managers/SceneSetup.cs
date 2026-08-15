using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class SceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetup = true;

    private GameObject playerObj;
    private GameObject enemyObj;
    private Camera mainCamera;

    private void Start()
    {
        if (autoSetup) SetupScene();
    }

    public void SetupScene()
    {
        SafeSetup("SetupCamera", SetupCamera);
        SafeSetup("SetupBackground", SetupBackground);
        SafeSetup("SetupGameManager", SetupGameManager);
        SafeSetup("SetupGroundBuildings", SetupGroundBuildings);
        SafeSetup("SetupUI", SetupUI);
        SafeSetup("SetupPlayer", SetupPlayer);
        SafeSetup("SetupEnemy", SetupEnemy);
        SafeSetup("AssignHealthBars", AssignHealthBars);
    }

    private static void SafeSetup(string name, Action action)
    {
        try { action(); Debug.Log($"[SceneSetup] {name} OK"); }
        catch (Exception e) { Debug.LogError($"[SceneSetup] {name} FAILED: {e.Message}\n{e.StackTrace}"); }
    }

    // ============ Camera ============
    private void SetupCamera()
    {
        GameObject camObj = GameObject.Find("Main Camera");
        if (camObj == null) camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        mainCamera = camObj.GetComponent<Camera>();
        if (mainCamera == null) mainCamera = camObj.AddComponent<Camera>();
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.transform.position = new Vector3(0, 0, -10);
        mainCamera.backgroundColor = new Color(0.05f, 0.04f, 0.12f, 1f); // deep dark blue sky
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        CameraFollow cf = camObj.GetComponent<CameraFollow>();
        if (cf == null) cf = camObj.AddComponent<CameraFollow>();
        cf.offset = new Vector3(0, 0, -10);
    }

    // ============ Starry Sky + Meteor Background ============
    private void SetupBackground()
    {
        Camera cam = Camera.main;
        float camHeight = cam != null ? cam.orthographicSize * 2f : 10f;
        float camWidth = camHeight * (cam != null ? cam.aspect : 1.78f);

        // Dark sky base layer — 跟随相机大小
        GameObject sky = new GameObject("Sky");
        SpriteRenderer sr = sky.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite(new Color(0.06f, 0.05f, 0.14f, 1f));
        sr.sortingOrder = -20;
        sky.transform.localScale = new Vector3(camWidth + 2, camHeight + 2, 1);
        // 添加缩放跟随脚本
        BackgroundScaler skyScaler = sky.AddComponent<BackgroundScaler>();
        skyScaler.padding = 2f;

        // Star field (random static stars)
        GameObject stars = new GameObject("Stars");
        SpriteRenderer starsR = stars.AddComponent<SpriteRenderer>();
        starsR.sprite = GenerateStarsTexture(256, 128, 220);
        starsR.drawMode = SpriteDrawMode.Tiled;
        starsR.size = new Vector2(camWidth, camHeight);
        starsR.sortingOrder = -19;
        BackgroundScaler starScaler = stars.AddComponent<BackgroundScaler>();
        starScaler.useTiledSize = true;
        starScaler.padding = 0f;

        // Meteors (shooting stars) animated
        SpawnMeteors(6);

        // Ground layer — 宽度跟随相机，带实体碰撞
        GameObject ground = new GameObject("Ground");
        SpriteRenderer gr = ground.AddComponent<SpriteRenderer>();
        gr.sprite = SolidSprite(new Color(0.22f, 0.2f, 0.25f, 1f));
        gr.sortingOrder = -15;
        ground.transform.position = new Vector3(0, -3.5f, 0);
        ground.transform.localScale = new Vector3(camWidth + 2, 1.5f, 1);
        // 实体地面碰撞
        BoxCollider2D groundCol = ground.AddComponent<BoxCollider2D>();
        groundCol.size = new Vector2(1, 1);
        BackgroundScaler groundScaler = ground.AddComponent<BackgroundScaler>();
        groundScaler.isGround = true;
        groundScaler.padding = 2f;

        // Top subtle ground line
        GameObject groundLine = new GameObject("GroundLine");
        SpriteRenderer gl = groundLine.AddComponent<SpriteRenderer>();
        gl.sprite = SolidSprite(new Color(0.28f, 0.26f, 0.32f, 1f));
        gl.sortingOrder = -14;
        groundLine.transform.position = new Vector3(0, -2.72f, 0);
        groundLine.transform.localScale = new Vector3(camWidth + 2, 0.08f, 1);
        BackgroundScaler glScaler = groundLine.AddComponent<BackgroundScaler>();
        glScaler.isGround = true;
        glScaler.groundLineThickness = 0.08f;
        glScaler.padding = 2f;

        // 左右实体墙 — 防止机甲掉出场景
        CreateSideWall(camWidth + 2, camHeight + 2, true);
        CreateSideWall(camWidth + 2, camHeight + 2, false);
        // 顶部实体墙 — 防止跳跃飞出场景
        CreateTopWall(camWidth + 2, camHeight + 2);
    }

    private void CreateTopWall(float width, float height)
    {
        GameObject wall = new GameObject("Wall_Top");
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite(new Color(0.15f, 0.13f, 0.18f, 0.9f));
        sr.sortingOrder = -16;

        float wallThickness = 0.3f;
        // 放在血条正下方：相机顶部(orthographicSize) - 血条区域偏移
        Camera cam = Camera.main;
        float camTop = cam != null ? cam.orthographicSize : 5f;
        float topY = camTop - 0.6f; // 紧贴血条下方

        wall.transform.position = new Vector3(0, topY, 0);
        wall.transform.localScale = new Vector3(width, wallThickness, 1);

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        BackgroundScaler sc = wall.AddComponent<BackgroundScaler>();
        sc.isTopWall = true;
        sc.sideWallThickness = wallThickness;
        sc.topWallYOffset = 0.6f;
        sc.padding = 2f;
    }

    private void CreateSideWall(float width, float height, bool isLeft)
    {
        GameObject wall = new GameObject(isLeft ? "Wall_Left" : "Wall_Right");
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite(new Color(0.15f, 0.13f, 0.18f, 0.9f));
        sr.sortingOrder = -16;

        float wallThickness = 1f;
        float halfArena = width / 2f;
        // 墙的顶部位于相机高度以上一点，底部与地面下沿重合
        float topY = -2.7f + (height / 2f);
        float bottomY = -4.25f; // 地面下沿
        float wallHeight = topY - bottomY;
        float centerY = (topY + bottomY) / 2f;

        wall.transform.position = new Vector3(
            isLeft ? -halfArena : halfArena,
            centerY,
            0);
        wall.transform.localScale = new Vector3(wallThickness, wallHeight, 1);

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        // 同样跟随相机缩放
        BackgroundScaler sc = wall.AddComponent<BackgroundScaler>();
        sc.isSideWall = true;
        sc.sideWallThickness = wallThickness;
        sc.sideWallHeight = wallHeight;
        sc.sideWallIsLeft = isLeft;
        sc.padding = 2f;
    }

    private void SpawnMeteors(int count)
    {
        GameObject parent = new GameObject("Meteors");
        for (int i = 0; i < count; i++)
        {
            GameObject m = new GameObject("Meteor" + i);
            m.transform.SetParent(parent.transform, false);
            SpriteRenderer r = m.AddComponent<SpriteRenderer>();
            r.sprite = SolidSprite(Color.white);
            r.sortingOrder = -17;
            // Size varies
            float len = 0.4f + UnityEngine.Random.value * 0.5f;
            float thick = 0.02f;
            m.transform.localScale = new Vector3(len, thick, 1);
            m.transform.rotation = Quaternion.Euler(0, 0, -30 - UnityEngine.Random.value * 20);
            MeteorDrift drift = m.AddComponent<MeteorDrift>();
            drift.startX = 5f + UnityEngine.Random.value * 5f;
            drift.startY = 3f + UnityEngine.Random.value * 2f;
            drift.speed = 3f + UnityEngine.Random.value * 2f;
            drift.delay = UnityEngine.Random.value * 3.5f;
            drift.ResetPos();
        }
    }

    // ============ Yellow Arena Frame ============
    private void SetupArenaFrame()
    {
        Canvas canvas = GetOrCreateCanvas();
        GameObject frame = new GameObject("ArenaFrame");
        frame.transform.SetParent(canvas.transform, false);
        RectTransform rt = frame.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1000, 580);
        rt.anchoredPosition = new Vector2(0, 60);
        CreateFrameBorder(rt, new Color(1f, 0.85f, 0.15f, 1f), 14f);
    }

    private void CreateFrameBorder(RectTransform parent, Color color, float thickness)
    {
        // Top
        RectTransform top = MakeBar(parent, "Top", color);
        top.anchorMin = new Vector2(0, 1); top.anchorMax = new Vector2(1, 1);
        top.pivot = new Vector2(0.5f, 1);
        top.sizeDelta = new Vector2(0, thickness);
        top.anchoredPosition = Vector2.zero;

        // Bottom
        RectTransform bot = MakeBar(parent, "Bot", color);
        bot.anchorMin = new Vector2(0, 0); bot.anchorMax = new Vector2(1, 0);
        bot.pivot = new Vector2(0.5f, 0);
        bot.sizeDelta = new Vector2(0, thickness);
        bot.anchoredPosition = Vector2.zero;

        // Left
        RectTransform left = MakeBar(parent, "Left", color);
        left.anchorMin = new Vector2(0, 0); left.anchorMax = new Vector2(0, 1);
        left.pivot = new Vector2(0, 0.5f);
        left.sizeDelta = new Vector2(thickness, 0);
        left.anchoredPosition = Vector2.zero;

        // Right
        RectTransform right = MakeBar(parent, "Right", color);
        right.anchorMin = new Vector2(1, 0); right.anchorMax = new Vector2(1, 1);
        right.pivot = new Vector2(1, 0.5f);
        right.sizeDelta = new Vector2(thickness, 0);
        right.anchoredPosition = Vector2.zero;
    }

    private RectTransform MakeBar(RectTransform parent, string name, Color color)
    {
        Image img = MakeImage(parent, name, color);
        return (RectTransform)img.transform;
    }

    // ============ Ground & Buildings (decoration blocks) ============
    private void SetupGroundBuildings()
    {
        // Buildings - left group for player side
        MakeBuilding("BuildingL1", new Vector3(-4.2f, -3.2f, 0), new Vector2(1.0f, 2.0f),
            new Color(0.12f, 0.11f, 0.15f), new Color(0.9f, 0.75f, 0.25f));
        MakeBuilding("BuildingL2", new Vector3(-2.8f, -3.4f, 0), new Vector2(0.7f, 1.3f),
            new Color(0.12f, 0.11f, 0.15f), new Color(0.9f, 0.75f, 0.25f));

        // Buildings - right group for enemy side
        MakeBuilding("BuildingR1", new Vector3(4.2f, -3.2f, 0), new Vector2(1.0f, 2.0f),
            new Color(0.12f, 0.11f, 0.15f), new Color(0.9f, 0.75f, 0.25f));
        MakeBuilding("BuildingR2", new Vector3(2.8f, -3.4f, 0), new Vector2(0.7f, 1.3f),
            new Color(0.12f, 0.11f, 0.15f), new Color(0.9f, 0.75f, 0.25f));
    }

    private void MakeBuilding(string name, Vector3 pos, Vector2 size, Color bodyColor, Color windowColor)
    {
        GameObject b = new GameObject(name);
        b.transform.position = pos;
        SpriteRenderer body = b.AddComponent<SpriteRenderer>();
        body.sprite = SolidSprite(bodyColor);
        body.sortingOrder = -10;
        b.transform.localScale = new Vector3(size.x, size.y, 1);

        // Small window on body (as a child sprite with color)
        GameObject w = new GameObject("Window");
        w.transform.SetParent(b.transform, false);
        SpriteRenderer wr = w.AddComponent<SpriteRenderer>();
        wr.sprite = SolidSprite(windowColor);
        wr.sortingOrder = -9;
        w.transform.localScale = new Vector3(0.1f / size.x, 0.08f / size.y, 1);
        w.transform.localPosition = new Vector3(0.3f, 0.3f, 0);
    }

    // ============ Game Manager ============
    private void SetupGameManager()
    {
        GameObject gmObj = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        GameManager gm = gmObj.GetComponent<GameManager>() ?? gmObj.AddComponent<GameManager>();
    }

    // ============ UI (Healthbars + Control Guide) ============
    private Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
        {
            try { font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "LegacyRuntime" }, 14); }
            catch { }
        }
        return font;
    }

    private Canvas GetOrCreateCanvas()
    {
        Canvas c = FindObjectOfType<Canvas>();
        if (c != null) return c;
        GameObject co = new GameObject("Canvas");
        c = co.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler s = co.AddComponent<CanvasScaler>();
        s.referenceResolution = new Vector2(1920, 1080);
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.matchWidthOrHeight = 0.5f;
        co.AddComponent<GraphicRaycaster>();
        return c;
    }

    private void SetupUI()
    {
        Canvas canvas = GetOrCreateCanvas();
        Font font = GetDefaultFont();

        // 确保EventSystem存在（按钮点击需要）
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // UIManager (game over)
        GameObject uiObj = new GameObject("UIManager");
        uiObj.transform.SetParent(canvas.transform, false);
        UIManager uiManager = uiObj.AddComponent<UIManager>();

        // Player A (blue - left) health — Y为负值，从顶部向下偏移
        HealthBar playerBar = CreateHealthBarWithNumber(
            canvas.transform, "机甲A", new Vector2(-300, -30),
            new Color(0.15f, 0.7f, 1f), new Color(0.0f, 0.45f, 0.85f), font, out GameObject pbObj);
        pbObj.name = "PlayerHealthBar";

        // Enemy B (red - right) health
        HealthBar enemyBar = CreateHealthBarWithNumber(
            canvas.transform, "机甲B", new Vector2(300, -30),
            new Color(1f, 0.3f, 0.35f), new Color(0.8f, 0.1f, 0.2f), font, out GameObject ebObj);
        ebObj.name = "EnemyHealthBar";

        // 开始页面
        GameObject startPage = CreateStartPage(canvas.transform, font);
        uiManager.startPage = startPage;
        Transform sb = startPage.transform.Find("StartButton");
        if (sb != null) uiManager.startButton = sb.GetComponent<Button>();

        // 模式选择页面
        GameObject selectModePage = CreateSelectModePage(canvas.transform, font);
        uiManager.selectModePage = selectModePage;
        Transform spBtn = selectModePage.transform.Find("SinglePlayerButton");
        if (spBtn != null) uiManager.singlePlayerButton = spBtn.GetComponent<Button>();
        Transform tpBtn = selectModePage.transform.Find("TwoPlayerButton");
        if (tpBtn != null) uiManager.twoPlayerButton = tpBtn.GetComponent<Button>();

        // 胜利页面（标题动态设置，初始文本占位）
        GameObject victoryPage = CreateEndPage(canvas.transform, font, "胜  利", new Color(0.15f, 0.7f, 0.2f), "VictoryPage");
        uiManager.victoryPage = victoryPage;
        Transform vTitle = victoryPage.transform.Find("Title");
        if (vTitle != null) uiManager.victoryTitleText = vTitle.GetComponent<Text>();
        Transform vRetry = victoryPage.transform.Find("RetryButton");
        if (vRetry != null) uiManager.victoryRetryButton = vRetry.GetComponent<Button>();
        Transform vReturn = victoryPage.transform.Find("ReturnButton");
        if (vReturn != null) uiManager.victoryReturnButton = vReturn.GetComponent<Button>();

        // 失败页面
        GameObject defeatPage = CreateEndPage(canvas.transform, font, "失  败", new Color(0.8f, 0.2f, 0.2f), "DefeatPage");
        uiManager.defeatPage = defeatPage;
        Transform dRetry = defeatPage.transform.Find("RetryButton");
        if (dRetry != null) uiManager.defeatRetryButton = dRetry.GetComponent<Button>();
        Transform dReturn = defeatPage.transform.Find("ReturnButton");
        if (dReturn != null) uiManager.defeatReturnButton = dReturn.GetComponent<Button>();

        GameManager.Instance.uiManager = uiManager;

        HealthBarAssigner assigner = GameManager.Instance.GetComponent<HealthBarAssigner>()
            ?? GameManager.Instance.gameObject.AddComponent<HealthBarAssigner>();
        assigner.playerHealthBar = playerBar;
        assigner.enemyHealthBar = enemyBar;

        // Bottom control guide
        CreateControlGuidePanel(canvas.transform, font);
    }

    private HealthBar CreateHealthBarWithNumber(Transform parent, string labelText, Vector2 anchorPos,
        Color fillColor, Color frameColor, Font font, out GameObject rootObj)
    {
        GameObject root = new GameObject("HealthBar_" + labelText);
        root.transform.SetParent(parent, false);
        RectTransform rr = root.AddComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.5f, 1);
        rr.anchorMax = new Vector2(0.5f, 1);
        rr.pivot = new Vector2(0.5f, 1);
        rr.anchoredPosition = anchorPos;
        rr.sizeDelta = new Vector2(280, 52);
        rootObj = root;

        // Label (机甲A / 机甲B)
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        Text label = labelGo.AddComponent<Text>();
        label.text = labelText;
        label.font = font;
        label.fontSize = 16;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        RectTransform lr = (RectTransform)labelGo.transform;
        lr.anchorMin = new Vector2(0, 1); lr.anchorMax = new Vector2(1, 1);
        lr.pivot = new Vector2(0.5f, 1);
        lr.anchoredPosition = new Vector2(0, -4);
        lr.sizeDelta = new Vector2(0, 20);

        // Frame (colored border around bar)
        Image frameImg = MakeImage(root.transform, "Frame", frameColor);
        RectTransform fr = (RectTransform)frameImg.transform;
        fr.anchorMin = new Vector2(0, 0); fr.anchorMax = new Vector2(1, 0);
        fr.pivot = new Vector2(0.5f, 0);
        fr.anchoredPosition = new Vector2(0, 0);
        fr.sizeDelta = new Vector2(0, 24);
        GameObject frameGo = frameImg.gameObject;

        // Inner track
        Image track = MakeImage(frameGo.transform, "Track", new Color(0.05f, 0.05f, 0.08f, 0.9f));
        RectTransform tr = (RectTransform)track.transform;
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(4, 4); tr.offsetMax = new Vector2(-4, -4);

        // Fill — 用Simple类型 + anchorMax.x控制宽度（比Filled可靠）
        Image fillImg = MakeImage(track.transform, "Fill", fillColor);
        fillImg.type = Image.Type.Simple;
        RectTransform frr = (RectTransform)fillImg.transform;
        frr.anchorMin = Vector2.zero;
        frr.anchorMax = Vector2.one;
        frr.offsetMin = Vector2.zero;
        frr.offsetMax = Vector2.zero;

        // HP number text on top of bar
        GameObject hpTextGo = new GameObject("HPText");
        hpTextGo.transform.SetParent(frameGo.transform, false);
        Text hpText = hpTextGo.AddComponent<Text>();
        hpText.font = font;
        hpText.fontSize = 14;
        hpText.alignment = TextAnchor.MiddleCenter;
        hpText.color = Color.white;
        hpText.fontStyle = FontStyle.Bold;
        RectTransform hptr = (RectTransform)hpTextGo.transform;
        hptr.anchorMin = Vector2.zero; hptr.anchorMax = Vector2.one;
        hptr.offsetMin = Vector2.zero; hptr.offsetMax = Vector2.zero;

        HealthBar hb = root.AddComponent<HealthBar>();
        hb.fill = fillImg;
        hb.label = labelText;
        hb.hpText = hpText;
        hb.SetFillColor(fillColor);

        return hb;
    }

    private GameObject CreateStartPage(Transform parent, Font font)
    {
        GameObject panel = new GameObject("StartPage");
        panel.transform.SetParent(parent, false);

        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = WhiteSprite;
        panelImg.color = new Color(0.04f, 0.03f, 0.1f, 0.92f);
        RectTransform pr = (RectTransform)panel.transform;
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
        pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;

        // 标题
        CreatePageText(panel.transform, "Title", "机甲对战", font, 64,
            new Color(0.3f, 0.9f, 1f), new Vector2(0, 120), new Vector2(600, 100));

        // 副标题
        CreatePageText(panel.transform, "Subtitle", "PIXEL ROBOT BATTLE", font, 24,
            new Color(0.6f, 0.6f, 0.7f), new Vector2(0, 50), new Vector2(600, 40));

        // 操作提示
        CreatePageText(panel.transform, "Hint", "玩家1: A/D移动  W跳跃  左键射击  右键防御\n玩家2: ←/→移动  ↑跳跃  L射击  K防御", font, 18,
            new Color(0.8f, 0.8f, 0.85f), new Vector2(0, -10), new Vector2(750, 50));

        // 开始按钮
        GameObject btn = CreatePageButton(panel.transform, "StartButton", "开 始 游 戏",
            font, 32, new Color(0.15f, 0.7f, 0.3f), new Vector2(0, -100), new Vector2(280, 70));

        panel.SetActive(false);
        return panel;
    }

    private GameObject CreateSelectModePage(Transform parent, Font font)
    {
        GameObject panel = new GameObject("SelectModePage");
        panel.transform.SetParent(parent, false);

        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = WhiteSprite;
        panelImg.color = new Color(0.04f, 0.03f, 0.1f, 0.92f);
        RectTransform pr = (RectTransform)panel.transform;
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
        pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;

        // 标题
        CreatePageText(panel.transform, "Title", "选择模式", font, 56,
            new Color(0.3f, 0.9f, 1f), new Vector2(0, 120), new Vector2(600, 100));

        // 单人模式按钮
        CreatePageButton(panel.transform, "SinglePlayerButton", "单人模式\n（人机对战）",
            font, 26, new Color(0.15f, 0.65f, 0.9f), new Vector2(-160, -40), new Vector2(260, 100));

        // 双人模式按钮
        CreatePageButton(panel.transform, "TwoPlayerButton", "双人模式\n（双人对战）",
            font, 26, new Color(0.9f, 0.4f, 0.3f), new Vector2(160, -40), new Vector2(260, 100));

        panel.SetActive(false);
        return panel;
    }

    private GameObject CreateEndPage(Transform parent, Font font, string titleText, Color titleColor, string pageName)
    {
        GameObject panel = new GameObject(pageName);
        panel.transform.SetParent(parent, false);

        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = WhiteSprite;
        panelImg.color = new Color(0.04f, 0.03f, 0.1f, 0.92f);
        RectTransform pr = (RectTransform)panel.transform;
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
        pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;

        // 标题（胜利/失败）
        CreatePageText(panel.transform, "Title", titleText, font, 72,
            titleColor, new Vector2(0, 100), new Vector2(600, 120));

        // 再来一次按钮
        CreatePageButton(panel.transform, "RetryButton", "再 来 一 次",
            font, 28, new Color(0.15f, 0.65f, 0.9f), new Vector2(-130, -60), new Vector2(220, 64));

        // 返回开始页面按钮
        CreatePageButton(panel.transform, "ReturnButton", "返回开始页面",
            font, 28, new Color(0.6f, 0.6f, 0.65f), new Vector2(130, -60), new Vector2(220, 64));

        panel.SetActive(false);
        return panel;
    }

    private void CreatePageText(Transform parent, string name, string content, Font font,
        int fontSize, Color color, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content;
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false;
        RectTransform r = (RectTransform)obj.transform;
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    private GameObject CreatePageButton(Transform parent, string name, string label,
        Font font, int fontSize, Color bgColor, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.sprite = WhiteSprite;
        btnImg.color = bgColor;
        Button btn = btnObj.AddComponent<Button>();
        RectTransform r = (RectTransform)btnObj.transform;
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;

        // 按钮文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.fontStyle = FontStyle.Bold;
        RectTransform tr = (RectTransform)textObj.transform;
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

        return btnObj;
    }

    // ============ Bottom Control Guide ============
    private void CreateControlGuidePanel(Transform parent, Font font)
    {
        GameObject root = new GameObject("ControlGuide");
        root.transform.SetParent(parent, false);
        RectTransform rr = root.AddComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.5f, 0);
        rr.anchorMax = new Vector2(0.5f, 0);
        rr.pivot = new Vector2(0.5f, 0);
        rr.anchoredPosition = new Vector2(0, 10);
        rr.sizeDelta = new Vector2(1000, 120);

        // Split left / right panels for A and B
        RectTransform panelA = CreateKeyPanel(root.transform, "机甲A", new Vector2(-260, 0), new Color(0.15f, 0.45f, 0.85f, 0.5f), font, true);
        RectTransform panelB = CreateKeyPanel(root.transform, "机甲B", new Vector2(260, 0), new Color(0.85f, 0.2f, 0.25f, 0.5f), font, false);
    }

    private RectTransform CreateKeyPanel(Transform parent, string title, Vector2 pos, Color accent, Font font, bool isA)
    {
        GameObject go = new GameObject("Panel_" + title);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(480, 110);

        // Title
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(go.transform, false);
        Text titleT = titleGo.AddComponent<Text>();
        titleT.text = title;
        titleT.font = font;
        titleT.fontSize = 18;
        titleT.color = Color.white;
        titleT.fontStyle = FontStyle.Bold;
        titleT.alignment = TextAnchor.MiddleCenter;
        RectTransform tr = (RectTransform)titleGo.transform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1);
        tr.anchoredPosition = new Vector2(0, -6);
        tr.sizeDelta = new Vector2(0, 22);

        // Key grid: row of keycaps + label
        string[] keys = isA
            ? new[] { "W", "A", "S", "D", "攻击", "防御" }
            : new[] { "↑", "←", "↓", "→", "攻击", "防御" };
        string[] descriptions = isA
            ? new[] { "", "", "", "", "左键", "右键" }
            : new[] { "", "", "", "", "→(I)", "↓(K)" }; // AI追击，但保留标识说明
        Color keyBg = new Color(accent.r, accent.g, accent.b, 0.85f);

        // first 4 = movement keys, 2 = action
        float startX = -200f;
        float spacing = 68f;
        // Movement 4 keys
        for (int i = 0; i < 4; i++)
        {
            float x = startX + i * spacing;
            MakeKeyCap(go.transform, keys[i], new Vector2(x, -8), new Vector2(44, 44), keyBg, font);
        }
        // Move label
        GameObject moveLabel = new GameObject("MoveLabel");
        moveLabel.transform.SetParent(go.transform, false);
        Text ml = moveLabel.AddComponent<Text>();
        ml.text = isA ? "A/D 移动  W 跳跃" : "←/→ 移动  ↑ 跳跃";
        ml.font = font; ml.fontSize = 13; ml.color = Color.white; ml.alignment = TextAnchor.MiddleCenter;
        RectTransform mlr = (RectTransform)moveLabel.transform;
        mlr.anchorMin = new Vector2(0.5f, 0); mlr.anchorMax = new Vector2(0.5f, 0);
        mlr.pivot = new Vector2(0.5f, 0);
        mlr.anchoredPosition = new Vector2(-120, -22);
        mlr.sizeDelta = new Vector2(180, 20);

        // Action 1 key
        MakeKeyCap(go.transform, keys[4], new Vector2(80, -8), new Vector2(72, 44), keyBg, font);
        MakeKeyCap(go.transform, keys[5], new Vector2(180, -8), new Vector2(72, 44), keyBg, font);
        // Action labels
        MakeTextUnderKey(go.transform, descriptions[4], new Vector2(80, -40), font);
        MakeTextUnderKey(go.transform, descriptions[5], new Vector2(180, -40), font);

        // AI hint for B (small text)
        if (!isA)
        {
            GameObject hint = new GameObject("AIHint");
            hint.transform.SetParent(go.transform, false);
            Text ht = hint.AddComponent<Text>();
            ht.text = "※ 机甲B自动追击";
            ht.font = font; ht.fontSize = 12;
            ht.color = new Color(1f, 0.8f, 0.8f);
            ht.alignment = TextAnchor.MiddleRight;
            RectTransform htr = (RectTransform)hint.transform;
            htr.anchorMin = new Vector2(1, 0); htr.anchorMax = new Vector2(1, 0);
            htr.pivot = new Vector2(1, 0);
            htr.anchoredPosition = new Vector2(-10, -60);
            htr.sizeDelta = new Vector2(200, 18);
        }

        return r;
    }

    private void MakeKeyCap(Transform parent, string text, Vector2 pos, Vector2 size, Color bg, Font font)
    {
        GameObject k = new GameObject("Key_" + text);
        k.transform.SetParent(parent, false);
        Image img = k.AddComponent<Image>();
        img.sprite = WhiteSprite;
        img.color = bg;
        RectTransform kr = (RectTransform)k.transform;
        kr.anchorMin = new Vector2(0.5f, 0.5f);
        kr.anchorMax = new Vector2(0.5f, 0.5f);
        kr.pivot = new Vector2(0.5f, 0.5f);
        kr.anchoredPosition = pos;
        kr.sizeDelta = size;

        GameObject t = new GameObject("T");
        t.transform.SetParent(k.transform, false);
        Text tt = t.AddComponent<Text>();
        tt.text = text; tt.font = font; tt.fontSize = 16; tt.color = Color.white; tt.fontStyle = FontStyle.Bold;
        tt.alignment = TextAnchor.MiddleCenter;
        RectTransform ttr = (RectTransform)t.transform;
        ttr.anchorMin = Vector2.zero; ttr.anchorMax = Vector2.one;
        ttr.offsetMin = Vector2.zero; ttr.offsetMax = Vector2.zero;
    }

    private void MakeTextUnderKey(Transform parent, string text, Vector2 pos, Font font)
    {
        GameObject g = new GameObject("Label");
        g.transform.SetParent(parent, false);
        Text t = g.AddComponent<Text>();
        t.text = text; t.font = font; t.fontSize = 12; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter;
        RectTransform tr = (RectTransform)g.transform;
        tr.anchorMin = new Vector2(0.5f, 0); tr.anchorMax = new Vector2(0.5f, 0);
        tr.pivot = new Vector2(0.5f, 0);
        tr.anchoredPosition = new Vector2(pos.x, pos.y);
        tr.sizeDelta = new Vector2(120, 18);
    }

    // ============ Player / Enemy ============
    private void SetupPlayer()
    {
        playerObj = CreateMech("Player-A", new Vector3(-3.5f, -2.55f, 0), true,
            new Color(0.2f, 0.85f, 1f), new Color(0f, 0.45f, 0.8f));
        playerObj.tag = "Player";
        PlayerController pc = playerObj.AddComponent<PlayerController>();
        SetupRobotComponents(playerObj, pc, "Player");

        if (Camera.main != null)
        {
            CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
            if (cf != null) cf.target = null; // fixed camera, not follow
        }
        GameManager.Instance.RegisterPlayer(pc);
    }

    private void SetupEnemy()
    {
        enemyObj = CreateMech("Enemy-B", new Vector3(3.5f, -2.55f, 0), false,
            new Color(1f, 0.35f, 0.35f), new Color(0.85f, 0.15f, 0.3f));
        enemyObj.tag = "Enemy";

        // 始终添加EnemyController（单人模式AI）
        EnemyController ec = enemyObj.AddComponent<EnemyController>();
        SetupRobotComponents(enemyObj, ec, "Enemy");

        // 双人模式时额外添加Player2Controller（它会在双人模式时接管控制）
        Player2Controller p2 = enemyObj.AddComponent<Player2Controller>();
        // Player2Controller共享同一个SetupRobotComponents配置
        p2.attackPoint = ec.attackPoint;
        p2.spriteRenderer = ec.spriteRenderer;
        p2.enemyLayer = ec.enemyLayer;
        p2.bulletColor = ec.bulletColor;

        GameManager.Instance.RegisterEnemy(ec);
    }

    private GameObject CreateMech(string name, Vector3 position, bool facingRight, Color bodyColor, Color detailColor)
    {
        GameObject mech = new GameObject(name);
        mech.transform.position = position;
        SpriteRenderer renderer = mech.AddComponent<SpriteRenderer>();
        Texture2D tex = PixelSpriteGenerator.GenerateMechTexture(bodyColor, detailColor, facingRight);
        renderer.sprite = PixelSpriteGenerator.CreateSpriteFromTexture(tex, 16);
        renderer.sortingOrder = 5;

        BoxCollider2D col = mech.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 1.2f);
        col.offset = new Vector2(0, 0.4f);

        Rigidbody2D rb = mech.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        GameObject atk = new GameObject("AttackPoint");
        atk.transform.SetParent(mech.transform, false);
        atk.transform.localPosition = new Vector3(facingRight ? 0.8f : -0.8f, 0.4f, 0);

        return mech;
    }

    private void SetupRobotComponents(GameObject obj, RobotController robot, string role)
    {
        Transform atk = obj.transform.Find("AttackPoint");
        robot.attackPoint = atk != null ? atk : obj.transform;
        robot.spriteRenderer = obj.GetComponent<SpriteRenderer>();
        int ln;
        if (role == "Player")
        {
            ln = LayerMask.NameToLayer("Enemy");
            robot.enemyLayer = ln >= 0 ? 1 << ln : Physics2D.AllLayers;
            robot.bulletColor = new Color(0.3f, 0.9f, 1f, 1f); // 蓝色子弹
        }
        else
        {
            ln = LayerMask.NameToLayer("Player");
            robot.enemyLayer = ln >= 0 ? 1 << ln : Physics2D.AllLayers;
            robot.bulletColor = new Color(1f, 0.4f, 0.3f, 1f); // 红色子弹
        }
    }

    private void AssignHealthBars()
    {
        HealthBarAssigner a = GameManager.Instance.GetComponent<HealthBarAssigner>();
        if (a == null) return;
        if (playerObj != null)
        {
            RobotController r = playerObj.GetComponent<RobotController>();
            if (r != null && a.playerHealthBar != null) r.InitHealthBar(a.playerHealthBar);
        }
        if (enemyObj != null)
        {
            RobotController r = enemyObj.GetComponent<RobotController>();
            if (r != null && a.enemyHealthBar != null) r.InitHealthBar(a.enemyHealthBar);
        }
    }

    // ============ Texture helpers ============
    private static Sprite _whiteSprite;
    private static Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
                _whiteSprite = SolidSprite(Color.white);
            return _whiteSprite;
        }
    }

    private static Image MakeImage(Transform parent, string name, Color color)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        Image img = g.AddComponent<Image>();
        img.sprite = WhiteSprite;
        img.color = color;
        return img;
    }

    private static Sprite SolidSprite(Color c)
    {
        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        Color[] pixels = new Color[4] { c, c, c, c };
        t.SetPixels(pixels); t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1);
    }

    private static Sprite GenerateStarsTexture(int w, int h, int starCount)
    {
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        for (int i = 0; i < starCount; i++)
        {
            int x = UnityEngine.Random.Range(0, w);
            int y = UnityEngine.Random.Range(0, h);
            float brightness = UnityEngine.Random.value;
            Color c = new Color(1f, 1f, 1f, 0.3f + brightness * 0.7f);
            px[y * w + x] = c;
            if (brightness > 0.7f && x + 1 < w && y + 1 < h)
                px[(y + 1) * w + x + 1] = new Color(1f, 1f, 1f, 0.3f);
        }
        t.SetPixels(px); t.Apply();
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
    }
}

public class HealthBarAssigner : MonoBehaviour
{
    [HideInInspector] public HealthBar playerHealthBar;
    [HideInInspector] public HealthBar enemyHealthBar;
}

// Meteor animation script (lightweight)
public class MeteorDrift : MonoBehaviour
{
    public float startX;
    public float startY;
    public float speed = 3f;
    public float delay = 0f;
    private float t;

    private void Start() { t = -delay; }

    public void ResetPos() { transform.position = new Vector3(startX, startY, 0); t = -delay; }

    private void Update()
    {
        t += Time.deltaTime;
        if (t < 0) { gameObject.SetActive(false); return; }
        gameObject.SetActive(true);
        float x = startX - speed * t;
        float y = startY - speed * 0.6f * t;
        transform.position = new Vector3(x, y, 0);
        if (x < -12f || y < -6f) ResetPos();
    }
}

/// <summary>背景跟随相机视口大小自动缩放</summary>
public class BackgroundScaler : MonoBehaviour
{
    public float padding = 2f;
    public bool isGround = false;
    public bool useTiledSize = false;
    public float groundLineThickness = 0f;

    public bool isSideWall = false;
    public bool isTopWall = false;
    public float sideWallThickness = 1f;
    public float sideWallHeight = 6f;
    public bool sideWallIsLeft = false;
    public float topWallYOffset = 0.6f;

    private SpriteRenderer sr;
    private Camera cam;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        UpdateScale();
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) UpdateScale();
    }

    private void UpdateScale()
    {
        if (cam == null || sr == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        if (useTiledSize)
        {
            sr.size = new Vector2(camWidth + padding, camHeight + padding);
        }
        else if (isGround)
        {
            if (groundLineThickness > 0f)
                transform.localScale = new Vector3(camWidth + padding, groundLineThickness, 1);
            else
                transform.localScale = new Vector3(camWidth + padding, transform.localScale.y, 1);
        }
        else if (isSideWall)
        {
            // 左右墙：从地面底部延伸到天空顶部
            float bottomY = -4.25f; // 地面下沿
            float wallH = camHeight + padding; // 总高度
            float centerY = bottomY + wallH / 2f;
            float halfArena = (camWidth + padding) / 2f;
            float x = sideWallIsLeft ? -halfArena : halfArena;

            transform.localScale = new Vector3(sideWallThickness, wallH, 1);
            transform.position = new Vector3(x, centerY, transform.position.z);
        }
        else if (isTopWall)
        {
            // 顶部墙：紧贴血条下方
            float camTop = cam.orthographicSize;
            float topY = camTop - topWallYOffset;
            transform.localScale = new Vector3(camWidth + padding, sideWallThickness, 1);
            transform.position = new Vector3(0, topY, transform.position.z);
        }
        else
        {
            transform.localScale = new Vector3(camWidth + padding, camHeight + padding, 1);
        }
    }
}