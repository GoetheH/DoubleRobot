#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;

public static class NeonBuild
{
    public static void WebGL()
    {
        string output=Environment.GetEnvironmentVariable("NEON_WEBGL_PATH");
        if(string.IsNullOrEmpty(output))throw new Exception("NEON_WEBGL_PATH is required");
        NeonAuthoringValidation.ValidateAssets();
        if(!UnityEngine.Resources.Load<UnityEngine.Font>("Fonts/ChineseUI"))throw new Exception("WebGL requires the bundled Chinese UI font.");
        PlayerSettings.productName="霓虹突围（NEON BREAKOUT）";
        PlayerSettings.bundleVersion="1.1.1";
        PlayerSettings.defaultWebScreenWidth=1600;PlayerSettings.defaultWebScreenHeight=900;
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback=true;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL,ScriptingImplementation.IL2CPP);
        BuildReport report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Main.unity"},output,BuildTarget.WebGL,BuildOptions.None);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("WebGL build failed: "+report.summary.result);
    }
    public static void Windows()
    {
        string output = Environment.GetEnvironmentVariable("NEON_BUILD_PATH");
        if (string.IsNullOrEmpty(output)) throw new Exception("NEON_BUILD_PATH is required");
        NeonAuthoringValidation.ValidateAssets();
        PlayerSettings.productName = "霓虹突围（NEON BREAKOUT）";
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = UnityEngine.FullScreenMode.Windowed;
        BuildReport report = BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/Main.unity" }, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows build failed: " + report.summary.result);
    }
}
#endif
