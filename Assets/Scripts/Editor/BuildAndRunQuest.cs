using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAndRunQuest
{
    public const string OUTPUT_APK_PATH = "unity-360_video_tour.apk";

    [MenuItem("VR Tour/Build Android APK", false, 20)]
    public static void BuildApkMenu()
    {
        BuildApk();
    }

    public static void BuildApk()
    {
        Debug.Log("=== [BuildAndRunQuest] Starting automated VR build for Meta Quest ===");

        // Run full scene setup to ensure XR rigs, raycasters, and build settings are up-to-date
        CompleteSceneConfigurator.RunCompleteSetup();

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        string[] scenes = new string[]
        {
            "Assets/Scenes/MainMenuScene.unity",
            "Assets/Scenes/IntranetTourScene.unity",
            "Assets/Scenes/CustomCampusTourScene.unity"
        };

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OUTPUT_APK_PATH,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log($"=== [BuildAndRunQuest] Building APK to '{OUTPUT_APK_PATH}' with {scenes.Length} scenes ===");

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"=== [BuildAndRunQuest] Build SUCCEEDED! Output size: {summary.totalSize} bytes ===");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"=== [BuildAndRunQuest] Build FAILED with {summary.totalErrors} errors! ===");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
