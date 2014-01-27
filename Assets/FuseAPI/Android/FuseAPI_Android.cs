
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseAPI_Android : FuseAPI
{
	public bool persistent = true;
#if UNITY_ANDROID && !UNITY_EDITOR	
	void Awake()
	{
		// preserve the prefab and all attached scripts through level loads
		if( persistent )
		{
			DontDestroyOnLoad(transform.gameObject);
		}
		
		_callback = this.gameObject;

		if (Application.platform == RuntimePlatform.Android)
		{
			_fusePlugin = new AndroidJavaClass("com.fusepowered.fuseapi.FuseAPI");
			Debug.Log("FuseAPI:FusePlugin " + (_fusePlugin == null ? "NOT FOUND" : "FOUND"));
			_fuseUnityPlugin = new AndroidJavaClass("com.fusepowered.unity.FuseUnityAPI");
			Debug.Log("FuseAPI:FuseUnityPlugin " + (_fuseUnityPlugin == null ? "NOT FOUND" : "FOUND"));
			Debug.Log("Callback object is: " + _callback.name);
			_fuseUnityPlugin.CallStatic("SetGameObjectCallback", _callback.name);
		}
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
	new public static void StartSession(string gameId)
	{
		Debug.Log("FuseAPI:StartSession(" + gameId + ")");
		_fuseUnityPlugin.CallStatic("startSession", gameId);
	}

	private void _SessionStartReceived(string dummy)
	{
		Debug.Log("FuseAPI:SessionStartReceived(" + dummy + ")");
		OnSessionStartReceived();
	}

	private void _SessionLoginError(string error)
	{
		Debug.Log("FuseAPI:SessionLoginError(" + error + ")");
		OnSessionLoginError(int.Parse(error));
	}
	
	new public static void SetupPushNotifications(string gcmProjectID)
	{
		Debug.Log("FuseAPI:SetupPushNotifications(" + gcmProjectID + ")");
		_fuseUnityPlugin.CallStatic("registerForPushNotifications", gcmProjectID);
	}
#endregion

#region Analytics Event
	new public static void RegisterEvent(string message)
	{
		Debug.Log("FuseAPI:RegisterEvent(" + message + ")");
		_fuseUnityPlugin.CallStatic("registerEvent", message);
	}
	
	new public static void RegisterEvent(string message, Hashtable values)
	{
		Debug.Log ("FuseAPI:RegisterEvent(" + message + ", [variables])");
		
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
		Debug.Log ("FuseAPI:RegisterEvent(" + name + "," + paramName + "," + paramValue + ", [variables])");

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
		Debug.Log ("FuseAPI:RegisterEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");
		return _fuseUnityPlugin.CallStatic<int>("registerEvent", name, paramName, paramValue, variableName, variableValue);
	}

#endregion

#region In-App Purchase Logging
	new public static void RegisterInAppPurchaseList(Product[] products)
	{
		Debug.Log("***** NOT YET SUPPORTED BY FUSE'S ANDROID API ***** FuseAPI:RegisterInAppPurchaseList([data])");
	}

	new public static void RegisterInAppPurchase(PurchaseState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload)
	{
		Debug.Log("FuseAPI:RegisterInAppPurchase(" + purchaseState.ToString() + "," + purchaseToken + "," + productId + "," + orderId + "," + DateTimeToTimestamp(purchaseTime) + "," + developerPayload + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), purchaseToken, productId, orderId, DateTimeToTimestamp(purchaseTime), developerPayload);
	}

	new public static void RegisterInAppPurchase(PurchaseState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
		Debug.Log("FuseAPI:RegisterInAppPurchase(" + purchaseState.ToString() + "," + purchaseToken + "," + productId + "," + orderId + "," + DateTimeToTimestamp(purchaseTime) + "," + developerPayload + "," + price + "," + currency + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), purchaseToken, productId, orderId, DateTimeToTimestamp(purchaseTime), developerPayload, price, currency);
	}
#endregion

#region Fuse Interstitial Ads
	new public static void CheckAdAvailable()
	{
		Debug.Log("FuseAPI:CheckAdAvailable()");
		_fuseUnityPlugin.CallStatic("checkAdAvailable");
	}

	new public static void ShowAd()
	{
		Debug.Log("FuseAPI:ShowAd()");
		_fuseUnityPlugin.CallStatic("showAd");
	}

	private void _AdAvailabilityResponse(string available)
	{
		Debug.Log("FuseAPI:AdAvailabilityResponse(" + available + "," + _args[0] + ")");
		OnAdAvailabilityResponse(int.Parse(available), int.Parse(_args[0]));
		_args.Clear();
	}

	private void _AdDisplayed(string dummy)
	{
		Debug.Log("FuseAPI:AdDisplayed()");
		OnAdDisplayed();
	}

	private void _AdClicked(string dummy)
	{
		Debug.Log("FuseAPI:AdClicked()");
		OnAdClicked();
	}

	private void _AdWillClose(string dummy)
	{
		Debug.Log("FuseAPI:AdWillClose()");
		OnAdWillClose();
	}
#endregion

#region Notifications	
	new public static void DisplayNotifications()
	{
		Debug.Log("FuseAPI:DisplayNotifications()");
		_fuseUnityPlugin.CallStatic("displayNotifications");
	}

	private void _NotificationAction(string action)
	{
		Debug.Log("FuseAPI:NotificationAction()");
		OnNotificationAction(action);
	}
#endregion

#region More Games
	new public static void DisplayMoreGames()
	{
		Debug.Log("FuseAPI:DisplayMoreGames()");
		_fuseUnityPlugin.CallStatic("displayMoreGames");
	}
#endregion
	
#region Gender
	new public static void RegisterGender(Gender gender)
	{
		Debug.Log("FuseAPI:RegisterGender()");
		_fuseUnityPlugin.CallStatic("registerGender", (int)gender);
	}
#endregion

#region Account Login
	new public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		Debug.Log("FuseAPI:FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		_fuseUnityPlugin.CallStatic("facebookLogin", facebookId, name, accessToken);
	}

	new public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
		Debug.Log("FuseAPI:FacebookLogin(" + facebookId + "," + name + "," + gender + "," + accessToken + ")");
		_fuseUnityPlugin.CallStatic("facebookLoginGender", facebookId, name, (int)gender, accessToken);
	}

	new public static void TwitterLogin(string twitterId)
	{
		Debug.Log("FuseAPI:TwitterLogin(" + twitterId + ")");
		_fuseUnityPlugin.CallStatic("twitterLogin", twitterId);
	}	
	
	new public static void DeviceLogin(string alias)
	{
		Debug.Log("FuseAPI:DeviceLogin(" + alias + ")");
		_fuseUnityPlugin.CallStatic("deviceLogin", alias);
	}
	
	new public static void OpenFeintLogin(string openFeintId)
	{
		Debug.Log("FuseAPI:OpenFeintLogin(" + openFeintId + ")");
		_fuseUnityPlugin.CallStatic("openFeintLogin", openFeintId);
	}
	
	new public static void FuseLogin(string fuseId, string alias)
	{
		Debug.Log("FuseAPI:FuseLogin(" + fuseId + "," + alias + ")");
		_fuseUnityPlugin.CallStatic("fuseLogin", fuseId, alias);
	}
	
	new public static void GooglePlayLogin(string alias, string token)
	{
		Debug.Log("FuseAPI:GooglePlayLogin(" + alias + "," + token + ")");
		_fuseUnityPlugin.CallStatic("googlePlayLogin", alias, token);
	}

	public static void GameCenterLogin(string accountID, string alias)
	{
		// only for testing - does not actually log you into game center
		_fuseUnityPlugin.CallStatic("gamecenterLogin", accountID, alias);
	}
	
	new public static string GetOriginalAccountAlias()
	{
		Debug.Log("FuseAPI:GetOriginalAccountAlias()");
		return _fuseUnityPlugin.CallStatic<string>("getOriginalAccountAlias");
	}
	
	new public static string GetOriginalAccountId()
	{
		Debug.Log("FuseAPI:GetOriginalAccountId()");
		return _fuseUnityPlugin.CallStatic<string>("getOriginalAccountId");
	}
	
	new public static AccountType GetOriginalAccountType()
	{
		Debug.Log("FuseAPI:GetOriginalAccountType()");
		return (AccountType)_fuseUnityPlugin.CallStatic<int>("getOriginalAccountType");
	}

	private void _AccountLoginComplete(string accountId)
	{
		Debug.Log("FuseAPI:AccountLoginComplete(" + _args[0] + "," + accountId + ")");
		OnAccountLoginComplete((FuseAPI.AccountType)int.Parse(_args[0]), accountId);
	}
#endregion

#region Miscellaneous
	new public static int GamesPlayed()
	{
		Debug.Log("FuseAPI:GamesPlayed()");
		return _fuseUnityPlugin.CallStatic<int>("gamesPlayed");
	}
	
	new public static string LibraryVersion()
	{
		Debug.Log("FuseAPI:LibraryVersion()");
		return _fuseUnityPlugin.CallStatic<string>("libraryVersion");
	}

	new public static bool Connected()
	{
		Debug.Log("FuseAPI:Connected()");
		return _fuseUnityPlugin.CallStatic<bool>("connected");
	}

	new public static void TimeFromServer()
	{
		Debug.Log("FuseAPI:TimeFromServer()");
		_fuseUnityPlugin.CallStatic("timeFromServer");
	}

	private void _TimeUpdated(string timestamp)
	{
		Debug.Log("FuseAPI:TimeUpdated(" + timestamp + ")");
		OnTimeUpdated(TimestampToDateTime(long.Parse(timestamp)));
	}

	new public static bool NotReadyToTerminate()
	{
		Debug.Log("FuseAPI:NotReadyToTerminate()");
		return _fuseUnityPlugin.CallStatic<bool>("notReadyToTerminate");
	}
#endregion

#region Data Opt In/Out
	new public static void EnableData(bool enable)
	{
		Debug.Log("FuseAPI:EnableData(" + enable + ")");
		_fuseUnityPlugin.CallStatic("enableData", enable);
	}

	new public static bool DataEnabled()
	{
		Debug.Log("FuseAPI:DataEnabled()");
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
		Debug.Log ("FuseAPI:SetGameData(" + key + ", [data]," + isCollection + "," + fuseId + ")");

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
		Debug.Log("FuseAPI:GameDataSetAcknowledged(" + requestId + ")");
		OnGameDataSetAcknowledged(int.Parse(requestId));
	}

	private void _GameDataError(string fuseGameDataError)
	{
		Debug.Log("FuseAPI:GameDataError(" + fuseGameDataError + "," + _args[0] + ")");
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
		Debug.Log ("FuseAPI:GetGameData(" + fuseId + "," + key + ",[keys])");
		
		_fuseUnityPlugin.CallStatic("getGameDataStart");
			
		foreach (string entry in keys)
		{
			_fuseUnityPlugin.CallStatic("getGameDataKey", entry);
		}

		return _fuseUnityPlugin.CallStatic<int>("getGameDataEnd", key, fuseId);
	}

	private void _GameDataReceived(string requestId)
	{
		Debug.Log("FuseAPI:GameDataReceived(" + requestId + ")");

		Hashtable gameData = new Hashtable();
		for (int index = 1; index < _args.Count; index += 3)
		{
			Debug.Log(_args[index] + " " + _args[index+1] + " " + _args[index+2]);
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
	
	new public static void MigrateFriends(string fuseId)
	{
		Debug.Log("FuseAPI:MigrateFriends(" + fuseId + ")");
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
		Debug.Log("FuseAPI:UpdateFriendsListFromServer()");
		_fuseUnityPlugin.CallStatic("updateFriendsListFromServer");
	}
	
	new public static List<Friend> GetFriendsList()
	{
		Debug.Log("FuseAPI:GetFriendsList()");
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
		Debug.Log("FuseAPI:FriendsListUpdated()");
		OnFriendsListUpdated(ArgsToFriendsList());
	}

	private void _FriendsListError(string error)
	{
		Debug.Log("FuseAPI:FriendsListError(" + error + ")");
		OnFriendsListError(int.Parse(error));
	}
#endregion

#region User-to-User Push Notifications
	new public static void UserPushNotification(string fuseId, string message)
	{
		Debug.Log("FuseAPI:UserPushNotification(" + fuseId + "," + message + ")");
		_fuseUnityPlugin.CallStatic("userPushNotification", fuseId, message);
	}
	
	new public static void FriendsPushNotification(string message)
	{
		Debug.Log("FuseAPI:FriendsPushNotification(" + message + ")");
		_fuseUnityPlugin.CallStatic("friendsPushNotification", message);
	}
#endregion

#region Gifting
	new public static void GetMailListFromServer()
	{
		Debug.Log("FuseAPI:GetMailListFromServer()");
		_fuseUnityPlugin.CallStatic("getMailListFromServer");
	}
	
	new public static void GetMailListFriendFromServer(string fuseId)
	{
		Debug.Log("FuseAPI:GetMailListFriendFromServer(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("getMailListFriendFromServer", fuseId);
	}
	
	new public static List<Mail> GetMailList(string fuseId)
	{
		Debug.Log("FuseAPI:GetMailList(" + fuseId + ")");
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
		Debug.Log("FuseAPI:SetMailAsReceived(" + messageId + ")");
		_fuseUnityPlugin.CallStatic("setMailAsReceived", messageId);
	}
	
	new public static int SendMailWithGift(string fuseId, string message, int giftId, int giftAmount)
	{
		Debug.Log("FuseAPI:SendMailWithGift(" + fuseId + "," + message + "," + giftId + "," + giftAmount + ")");
		return _fuseUnityPlugin.CallStatic<int>("sendMailWithGift", fuseId, message, giftId, giftAmount);
	}
	
	new public static int SendMail(string fuseId, string message)
	{
		Debug.Log("FuseAPI:SendMail(" + fuseId + "," + message + ")");
		return _fuseUnityPlugin.CallStatic<int>("sendMail", fuseId, message);
	}

	private void _MailListReceived(string fuseId)
	{
		Debug.Log("FuseAPI:MailListReceived(" + fuseId + ")");
		OnMailListReceived(ArgsToMailList(), fuseId);
	}

	private void _MailListError(string error)
	{
		Debug.Log("FuseAPI:MailListError(" + error + ")");
		OnMailListError(int.Parse(error));
	}

	private void _MailAcknowledged()
	{
		string messageId = _args[0];
		string fuseId = _args[1];
		string requestID = _args[2];
		Debug.Log("FuseAPI:MailAcknowledged(" + messageId + "," + fuseId + "," + requestID + ")");
		OnMailAcknowledged(int.Parse(messageId), fuseId, int.Parse (requestID));
		_args.Clear();
	}

	private void _MailError(string error)
	{
		Debug.Log("FuseAPI:MailError(" + error + ")");
		OnMailError(int.Parse(error));
	}
#endregion

#region Game Configuration Data
	new public static string GetGameConfigurationValue(string key)
	{
//		Debug.Log("FuseAPI:GetGameConfigurationValue(" + key + ")");
		_fuseUnityPlugin.SetStatic<string>("_stringConduit", key);
		_fuseUnityPlugin.CallStatic("getGameConfigurationValue");
		return _fuseUnityPlugin.GetStatic<string>("_stringConduit");
	}
	
	new public static Dictionary<string, string> GetGameConfig()
	{
		//Debug.Log("FuseAPI:GetGameConfig()");		
	
		Dictionary<string, string> gameConfig = new Dictionary<string, string>();
		// get a list of all the keys:
		string[] keys = _fuseUnityPlugin.CallStatic<string[]>("getGameConfigKeys");
		for( int i = 0; i < keys.Length; i++ )
		{
			gameConfig.Add(keys[i], GetGameConfigurationValue(keys[i]));
		}
		
		return gameConfig;
	}

	private void _GameConfigurationReceived(string dummy)
	{
		Debug.Log("FuseAPI:GameConfigurationReceived()");
		OnGameConfigurationReceived();
	}
#endregion

#region Specific Event Registration
	new public static void RegisterLevel(int level)
	{
		Debug.Log("FuseAPI:RegisterLevel(" + level + ")");
		_fuseUnityPlugin.CallStatic("registerLevel", level);
	}
	
	new public static void RegisterCurrency(int type, int balance)
	{
		Debug.Log("FuseAPI:RegisterCurrency(" + type + "," + balance + ")");
		_fuseUnityPlugin.CallStatic("registerCurrency", type, balance);
	}
	
	new public static void RegisterFlurryView()
	{
		Debug.Log("FuseAPI:RegisterFlurryView()");
		_fuseUnityPlugin.CallStatic("registerFlurryView");
	}
	
	new public static void RegisterFlurryClick()
	{
		Debug.Log("FuseAPI:RegisterFlurryClick()");
		_fuseUnityPlugin.CallStatic("registerFlurryClick");
	}
	
	new public static void RegisterTapjoyReward(int amount)
	{
		Debug.Log("FuseAPI:RegisterTapjoyReward(" + amount + ")");
		_fuseUnityPlugin.CallStatic("registerTapjoyReward", amount);
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
		Debug.Log("_AddArgument: " + message);
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

