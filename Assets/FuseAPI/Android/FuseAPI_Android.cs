using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseAPI_Android : FuseAPI
{

#if UNITY_ANDROID && !UNITY_EDITOR
	
	public static bool debugOutput = false;
	
	private static GameObject _callback;
	private static AndroidJavaClass _fusePlugin;
	private static AndroidJavaClass _fuseUnityPlugin;

	private void Awake()
	{
		GameObject go = GameObject.Find("FuseSDK");
		if(go != null && go != gameObject && go.GetComponent<FuseAPI>() != null)
			UnityEngine.Object.DestroyImmediate(gameObject, true);
	}

	protected override void Init()
	{
		// preserve the prefab and all attached scripts through level loads
		DontDestroyOnLoad(transform.gameObject);

		if(logging)
		{
			FuseAPI_Android.debugOutput = true;
		}
		_callback = this.gameObject;

		if (Application.platform == RuntimePlatform.Android)
		{
			_fusePlugin = new AndroidJavaClass("com.fusepowered.fuseapi.FuseAPI");
			FuseLog("FusePlugin " + (_fusePlugin == null ? "NOT FOUND" : "FOUND"));
			_fuseUnityPlugin = new AndroidJavaClass("com.fusepowered.unity.FuseUnityAPI");
			FuseLog("FuseUnityPlugin " + (_fuseUnityPlugin == null ? "NOT FOUND" : "FOUND"));
			FuseLog("Callback object is: " + _callback.name);
			_fuseUnityPlugin.CallStatic("SetGameObjectCallback", _callback.name);
		}
	}

	void Start()
	{
		_StartSession(AndroidGameID);
	}
	
	void OnApplicationPause(bool pausing)
	{
		if( pausing )
		{
			_fuseUnityPlugin.CallStatic("onPause");
		}
		else
		{
			_fuseUnityPlugin.CallStatic("onResume");
		}
	}

	#region Session Creation
	
	[Obsolete("StartSession now called automatically. Set gameID in the FuseAPI Component.", true)]
	new public static void StartSession(string gameId)
	{
	}

	new protected static void _StartSession(string gameId)
	{
		if(string.IsNullOrEmpty(gameId))
			Debug.LogError("FuseSDK: Null or empty API Key. Make sure your API Key is entered in the FuseSDK prefab");

		FuseLog("StartSession(" + gameId + ")");
		_fuseUnityPlugin.CallStatic("startSession", gameId);
	}

	private void _SessionStartReceived(string dummy)
	{
		FuseLog("SessionStartReceived(" + dummy + ")");
        // push notification registration
		if( registerForPushNotifications )
		{
			SetupPushNotifications(GCM_SenderID);
		}
		OnSessionStartReceived();
	}

	private void _SessionLoginError(string error)
	{
		FuseLog("SessionLoginError(" + error + ")");
		OnSessionLoginError(int.Parse(error));
	}
	
	new public static void FuseAPI_RegisterForPushNotifications()
	{
		// Do nothing on androids
	}
	
	new public static void SetupPushNotifications(string gcmSenderID)
	{
		FuseLog("SetupPushNotifications(" + gcmSenderID + ")");
		_fuseUnityPlugin.CallStatic("testGCMSetup");
		_fuseUnityPlugin.CallStatic("registerForPushNotifications", gcmSenderID);
	}
#endregion

	#region Analytics Event
	new public static void RegisterEvent(string message)
	{
		FuseLog("RegisterEvent(" + message + ")");
		_fuseUnityPlugin.CallStatic("registerEvent", message);
	}
	
	new public static void RegisterEvent(string message, Hashtable values)
	{
		FuseLog ("RegisterEvent(" + message + ", [variables])");
		
		if( values == null )
		{
			RegisterEvent(message);
			return;
		}
		
		int max_entries = values.Keys.Count;
		string[] keys = new string[max_entries];			
		string[] attributes = new string[max_entries];
		keys.Initialize();
		attributes.Initialize();
		int numEntries = 0;
		foreach (DictionaryEntry entry in values)
		{
			string entryKey = entry.Key.ToString();
			string entryValue = "";
			if( entry.Value != null )
			{
				entryValue = entry.Value.ToString();
			}
			
			keys[numEntries] = entryKey;
			attributes[numEntries] = entryValue;
			numEntries++;
		}
		_fuseUnityPlugin.CallStatic("registerEventWithDictionary", message, keys, attributes, numEntries);
	}

	new public static int RegisterEvent(string name, string paramName, string paramValue, Hashtable variables)
	{
		FuseLog ("RegisterEvent(" + name + "," + paramName + "," + paramValue + ", [variables])");

		_fuseUnityPlugin.CallStatic("registerEventStart");
			
		foreach (DictionaryEntry entry in variables)
		{
			string entryKey = entry.Key as string;
			double entryValue = 0.0;
			if( entryKey != null )
			{
				if( entry.Value != null )
				{
					try
					{
						entryValue = Convert.ToDouble(entry.Value);
					}
					catch
					{
						string entryString = (entry.Value == null) ? "" : entry.Value.ToString();
						Debug.LogWarning("Key/value pairs in FuseAPI::RegisterEvent must be String/Number");
						Debug.LogWarning("For Key: " + entryKey + " and Value: " + entryString);
					}
				}
				_fuseUnityPlugin.CallStatic("registerEventKeyValue", entryKey, entryValue);					
			}			
		}

		return _fuseUnityPlugin.CallStatic<int>("registerEventEnd", name, paramName, paramValue);
	}

	new public static int RegisterEvent(string name, string paramName, string paramValue, string variableName, double variableValue)
	{
		FuseLog ("RegisterEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");
		return _fuseUnityPlugin.CallStatic<int>("registerEvent", name, paramName, paramValue, variableName, variableValue);
	}

#endregion

	#region In-App Purchase Logging
	new public static void RegisterInAppPurchaseList(Product[] products)
	{
		FuseLog("***** NOT YET SUPPORTED BY FUSE'S ANDROID API ***** FuseAPI:RegisterInAppPurchaseList([data])");
	}

	new public static void RegisterInAppPurchase(PurchaseState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload)
	{
		FuseLog("RegisterInAppPurchase(" + purchaseState.ToString() + "," + purchaseToken + "," + productId + "," + orderId + "," + DateTimeToTimestamp(purchaseTime) + "," + developerPayload + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), purchaseToken, productId, orderId, DateTimeToTimestamp(purchaseTime), developerPayload);
	}

	new public static void RegisterInAppPurchase(PurchaseState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
		FuseLog("RegisterInAppPurchase(" + purchaseState.ToString() + "," + purchaseToken + "," + productId + "," + orderId + "," + DateTimeToTimestamp(purchaseTime) + "," + developerPayload + "," + price + "," + currency + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), purchaseToken, productId, orderId, DateTimeToTimestamp(purchaseTime), developerPayload, price, currency);
	}

	new public static void RegisterInAppPurchase(PurchaseState purchaseState, string purchaseToken, string productId, string orderId, long purchaseTime, string developerPayload, double price, string currency)
	{
		FuseLog("RegisterInAppPurchase(" + purchaseState.ToString() + "," + purchaseToken + "," + productId + "," + orderId + "," + purchaseTime + "," + developerPayload + "," + price + "," + currency + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), purchaseToken, productId, orderId, purchaseTime, developerPayload, price, currency);
	}

	new public static void RegisterUnibillPurchase(string productID, byte[] receipt)
	{
		// do nothing.  This method is for iOS only at this point.  Unibill Android can be handled with current methods
	}

	private void _PurchaseVerification(string param)
	{
		FuseLog ("PurchaseVerification(" + param + ")");

		int verified;

		var pars = param.Split(',');
		if(pars.Length == 3 && int.TryParse(pars[0], out verified))
			OnPurchaseVerification(verified, pars[1], pars[2]);
		else
			Debug.LogError("FuseSDK: Parsing error in _PurchaseVerification");

	}
	#endregion

	#region Fuse Ads
	new public static void PreLoadAd(string adZone)
	{
		FuseLog("PreloadAd");
		_fuseUnityPlugin.CallStatic("preloadAd",adZone);
	}
	
	new public static void CheckAdAvailable(string adZone)
	{
		FuseLog("CheckAdAvailable()");
		_fuseUnityPlugin.CallStatic("checkAdAvailable",adZone);
	}
	
	new public static void ShowAd(string adZone)
	{
		FuseLog("ShowAd()");
		_fuseUnityPlugin.CallStatic("showAd",adZone);
	}

	private void _AdAvailabilityResponse(string param)
	{
		FuseLog("AdAvailabilityResponse(" + param + ")");
		int available;
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out available) && int.TryParse(pars[1], out error))
			OnAdAvailabilityResponse(available, error);
		else
			Debug.LogError("FuseSDK: Parsing error in _AdAvailabilityResponse");
	}

	private void _AdWillClose(string dummy)
	{
		FuseLog("AdWillClose()");
		OnAdWillClose();
	}
	
	private void _RewardedVideoCompleted(string adZone)
	{
		FuseLog("RewardedVideoCompleted(" + adZone + ")");
		OnRewardedVideoCompleted(adZone);
	}

	#endregion

	#region Notifications	
	new public static void DisplayNotifications()
	{
		FuseLog("DisplayNotifications()");
		_fuseUnityPlugin.CallStatic("displayNotifications");
	}

    new public static bool IsNotificationAvailable()
    {
        FuseLog("IsNotificationAvailable()");
		return _fuseUnityPlugin.CallStatic<bool>("isNotificationAvailable");
    }

	private void _NotificationAction(string action)
	{
		FuseLog("NotificationAction()");
		OnNotificationAction(action);
	}
	#endregion

	#region More Games
	new public static void DisplayMoreGames()
	{
		FuseLog("DisplayMoreGames()");
		_fuseUnityPlugin.CallStatic("displayMoreGames");
	}
	#endregion
	
	#region Gender
	new public static void RegisterGender(Gender gender)
	{
		FuseLog("RegisterGender()");
		_fuseUnityPlugin.CallStatic("registerGender", (int)gender);
	}
	#endregion

	#region Account Login
	new public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		FuseLog("FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		_fuseUnityPlugin.CallStatic("facebookLogin", facebookId, name, accessToken);
	}

	new public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
		FuseLog("FacebookLogin(" + facebookId + "," + name + "," + gender + "," + accessToken + ")");
		_fuseUnityPlugin.CallStatic("facebookLoginGender", facebookId, name, (int)gender, accessToken);
	}

	new public static void TwitterLogin(string twitterId)
	{
		FuseLog("TwitterLogin(" + twitterId + ")");
		_fuseUnityPlugin.CallStatic("twitterLogin", twitterId);
	}	
	
	new public static void DeviceLogin(string alias)
	{
		FuseLog("DeviceLogin(" + alias + ")");
		_fuseUnityPlugin.CallStatic("deviceLogin", alias);
	}
	
	new public static void FuseLogin(string fuseId, string alias)
	{
		FuseLog("FuseLogin(" + fuseId + "," + alias + ")");
		_fuseUnityPlugin.CallStatic("fuseLogin", fuseId, alias);
	}
	
	new public static void GooglePlayLogin(string alias, string token)
	{
		FuseLog("GooglePlayLogin(" + alias + "," + token + ")");
		_fuseUnityPlugin.CallStatic("googlePlayLogin", alias, token);
	}

	new public static void GameCenterLogin()
	{
		// only for testing - does not actually log you into game center
		_fuseUnityPlugin.CallStatic("gamecenterLogin", "", "");
	}
	
	new public static string GetOriginalAccountAlias()
	{
		FuseLog("GetOriginalAccountAlias()");
		return _fuseUnityPlugin.CallStatic<string>("getOriginalAccountAlias");
	}
	
	new public static string GetOriginalAccountId()
	{
		FuseLog("GetOriginalAccountId()");
		return _fuseUnityPlugin.CallStatic<string>("getOriginalAccountId");
	}
	
	new public static AccountType GetOriginalAccountType()
	{
		FuseLog("GetOriginalAccountType()");
		return (AccountType)_fuseUnityPlugin.CallStatic<int>("getOriginalAccountType");
	}

	private void _AccountLoginComplete(string param)
	{
		FuseLog("AccountLoginComplete(" + param + ")");

		int type;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out type))
			OnAccountLoginComplete((FuseAPI.AccountType) type, pars[1]);
		else
			Debug.LogError("FuseSDK: Parsing error in _AccountLoginComplete");
	}
	#endregion

	#region Miscellaneous
	new public static int GamesPlayed()
	{
		FuseLog("GamesPlayed()");
		return _fuseUnityPlugin.CallStatic<int>("gamesPlayed");
	}
	
	new public static string LibraryVersion()
	{
		FuseLog("LibraryVersion()");
		return _fuseUnityPlugin.CallStatic<string>("libraryVersion");
	}

	new public static bool Connected()
	{
		FuseLog("Connected()");
		return _fuseUnityPlugin.CallStatic<bool>("connected");
	}

	new public static void TimeFromServer()
	{
		FuseLog("TimeFromServer()");
		_fuseUnityPlugin.CallStatic("timeFromServer");
	}

	private void _TimeUpdated(string timestamp)
	{
		FuseLog("TimeUpdated(" + timestamp + ")");
		OnTimeUpdated(TimestampToDateTime(long.Parse(timestamp)));
	}

	new public static bool NotReadyToTerminate()
	{
		FuseLog("NotReadyToTerminate()");
		return _fuseUnityPlugin.CallStatic<bool>("notReadyToTerminate");
	}

	
	new public static void FuseLog(string str)
	{
		if(debugOutput)
		{
			Debug.Log("FuseAPI: " + str);
		}
	}

	new public static string GetFuseId()
	{
		return _fuseUnityPlugin.CallStatic<string>("getFuseID");
	}
		
	#endregion

	#region Data Opt In/Out
	new public static void EnableData(bool enable)
	{
		FuseLog("EnableData(" + enable + ")");
		_fuseUnityPlugin.CallStatic("enableData", enable);
	}

	new public static bool DataEnabled()
	{
		FuseLog("DataEnabled()");
		return _fuseUnityPlugin.CallStatic<bool>("dataEnabled");
	}
	#endregion

	#region Friend List	
	new public static void AddFriend(string fuseId)
	{
		FuseLog("AddFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("addFriend", fuseId);
	}
	new public static void RemoveFriend(string fuseId)
	{
		FuseLog("RemoveFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("removeFriend", fuseId);
	}
	new public static void AcceptFriend(string fuseId)
	{
		FuseLog("AcceptFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("acceptFriend", fuseId);
	}
	new public static void RejectFriend(string fuseId)
	{
		FuseLog("RejectFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("rejectFriend", fuseId);
	}
	
	
	private void _FriendAdded(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAdded(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}
	private void _FriendRemoved(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRemoved(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}
	private void _FriendAccepted(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAccepted(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}
	private void _FriendRejected(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRejected(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}	
	
	new public static void MigrateFriends(string fuseId)
	{
		FuseLog("MigrateFriends(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("migrateFriends", fuseId);
	}
	
	private void _FriendsMigrated(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendsMigrated(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsMigrated");
	}
	
	new public static void UpdateFriendsListFromServer()
	{
		FuseLog("UpdateFriendsListFromServer()");
		_fuseUnityPlugin.CallStatic("updateFriendsListFromServer");
	}
	
	new public static List<Friend> GetFriendsList()
	{
		FuseLog("GetFriendsList()");
		return DeserializeFriendsList(_fuseUnityPlugin.CallStatic<string[]>("getFriendsList"));
	}
	
	private static List<Friend> DeserializeFriendsList(string[] friendList)
	{
		List<Friend> retList = new List<Friend>();

		if(friendList == null)
		{
			Debug.LogWarning("FuseSDK: NULL FriendsList.");
			return retList;
		}

		foreach(var line in friendList)
		{
			var parts = line.Split(',');
			if(parts.Length == 4)
			{
				Friend friend = new Friend();
				friend.fuseId = parts[0];
				friend.accountId = parts[1];
				friend.alias = parts[2];
				friend.pending = parts[3] != "0";

				retList.Add(friend);
			}
			else
			{
				Debug.LogError("FuseSDK: Error reading FriendsList data. Invalid line: " + line);
			}
		}

		return retList;
	}

	private void _FriendsListUpdated(string dummy)
	{
		FuseLog("FriendsListUpdated()");
		OnFriendsListUpdated(GetFriendsList());
	}

	private void _FriendsListError(string error)
	{
		FuseLog("FriendsListError(" + error + ")");
		OnFriendsListError(int.Parse(error));
	}
	#endregion

	#region User-to-User Push Notifications
	new public static void UserPushNotification(string fuseId, string message)
	{
		FuseLog("UserPushNotification(" + fuseId + "," + message + ")");
		_fuseUnityPlugin.CallStatic("userPushNotification", fuseId, message);
	}
	
	new public static void FriendsPushNotification(string message)
	{
		FuseLog("FriendsPushNotification(" + message + ")");
		_fuseUnityPlugin.CallStatic("friendsPushNotification", message);
	}
	#endregion

	#region Game Configuration Data
	new public static string GetGameConfigurationValue(string key)
	{
		FuseLog("GetGameConfigurationValue(" + key + ")");
		return _fuseUnityPlugin.CallStatic<string>("getGameConfigurationValue", key);
	}
	
	new public static Dictionary<string, string> GetGameConfig()
	{
		FuseLog("GetGameConfig()");
	
		Dictionary<string, string> gameConfig = new Dictionary<string, string>();
		// get a list of all the keys:
		string[] keys = _fuseUnityPlugin.CallStatic<string[]>("getGameConfigKeys");
		if (keys != null)
		{
			for( int i = 0; i < keys.Length; i++ )
			{
				gameConfig.Add(keys[i], GetGameConfigurationValue(keys[i]));
			}
		}
		return gameConfig;
	}

	private void _GameConfigurationReceived(string dummy)
	{
		FuseLog("GameConfigurationReceived()");
		OnGameConfigurationReceived();
	}
	#endregion

	#region Specific Event Registration
	new public static void RegisterLevel(int level)
	{
		FuseLog("RegisterLevel(" + level + ")");
		_fuseUnityPlugin.CallStatic("registerLevel", level);
	}
	
	new public static void RegisterCurrency(int type, int balance)
	{
		FuseLog("RegisterCurrency(" + type + "," + balance + ")");
		_fuseUnityPlugin.CallStatic("registerCurrency", type, balance);
	}
	
	new public static void RegisterAge(int age)
	{
		FuseLog("RegisterAge(" + age + ")");
		_fuseUnityPlugin.CallStatic("registerAge", age);
	}
	
	new public static void RegisterBirthday(int year, int month, int day)
	{
		FuseLog("RegisterBirthday(" + year + ", " + month + ", " + day + ")");
		_fuseUnityPlugin.CallStatic("registerBirthday", year, month, day);
	}	
	#endregion
#endif
}

