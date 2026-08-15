#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class SceneBuilder
{
    [MenuItem("Tools/Build Game Scene")]
    public static void BuildGameScene()
    {
        CreateMainScene();
        ConfigureBuildSettings();
        Debug.Log("Main scene built successfully! Press Play to start the game.");
    }

    public static void CreateMainScene()
    {
        // Create new scene
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create SceneSetup GameObject
        GameObject sceneSetupObj = new GameObject("SceneSetup");
        sceneSetupObj.AddComponent<SceneSetup>();

        // Ensure SceneSetup will auto-run
        SceneSetup setup = sceneSetupObj.GetComponent<SceneSetup>();
        setup.autoSetup = true;

        // Save scene
        string scenePath = "Assets/Scenes/Main.unity";
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(scenePath));
        EditorSceneManager.SaveScene(scene, scenePath);
    }

    public static void ConfigureBuildSettings()
    {
        string scenePath = "Assets/Scenes/Main.unity";
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = scenes;
    }

    [MenuItem("Tools/Start Game _F5")]
    public static void StartGame()
    {
        // Make sure scene exists
        if (!System.IO.File.Exists("Assets/Scenes/Main.unity"))
        {
            BuildGameScene();
        }

        // Open the scene and press play
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        EditorApplication.EnterPlaymode();
    }
}
#endif