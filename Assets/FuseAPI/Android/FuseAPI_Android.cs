
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseAPI_Android : FuseAPI
{

#if UNITY_ANDROID && !UNITY_EDITOR	
	
	public static bool debugOutput = false;

	protected override void Init()
	{
		// preserve the prefab and all attached scripts through level loads
		if( persistent )
		{
			DontDestroyOnLoad(transform.gameObject);
		}
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

	private void _PurchaseVerification(string dummy)
	{
		// args: bool verified, string transactionID, string originalTransactionID
		if( _args.Count == 3 )
		{
			FuseLog ("PurchaseVerification(" + _args[0] + "," + _args[1] + "," + _args[2] + ")");
			OnPurchaseVerification(int.Parse(_args[0]), _args[1], _args[2]);
		}
		else
		{
			Debug.LogError("Error: Invalid number of arguments for FuseAPI::PurchaseVerification");
		}
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

	private void _AdAvailabilityResponse(string available)
	{
		FuseLog("AdAvailabilityResponse(" + available + "," + _args[0] + ")");
		OnAdAvailabilityResponse(int.Parse(available), int.Parse(_args[0]));
		_args.Clear();
	}

	private void _AdDisplayed(string dummy)
	{
		FuseLog("AdDisplayed()");
		OnAdDisplayed();
	}

	private void _AdClicked(string dummy)
	{
		FuseLog("AdClicked()");
		OnAdClicked();
	}

	private void _AdWillClose(string dummy)
	{
		FuseLog("AdWillClose()");
		OnAdWillClose();
	}
	
	private void _VideoCompleted(string adZone)
	{
		FuseLog("VideoCompleted(" + adZone + ")");
		OnVideoCompleted(adZone);
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
	
	new public static void OpenFeintLogin(string openFeintId)
	{
		FuseLog("OpenFeintLogin(" + openFeintId + ")");
		_fuseUnityPlugin.CallStatic("openFeintLogin", openFeintId);
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

	public static void GameCenterLogin(string accountID, string alias)
	{
		// only for testing - does not actually log you into game center
		_fuseUnityPlugin.CallStatic("gamecenterLogin", accountID, alias);
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

	private void _AccountLoginComplete(string accountId)
	{
		FuseLog("AccountLoginComplete(" + _args[0] + "," + accountId + ")");
		OnAccountLoginComplete((FuseAPI.AccountType)int.Parse(_args[0]), accountId);
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

    #region User Game Data
	new public static int SetGameData(Hashtable data)
	{
		return SetGameData("", data, false, GetFuseId());
	}
	
	new public static int SetGameData(string key, Hashtable data)
	{
		return SetGameData(key, data, false, GetFuseId());
	}
	
	new public static int SetGameData(string key, Hashtable data, bool isCollection)
	{
		return SetGameData(key, data, isCollection, GetFuseId());
	}
	
	new public static int SetGameData(string key, Hashtable data, bool isCollection, string fuseId)
	{
		FuseLog ("SetGameData(" + key + ", [data]," + isCollection + "," + fuseId + ")");

		_fuseUnityPlugin.CallStatic("setGameDataStart");
			
		foreach (DictionaryEntry entry in data)
		{
			string entryKey = entry.Key as string;
			byte[] buffer = entry.Value as byte[];
			if (buffer != null)
			{
				string entryValue = Convert.ToBase64String(buffer);
				_fuseUnityPlugin.CallStatic("setGameDataKeyValue", entryKey, entryValue, true);
			}
			else
			{
				string entryValue = entry.Value.ToString();
				_fuseUnityPlugin.CallStatic("setGameDataKeyValue", entryKey, entryValue, false);
			}
		}

		return _fuseUnityPlugin.CallStatic<int>("setGameDataEnd", key, isCollection, fuseId);
	}

	new public static string GetFuseId()
	{
		return _fuseUnityPlugin.CallStatic<string>("getFuseID");
	}

	private void _GameDataSetAcknowledged(string requestId)
	{
		FuseLog("GameDataSetAcknowledged(" + requestId + ")");
		OnGameDataSetAcknowledged(int.Parse(requestId));
	}

	private void _GameDataError(string fuseGameDataError)
	{
		FuseLog("GameDataError(" + fuseGameDataError + "," + _args[0] + ")");
		OnGameDataError(int.Parse(fuseGameDataError), int.Parse(_args[0]));
		_args.Clear();
	}

	new public static int GetGameData(string[] keys)
	{
		return GetFriendGameData("", "", keys);
	}
	
	new public static int GetGameData(string key, string[] keys)
	{
		return GetFriendGameData("", key, keys);
	}
	
	new public static int GetFriendGameData(string fuseId, string[] keys)
	{
		return GetFriendGameData(fuseId, "", keys);
	}
	
	new public static int GetFriendGameData(string fuseId, string key, string[] keys)
	{
		FuseLog ("GetGameData(" + fuseId + "," + key + ",[keys])");
		
		_fuseUnityPlugin.CallStatic("getGameDataStart");
			
		foreach (string entry in keys)
		{
			_fuseUnityPlugin.CallStatic("getGameDataKey", entry);
		}

		return _fuseUnityPlugin.CallStatic<int>("getGameDataEnd", key, fuseId);
	}

	private void _GameDataReceived(string requestId)
	{
		FuseLog("GameDataReceived(" + requestId + ")");

		Hashtable gameData = new Hashtable();
		for (int index = 1; index < _args.Count; index += 3)
		{
			FuseLog(_args[index] + " " + _args[index+1] + " " + _args[index+2]);
			if (_args[index] == "0")
			{
				gameData.Add(_args[index+1],_args[index+2]);
			}
			else
			{
				gameData.Add(_args[index+1],Convert.FromBase64String(_args[index+2]));
			}
		}

		OnGameDataReceived(_args[0], "", gameData, int.Parse(requestId));
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
	
	
	private void _FriendAdded(string dummy)
	{
		if( _args.Count == 2 && _args[0] != null && _args[1] != null )
		{
			OnFriendAdded(_args[0], Convert.ToInt32(_args[1]));
		}
	}
	private void _FriendRemoved(string dummy)
	{
		if( _args.Count == 2 && _args[0] != null && _args[1] != null )
		{
			OnFriendRemoved(_args[0], Convert.ToInt32(_args[1]));
		}
	}
	private void _FriendAccepted(string dummy)
	{
		if( _args.Count == 2 && _args[0] != null && _args[1] != null )
		{
			OnFriendAccepted(_args[0], Convert.ToInt32(_args[1]));
		}
	}
	private void _FriendRejected(string dummy)
	{
		if( _args.Count == 2 && _args[0] != null && _args[1] != null )
		{
			OnFriendRejected(_args[0], Convert.ToInt32(_args[1]));
		}
	}	
	
	new public static void MigrateFriends(string fuseId)
	{
		FuseLog("MigrateFriends(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("migrateFriends", fuseId);
	}
	
	private void _FriendsMigrated(string dummy)
	{
		if( _args.Count == 2 && _args[0] != null && _args[1] != null )
		{
			OnFriendsMigrated(_args[0], Convert.ToInt32(_args[1]));
		}
	}
	
	new public static void UpdateFriendsListFromServer()
	{
		FuseLog("UpdateFriendsListFromServer()");
		_fuseUnityPlugin.CallStatic("updateFriendsListFromServer");
	}
	
	new public static List<Friend> GetFriendsList()
	{
		FuseLog("GetFriendsList()");
		_ParseCompositeValueIntoArgs(_fuseUnityPlugin.CallStatic<string>("getFriendsList"));
		return ArgsToFriendsList();
	}
	
	private static List<Friend> ArgsToFriendsList()
	{
		List<Friend> friendsList = new List<Friend>();

		for (int index = 0; index < _args.Count; index += 4)
		{
			Friend friend = new Friend();
			friend.fuseId = _args[index];
			friend.accountId = _args[index+1];
			friend.alias = _args[index+2];
			friend.pending = _args[index+3] == "0" ? false : true;
				
			friendsList.Add(friend);
		}

		return friendsList;
	}

	private void _FriendsListUpdated(string dummy)
	{
		FuseLog("FriendsListUpdated()");
		OnFriendsListUpdated(ArgsToFriendsList());
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

    #region Gifting
	new public static void GetMailListFromServer()
	{
		FuseLog("GetMailListFromServer()");
		_fuseUnityPlugin.CallStatic("getMailListFromServer");
	}
	
	new public static void GetMailListFriendFromServer(string fuseId)
	{
		FuseLog("GetMailListFriendFromServer(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("getMailListFriendFromServer", fuseId);
	}
	
	new public static List<Mail> GetMailList(string fuseId)
	{
		FuseLog("GetMailList(" + fuseId + ")");
		_ParseCompositeValueIntoArgs(_fuseUnityPlugin.CallStatic<string>("getMailList", fuseId));
		return ArgsToMailList();
	}

	private static List<Mail> ArgsToMailList()
	{
		List<Mail> mailList = new List<Mail>();

		for (int index = 0; index < _args.Count; index += 7)
		{
			Mail mail = new Mail();
			mail.messageId = int.Parse(_args[index]);
			mail.timestamp = TimestampToDateTime(int.Parse(_args[index+1]));
			mail.alias = _args[index+2];
			mail.message = _args[index+3];
			mail.giftId = int.Parse(_args[index+4]);
			mail.giftName = _args[index+5];
			mail.giftAmount = int.Parse(_args[index+6]);
				
			mailList.Add(mail);
		}

		return mailList;
	}

	new public static void SetMailAsReceived(int messageId)
	{
		FuseLog("SetMailAsReceived(" + messageId + ")");
		_fuseUnityPlugin.CallStatic("setMailAsReceived", messageId);
	}
	
	new public static int SendMailWithGift(string fuseId, string message, int giftId, int giftAmount)
	{
		FuseLog("SendMailWithGift(" + fuseId + "," + message + "," + giftId + "," + giftAmount + ")");
		return _fuseUnityPlugin.CallStatic<int>("sendMailWithGift", fuseId, message, giftId, giftAmount);
	}
	
	new public static int SendMail(string fuseId, string message)
	{
		FuseLog("SendMail(" + fuseId + "," + message + ")");
		return _fuseUnityPlugin.CallStatic<int>("sendMail", fuseId, message);
	}

	private void _MailListReceived(string fuseId)
	{
		FuseLog("MailListReceived(" + fuseId + ")");
		OnMailListReceived(ArgsToMailList(), fuseId);
	}

	private void _MailListError(string error)
	{
		FuseLog("MailListError(" + error + ")");
		OnMailListError(int.Parse(error));
	}

	private void _MailAcknowledged()
	{
		string messageId = _args[0];
		string fuseId = _args[1];
		string requestID = _args[2];
		FuseLog("MailAcknowledged(" + messageId + "," + fuseId + "," + requestID + ")");
		OnMailAcknowledged(int.Parse(messageId), fuseId, int.Parse (requestID));
		_args.Clear();
	}

	private void _MailError(string error)
	{
		FuseLog("MailError(" + error + ")");
		OnMailError(int.Parse(error));
	}
    #endregion

    #region Game Configuration Data
	new public static string GetGameConfigurationValue(string key)
	{
		FuseLog("GetGameConfigurationValue(" + key + ")");
		_fuseUnityPlugin.SetStatic<string>("_stringConduit", key);
		_fuseUnityPlugin.CallStatic("getGameConfigurationValue");
		return _fuseUnityPlugin.GetStatic<string>("_stringConduit");
	}
	
	new public static Dictionary<string, string> GetGameConfig()
	{
		FuseLog("GetGameConfig()");		
	
		Dictionary<string, string> gameConfig = new Dictionary<string, string>();
		// get a list of all the keys:
		string[] keys = _fuseUnityPlugin.CallStatic<string[]>("getGameConfigKeys");
		if (keys != null) {
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
	
	new public static void RegisterFlurryView()
	{
		FuseLog("RegisterFlurryView()");
		_fuseUnityPlugin.CallStatic("registerFlurryView");
	}
	
	new public static void RegisterFlurryClick()
	{
		FuseLog("RegisterFlurryClick()");
		_fuseUnityPlugin.CallStatic("registerFlurryClick");
	}
	
	new public static void RegisterTapjoyReward(int amount)
	{
		FuseLog("RegisterTapjoyReward(" + amount + ")");
		_fuseUnityPlugin.CallStatic("registerTapjoyReward", amount);
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

    #region API bridge helpers
	private void _ClearArgumentList(string dummy)
	{
		_args.Clear();
	}

	private void _ClearArgumentListAndSetFirst(string message)
	{
		_args.Clear();
		_args.Add(message);
	}

	private void _AddArgument(string message)
	{
		FuseLog("_AddArgument: " + message);
		_args.Add(message);
	}

	private static void _ParseCompositeValueIntoArgs(string compositeValue) // 7:Example9:composite5:value
	{
		_args.Clear();
		int index = 0;
		while (index < compositeValue.Length)
		{
			int numberLength = 0;
			while (compositeValue[index + numberLength] != ':')
			{
				++numberLength;
			}

			int valueLength = int.Parse(compositeValue.Substring(index, numberLength));

			_args.Add(compositeValue.Substring(index + numberLength + 1, valueLength));

			index += numberLength + 1 + valueLength;
		}
	}

	private static List<string> _args = new List<string>();
    #endregion


	private static GameObject _callback;
	private static AndroidJavaClass _fusePlugin;
	private static AndroidJavaClass _fuseUnityPlugin;
#endif
}

