/**
 * HoloCall Builder - Automated build scripts for all platforms
 * Place in Assets/Editor/ folder
 */

using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public class HoloCallBuilder
{
    // Build paths
    private static readonly string BUILD_DIR = "Builds";
    private static readonly string DESKTOP_DIR = "Desktop";
    private static readonly string ANDROID_DIR = "Android";
    private static readonly string IOS_DIR = "iOS";

    // Scenes to include in build
    private static readonly string[] SCENES = new string[]
    {
        "Assets/Scenes/MainScene.unity"
    };

    #region Desktop Builds

    [MenuItem("HoloCall/Build/Desktop/Windows 64-bit")]
    public static void BuildWindows64()
    {
        string buildPath = Path.Combine(BUILD_DIR, DESKTOP_DIR, "Windows", "HoloCall.exe");
        BuildDesktop(BuildTarget.StandaloneWindows64, buildPath);
    }

    [MenuItem("HoloCall/Build/Desktop/macOS")]
    public static void BuildMacOS()
    {
        string buildPath = Path.Combine(BUILD_DIR, DESKTOP_DIR, "macOS", "HoloCall.app");
        BuildDesktop(BuildTarget.StandaloneOSX, buildPath);
    }

    [MenuItem("HoloCall/Build/Desktop/Linux 64-bit")]
    public static void BuildLinux64()
    {
        string buildPath = Path.Combine(BUILD_DIR, DESKTOP_DIR, "Linux", "HoloCall.x86_64");
        BuildDesktop(BuildTarget.StandaloneLinux64, buildPath);
    }

    [MenuItem("HoloCall/Build/Desktop/All Platforms")]
    public static void BuildAllDesktop()
    {
        BuildWindows64();
        BuildMacOS();
        BuildLinux64();
        Debug.Log("✅ All desktop builds complete!");
    }

    private static void BuildDesktop(BuildTarget target, string buildPath)
    {
        Debug.Log($"🔨 Building HoloCall for {target}...");

        // Ensure build directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        // Configure build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = SCENES,
            locationPathName = buildPath,
            target = target,
            options = BuildOptions.None
        };

        // Execute build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Desktop build succeeded: {buildPath}");
            Debug.Log($"   Size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"   Time: {report.summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"❌ Desktop build failed: {report.summary.result}");
        }
    }

    #endregion

    #region Android Build

    [MenuItem("HoloCall/Build/Android/APK (Debug)")]
    public static void BuildAndroidDebug()
    {
        BuildAndroid(false);
    }

    [MenuItem("HoloCall/Build/Android/AAB (Release)")]
    public static void BuildAndroidRelease()
    {
        BuildAndroid(true);
    }

    private static void BuildAndroid(bool buildAAB)
    {
        Debug.Log($"🔨 Building HoloCall for Android ({(buildAAB ? "AAB" : "APK")})...");

        // Switch to Android platform if needed
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("Switching to Android platform...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android
            );
        }

        // Configure Android settings
        ConfigureAndroidSettings();

        // Build path
        string extension = buildAAB ? ".aab" : ".apk";
        string buildPath = Path.Combine(BUILD_DIR, ANDROID_DIR, $"HoloCall{extension}");
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        // Set build format
        EditorUserBuildSettings.buildAppBundle = buildAAB;

        // Configure build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = SCENES,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = buildAAB ? BuildOptions.None : BuildOptions.Development
        };

        // Execute build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Android build succeeded: {buildPath}");
            Debug.Log($"   Size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"   Time: {report.summary.totalTime.TotalSeconds:F1}s");

            // Show in folder
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError($"❌ Android build failed: {report.summary.result}");
        }
    }

    private static void ConfigureAndroidSettings()
    {
        // Minimum API Level: Android 7.0 (API 24)
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        // Target API Level: Latest
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

        // Scripting backend: IL2CPP (required for ARCore)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // Target architectures: ARM64 (required for ARCore)
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Internet access
        PlayerSettings.Android.forceInternetPermission = true;

        // Package name
        if (string.IsNullOrEmpty(PlayerSettings.applicationIdentifier))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yourcompany.holocall");
        }

        Debug.Log("✅ Android settings configured");
    }

    #endregion

    #region iOS Build

    [MenuItem("HoloCall/Build/iOS/Xcode Project")]
    public static void BuildiOS()
    {
        Debug.Log("🔨 Building HoloCall for iOS (Xcode project)...");

        #if !UNITY_EDITOR_OSX
        Debug.LogError("❌ iOS builds require macOS with Xcode installed");
        return;
        #endif

        // Switch to iOS platform if needed
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            Debug.Log("Switching to iOS platform...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.iOS,
                BuildTarget.iOS
            );
        }

        // Configure iOS settings
        ConfigureiOSSettings();

        // Build path
        string buildPath = Path.Combine(BUILD_DIR, IOS_DIR);
        Directory.CreateDirectory(buildPath);

        // Configure build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = SCENES,
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        // Execute build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ iOS Xcode project created: {buildPath}");
            Debug.Log($"   Time: {report.summary.totalTime.TotalSeconds:F1}s");
            Debug.Log("📝 Next steps:");
            Debug.Log("   1. Open the Xcode project");
            Debug.Log("   2. Configure signing & capabilities");
            Debug.Log("   3. Connect iOS device");
            Debug.Log("   4. Build and run in Xcode");

            // Show in folder
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError($"❌ iOS build failed: {report.summary.result}");
        }
    }

    private static void ConfigureiOSSettings()
    {
        // Minimum iOS version: 13.0
        PlayerSettings.iOS.targetOSVersionString = "13.0";

        // Scripting backend: IL2CPP (required)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

        // Architecture: ARM64
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;

        // Camera usage description (required for ARKit)
        PlayerSettings.iOS.cameraUsageDescription = "Required for AR hologram capture and face tracking";

        // Microphone usage description
        PlayerSettings.iOS.microphoneUsageDescription = "Required for audio calls";

        // Bundle identifier
        if (string.IsNullOrEmpty(PlayerSettings.applicationIdentifier))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.yourcompany.holocall");
        }

        // Requires ARKit
        PlayerSettings.iOS.requiresFullScreen = false;

        Debug.Log("✅ iOS settings configured");
    }

    #endregion

    #region Build All

    [MenuItem("HoloCall/Build/All Platforms")]
    public static void BuildAllPlatforms()
    {
        Debug.Log("🔨 Building HoloCall for ALL platforms...");

        // Desktop
        BuildWindows64();
        BuildMacOS();
        BuildLinux64();

        // Android
        BuildAndroidRelease();

        // iOS (macOS only)
        #if UNITY_EDITOR_OSX
        BuildiOS();
        #endif

        Debug.Log("✅ All platform builds complete!");
    }

    #endregion

    #region Utility

    [MenuItem("HoloCall/Open Build Folder")]
    public static void OpenBuildFolder()
    {
        string path = Path.GetFullPath(BUILD_DIR);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("HoloCall/Clean Builds")]
    public static void CleanBuilds()
    {
        if (Directory.Exists(BUILD_DIR))
        {
            Directory.Delete(BUILD_DIR, true);
            Debug.Log($"✅ Cleaned build directory: {BUILD_DIR}");
        }
        else
        {
            Debug.Log("Build directory already clean");
        }
    }

    [MenuItem("HoloCall/Validate Build Settings")]
    public static void ValidateBuildSettings()
    {
        Debug.Log("🔍 Validating build settings...");

        // Check scenes
        if (SCENES.Length == 0)
        {
            Debug.LogError("❌ No scenes configured in build!");
        }
        else
        {
            Debug.Log($"✅ Scenes: {SCENES.Length}");
            foreach (var scene in SCENES)
            {
                if (!File.Exists(scene))
                {
                    Debug.LogError($"❌ Scene not found: {scene}");
                }
                else
                {
                    Debug.Log($"   - {scene}");
                }
            }
        }

        // Check package identifier
        string packageId = PlayerSettings.applicationIdentifier;
        if (string.IsNullOrEmpty(packageId) || packageId.StartsWith("com.defaultcompany"))
        {
            Debug.LogWarning("⚠️ Package identifier not set or using default!");
            Debug.LogWarning("   Set in: Edit > Project Settings > Player > Other Settings");
        }
        else
        {
            Debug.Log($"✅ Package ID: {packageId}");
        }

        // Check version
        Debug.Log($"✅ Version: {Application.version}");

        // Platform-specific checks
        #if UNITY_ANDROID
        Debug.Log($"✅ Android Min SDK: {PlayerSettings.Android.minSdkVersion}");
        Debug.Log($"✅ Android Target SDK: {PlayerSettings.Android.targetSdkVersion}");
        #endif

        #if UNITY_IOS
        Debug.Log($"✅ iOS Target: {PlayerSettings.iOS.targetOSVersionString}");
        #endif

        Debug.Log("✅ Validation complete");
    }

    #endregion
}
