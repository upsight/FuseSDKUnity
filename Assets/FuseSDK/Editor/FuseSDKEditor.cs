using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Net;
using System;
using Ionic.Zip;

[CustomEditor(typeof(FuseSDK))]
public class FuseSDKEditor : Editor
{
	public static readonly string ANNOUNCEMENT_KEY = "FuseSDKAnnouncement";
	public static readonly string ICON_PATH = "/Plugins/Android/res/drawable/ic_launcher.png";
	public static readonly string AAR_ICON_COPY = "/FuseSDK/Editor/ic_launcher.png";
	public static readonly string MANIFEST_COPY = "/FuseSDK/Editor/AndroidManifest.xml";
	public static readonly string AAR_PATH = "/Plugins/Android/FuseSDK.aar";
	public static readonly string VERSION_PATH = "Assets/FuseSDK/version";
	public static readonly int ICON_HEIGHT = 72;
	public static readonly int ICON_WIDTH = 72;

	private static readonly string API_KEY_PATTERN = @"^[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}$"; //8-4-4-4-12
	private static readonly string API_STRIP_PATTERN = @"[^\da-f\-]"; //8-4-4-4-12

	private static readonly string IOS_PLUGIN_M_FLAGS = "-fno-objc-arc";
	private static readonly string IOS_PLUGIN_A_FLAGS = "-ObjC";
	private static readonly string[] IOS_PLUGIN_A_FRAMEWORKS = new string[] {
		"AdSupport",
		"CoreFoundation",
		"CoreTelephony",
		"GameKit",
		"MobileCoreServices",
		"Security",
		"Social",
		"StoreKit",
		"Twitter",
		"EventKit",
		"EventKitUI",
		"MessageUI"
	};

	private static readonly string[] MANIFEST_GCM_RECEIVER_ENTRY = new string[] { "\t\t<meta-data android:name=\"com.fusepowered.replace.gcmReceiver\" android:value=\"{{timestamp}}\" />",
					"\t\t<!-- GCM -->",
					"\t\t<activity",
					"            android:name=\"com.fusepowered.unity.GCMJava\"",
					"            android:label=\"@string/app_name\" >",
					"\t\t</activity>",
					"\t\t<receiver",
					"            android:name=\"com.fusepowered.unity.FuseUnityGCMReceiver\"",
					"            android:permission=\"com.google.android.c2dm.permission.SEND\" >",
					"            <intent-filter>",
					"                <!-- Receives the actual messages. -->",
					"                <action android:name=\"com.google.android.c2dm.intent.RECEIVE\" />",
					"                <!-- Receives the registration id. -->",
					"                <action android:name=\"com.google.android.c2dm.intent.REGISTRATION\" />",
					"\t\t\t\t",
					"\t\t\t\t<meta-data android:name=\"com.fusepowered.replace.packageId\" android:value=\"{{packageId}}\" />",
					"                <category android:name=\"{{packageId}}\" />",
					"",
					"            </intent-filter>",
					"\t\t</receiver>",
					"\t\t<service android:name=\"com.fusepowered.unity.GCMIntentService\" />",
					"\t\t<!-- end -->",
	};

	private static readonly string[] MANIFEST_GCM_PERMISSIONS_ENTRY = new string[] { "\t<meta-data android:name=\"com.fusepowered.replace.gcmPermissions\" android:value=\"{{timestamp}}\" />",
					"\t<!-- Permissions for GCM -->",
					"\t<!-- GCM requires a Google account. -->",
					"\t<uses-permission android:name=\"android.permission.GET_ACCOUNTS\" />",
					"\t",
					"\t<!-- Keeps the processor from sleeping when a message is received. -->",
					"\t<uses-permission android:name=\"android.permission.WAKE_LOCK\" />",
					"\t",
					"\t<!-- Creates a custom permission so only this app can receive its messages. -->",
					"\t<meta-data android:name=\"com.fusepowered.replace.packageId\" android:value=\"{{packageId}}\" />",
					"\t<permission android:name=\"{{packageId}}.permission.C2D_MESSAGE\" android:protectionLevel=\"signature\" />",
					"",
					"\t<meta-data android:name=\"com.fusepowered.replace.packageId\" android:value=\"{{packageId}}\" />",
					"\t<uses-permission android:name=\"{{packageId}}.permission.C2D_MESSAGE\" />",
					"\t",
					"\t<!-- This app has permission to register and receive data message. -->",
					"\t<uses-permission android:name=\"com.google.android.c2dm.permission.RECEIVE\" />",
	};

	private static bool _iconFoldout = false;

	private FuseSDK _self;
	private Texture2D _logo, _icon;
	private string _error, _newIconPath;
	private bool _p31Android, _p31iOS, _unibill, _soomla;
	private bool _needReimport, _guiDrawn;
	private Regex _idRegex, _stripRegex;

	private void OnEnable()
	{
		string logoPath = "Assets/FuseSDK/logo.png";

		//Fix logo import settings
		TextureImporter importer = AssetImporter.GetAtPath(logoPath) as TextureImporter;
		if(importer != null)
		{
			importer.textureType = TextureImporterType.GUI;
			AssetDatabase.WriteImportSettingsIfDirty(logoPath);
		}

		_self = (FuseSDK)target;
		_logo = LoadAsset<Texture2D>(logoPath);
		_icon = null;
		_error = null;
		_newIconPath = null;
		_guiDrawn = false;
		
		_p31Android = DoClassesExist("GoogleIABManager", "GoogleIAB", "GooglePurchase");
		_p31iOS = DoClassesExist("StoreKitManager", "StoreKitTransaction");
		_unibill = DoClassesExist("Unibiller");
		_soomla = DoClassesExist("SoomlaStore", "StoreEvents");

		_idRegex = new Regex(API_KEY_PATTERN);
		_stripRegex = new Regex(API_STRIP_PATTERN);

		_self.AndroidAppID = string.IsNullOrEmpty(_self.AndroidAppID) ? string.Empty : _self.AndroidAppID;
		_self.iOSAppID = string.IsNullOrEmpty(_self.iOSAppID) ? string.Empty : _self.iOSAppID;
		_self.GCM_SenderID = string.IsNullOrEmpty(_self.GCM_SenderID) ? string.Empty : _self.GCM_SenderID;

		_needReimport = DoSettingsNeedUpdate();
	}

	private void OnDisable()
	{
		Resources.UnloadAsset(_logo);
		if(_icon != null)
			DestroyImmediate(_icon, true);
	}
	
	public override void OnInspectorGUI()
	{
		if(_needReimport && _guiDrawn)
		{
			UpdateAllSettings();
			_needReimport = false;
		}

#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
		Undo.RecordObject(target, "FuseSDK modified");
#endif
		_self = (FuseSDK)target;

		GUILayout.Space(8);
		if(_logo != null)
			GUILayout.Label(_logo);
		GUILayout.Space(4);

		if(_needReimport)
		{
			var oldGUIColor = GUI.color;
			var newStyle = new GUIStyle(EditorStyles.boldLabel);
			newStyle.fontStyle = FontStyle.Bold;
			newStyle.fontSize = 24;
			GUI.color = Color.yellow;
			GUILayout.Label("Updating Unity settings for Fuse...", newStyle);
			GUI.color = oldGUIColor;

			if(Event.current.type == EventType.Repaint)
			{
				_guiDrawn = true;
				EditorUtility.SetDirty(target);
			}
			return;
		}

		GUILayout.Space(4);

		_self.AndroidAppID = _stripRegex.Replace(EditorGUILayout.TextField("Android App ID", _self.AndroidAppID), "");
#if UNITY_ANDROID
		int idVer = CheckGameID(_self.AndroidAppID);
		if(idVer < -99)
			EditorGUILayout.HelpBox("Invalid App ID: Valid form is XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX where X is a haxadecimal character", MessageType.Error);
		else if(idVer < 0)
			EditorGUILayout.HelpBox("Invalid App ID: Too short", MessageType.Error);
		else if(idVer > 0)
			EditorGUILayout.HelpBox("Invalid App ID: Too long", MessageType.Error);
#endif
		_self.iOSAppID = _stripRegex.Replace(EditorGUILayout.TextField("iOS App ID", _self.iOSAppID), "");
#if UNITY_IOS
		int idVer = CheckGameID(_self.iOSAppID);
		if(idVer < -99)
			EditorGUILayout.HelpBox("Invalid App ID: Valid form is XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX where X is a haxadecimal character", MessageType.Error);
		else if(idVer < 0)
			EditorGUILayout.HelpBox("Invalid App ID: Too short", MessageType.Error);
		else if(idVer > 0)
			EditorGUILayout.HelpBox("Invalid App ID: Too long", MessageType.Error);
#endif

		GUILayout.Space(8);

		_self.GCM_SenderID = EditorGUILayout.TextField("GCM Sender ID", _self.GCM_SenderID);
		_self.registerForPushNotifications = EditorGUILayout.Toggle("Push Notifications", _self.registerForPushNotifications);

		if(_self.registerForPushNotifications && !string.IsNullOrEmpty(_self.AndroidAppID) && string.IsNullOrEmpty(_self.GCM_SenderID))
			EditorGUILayout.HelpBox("GCM Sender ID is required for Push Notifications on Android", MessageType.Warning);

		GUILayout.Space(8);
		
		EditorGUILayout.BeginHorizontal();
		_self.logging = EditorGUILayout.Toggle("Logging", _self.logging);
		GUILayout.Space(12);
		_self.StartAutomatically = EditorGUILayout.Toggle("Start Session Automatically", _self.StartAutomatically);
		EditorGUILayout.EndHorizontal();


		GUILayout.Space(16);

		bool oldAndroidIAB = _self.androidIAB;
		bool oldandroidUnibill = _self.androidUnibill;
		bool oldiosStoreKit = _self.iosStoreKit;
		bool oldiosUnibill = _self.iosUnibill;
		bool oldSoomlaStore = _self.soomlaStore;

		
		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.BeginVertical();
		GUI.enabled = _p31Android || _self.androidIAB;
		_self.androidIAB = EditorGUILayout.Toggle("Android Prime31 Billing", _self.androidIAB);

		GUI.enabled = _unibill || _self.androidUnibill;
		_self.androidUnibill = EditorGUILayout.Toggle("Android Unibill Billing", _self.androidUnibill);
		EditorGUILayout.EndVertical();

		GUILayout.Space(12);
		
		EditorGUILayout.BeginVertical();
		GUI.enabled = _p31iOS || _self.iosStoreKit;
		_self.iosStoreKit = EditorGUILayout.Toggle("iOS Prime31 Billing", _self.iosStoreKit);

		GUI.enabled = _unibill || _self.iosUnibill;
		_self.iosUnibill = EditorGUILayout.Toggle("iOS Unibill Billing", _self.iosUnibill);
		EditorGUILayout.EndVertical();

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();

		GUI.enabled = _soomla || _self.soomlaStore;
		_self.soomlaStore = EditorGUILayout.Toggle("Soomla Store", _self.soomlaStore);
		
		EditorGUILayout.EndHorizontal();

		GUI.enabled = true;

		CheckToggle(_self.androidIAB, oldAndroidIAB, BuildTargetGroup.Android, "USING_PRIME31_ANDROID");
		CheckToggle(_self.androidUnibill, oldandroidUnibill, BuildTargetGroup.Android, "USING_UNIBILL_ANDROID");
		CheckToggle(_self.soomlaStore, oldSoomlaStore, BuildTargetGroup.Android, "USING_SOOMLA_IAP");
#if UNITY_5
		CheckToggle(_self.iosStoreKit, oldiosStoreKit, BuildTargetGroup.iOS, "USING_PRIME31_IOS");
		CheckToggle(_self.iosUnibill, oldiosUnibill, BuildTargetGroup.iOS, "USING_UNIBILL_IOS");
		CheckToggle(_self.soomlaStore, oldSoomlaStore, BuildTargetGroup.iOS, "USING_SOOMLA_IAP");
#else
		CheckToggle(_self.iosStoreKit, oldiosStoreKit, BuildTargetGroup.iPhone, "USING_PRIME31_IOS");
		CheckToggle(_self.iosUnibill, oldiosUnibill, BuildTargetGroup.iPhone, "USING_UNIBILL_IOS");
		CheckToggle(_self.soomlaStore, oldSoomlaStore, BuildTargetGroup.iPhone, "USING_SOOMLA_IAP");
#endif

		GUILayout.Space(4);

		if(_iconFoldout = EditorGUILayout.Foldout(_iconFoldout, "Android notification icon"))
		{
			if(_icon == null)
			{
				_icon = new Texture2D(ICON_WIDTH, ICON_HEIGHT);
				_icon.LoadImage(File.ReadAllBytes(LoadIcon()));
			}

			GUILayout.Space(10);
			
			if(GUILayout.Button("Click to select icon:", EditorStyles.label))
			{
				_newIconPath = EditorUtility.OpenFilePanel("Choose icon", Application.dataPath, "png");
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button(_icon, EditorStyles.objectFieldThumb, GUILayout.Height(75), GUILayout.Width(75)))
			{
				_newIconPath = EditorUtility.OpenFilePanel("Choose icon", Application.dataPath, "png");
			}

			if(_error == null)
				EditorGUILayout.HelpBox("Your texture must be " + ICON_WIDTH + "x" + ICON_HEIGHT + " pixels", MessageType.None);
			else
				EditorGUILayout.HelpBox(_error, MessageType.Error);

			GUILayout.EndHorizontal();

			if(!string.IsNullOrEmpty(_newIconPath) && _newIconPath != (Application.dataPath + ICON_PATH))
			{
				UpdateIcon();
			}
			else
			{
				_newIconPath = null;
				_error = null;
			}
		}
		else if(_icon != null)
		{
			DestroyImmediate(_icon);
			_icon = null;
#if UNITY_5
			string path = LoadIcon(true);
			if(File.Exists(path))
				File.Delete(path);

			path += ".meta";
			if(File.Exists(path))
				File.Delete(path);
#endif
		}

		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Open Preferences"))
		{
			FuseSDKPrefs.Init();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		//GUILayout.BeginHorizontal();
		//GUILayout.FlexibleSpace();
		//Color oldColor = GUI.color;
		//GUI.color = Color.green;
		//GUILayout.Label("Ready to build!", EditorStyles.boldLabel);
		//GUI.color = oldColor;
		//GUILayout.FlexibleSpace();
		//GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		if(GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}

	private string LoadIcon(bool getOnly = false)
	{
#if UNITY_5
		string path = Application.dataPath + AAR_ICON_COPY;
		if(!getOnly && File.GetLastWriteTime(path) < File.GetLastWriteTime(Application.dataPath + AAR_PATH))
		{
			try
			{
				string tempPath = "/FuseSDK/Editor/.fusetemp/";
				Directory.CreateDirectory(Application.dataPath + tempPath);
				using(var aar = ZipFile.Read(Application.dataPath + AAR_PATH))
				{
					aar.ExtractSelectedEntries("ic_launcher.png", "res/drawable", Application.dataPath + tempPath, ExtractExistingFileAction.OverwriteSilently);
				}
				File.Copy(Application.dataPath + tempPath + "res/drawable/ic_launcher.png", path, true);
				Directory.Delete(Application.dataPath + tempPath, true);
			}
			catch(Exception e)
			{
				_error = "Error extracting icon: " + e.Message;
			}
		}
		return path;
#else
		return Application.dataPath + ICON_PATH;
#endif
	}

	private void UpdateIcon()
	{
		try
		{
			var bytes = File.ReadAllBytes(_newIconPath);
			string header = System.Text.Encoding.ASCII.GetString(bytes, 1, 3);
			_icon.LoadImage(bytes);
			if((bytes[0] == 'J' && header == "FIF") || (bytes[0] == (byte)0x89 && header == "PNG"))
			{
				if(_icon.height == ICON_HEIGHT && _icon.width == ICON_WIDTH)
				{
#if UNITY_5
					string path = Application.dataPath + AAR_ICON_COPY;
					File.WriteAllBytes(path, _icon.EncodeToPNG());
					try
					{
						using(var aar = ZipFile.Read(Application.dataPath + AAR_PATH))
						{
							aar.UpdateFile(path, "res/drawable");
							aar.Save();
						}
					}
					catch(Exception e)
					{
						_error = "Error writing to FuseSDK.aar: " + e.Message;
					}
					File.Delete(path);
#else
					File.WriteAllBytes(Application.dataPath + ICON_PATH, _icon.EncodeToPNG());
#endif
					_newIconPath = null;
					_error = null;
				}
				else
				{
					_error = "The image is not " + ICON_WIDTH + "x" + ICON_HEIGHT + " pixels.";
				}
			}
			else
			{
				_error = "File is not a valid image.";
			}
		}
		catch
		{
			_error = "File could not be read.";
		}
	}

	private int CheckGameID(string id)
	{
		if(id.Length != 36)
			return id.Length - 36;
		return _idRegex.Match(id).Success ? 0 : -100;
	}

	private bool DoClassesExist(params string[] classes)
	{
		var types = from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
			from type in assembly.GetTypes()
			select type.Name;

		return (classes == null)
			|| (classes.Length == 0)
			|| (classes.Length == 1 && types.Contains(classes[0]))
			|| (classes.Length == classes.Intersect(types).Distinct().Count());
	}

	private void CheckToggle(bool newVal, bool oldVal, BuildTargetGroup buildGroup, string tag)
	{
		if(oldVal != newVal)
		{
			string oldDef = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup);
			
			if(oldDef.Contains(tag) && !newVal)
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, string.Join(";", oldDef.Split(';').Where(s => s != tag).ToArray()));
			}
			else if(!oldDef.Contains(tag) && newVal)
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, tag + (oldDef.Length != 0 ? ";" + oldDef : ""));
			}
		}
	}

	private bool DoSettingsNeedUpdate()
	{
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
		string currentVersion, metaVersion, bundleId;
		if(!FuseSDKUpdater.ReadVersionFile(out currentVersion, out metaVersion))
			return true;

		var versionMeta = AssetImporter.GetAtPath(VERSION_PATH);
		var sp = (versionMeta.userData ?? string.Empty).Split('#');
		metaVersion = (sp.Length < 1) ? string.Empty : sp[0];
		bundleId = (sp.Length < 2) ? string.Empty : sp[1];

		string _ = null;
		int[] actualVer = FuseSDKUpdater.ParseVersion(currentVersion, ref _);
		int[] metaVer = FuseSDKUpdater.ParseVersion(metaVersion, ref _);

		if(metaVer == null || bundleId != PlayerSettings.bundleIdentifier || FuseSDKUpdater.HowOldIsVersion(metaVer, actualVer) >= 0)
			return true;

		List<PluginImporter> iosPlugins =
			Directory.GetFiles(Application.dataPath + "/Plugins/iOS")
			.Select(file => file.Substring(file.LastIndexOfAny(new char[] { '\\', '/' }) + 1))
			.Where(file => !file.EndsWith(".meta"))
			.Where(file => file.Contains("FuseSDK") || file.Contains("libFuse") || file.Contains("FuseUnity") || file.Contains("NSData-Base64"))
			.Select(file => PluginImporter.GetAtPath("Assets/Plugins/iOS/" + file) as PluginImporter)
			.Where(plugin => plugin != null)
			.ToList();

		foreach(var plugin in iosPlugins)
		{
			if(plugin.assetPath.EndsWith(".m"))
			{
				string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
				if(!flags.Contains(IOS_PLUGIN_M_FLAGS))
					return true;
			}
			else if(plugin.assetPath.EndsWith(".a"))
			{
				string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
				var frameworks = plugin.GetPlatformData(BuildTarget.iOS, "FrameworkDependencies").Split(';').Where(f => !string.IsNullOrEmpty(f));
				if(!flags.Contains(IOS_PLUGIN_A_FLAGS) || frameworks.Intersect(IOS_PLUGIN_A_FRAMEWORKS).Count() != IOS_PLUGIN_A_FRAMEWORKS.Length)
					return true;
			}
		}

		List<PluginImporter> androidPlugins =
			Directory.GetFiles(Application.dataPath + "/Plugins/Android")
			.Union(Directory.GetDirectories(Application.dataPath + "/Plugins/Android", "res"))
			.Select(file => file.Substring(file.LastIndexOfAny(new char[] { '\\', '/' }) + 1))
			.Where(file => !file.EndsWith(".meta"))
			.Where(file => file.Contains("FuseSDK") || file.Contains("FuseUnity") || file.Contains("android-support-v4.jar") || file.Contains("google-play-services.jar") || file == "res")
			.Select(file => PluginImporter.GetAtPath("Assets/Plugins/Android/" + file) as PluginImporter)
			.Where(plugin => plugin != null)
			.ToList();

		foreach(var plugin in androidPlugins)
		{
			if(!plugin.GetCompatibleWithPlatform(BuildTarget.Android))
				return true;
		}
#endif

		return false;
	}

	private void UpdateAllSettings()
	{
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
		string currentVersion, _;
		if(!FuseSDKUpdater.ReadVersionFile(out currentVersion, out _))
			return;

		var versionMeta = AssetImporter.GetAtPath(VERSION_PATH);

		List<PluginImporter> iosPlugins =
			Directory.GetFiles(Application.dataPath + "/Plugins/iOS")
			.Select(file => file.Substring(file.LastIndexOfAny(new char[] { '\\', '/' }) + 1))
			.Where(file => !file.EndsWith(".meta"))
			.Where(file => file.Contains("FuseSDK") || file.Contains("libFuse") || file.Contains("FuseUnity") || file.Contains("NSData-Base64"))
			.Select(file => PluginImporter.GetAtPath("Assets/Plugins/iOS/" + file) as PluginImporter)
			.Where(plugin => plugin != null)
			.ToList();

		foreach(var plugin in iosPlugins)
		{
			plugin.SetCompatibleWithAnyPlatform(false);
			plugin.SetCompatibleWithPlatform(BuildTarget.iOS, true);
			if(plugin.assetPath.EndsWith(".m"))
			{
				string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
				if(!flags.Contains(IOS_PLUGIN_M_FLAGS))
				{
					plugin.SetPlatformData(BuildTarget.iOS, "CompileFlags", flags + " " + IOS_PLUGIN_M_FLAGS);
				}
			}
			else if(plugin.assetPath.EndsWith(".a"))
			{
				string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
				if(!flags.Contains(IOS_PLUGIN_A_FLAGS))
				{
					plugin.SetPlatformData(BuildTarget.iOS, "CompileFlags", flags + " " + IOS_PLUGIN_A_FLAGS);
				}
				var frameworks = plugin.GetPlatformData(BuildTarget.iOS, "FrameworkDependencies").Split(';');
				string combined = frameworks.Union(IOS_PLUGIN_A_FRAMEWORKS).Where(f => !string.IsNullOrEmpty(f)).Aggregate((accum, f) => accum + ";" + f) + ";";
				plugin.SetPlatformData(BuildTarget.iOS, "FrameworkDependencies", combined);
			}

			plugin.SaveAndReimport();
		}

#if !UNITY_5_0 && !UNITY_5_1
		PlayerSettings.iOS.allowHTTPDownload = true;
#endif

		List<PluginImporter> androidPlugins =
			Directory.GetFiles(Application.dataPath + "/Plugins/Android")
			.Union(Directory.GetDirectories(Application.dataPath + "/Plugins/Android", "res"))
			.Select(file => file.Substring(file.LastIndexOfAny(new char[] { '\\', '/' }) + 1))
			.Where(file => !file.EndsWith(".meta"))
			.Where(file => file.Contains("FuseSDK") || file.Contains("FuseUnity") || file.Contains("android-support-v4.jar") || file.Contains("google-play-services.jar") || file == "res")
			.Select(file => PluginImporter.GetAtPath("Assets/Plugins/Android/" + file) as PluginImporter)
			.Where(plugin => plugin != null)
			.ToList();

		foreach(var plugin in androidPlugins)
		{
			plugin.SetCompatibleWithAnyPlatform(false);
			plugin.SetCompatibleWithPlatform(BuildTarget.Android, true);
			plugin.SaveAndReimport();
		}

		UpdateAndroidManifest();

		versionMeta.userData = currentVersion + "#" + PlayerSettings.bundleIdentifier;
		versionMeta.SaveAndReimport();
#endif
	}

#if !FUSE_DISABLE_INTERNAL_ANALYTICS
	[PostProcessBuild]
	public static void SendAnalytics(BuildTarget target, string path)
	{
		if(Application.isPlaying)
			return;
		try
		{
			string appID = "", bundleID, prodName, compName, gameVer, unityVer, isPro, targetPlat;
			string url = "http://api.fusepowered.com/unity/buildstats.php";
			string baseJson = @"{
							""game_id"" : ""{{game_id}}"",
							""bundle_id"" : ""{{bundle_id}}"",
							""product_name"" : ""{{product_name}}"",
							""company_name"" : ""{{company_name}}"",
							""game_ver"" : ""{{game_ver}}"",
							""platform"": 
								{
									""version"": ""{{version}}"",
									""is_pro"": ""{{is_pro}}"",
									""target"": ""{{target}}""
								}
							}";

			//App ID
			var fuse = LoadAsset<FuseSDK>("Assets/FuseSDK/FuseSDK.prefab");
			if(fuse != null)
			{
#if UNITY_IOS
				appID = fuse.iOSAppID;
#elif UNITY_ANDROID
				appID = fuse.AndroidAppID;
#endif
			}

			//Bundle ID
			bundleID = PlayerSettings.bundleIdentifier;
			
			//Bundle ID
			prodName = PlayerSettings.productName;
			
			//Bundle ID
			compName = PlayerSettings.companyName;

			//Game Ver
			gameVer = PlayerSettings.bundleVersion;

			//Unity version
			unityVer = Application.unityVersion;
			
			//Unity Pro
			isPro = PlayerSettings.advancedLicense ? "1" : "0";

			//Target platform
			targetPlat = target.ToString();

			//Fill out json
			string json = System.Text.RegularExpressions.Regex.Replace(baseJson, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1")
				.Replace("{{game_id}}", appID)
				.Replace("{{bundle_id}}", bundleID)
				.Replace("{{product_name}}", prodName)
				.Replace("{{company_name}}", compName)
				.Replace("{{game_ver}}", gameVer)
				.Replace("{{version}}", unityVer)
				.Replace("{{is_pro}}", isPro)
				.Replace("{{target}}", targetPlat);

			string query = "d=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

			WebRequest request = WebRequest.Create(url);
			var creds = new CredentialCache();
			creds.Add(new Uri(url), "Basic", new NetworkCredential("jimmyjimmyjango", "awP2yTECEbXErKcn")); //oh noes, you gots our passw0rds
			request.Credentials = creds;
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = query.Length;
			request.Timeout = 500;

			Stream dataStream = request.GetRequestStream();
			byte[] data = Encoding.UTF8.GetBytes(query);
			dataStream.Write(data, 0, data.Length);
			dataStream.Close();

			request.BeginGetResponse(new AsyncCallback(_=>{}), null);
		}
		catch{}
	}
#endif

	public static void CheckForAnnouncements()
	{
		var stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Reset();
		stopwatch.Start();

		try
		{
			using(WWW req = new WWW(FuseSDKUpdater.ANNOUNCEMENT_URL))
			{
				while(!req.isDone && string.IsNullOrEmpty(req.error) && stopwatch.ElapsedMilliseconds < (FuseSDKUpdater.TIMEOUT / 2)) ;
				if(req.isDone && string.IsNullOrEmpty(req.error))
				{
					int id;
					string message;
					string fileContent = req.text;
					var parts = fileContent.Split(new char[] { ':' }, 2);
					if(parts.Length != 2 || !int.TryParse(parts[0], out id) || string.IsNullOrEmpty(message = parts[1]))
						return;

					int lastSeenMessage = EditorPrefs.GetInt(ANNOUNCEMENT_KEY, -1);
					if(lastSeenMessage < id)
					{
						Debug.Log("Fuse Announcement: " + message);
						EditorPrefs.SetInt(ANNOUNCEMENT_KEY, id);
					}
				}
			}
		}
		catch{}
		stopwatch.Stop();
	}
	
	[MenuItem ("FuseSDK/Regenerate Prefab", false, 0)]
	static void RegeneratePrefab()
	{
		var oldFuse = LoadAsset<FuseSDK>("Assets/FuseSDK/FuseSDK.prefab");
		
		// re-create the prefab
		Debug.Log("Creating new prefab...");
		GameObject temp = new GameObject();
		
		// add script components
		Debug.Log("Adding script components...");

		temp.SetActive(true);

		var newFuse = temp.AddComponent<FuseSDK>();

		if(oldFuse != null)
		{
			// copy fields
			Debug.Log("Copying settings into new prefab...");

			newFuse.AndroidAppID = oldFuse.AndroidAppID;
			newFuse.iOSAppID = oldFuse.iOSAppID;
			newFuse.GCM_SenderID = oldFuse.GCM_SenderID;

			DestroyImmediate(oldFuse, true);
		}
		
		// delete the prefab
		Debug.Log("Deleting old prefab...");
		AssetDatabase.DeleteAsset("Assets/FuseSDK/FuseSDK.prefab");
		
		// save the prefab
		Debug.Log("Saving new prefab...");
		PrefabUtility.CreatePrefab("Assets/FuseSDK/FuseSDK.prefab", temp);
		DestroyImmediate (temp); // Clean up our Object
		AssetDatabase.SaveAssets();
	}

	//FusePostProcess.OnPostProcessScene calls this by the menu name
	[MenuItem("FuseSDK/Update Android Manifest", false, 1)]
	public static void UpdateAndroidManifest()
	{
		string packageName = PlayerSettings.bundleIdentifier;
		string path = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";

		if(string.IsNullOrEmpty(packageName))
			return;

#if UNITY_5
		path = Application.dataPath + MANIFEST_COPY;
		try
		{
			string tempPath = "/FuseSDK/Editor/.fusetemp/";
			Directory.CreateDirectory(Application.dataPath + tempPath);
			using(var aar = ZipFile.Read(Application.dataPath + AAR_PATH))
			{
				aar.ExtractSelectedEntries("AndroidManifest.xml", "", Application.dataPath + tempPath, ExtractExistingFileAction.OverwriteSilently);
			}
			File.Copy(Application.dataPath + tempPath + "AndroidManifest.xml", path, true);
			Directory.Delete(Application.dataPath + tempPath, true);
		}
		catch(Exception e)
		{
			Debug.LogError("Error extracting AndroidManifest.xml. Unable to set package ID: " + e.Message);
			File.Delete(path);
			return;
		}
#endif

		Regex packageIdRegex = new Regex(@"\s*<\s*meta-data\s+android:name\s*=\s*""com.fusepowered.replace.packageId""\s*android:value\s*=\s*""(?<id>\S+)""\s*/>.*", RegexOptions.Singleline);
		Regex gcmReceiverRegex = new Regex(@"\s*<\s*meta-data\s+android:name\s*=\s*""com.fusepowered.replace.gcmReceiver""\s*android:value\s*=\s*""(?<time>\S+)""\s*/>.*", RegexOptions.Singleline);
		Regex gcmPermissionsRegex = new Regex(@"\s*<\s*meta-data\s+android:name\s*=\s*""com.fusepowered.replace.gcmPermissions""\s*android:value\s*=\s*""(?<time>\S+)""\s*/>.*", RegexOptions.Singleline);
		string[] manifest = File.ReadAllLines(path);
		List<string> newManifest = new List<string>();

		string tsNow = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.ToString("F0");

		if(manifest.Length > 0)
		{
			for(int i = 0; i < manifest.Length; i++)
			{
				Match match;
				string captured;
				long time;
				if((match = gcmReceiverRegex.Match(manifest[i])).Success && !string.IsNullOrEmpty(captured = match.Groups["time"].Value) && long.TryParse(captured, out time))
				{
					if(time == 0)
					{
						//Manifest doesn't contain the GCM Receiver entries
						foreach(var l in MANIFEST_GCM_RECEIVER_ENTRY)
							newManifest.Add(l.Replace("{{timestamp}}", tsNow).Replace("{{packageId}}", packageName));
					}
					else
					{
						//Manifest contains the GCM Receiver entries, update them by continuing
						newManifest.Add(manifest[i].Replace("{{timestamp}}", tsNow));
					}

				}
				else if((match = gcmPermissionsRegex.Match(manifest[i])).Success && !string.IsNullOrEmpty(captured = match.Groups["time"].Value) && long.TryParse(captured, out time))
				{
					if(time == 0)
					{
						//Manifest doesn't contain the GCM Permissions entries
						foreach(var l in MANIFEST_GCM_PERMISSIONS_ENTRY)
							newManifest.Add(l.Replace("{{timestamp}}", tsNow).Replace("{{packageId}}", packageName));
					}
					else
					{
						//Manifest contains the GCM Permissions entries, update them by continuing
						newManifest.Add(manifest[i].Replace("{{timestamp}}", tsNow));
					}
				}
				else if((match = packageIdRegex.Match(manifest[i])).Success && !string.IsNullOrEmpty(captured = match.Groups["id"].Value) && captured != packageName)
				{
					//Found pre-existing entries, update them
					newManifest.Add(manifest[i].Replace(captured, packageName));
					i++;
					newManifest.Add(manifest[i].Replace(captured, packageName));

					if(!manifest[i].Contains(packageName))
					{
						Debug.LogWarning("Fuse SDK: Android Manifest has been changed manually. Unable to set package ID.");
					}
				}
				else
				{
					//Line doesn't match anything special, just copy it over
					newManifest.Add(manifest[i]);
				}
			}
		}
		else
		{
			Debug.LogError("Fuse SDK: Unable to read Android Manifest. Unable to set package ID.");
			return;
		}

		File.WriteAllLines(path, newManifest.ToArray());

#if UNITY_5
		try
		{
			using(var aar = ZipFile.Read(Application.dataPath + AAR_PATH))
			{
				aar.UpdateFile(path, "");
				aar.UpdateFile(path, "aapt");
				aar.Save();
			}
		}
		catch(Exception e)
		{
			Debug.LogError("Error writing AndroidManifest.xml to aar. Unable to set package ID: " + e.Message);
		}
		File.Delete(path);
#endif
	}

	[MenuItem("FuseSDK/Open Documentation", false, 20)]
	static void OpenDocumentation()
	{
		Application.OpenURL(@"http://wiki.fusepowered.com/index.php/Unity");
	}
	
	[MenuItem("FuseSDK/Open GitHub Project", false, 21)]
	static void GoToGitHub()
	{
		Application.OpenURL(FuseSDKUpdater.LATEST_SDK_URL);
	}
	
	[MenuItem("FuseSDK/Check For Updates", false, 40)]
	static void CheckForUpdate()
	{
		FuseSDKUpdater.CheckForUpdates(true);
	}

	internal static T LoadAsset<T>(string path) where T : UnityEngine.Object
	{
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
		return Resources.LoadAssetAtPath<T>(path);
#else
		return AssetDatabase.LoadAssetAtPath<T>(path);
#endif
	}
}

public class FuseSDKPrefs : EditorWindow
{
	public enum DownloadType
	{
		AutoDownloadAndImport = 0,
		AutoDownloadAndPromptForImport = 1,
		AutoDownloadOnly = 2,
		GoToWebsite = 3,
		AskEverytime = 4,
	}

	enum UpdateType
	{
		Never = 0,
		MajorReleases = 1,
		RegularReleases = 2,
		Bugfixes = 3,
	}

	enum ActiveAdapters
	{
		AdColony = 1 << 0,
		AppLovin = 1 << 1,
		HyprMX = 1 << 2,
		LeadBolt = 1 << 3,
		NativeX = 1 << 4,
		AerServ = 1 << 5,
	}

	private static readonly string[] AdapterFilenames = new string[]
	{
		"libFuseAdapterAdcolony.a",
		"libFuseAdapterAppLovin.a",
		"libFuseAdapterHyprMx.a",
		"libFuseAdapterLeadBolt.a",
		"libFuseAdapterNativeX.a",
		"libFuseAdapterAerServ.a",
	};

	private static readonly string ADAPTERS_KEY = "FuseSDKActiveAdapters";
	private static readonly string DISABLED_ADAPTERS_PATH = "/FuseSDK/UnusedAdapters/";
	private static readonly string ENABLED_ADAPTERS_PATH = "/Plugins/iOS/";

	[MenuItem("FuseSDK/Preferences", false, 60)]
	public static void Init()
	{
		GetWindowWithRect<FuseSDKPrefs>(new Rect(0, 0, 400, 100), true, "Fuse SDK Preferences").ShowUtility();
	}

	void OnGUI()
	{
		UpdateType updateStream = (UpdateType)Mathf.Min(EditorPrefs.GetInt(FuseSDKUpdater.AUTOUPDATE_KEY, 4) + 1, (int)UpdateType.Bugfixes);
		DownloadType autoDL = (DownloadType)EditorPrefs.GetInt(FuseSDKUpdater.AUTODOWNLOAD_KEY, 1);
		ActiveAdapters activeAdapters = (ActiveAdapters)EditorPrefs.GetInt(ADAPTERS_KEY, 31);

		UpdateType newStream = (UpdateType)EditorGUILayout.EnumPopup("Auto Update Checking", updateStream);
		
		if(updateStream != newStream)
			EditorPrefs.SetInt(FuseSDKUpdater.AUTOUPDATE_KEY, ((int)newStream) - 1);

		if(newStream != UpdateType.Never)
		{
			EditorGUILayout.Space();
			DownloadType newDL = (DownloadType)EditorGUILayout.EnumPopup("'Download now' action", autoDL);

			if(newDL != autoDL)
				EditorPrefs.SetInt(FuseSDKUpdater.AUTODOWNLOAD_KEY, (int)newDL);
		}

		GUILayout.Space(30);

		ActiveAdapters newAdapters = (ActiveAdapters)EditorGUILayout.EnumMaskField("Active Adapters", activeAdapters);

		if(newAdapters != activeAdapters)
		{
			EditorPrefs.SetInt(ADAPTERS_KEY, (int)newAdapters);
			UpdateAdapters((uint)activeAdapters, (uint)newAdapters);
		}
	}

	private void UpdateAdapters(uint oldAdapters, uint newAdapters)
	{
		if(!Directory.Exists(Application.dataPath + DISABLED_ADAPTERS_PATH))
			Directory.CreateDirectory(Application.dataPath + DISABLED_ADAPTERS_PATH);

		for(int i = 0; i < AdapterFilenames.Length && (oldAdapters >> i > 0 || newAdapters >> i > 0); i++)
		{
			if((oldAdapters>>i) % 2 > (newAdapters>>i) % 2)
			{
				try
				{
					File.Move(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i], Application.dataPath + DISABLED_ADAPTERS_PATH + AdapterFilenames[i]);
				}
				catch(IOException)
				{
					if(File.Exists(Application.dataPath + DISABLED_ADAPTERS_PATH + AdapterFilenames[i]) && File.Exists(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]))
					{
						File.Delete(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]);
					}
					else if(!File.Exists(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]) && !File.Exists(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]))
					{
						Debug.LogWarning("FuseSDK: Could not disable " + AdapterFilenames[i] + ". File does not exist.");
					}
				}
				catch(System.UnauthorizedAccessException)
				{
					Debug.LogWarning("FuseSDK: Could not disable " + AdapterFilenames[i] + ". You do not have the required permission to move " + Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]);
				}
			}
			else if((oldAdapters>>i) % 2 < (newAdapters>>i) % 2)
			{
				try
				{
					File.Move(Application.dataPath + DISABLED_ADAPTERS_PATH + AdapterFilenames[i], Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]);
				}
				catch(IOException)
				{
					if(File.Exists(Application.dataPath + DISABLED_ADAPTERS_PATH + AdapterFilenames[i]) && File.Exists(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]))
					{
						File.Delete(Application.dataPath + DISABLED_ADAPTERS_PATH + AdapterFilenames[i]);
					}
					else if(!File.Exists(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]) && !File.Exists(Application.dataPath + ENABLED_ADAPTERS_PATH + AdapterFilenames[i]))
					{
						Debug.LogWarning("FuseSDK: Could not enable " + AdapterFilenames[i] + ". File does not exist.");
					}
				}
				catch(System.UnauthorizedAccessException)
				{
					Debug.LogWarning("FuseSDK: Could not enable " + AdapterFilenames[i] + ". You do not have the required permission to move " + Application.dataPath + DISABLED_ADAPTERS_PATH + AdapterFilenames[i]);
				}
			}
		}
		AssetDatabase.Refresh();
	}
}
