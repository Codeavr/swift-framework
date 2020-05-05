using System.Linq;
using System.Text;
using SwiftFramework.Core;
using UnityEditorInternal;

namespace SwiftFramework.EditorUtils
{
    using SwiftFramework.Core.Editor;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    internal class Builder : ScriptableObject
	{
        public ModuleManifestLink manifest;

        public GlobalConfigLink globalConfig;

        public const string kLogType = "#### [Builder] ";

        public const string defaultAppleCert = "MIIEuzCCA6OgAwIBAgIBAjANBgkqhkiG9w0BAQUFADBiMQswCQYDVQQGEwJVUzETMBEGA1UEChMKQXBwbGUgSW5jLjEmMCQGA1UECxMdQXBwbGUgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkxFjAUBgNVBAMTDUFwcGxlIFJvb3QgQ0EwHhcNMDYwNDI1MjE0MDM2WhcNMzUwMjA5MjE0MDM2WjBiMQswCQYDVQQGEwJVUzETMBEGA1UEChMKQXBwbGUgSW5jLjEmMCQGA1UECxMdQXBwbGUgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkxFjAUBgNVBAMTDUFwcGxlIFJvb3QgQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDkkakJH5HbHkdQ6wXtXnmELes2oldMVeyLGYne+Uts9QerIjAC6Bg++FAJ039BqJj50cpmnCRrEdCju+QbKsMflZ56DKRHi1vUFjczy8QPTc4UadHJGXL1XQ7Vf1+b8iUDulWPTV0N8WQ1IxVLFVkds5T39pyez1C6wVhQZ48ItCD3y6wsIG9wtj8BMIy3Q88PnT3zK0koGsj+zrW5DtleHNbLPbU6rfQPDgCSC7EhFi501TwN22IWq6NxkkdTVcGvL0Gz+PvjcM3mo0xFfh9Ma1CWQYnEdGILEINBhzOKgbEwWOxaBDKMaLOPHd5lc/9nXmW8Sdh2nzMUZaF3lMktAgMBAAGjggF6MIIBdjAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUK9BpR5R2Cf70a40uQKb3R01/CF4wHwYDVR0jBBgwFoAUK9BpR5R2Cf70a40uQKb3R01/CF4wggERBgNVHSAEggEIMIIBBDCCAQAGCSqGSIb3Y2QFATCB8jAqBggrBgEFBQcCARYeaHR0cHM6Ly93d3cuYXBwbGUuY29tL2FwcGxlY2EvMIHDBggrBgEFBQcCAjCBthqBs1JlbGlhbmNlIG9uIHRoaXMgY2VydGlmaWNhdGUgYnkgYW55IHBhcnR5IGFzc3VtZXMgYWNjZXB0YW5jZSBvZiB0aGUgdGhlbiBhcHBsaWNhYmxlIHN0YW5kYXJkIHRlcm1zIGFuZCBjb25kaXRpb25zIG9mIHVzZSwgY2VydGlmaWNhdGUgcG9saWN5IGFuZCBjZXJ0aWZpY2F0aW9uIHByYWN0aWNlIHN0YXRlbWVudHMuMA0GCSqGSIb3DQEBBQUAA4IBAQBcNplMLXi37Yyb3PN3m/J20ncwT8EfhYOFG5k9RzfyqZtAjizUsZAS2L70c5vu0mQPy3lPNNiiPvl4/2vIB+x9OYOLUyDTOMSxv5pPCmv/K/xZpwUJfBdAVhEedNO3iyM7R6PVbyTi69G3cN8PReEnyvFteO3ntRcXqNx+IjXKJdXZD9Zr1KIkIxH3oayPc4FgxhtbCS+SsvhESPBgOJ4V9T0mZyCKM2r3DYLP3uujL/lTaltkwGMzd/c6ByxW69oPIQ7aunMZT7XZNn/Bh1XZp5m5MkL72NVxnn6hUrcbvZNCJBIqxw8dtk2cXmPIS4AXUKqK1drk/NAJBzewdXUh";

		public BuildTarget buildTarget;

		public BuildTarget actualBuildTarget { get { return buildTarget; } }

		public BuildTargetGroup buildTargetGroup { get { return BuildPipeline.GetBuildTargetGroup(actualBuildTarget); } }

		public string productName;

		public string companyName;

		public string applicationIdentifier;

        public bool buildAppBundle;

        public string buildName
		{
			get
			{
                if (actualBuildTarget == BuildTarget.Android && !EditorUserBuildSettings.exportAsGoogleAndroidProject)
                {
                    if (EditorUserBuildSettings.buildAppBundle || buildAppBundle)
                    {
                        return $"{name}_{version}.{versionCode}.aab";
                    }
                    else
                    {
                        return $"{name}_{version}.{versionCode}.apk";
                    }
                }
             
				else
					return "build";
			}
		}

        public string OutputFolderPath
        {
            get
            {
                return EditorPrefs.GetString(name + "_outputFolderPath");
            }
            set
            {
                EditorPrefs.SetString(name + "_outputFolderPath", value);
            }
        }

		public string version;

        public Texture2D icon;

        public Texture2D defaultIconToOverwrite;

        public int versionCode = 0;

		public bool developmentBuild = false;

		public SceneSetting[] scenes = new SceneSetting[]{ };

        public string unityProjectCloudId;

        public string facebookAppId;

        public string[] excludeDirectories = new string[]{ };

		public bool copyToStreamingAssets;

		public BundleOptions bundleOptions;

        public bool showUnitySplashScreen;

        public bool showSplashScreen;

		public BuildTargetSettings_iOS iosSettings = new BuildTargetSettings_iOS();
		public BuildTargetSettings_Android androidSettings = new BuildTargetSettings_Android();
		public BuildTargetSettings_WebGL webGlSettings = new BuildTargetSettings_WebGL();

        [ContextMenu("Save Json")]
        public void SaveJson()
        {
            string path = EditorUtility.SaveFilePanel(name, Application.dataPath, name, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllText(path, JsonUtility.ToJson(this, true));
        }

		[System.Serializable]
		public class SceneSetting
		{
			public bool enable = true;
			public string name;
		}

		public enum BundleOptions
		{
			LZMA = BuildAssetBundleOptions.None,
			LZ4 = BuildAssetBundleOptions.ChunkBasedCompression,
			Uncompressed = BuildAssetBundleOptions.UncompressedAssetBundle,
		}

		protected virtual void OnApplySetting()
		{
		}

        private string GetOutputFolder()
        {
            return Path.Combine(OutputFolderPath, buildName);
        }

		public virtual void Reset()
		{
			buildTarget = EditorUserBuildSettings.activeBuildTarget;
			productName = PlayerSettings.productName;
			companyName = PlayerSettings.companyName;
            unityProjectCloudId = CloudProjectSettings.projectId;
            applicationIdentifier = PlayerSettings.GetApplicationIdentifier(buildTargetGroup);

			version = PlayerSettings.bundleVersion;

			androidSettings.Reset();
			iosSettings.Reset();
		}

        public void BuildAddressableAssets()
        {
            AddressableAssetSettings.BuildPlayerContent();
        }

		public void ApplySettings()
		{
            PlayerSettings.SplashScreen.showUnityLogo = showUnitySplashScreen;
            PlayerSettings.SplashScreen.show = showSplashScreen;
            PlayerSettings.SetApplicationIdentifier(buildTargetGroup, applicationIdentifier);
			PlayerSettings.productName = productName;
			PlayerSettings.companyName = companyName;

            EditorUserBuildSettings.development = developmentBuild;
			EditorUserBuildSettings.allowDebugging = developmentBuild;

            PlayerSettings.bundleVersion = version;
			string buildNumber;
			if (developmentBuild && Util.executeArguments.TryGetValue(Util.OPT_DEV_BUILD_NUM, out buildNumber) && !string.IsNullOrEmpty(buildNumber))
				PlayerSettings.bundleVersion += "." + buildNumber;


            File.WriteAllText(Path.Combine(Util.projectDir, "BUILD_VERSION"), PlayerSettings.bundleVersion);

            if (defaultIconToOverwrite == null)
            {
                var defaultIconPath = string.Format("{0}", EditorUtility.OpenFilePanelWithFilters("Select Your Default Icon", "Assets", new string[] { "Texture", "png", "Texture", "jpeg", "Texture", "jpg" }));
                defaultIconToOverwrite = AssetDatabase.LoadAssetAtPath<Texture2D>(Util.ToRelativePath(defaultIconPath));
                if(defaultIconToOverwrite != null)
                {
                    foreach (var builder in Util.GetAssets<Builder>())
                    {
                        builder.defaultIconToOverwrite = defaultIconToOverwrite;
                        EditorUtility.SetDirty(builder);
                    }
                }
            }

            string defaultIconFilePath = Path.Combine(Util.projectDir, AssetDatabase.GetAssetPath(defaultIconToOverwrite));

            string selectedIconFilePath = Path.Combine(Util.projectDir, AssetDatabase.GetAssetPath(icon));

            File.WriteAllBytes(defaultIconFilePath, File.ReadAllBytes(selectedIconFilePath));

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(defaultIconToOverwrite));

            EditorBuildSettingsScene[] buildSettingsScenes = EditorBuildSettings.scenes;

            BootConfig bootConfig = Util.FindScriptableObject<BootConfig>();
            AddrHelper.GetAsset<GlobalConfig>(globalConfig).bundleId = applicationIdentifier;
            bootConfig.globalConfig = globalConfig;
            bootConfig.modulesManifest = manifest;
            bootConfig.buildNumber = versionCode;

            EditorUtility.SetDirty(bootConfig);
            EditorUtility.SetDirty(AddrHelper.GetAsset<GlobalConfig>(globalConfig));

            BaseModuleManifest manufestAsset = AddrHelper.GetAsset<BaseModuleManifest>(manifest);

            foreach ((ModuleLink m, FieldInfo f) in manufestAsset.GetModuleFields())
            {
                if (m.ConfigLink.HasValue)
                {
                    Util.ApplyModuleConfig(m.ImplementationType, AddrHelper.GetAsset<ModuleConfig>(m.ConfigLink));
                }
            }

			for (int i = 0; i < buildSettingsScenes.Length;i++)
			{
				var scene = buildSettingsScenes[i];
				var setting = scenes.FirstOrDefault(x =>x.name == Path.GetFileName(scene.path));
				if(setting != null)
				{
					scene.enabled = setting.enable;
				}
					
				buildSettingsScenes[i] = scene;
			}
			EditorBuildSettings.scenes = buildSettingsScenes;
            EditorUserBuildSettings.buildAppBundle = buildAppBundle;


            iosSettings.ApplySettings(this);
			androidSettings.ApplySettings(this);
            ApplyFacebook();
            OnApplySetting();
			AssetDatabase.SaveAssets();

		}

        private void ApplyFacebook()
        {
#if ENABLE_FACEBOOK
            var facebookSettings = Util.FindScriptableObject<Facebook.Unity.Settings.FacebookSettings>();

            if (facebookSettings != null && string.IsNullOrEmpty(facebookAppId) == false)
            {
                var so = new SerializedObject(facebookSettings);
                so.FindProperty("appIds").GetArrayElementAtIndex(0).stringValue = facebookAppId;
                so.ApplyModifiedProperties();
                FacebookManifestMod.GenerateManifest(facebookAppId);
            }
#endif
        }

		public bool BuildPlayer(bool autoRunPlayer)
		{
            if (Directory.Exists(OutputFolderPath) == false)
            {
                string newFolder = EditorUtility.SaveFolderPanel("Select build folder", Application.dataPath, Application.dataPath);
                if (Directory.Exists(newFolder))
                {
                    OutputFolderPath = newFolder;
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    return false;
                }
                return false;
            }

            string buildFilePath = GetOutputFolder();

            foreach (var dir in excludeDirectories)
				Util.ExcludeDirectory(dir);

			BuildOptions opt = developmentBuild ? (BuildOptions.Development & BuildOptions.AllowDebugging) : BuildOptions.None
				                | (autoRunPlayer ? BuildOptions.AutoRunPlayer : BuildOptions.None);

			string[] scenesToBuild = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
			Debug.Log(kLogType + "Scenes to build : " + scenesToBuild.Aggregate((a,b)=>a+", "+b));

				
			Debug.Log(kLogType + "BuildPlayer is started. Defined symbols : " + PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup));
			var report = BuildPipeline.BuildPlayer(scenesToBuild, buildFilePath, actualBuildTarget, opt);

			Util.RevertExcludedDirectory();

            switch (report.summary.result)
            {
                case UnityEditor.Build.Reporting.BuildResult.Succeeded:
                    Debug.Log($"<color=green>Build Succeeded!</color> Build time: {report.summary.totalTime.ToString()}"); 
                    break;
                default:
                    Debug.LogError("Build failed!");
                    break;
            }
            
			return true;
		}

		static void Build()
		{
			Util.StartBuild(Util.GetBuilderFromExecuteArgument(), true, false);
		}
	}
}

namespace Building
{
    using SwiftFramework.EditorUtils;

    public static class ProjectBuilder
    {
        public static void BuildAndroid()
        {
            Util.StartBuild(Util.GetBuilderFromExecuteArgument(), false, false);
        }

        public static void BuildIOS()
        {
            Util.StartBuild(Util.GetBuilderFromExecuteArgument(), false, false);
        }
    }
}