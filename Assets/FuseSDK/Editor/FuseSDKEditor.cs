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

[CustomEditor(typeof(FuseSDK))]
public class FuseSDKEditor : Editor
{
	public static readonly string ICON_PATH = "/Plugins/Android/res/drawable/ic_launcher.png";
	public static readonly int ICON_HEIGHT = 72;
	public static readonly int ICON_WIDTH = 72;

	private static readonly string API_KEY_PATTERN = @"^[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}$"; //8-4-4-4-12
	private static readonly string API_STRIP_PATTERN = @"[^\da-f\-]"; //8-4-4-4-12

	private static bool _iconFoldout = false;

	private FuseSDK _self;
	private Texture2D _logo, _icon;
	private string _error, _newIconPath;
	private bool _p31Android, _p31iOS, _unibill;
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
#if UNITY_3_5
		_logo = Resources.LoadAssetAtPath(logoPath, typeof(Texture2D)) as Texture2D;
#else
		_logo = Resources.LoadAssetAtPath<Texture2D>(logoPath);
#endif
		_icon = null;
		_error = null;
		_newIconPath = null;
		
		_p31Android = DoClassesExist("GoogleIABManager", "GoogleIAB", "GooglePurchase");
		_p31iOS = DoClassesExist("StoreKitManager", "StoreKitTransaction");
		_unibill = DoClassesExist("Unibiller");

		_idRegex = new Regex(API_KEY_PATTERN);
		_stripRegex = new Regex(API_STRIP_PATTERN);

		_self.AndroidAppID = string.IsNullOrEmpty(_self.AndroidAppID) ? string.Empty : _self.AndroidAppID;
		_self.iOSAppID = string.IsNullOrEmpty(_self.iOSAppID) ? string.Empty : _self.iOSAppID;
		_self.GCM_SenderID = string.IsNullOrEmpty(_self.GCM_SenderID) ? string.Empty : _self.GCM_SenderID;
	}

	private void OnDisable()
	{
		Resources.UnloadAsset(_logo);
		if(_icon != null)
			DestroyImmediate(_icon, true);
	}
	
	public override void OnInspectorGUI()
	{
		GUILayout.Space(8);
		if(_logo != null)
			GUILayout.Label(_logo);
		GUILayout.Space(12);

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

#if !UNITY_3_5
		bool oldAndroidIAB = _self.androidIAB;
		bool oldandroidUnibill = _self.androidUnibill;
		bool oldiosStoreKit = _self.iosStoreKit;
		bool oldiosUnibill = _self.iosUnibill;

		
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

		GUI.enabled = true;

		CheckToggle(_self.androidIAB, oldAndroidIAB, BuildTargetGroup.Android, "USING_PRIME31_ANDROID");
		CheckToggle(_self.androidUnibill, oldandroidUnibill, BuildTargetGroup.Android, "USING_UNIBILL_ANDROID");
#if UNITY_5
		CheckToggle(_self.iosStoreKit, oldiosStoreKit, BuildTargetGroup.iOS, "USING_PRIME31_IOS");
		CheckToggle(_self.iosUnibill, oldiosUnibill, BuildTargetGroup.iOS, "USING_UNIBILL_IOS");
#else
		CheckToggle(_self.iosStoreKit, oldiosStoreKit, BuildTargetGroup.iPhone, "USING_PRIME31_IOS");
		CheckToggle(_self.iosUnibill, oldiosUnibill, BuildTargetGroup.iPhone, "USING_UNIBILL_IOS");
#endif
#endif
		GUILayout.Space(4);

		if(_iconFoldout = EditorGUILayout.Foldout(_iconFoldout, "Android notification icon"))
		{
			if(_icon == null)
			{
				_icon = new Texture2D(ICON_WIDTH, ICON_HEIGHT);
				_icon.LoadImage(File.ReadAllBytes(Application.dataPath + ICON_PATH));
			}

			string path = Application.dataPath + ICON_PATH;

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

			if(!string.IsNullOrEmpty(_newIconPath) && _newIconPath != path)
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
							File.WriteAllBytes(path, _icon.EncodeToPNG());
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
		}

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Open Preferences"))
		{
			FuseSDKPrefs.Init();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		if(GUI.changed)
		{
			EditorUtility.SetDirty(target);
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

#if !UNITY_3_5
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
#endif

	[PostProcessBuild]
	public static void SendAnalytics(BuildTarget target, string path)
	{
		if(Application.isPlaying)
			return;
		try
		{
			string appID = "", bundleID, prodName, compName, gameVer, unityVer, isPro, targetPlat;
			string url = "http://api.staging.fusepowered.com/buildstats/";
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
			var fuse = AssetDatabase.LoadAssetAtPath("Assets/FuseSDK/FuseSDK.prefab", typeof(FuseSDK)) as FuseSDK;
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
			request.Timeout = 2000;

			Stream dataStream = request.GetRequestStream();
			byte[] data = Encoding.UTF8.GetBytes(query);
			dataStream.Write(data, 0, data.Length);
			dataStream.Close();

			request.BeginGetResponse(new AsyncCallback(_=>{}), null);
		}
		catch{}
	}


	[MenuItem ("FuseSDK/Regenerate Prefab", false, 0)]
	static void RegeneratePrefab()
	{
		var oldFuse = AssetDatabase.LoadAssetAtPath("Assets/FuseSDK/FuseSDK.prefab", typeof(FuseSDK)) as FuseSDK;
		
		// re-create the prefab
		Debug.Log("Creating new prefab...");
		GameObject temp = new GameObject();
		
		// add script components
		Debug.Log("Adding script components...");

#if UNITY_3_5
		temp.active = true;
#else
		temp.SetActive(true);
#endif


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

	[MenuItem("FuseSDK/Update Android Manifest", false, 1)]
	public static void UpdateAndroidManifest()
	{
		FusePostProcess.UpdateAndroidManifest(PlayerSettings.bundleIdentifier);
	}

	[MenuItem("FuseSDK/Open Documentation", false, 20)]
	static void OpenDocumentation()
	{
		Application.OpenURL(@"http://wiki.adrally.com/index.php/Unity");
	}
	
	[MenuItem("FuseSDK/Open GitHub Project", false, 21)]
	static void GoToGitHUb()
	{
		Application.OpenURL(FuseSDKUpdater.LATEST_SDK_URL);
	}
	
	[MenuItem("FuseSDK/Check For Updates", false, 40)]
	static void CheckForUpdate()
	{
		FuseSDKUpdater.CheckForUpdates(true);
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
		MinorReleases = 2,
		Bugfixes = 3,
	}

	enum ActiveAdapters
	{
		AdColony = 1,
		AppLovin = 2,
		HyprMX = 4,
	}

	private static readonly string[] AdapterFilenames = new string[]
	{
		"libFuseAdapterAdcolony.a",
		"libFuseAdapterAppLovin.a",
		"libFuseAdapterHyprMx.a",
	};

	private static readonly string ADAPTERS_KEY = "FuseSDKActiveAdapters";
	private static readonly string DISABLED_ADAPTERS_PATH = "/FuseSDK/UnusedAdapters/";
	private static readonly string ENABLED_ADAPTERS_PATH = "/Plugins/iOS/";

	[MenuItem("FuseSDK/Preferences", false, 60)]
	public static void Init()
	{
#if UNITY_3_5
		FuseSDKPrefs me = GetWindowWithRect(typeof(FuseSDKPrefs), new Rect(0, 0, 400, 100), true, "Fuse SDK Preferences") as FuseSDKPrefs;
#else
		FuseSDKPrefs me = GetWindowWithRect<FuseSDKPrefs>(new Rect(0, 0, 400, 100), true, "Fuse SDK Preferences");
#endif
		me.ShowUtility();
	}

	void OnGUI()
	{
		UpdateType updateStream = (UpdateType)Mathf.Min(EditorPrefs.GetInt(FuseSDKUpdater.AUTOUPDATE_KEY, 4) + 1, (int)UpdateType.Bugfixes);
		DownloadType autoDL = (DownloadType)EditorPrefs.GetInt(FuseSDKUpdater.AUTODOWNLOAD_KEY, 1);
		ActiveAdapters activeAdapters = (ActiveAdapters)EditorPrefs.GetInt(ADAPTERS_KEY, 7);

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
