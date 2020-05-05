﻿using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace SwiftFramework.EditorUtils
{
	[System.Serializable]
	internal class BuildTargetSettings_iOS : IBuildTargetSettings
	{
		public BuildTarget buildTarget{get{ return BuildTarget.iOS;}}

		public Texture icon{get{ return EditorGUIUtility.FindTexture("BuildSettings.iPhone.Small");}}

		/// <summary>Enable automatically sign.</summary>
		[Tooltip("Enable automatically sign.")]
		public bool automaticallySign = false;

		/// <summary>Developer Team Id.</summary>
		[Tooltip("Developer Team Id.")]
		public string developerTeamId = "";

		/// <summary>Code Sign Identifier.</summary>
		[Tooltip("Code Sign Identifier.")]
		public string codeSignIdentity = "";


		/// <summary>Provisioning Profile Id. For example: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</summary>
		[Tooltip("Provisioning Profile Id.\nFor example: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")]
		public string profileId = "";


		/// <summary>Provisioning Profile Specifier. For example: com campany app_name</summary>
		[Tooltip("Provisioning Profile Specifier.\nFor example: com campany app_name")]
		public string profileSpecifier = "";



		/// <summary>Support languages. If you have multiple definitions, separate with a semicolon(;)</summary>
		[Tooltip("Support languages.\nIf you have multiple definitions, separate with a semicolon(;)")]
		public string languages = "jp;en";


		/// <summary>Generate exportOptions.plist automatically for xcodebuild (XCode7 and later).</summary>
		[Tooltip("Generate exportOptions.plist under build path for xcodebuild (XCode7 and later).")]
		public bool generateExportOptionPlist = false;

		/// <summary>The method of distribution, which can be set as any of the following: app-store, ad-hoc, package, enterprise, development, developer-id.</summary>
		[Tooltip("The method of distribution, which can be set as any of the following:\napp-store, ad-hoc, package, enterprise, development, developer-id.")]
		public string exportMethod = "development";

		/// <summary>Option to include Bitcode.</summary>
		[Tooltip("Option to include Bitcode.")]
		public bool uploadBitcode = false;

		/// <summary>Option to include symbols in the generated ipa file.</summary>
		[Tooltip("Option to include symbols in the generated ipa file.")]
		public bool uploadSymbols = false;

		/// <summary>Entitlements file(*.entitlement).</summary>
		[Tooltip("Entitlements file(*.entitlements).")]
		public string entitlementsFile = "";

		/// <summary>Apple services. If you have multiple definitions, separate with a semicolon(;)</summary>
		[Tooltip("Apple services.\nIf you have multiple definitions, separate with a semicolon(;)")]
		public string services = "";

		/// <summary>Additional frameworks. If you have multiple definitions, separate with a semicolon(;)</summary>
		[Tooltip("Additional frameworks.\nIf you have multiple definitions, separate with a semicolon(;)")]
		public string frameworks = "";





		static readonly string[] s_AvailableExportMethods =
		{
			"app-store",
			"ad-hoc",
			"package",
			"enterprise",
			"development",
			"developer-id",
		};

		static readonly string[] s_AvailableLanguages =
		{
			"jp",
			"en",
		};


		static readonly string[] s_AvailableFrameworks =
		{
			"iAd.framework",
		};

		static readonly string[] s_AvailableServices =
		{
			"com.apple.ApplePay",
			"com.apple.ApplicationGroups.iOS",
			"com.apple.BackgroundModes",
			"com.apple.DataProtection",
			"com.apple.GameCenter",
			"com.apple.GameControllers.appletvos",
			"com.apple.HealthKit",
			"com.apple.HomeKit",
			"com.apple.InAppPurchase",
			"com.apple.InterAppAudio",
			"com.apple.Keychain",
			"com.apple.Maps.iOS",
			"com.apple.NetworkExtensions",
			"com.apple.Push",
			"com.apple.SafariKeychain",
			"com.apple.Siri",
			"com.apple.VPNLite",
			"com.apple.WAC",
			"com.apple.Wallet",
			"com.apple.iCloud",
		};



		public void Reset()
		{
#if UNITY_5_4_OR_NEWER
			developerTeamId = PlayerSettings.iOS.appleDeveloperTeamID;
#endif
#if UNITY_5_5_OR_NEWER
			automaticallySign = PlayerSettings.iOS.appleEnableAutomaticSigning;
			profileId = PlayerSettings.iOS.iOSManualProvisioningProfileID;
#endif
		}

		public void ApplySettings(Builder builder)
		{
			PlayerSettings.iOS.buildNumber = builder.versionCode.ToString();
#if UNITY_5_4_OR_NEWER
			PlayerSettings.iOS.appleDeveloperTeamID = developerTeamId;
#endif
#if UNITY_5_5_OR_NEWER
			PlayerSettings.iOS.appleEnableAutomaticSigning = automaticallySign;
			if(!automaticallySign)
			{
				PlayerSettings.iOS.iOSManualProvisioningProfileID = profileId;
			}
#endif
		}


		/// <summary>
		/// Draws the ios settings.
		/// </summary>
		public void DrawSetting(SerializedObject serializedObject)
		{
            var settings = serializedObject.FindProperty("iosSettings");

            using (new EditorGUIEx.GroupScope("iOS Settings"))
            {
                // XCode Project.
                EditorGUILayout.LabelField("XCode Project", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    EditorGUIEx.TextFieldWithTemplate(settings.FindPropertyRelative("languages"), s_AvailableLanguages, true);
                    EditorGUIEx.TextFieldWithTemplate(settings.FindPropertyRelative("frameworks"), s_AvailableFrameworks, true);
                    EditorGUIEx.TextFieldWithTemplate(settings.FindPropertyRelative("services"), s_AvailableServices, true);
                    EditorGUIEx.FilePathField(settings.FindPropertyRelative("entitlementsFile"), "Select entitlement file.", "", "entitlements");
                }
                EditorGUI.indentLevel--;

                // Signing.
                EditorGUILayout.LabelField("Signing", EditorStyles.boldLabel);
                var spAutomaticallySign = settings.FindPropertyRelative("automaticallySign");
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(spAutomaticallySign);
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative("developerTeamId"));
                    if (!spAutomaticallySign.boolValue)
                    {
                        EditorGUILayout.PropertyField(settings.FindPropertyRelative("codeSignIdentity"));
                        EditorGUILayout.PropertyField(settings.FindPropertyRelative("profileId"));
                        EditorGUILayout.PropertyField(settings.FindPropertyRelative("profileSpecifier"));
                    }
                }
                EditorGUI.indentLevel--;


                // exportOptions.plist.
                EditorGUILayout.LabelField("exportOptions.plist Setting", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    var spGenerate = settings.FindPropertyRelative("generateExportOptionPlist");
                    EditorGUILayout.PropertyField(spGenerate, new GUIContent("Generate Automatically"));
                    if (spGenerate.boolValue)
                    {
                        EditorGUIEx.TextFieldWithTemplate(settings.FindPropertyRelative("exportMethod"), s_AvailableExportMethods, false);
                        EditorGUILayout.PropertyField(settings.FindPropertyRelative("uploadBitcode"));
                        EditorGUILayout.PropertyField(settings.FindPropertyRelative("uploadSymbols"));
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
	}
}