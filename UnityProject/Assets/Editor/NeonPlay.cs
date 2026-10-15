#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class NeonPlay
{
    static string RequestPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../../验证/unity-play.request"));
    static NeonPlay() { EditorApplication.update += CheckRequest; }

    static void CheckRequest()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
        if (File.ReadAllText(RequestPath).Trim() != "play") return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.isPlaying = false; return; }
        File.WriteAllText(RequestPath, "done");
        Play();
    }

    [MenuItem("霓虹突围/运行游戏")]
    public static void Play()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        EditorApplication.isPlaying = true;
        Debug.Log("[NEON BREAKOUT] Main scene loaded; entering Play mode.");
    }
}
#endif
