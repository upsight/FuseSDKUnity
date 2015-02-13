using UnityEngine;
using UnityEditor;
using System.Collections;

[InitializeOnLoad]
public static class FuseAPIUpdater
{
	public static readonly string LATEST_SDK_URL = @"https://github.com/fusepowered/FuseSDKUnity";
	public static readonly string LATEST_PACKAGE = @"https://raw.githubusercontent.com/fusepowered/FuseSDKUnity/master/FuseUnitySDK.unitypackage";
	public static readonly string UPDATE_URL = @"https://raw.githubusercontent.com/fusepowered/FuseSDKUnity/master/Assets/FuseAPI/version";
	public static readonly long TIMEOUT = 1000;
	public static readonly string AUTOUPDATE_KEY = "FuseSDKAutoUpdate";
	public static readonly string AUTODOWNLOAD_KEY = "FuseSDKAutoDownload";
	public static readonly string TEMP_PACKAGE_NAME = "/FuseSDK.temp.unitypackage";
	public static readonly string SAVED_PACKAGE_NAME = "/FuseSDKPackage.unitypackage";
	public static readonly string VERSION_PATH = "/FuseAPI/version";

	public static System.Net.WebClient Downloader = null;
	public static float DownloadProgress = 0f;
	public static long KBytes = 0;
	public static long TotalKBytes = 0;
	public static bool FinishedDL = false;
	public static bool DLError = false;

	private static int _delay = 1;
	private static bool _DoNotImport = false;

	static FuseAPIUpdater()
	{
		if(UnityEditorInternal.InternalEditorUtility.inBatchMode
		   || !UnityEditorInternal.InternalEditorUtility.isHumanControllingUs
		   || Application.isPlaying
		   || EditorApplication.isPlaying
		   || EditorApplication.isPlayingOrWillChangePlaymode
		   || EditorApplication.timeSinceStartup > 5
		   || EditorPrefs.GetInt(AUTOUPDATE_KEY, 4) < 0)
			return;

		EditorApplication.update += Update;
	}

	private static void Update()
	{
		//Wait 1 frame before checking for update to make sure Unity loaded
		if(--_delay < 0)
			CheckForUpdates();
	}

	public static void CheckForUpdates(bool force = false)
	{
		EditorApplication.update -= Update;

		string latestVersion = "";
		string versionFile = "";
		string ignoreVersion = "";
		string currentVersion = "";
		var stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Reset();
		stopwatch.Start();

		try
		{
			using(WWW req = new WWW(UPDATE_URL))
			{
				while(!req.isDone && string.IsNullOrEmpty(req.error) && stopwatch.ElapsedMilliseconds < TIMEOUT);
				if(req.isDone && string.IsNullOrEmpty(req.error))
				{
					latestVersion = req.text;
				}
			}
		}
		catch
		{
			return;
		}
		stopwatch.Stop();

		try
		{
			versionFile = System.IO.File.ReadAllText(Application.dataPath + VERSION_PATH);
		}
		catch
		{
			Debug.LogWarning("Fuse SDK: Couldn't check for update, your project does not contain a version file.");
			Debug.Log("You can find the latest FuseSDK at " + LATEST_SDK_URL);
			return;
		}

		var sp = versionFile.Split('#');
		currentVersion = (sp.Length < 1) ? "" : sp[0];
		ignoreVersion = (sp.Length < 2 || force) ? "" : sp[1];

		int[] myVer = ParseVersion(currentVersion);
		int[] ignore = ParseVersion(ignoreVersion);
		int[] newVer = ParseVersion(latestVersion);

		if(myVer == null)
		{
			Debug.LogWarning("Fuse SDK: Couldn't check for update, your version file is corrupt.");
			Debug.Log("You can find the latest FuseSDK at " + LATEST_SDK_URL);
			return;
		}
		if(newVer == null)
		{
			Debug.LogWarning("Fuse SDK: Couldn't check for update.");
			Debug.Log("You can find the latest FuseSDK at " + LATEST_SDK_URL);
			return;
		}
		if(ignore != null && HowOldIsVersion(ignore, newVer) == -1)
		{
			Debug.Log("Fuse SDK: A new version is available. Get the latest from " + LATEST_SDK_URL);
			return;
		}

		int outOfDate = HowOldIsVersion(myVer, newVer);

		string[] revType = new string[] {"This is a major release!", "This is a minor release.", "This is a bugfix revision.", "This is a bugfix revision."};
		if(outOfDate > -1 && (EditorPrefs.GetInt(AUTOUPDATE_KEY, 4) >= outOfDate || force))
		{
			int retVal = EditorUtility.DisplayDialogComplex("An update to Fuse SDK is available", 
			                                                revType[outOfDate] + System.Environment.NewLine +
			                                                "Your version: " + currentVersion + System.Environment.NewLine +
			                                                "New version: " + latestVersion,
			                                                "Download Now",
			                                                "Skip version",
			                                                "Remind me later");

			if(retVal == 0)
			{
				FuseSDKPrefs.DownloadType dlType = (FuseSDKPrefs.DownloadType) EditorPrefs.GetInt(AUTODOWNLOAD_KEY, 1);

				if(dlType == FuseSDKPrefs.DownloadType.GoToWebsite)
					Application.OpenURL(LATEST_SDK_URL);
				else if(dlType == FuseSDKPrefs.DownloadType.AskEverytime)
				{
					int dlRet = EditorUtility.DisplayDialogComplex("Fuse SDK Update", "What would you like to do?",
					                                                "Download and import",
					                                                "Download only",
					                                                "Go to website");
					if(dlRet == 0)
					{
						StartDownload();
					}
					else if(dlRet == 1)
					{
						_DoNotImport = true;
						StartDownload();
					}
					else if(dlRet == 2)
					{
						Application.OpenURL(LATEST_SDK_URL);
					}
				}
				else
					StartDownload();
			}
			else if(retVal == 1)
			{
				//NEVER AGAIN
				if(ignore != null)
				{
					//The second time the decline the update on either a revision or native build, ignore it forever
					int diff = HowOldIsVersion(ignore, newVer);
					if(diff > 2)
						latestVersion = newVer[0] + "." + newVer[1] + "." + newVer[2] + ".999";
					else if(diff == 2)
						latestVersion = newVer[0] + "." + newVer[1] + ".999.999";
				}
				try
				{
					System.IO.File.WriteAllText(Application.dataPath + VERSION_PATH, currentVersion + "#" + latestVersion);
				}
				catch
				{
					Debug.LogError("Fuse SDK: Couldn't write changes to Assets" + VERSION_PATH);
					return;
				}
			}
			else if(retVal == 2)
			{
				//LATER
			}
		}
		else if(force)
		{
			EditorUtility.DisplayDialog("Your version is up to date!", "Version: " + currentVersion, "OK");
		}
	}

	private static int[] ParseVersion(string version)
	{
		var ver = version.Split('.');
		if(ver.Length < 4)
			return null;

		int[] verInfo = new int[4];
		if(int.TryParse(ver[0], out verInfo[0])
		   && int.TryParse(ver[1], out verInfo[1])
		   && int.TryParse(ver[2], out verInfo[2])
		   && int.TryParse(ver[3], out verInfo[3]))
			return verInfo;
		else
			return null;
	}

	private static int HowOldIsVersion(int[] localVersion, int[] latestVersion)
	{
		for(int i = 0; i < localVersion.Length; i++)
			if(localVersion[i] < latestVersion[i])
				return i;
			else if(localVersion[i] > latestVersion[i])
				return -1;
		return -1;
	}

	private static void StartDownload()
	{
		DownloadProgress = 0f;
		KBytes = 0;
		TotalKBytes = 0;
		FinishedDL = false;
		DLError = false;

		Debug.Log("Downloading to " + Application.temporaryCachePath + TEMP_PACKAGE_NAME);

		EditorApplication.update += EditorUpdate;

		System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
		using(Downloader = new System.Net.WebClient())
		{
			Downloader.DownloadProgressChanged += HandleDownloadProgressChanged;
			Downloader.DownloadFileCompleted += HandleDownloadFileCompleted;
			Downloader.DownloadFileAsync(new System.Uri(LATEST_PACKAGE), Application.temporaryCachePath + TEMP_PACKAGE_NAME);
		}
	}

	static void HandleDownloadFileCompleted (object sender, System.ComponentModel.AsyncCompletedEventArgs e)
	{
		System.Net.ServicePointManager.ServerCertificateValidationCallback = null;
		if(e.Cancelled)
		{
			Debug.Log("Fuse SDK: Download cancelled.");
			return;
		}
		else if(e.Error != null)
		{
			Debug.LogError("Fuse SDK: Error while downloading update:" + System.Environment.NewLine + e.Error.ToString());
			Downloader.Dispose();
			Downloader = null;
			FuseAPIUpdater.DLError = true;
			return;
		}
		FuseAPIUpdater.FinishedDL = true;
	}

	static void HandleDownloadProgressChanged (object sender, System.Net.DownloadProgressChangedEventArgs e)
	{
		TotalKBytes = e.TotalBytesToReceive / 1024;
		DownloadProgress = e.ProgressPercentage / 100f;
		KBytes = (long)(TotalKBytes * DownloadProgress);
	}

	private static void EditorUpdate()
	{
		if(!DLError)
		{
			if(FinishedDL)
				DownloadFinished();
			else if(EditorUtility.DisplayCancelableProgressBar("Downloading latest Fuse SDK", "Downloaded: " + KBytes + "kB / " + TotalKBytes + "kB", DownloadProgress))
				Downloader.CancelAsync();
			else
				return;
		}
		
		EditorApplication.update -= EditorUpdate;
		EditorUtility.ClearProgressBar();
		if(Downloader != null)
		{
			Downloader.Dispose();
			Downloader = null;
		}
	}
	
	private static void DownloadFinished()
	{
		FuseSDKPrefs.DownloadType dlType = (FuseSDKPrefs.DownloadType) EditorPrefs.GetInt(AUTODOWNLOAD_KEY, 1);

		bool importNow = true;
		if(dlType == FuseSDKPrefs.DownloadType.AutoDownloadAndPromtForImport)
		{
			importNow = EditorUtility.DisplayDialog("Fuse SDK Update", "Do you want to import the new SDK now?", "Yes", "No");
		}

		if(_DoNotImport || !importNow || dlType == FuseSDKPrefs.DownloadType.AutoDownloadOnly)
		{
			_DoNotImport = false;

			string path = System.IO.Path.GetFullPath(Application.dataPath + "/.." + SAVED_PACKAGE_NAME);
			try
			{
				System.IO.File.Copy(Application.temporaryCachePath + TEMP_PACKAGE_NAME, path, true);
				Debug.Log("Fuse SDK: New SDK package saved to " + path);
			}
			catch
			{
				Debug.LogError("Fuse SDK: Error while copying package to " + path);
			}
		}
		else
		{
			try
			{
				Debug.Log("Fuse SDK: Download finished. Importing Package...");
				var icon = System.IO.File.ReadAllBytes(Application.dataPath + FuseAPIEditor.ICON_PATH);
				AssetDatabase.ImportPackage(Application.temporaryCachePath + TEMP_PACKAGE_NAME, false);
				System.IO.File.WriteAllBytes(Application.dataPath + FuseAPIEditor.ICON_PATH, icon);
				Debug.Log("Fuse SDK: Import finished. Update successful");

				if(Application.platform == RuntimePlatform.Android)
				{
					FusePostProcess.UpdateAndroidManifest(PlayerSettings.bundleIdentifier);
				}
			}
			catch
			{
				Debug.LogError("Fuse SDK: Error while importing package.");
			}
		}

		try
		{
			System.IO.File.Delete(Application.temporaryCachePath + TEMP_PACKAGE_NAME);
		}
		catch
		{
			Debug.LogError("Fuse SDK: Error while deleting temp file " + Application.temporaryCachePath + TEMP_PACKAGE_NAME);
		}
	}
}
