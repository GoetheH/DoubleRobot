#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public static class RuntimeTestRunner
{
    [MenuItem("Tools/Run Runtime Smoke Test")]
    public static void RunSmokeTest()
    {
        Debug.Log("=== Runtime Smoke Test Start ===");
        string log = "";

        // 1. Open Main scene (should already exist)
        string scenePath = "Assets/Scenes/Main.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            SceneBuilder.BuildGameScene();
        }
        EditorSceneManager.OpenScene(scenePath);
        log += "1. Scene opened OK\n";

        // 2. Run scene setup if not done
        SceneSetup setup = Object.FindObjectOfType<SceneSetup>();
        if (setup == null)
        {
            GameObject s = new GameObject("SceneSetup");
            setup = s.AddComponent<SceneSetup>();
        }
        setup.SetupScene();
        log += "2. SceneSetup executed OK\n";

        // 3. Verify core objects exist
        GameManager gm = GameManager.Instance;
        bool pass = gm != null && gm.player != null && gm.enemy != null && gm.uiManager != null;
        log += $"3. GameManager references: {(pass ? "OK" : "FAIL")} (GM={gm!=null}, Player={gm?.player!=null}, Enemy={gm?.enemy!=null}, UI={gm?.uiManager!=null})\n";

        // 4. Verify HealthBars
        bool hpOk = gm.player.healthBar != null && gm.enemy.healthBar != null
                    && gm.player.healthBar.fill != null
                    && gm.player.currentHealth == gm.player.maxHealth;
        log += $"4. HealthBar init: {(hpOk ? "OK" : "FAIL")} (playerHP={gm.player.currentHealth}/{gm.player.maxHealth}, fill={gm.player.healthBar?.fill != null})\n";

        // 5. Test attack damage flow
        int startHP = gm.enemy.currentHealth;
        gm.player.Attack();
        // Wait for attack to process via invoke (simulate 0.6s)
        EditorApplication.delayCall += () =>
        {
            int endHP = gm.enemy.currentHealth;
            int delta = startHP - endHP;
            log += $"5. Attack damage: {(delta > 0 ? "OK" : "FAIL")} (HP {startHP} -> {endHP}, delta={delta})\n";
        };

        // 6. Test defense
        gm.enemy.Defend();
        log += $"6. Enemy Defend: isDefending={gm.enemy.isDefending}\n";

        // 7. Test layers/tags setup
        string playerTag = gm.player.tag;
        string enemyTag = gm.enemy.tag;
        log += $"7. Tags: Player={playerTag}, Enemy={enemyTag}\n";

        Debug.Log(log + "\n=== Test Summary: Please check console for Attack result shortly ===");
        PlayerPrefs.SetString("SmokeTestLog", log);
        PlayerPrefs.Save();
    }

    [MenuItem("Tools/Start Game _F5")]
    public static void StartGame()
    {
        string scenePath = "Assets/Scenes/Main.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            SceneBuilder.BuildGameScene();
        }
        EditorSceneManager.OpenScene(scenePath);
        EditorApplication.EnterPlaymode();
    }
}
#endif