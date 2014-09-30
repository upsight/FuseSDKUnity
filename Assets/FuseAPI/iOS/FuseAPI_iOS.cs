using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FuseAPI_iOS : FuseAPI
{
	public bool logging = false;
	public bool persistent = true;
	public bool registerForPushNotifications = true;	
#if UNITY_IPHONE && !UNITY_EDITOR
	#region Session Creation
	[DllImport("__Internal")]
	private static extern void FuseAPI_StartSession(string gameId);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterPushToken(Byte[] token, int size);
	
	private bool waitingForToken = false;	
	private static bool _registerForPushNotificationsCalled = false;
	private static Dictionary<string, string> gameConfig = new Dictionary<string, string>();
	
	public static bool debugOutput = false;

	void Awake()
	{
		// preserve the prefab and all attached scripts through level loads
		if( persistent )
		{
			DontDestroyOnLoad(transform.gameObject);
		}
		if(logging)
		{
			FuseAPI_iOS.debugOutput = true;
		}
	}
	
	new public static void StartSession(string gameId)
	{
		FuseLog("StartSession(" + gameId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_StartSession(gameId);
		}
		else
		{
			_SessionStartReceived();
		}
	}
	
	void Update()
	{
		if( registerForPushNotifications || _registerForPushNotificationsCalled )
		{
			FuseLog(" Registering for push notifications...");
			NotificationServices.RegisterForRemoteNotificationTypes(RemoteNotificationType.Alert |
                                RemoteNotificationType.Badge |
                                RemoteNotificationType.Sound);
			
			registerForPushNotifications = false;
			_registerForPushNotificationsCalled = false;
			waitingForToken = true;
		}
		
		if( waitingForToken )
		{
        	byte[] token = NotificationServices.deviceToken;
            if (token != null) 
			{
				FuseLog(" Device token registered!");
                FuseAPI_RegisterPushToken(token, token.Length); 
                waitingForToken = false;
        	}
            if( NotificationServices.registrationError != null )
			{
				FuseLog("Failed to register for push notification device token with error: " + NotificationServices.registrationError);
				waitingForToken = false;
			}
		}
	}	
	
	private static void _SessionStartReceived()
	{
		FuseLog("SessionStartReceived()");
		
		OnSessionStartReceived();
	}
	
	private static void _SessionLoginError(int error)
	{
		FuseLog("SessionLoginError(" + error + ")");
		
		OnSessionLoginError(error);
	}
	
    #endregion
	
    #region Analytics Event
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterEvent(string message);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterEventStart();
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterEventKeyValue(string entryKey, double entryValue);
	[DllImport("__Internal")]
	private static extern int FuseAPI_RegisterEventEnd(string name, string paramName, string paramValue);
	[DllImport("__Internal")]
	private static extern int FuseAPI_RegisterEventVariable(string name, string paramName, string paramValue, string variableName, double variableValue);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterEventWithDictionary(string message, string[] keys, string[] values, int numEntries);	
	
	new public static void RegisterEvent(string message)
	{
		FuseLog("RegisterEvent(" + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterEvent(message);
		}
	}
	
	new public static void RegisterEvent(string message, Hashtable values)
	{
		FuseLog("RegisterEvent(" + message + ", [variables])");
		
		if( values == null )
		{
			RegisterEvent(message);
			return;
		}
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			int max_entries = values.Keys.Count;
			string[] keys = new string[max_entries];			
			string[] attributes = new string[max_entries];
			keys.Initialize();
			attributes.Initialize();
			int numEntries = 0;
			foreach (DictionaryEntry entry in values)
			{
				string entryKey = entry.Key as string;
				string entryValue = "";
				if( entry.Value != null )
				{
					entryValue = entry.Value.ToString();
				}
				
				keys[numEntries] = entryKey;
				attributes[numEntries] = entryValue;
				numEntries++;
			}
			FuseAPI_RegisterEventWithDictionary(message, keys, attributes, numEntries);
		}
	}
	
	new public static int RegisterEvent(string name, string paramName, string paramValue, Hashtable variables)
	{
		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + ", [variables])");

		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterEventStart();
			
			FuseLog("Registering KVPs");
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
					FuseAPI_RegisterEventKeyValue(entryKey, entryValue);					
				}
			}
			
			FuseLog("End Register Event");
			return FuseAPI_RegisterEventEnd(name, paramName, paramValue);
		}

		return -1;
	}

	new public static int RegisterEvent(string name, string paramName, string paramValue, string variableName, double variableValue)
	{
		if( name == null || paramName == null || paramValue == null || variableName == null )
		{
			return -1;
		}
		
		FuseLog ("RegisterEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return FuseAPI_RegisterEventVariable(name, paramName, paramValue, variableName, variableValue);
		}

		return -1;
	}
    #endregion
	
    #region In-App Purchase Logging
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterInAppPurchaseListStart();
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterInAppPurchaseListProduct(string productId, string priceLocale, float price);
	[DllImport("__Internal")]
	private static extern int FuseAPI_RegisterInAppPurchaseListEnd();
		
	new public static void RegisterInAppPurchaseList(Product[] products)
	{
		FuseLog ("RegisterInAppPurchaseList(" + products.Length + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if( products.Length == 0 )
				return;
			
			FuseAPI_RegisterInAppPurchaseListStart();
			
			foreach (Product product in products)
			{
				FuseAPI_RegisterInAppPurchaseListProduct(product.productId, product.priceLocale, product.price);
			}
			
			FuseAPI_RegisterInAppPurchaseListEnd();
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterInAppPurchase(string productId, string transactionId, byte[] transactionReceiptBuffer, int transactionReceiptLength, int transactionState);
	
//	new public enum TransactionState { PURCHASING, PURCHASED, FAILED, RESTORED }
	
	new public static void RegisterInAppPurchase(string productId, string transactionId, byte[] transactionReceipt, TransactionState transactionState)
	{
		FuseLog("RegisterInAppPurchase(" + productId + "," + transactionReceipt.Length + "," + transactionState + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterInAppPurchase(productId, transactionId, transactionReceipt, transactionReceipt.Length, (int)transactionState);
		}
		else
		{
			_PurchaseVerification(true, "", "");
		}
	}
	
	private static void _PurchaseVerification(bool verified, string transactionId, string originalTransactionId)
	{
		FuseLog("PurchaseVerification(" + verified + "," + transactionId + "," + originalTransactionId + ")");
		
		OnPurchaseVerification(verified, transactionId, originalTransactionId);
	}

	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterUnibillPurchase(string productID, byte[] receipt, int receiptLength);

	new public static void RegisterUnibillPurchase(string productID, byte[] receipt)
	{
		FuseLog("Registering Unibill transaction with product ID: " + productID);
		FuseAPI_RegisterUnibillPurchase(productID, receipt, receipt.Length);
	}

    #endregion
	
    #region Fuse Interstitial Ads
	[DllImport("__Internal")]
	private static extern void FuseAPI_CheckAdAvailable(string adZone);
	[DllImport("__Internal")]
	private static extern void FuseAPI_ShowAd(string asZone);
	[DllImport("__Internal")]
	private static extern void FuseAPI_PreloadAdForZone(string asZone);

	new public static void PreLoadAd(string adZone)
	{
		FuseLog("PreloadAd()");
		FuseAPI_PreloadAdForZone(adZone);
	}

	new public static void CheckAdAvailable(string adZone)
	{
		FuseLog("CheckAdAvailable()");
		FuseAPI_CheckAdAvailable(adZone);
	}

	new public static void ShowAd(string adZone)
	{
		FuseLog("ShowAd()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_ShowAd(adZone);
		}
		else
		{
			_AdWillClose();
		}
	}

	private static void _AdAvailabilityResponse(int available, int error)
	{
		FuseLog("AdAvailabilityResponse(" + available + "," + error + ")");

		OnAdAvailabilityResponse(available, error);
	}

	private static void _AdWillClose()
	{
		FuseLog("AdWillClose()");
		
		OnAdWillClose();
	}
	
    #endregion

    #region Notifications
	[DllImport("__Internal")]
	private static extern void FuseAPI_DisplayNotifications();
	
	new public static void FuseAPI_RegisterForPushNotifications()
	{
		_registerForPushNotificationsCalled = true;
	}
	
	new public static void DisplayNotifications()
	{
		FuseLog("DisplayNotifications()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_DisplayNotifications();
		}
	}

    [DllImport("__Internal")]
	private static extern bool FuseAPI_IsNotificationAvailable();
    new public static bool IsNotificationAvailable()
    {
		FuseLog("IsNotificationAvailable()");
        return FuseAPI_IsNotificationAvailable();
    }
	
	private static void _NotificationAction(string action)
	{
		FuseLog("NotificationAction(" + action + ")");
		
		OnNotificationAction(action);
	}
	
    #endregion

    #region More Games
	[DllImport("__Internal")]
	private static extern void FuseAPI_DisplayMoreGames();
	
	new public static void DisplayMoreGames()
	{
		FuseLog("DisplayMoreGames()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_DisplayMoreGames();
		}
		else
		{
			_OverlayWillClose();
		}
	}
	
	private static void _OverlayWillClose()
	{
		FuseLog("OverlayWillClose()");
		OnOverlayWillClose();
	}
	
    #endregion
	
    #region Gender
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterGender(int gender);
	
	new public static void RegisterGender(Gender gender)
	{
		FuseLog("RegisterGender(" + gender + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterGender((int)gender);
		}
	}
    #endregion
	
    #region Account Login
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_GameCenterLogin();
	
	new public static void GameCenterLogin()
	{
		FuseLog ("GameCenterLogin()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_GameCenterLogin();
		}
		else
		{
			_AccountLoginComplete(AccountType.GAMECENTER, "");
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_FacebookLogin(string facebookId, string name, string accessToken);
	
	new public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		FuseLog ("FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_FacebookLogin(facebookId, name, accessToken);
		}
		else
		{
			_AccountLoginComplete(AccountType.FACEBOOK, facebookId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_FacebookLoginGender(string facebookId, string name, Gender gender, string accessToken);
	
	new public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
		FuseLog ("FacebookLogin(" + facebookId + "," + name + "," + gender + "," + accessToken + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_FacebookLoginGender(facebookId, name, gender, accessToken);
		}
		else
		{
			_AccountLoginComplete(AccountType.FACEBOOK, facebookId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_TwitterLogin(string twitterId);
	
	new public static void TwitterLogin(string twitterId)
	{
		FuseLog ("TwitterLogin(" + twitterId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_TwitterLogin(twitterId);
		}
		else
		{
			_AccountLoginComplete(AccountType.TWITTER, twitterId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_DeviceLogin(string alias);
	new public static void DeviceLogin(string alias)
	{
		FuseLog ("DeviceLogin(" + alias + ")");
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_DeviceLogin(alias);
		}
		else
		{
			_AccountLoginComplete(AccountType.DEVICE_ID, alias);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_OpenFeintLogin(string openFeintId);
	
	new public static void OpenFeintLogin(string openFeintId)
	{
		FuseLog ("OpenFeintLogin(" + openFeintId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_OpenFeintLogin(openFeintId);
		}
		else
		{
			_AccountLoginComplete(AccountType.OPENFEINT, openFeintId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_FuseLogin(string fuseId, string alias);
	
	new public static void FuseLogin(string fuseId, string alias)
	{
		FuseLog(" FuseLogin(" + fuseId + "," + alias + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_FuseLogin(fuseId, alias);
		}
		else
		{
			_AccountLoginComplete(AccountType.USER, fuseId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_GooglePlayLogin(string alias, string token);
	
	new public static void GooglePlayLogin(string alias, string token)
	{		
		FuseLog("GooglePlayLogin(" + alias + "," + token + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_GooglePlayLogin(alias, token);
		}
		else
		{
			_AccountLoginComplete(AccountType.GOOGLE_PLAY, alias);
		}
	}
	
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetOriginalAccountAlias();
	
	new public static string GetOriginalAccountAlias()
	{		
		FuseLog("GetOriginalAccountAlias()");		
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string accountAlias = FuseAPI_GetOriginalAccountAlias();
			
			return accountAlias;
		}
		else
		{
			return "";
		}
	}
	
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetOriginalAccountId();
	
	new public static string GetOriginalAccountId()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string accountId = FuseAPI_GetOriginalAccountId();
			
			FuseLog("GetOriginalAccountId()==" + accountId);
			
			return accountId;
		}
		else
		{
			FuseLog("GetOriginalAccountId()");
			
			return "";
		}
	}
	
	[DllImport("__Internal")]
	private static extern AccountType FuseAPI_GetOriginalAccountType();
	
	new public static AccountType GetOriginalAccountType()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			AccountType accountType = FuseAPI_GetOriginalAccountType();
			
			FuseLog("GetOriginalAccountType()==" + accountType);
			
			return accountType;
		}
		else
		{
			FuseLog("GetOriginalAccountType()");
			
			return AccountType.NONE;
		}
	}
	
	private static void _AccountLoginComplete(AccountType type, string accountId)
	{
		FuseLog("AccountLoginComplete(" + type + "," + accountId + ")");
		
		OnAccountLoginComplete(type, accountId);
	}
	
    #endregion
	
    #region Miscellaneous
	[DllImport("__Internal")]
	private static extern int FuseAPI_GamesPlayed();
	
	new public static int GamesPlayed()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			int gamesPlayed = FuseAPI_GamesPlayed();
			
			FuseLog("GamesPlayed()==" + gamesPlayed);
		
			return gamesPlayed;
		}
		else
		{
			return 0;
		}
	}
	
	[DllImport("__Internal")]
	private static extern string FuseAPI_LibraryVersion();
	
	new public static string LibraryVersion()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string libraryVersion = FuseAPI_LibraryVersion();
			
			FuseLog("LibraryVersion()==" + libraryVersion);
		
			return libraryVersion;
		}
		else
		{
			return "1.22";
		}
	}
	[DllImport("__Internal")]
	private static extern bool FuseAPI_Connected();
	
	new public static bool Connected()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			bool connected = FuseAPI_Connected();
			
			FuseLog("Connected()==" + connected);
		
			return connected;
		}
		else
		{
			return true;
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_TimeFromServer();
	
	new public static void TimeFromServer()
	{
		FuseLog("TimeFromServer()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_TimeFromServer();
		}
		else
		{
			_TimeUpdated((DateTime.UtcNow - unixEpoch).Ticks / TimeSpan.TicksPerSecond);
		}
	}
	
	private static void _TimeUpdated(long timestamp)
	{
		FuseLog("TimeUpdated(" + timestamp + ")");
		
		if (timestamp >= 0)
		{
			OnTimeUpdated(unixEpoch + TimeSpan.FromTicks(timestamp * TimeSpan.TicksPerSecond));
		}
	}
	
	private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	
	[DllImport("__Internal")]
	private static extern bool FuseAPI_NotReadyToTerminate();
	
	new public static bool NotReadyToTerminate()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			bool notReadyToTerminate = FuseAPI_NotReadyToTerminate();
			
			FuseLog("NotReadyToTerminate()==" + notReadyToTerminate);
		
			return notReadyToTerminate;
		}
		else
		{
			return false;
		}
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
	[DllImport("__Internal")]
	private static extern void FuseAPI_EnableData(bool enable);
	
	new public static void EnableData(bool enable)
	{
		FuseLog("EnableData(" + enable + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_EnableData(enable);
		}
	}
	
	[DllImport("__Internal")]
	private static extern bool FuseAPI_DataEnabled();
	
	new public static bool DataEnabled()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			bool enabled = FuseAPI_DataEnabled();
			
			FuseLog("DataEnabled()==" + enabled);
		
			return enabled;
		}
		else
		{
			return true;
		}
	}
    #endregion
	
    #region User Game Data
	[DllImport("__Internal")]
	private static extern void FuseAPI_SetGameDataStart(string key, bool isCollection, string fuseId);
	[DllImport("__Internal")]
	private static extern void FuseAPI_SetGameDataKeyValue(string key, string value, bool isBinary);
	[DllImport("__Internal")]
	private static extern int FuseAPI_SetGameDataEnd();
	
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
		FuseLog("SetGameData(" + key + "," + isCollection + "," + fuseId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_SetGameDataStart(key, isCollection, fuseId);
			
			foreach (DictionaryEntry entry in data)
			{
				string entryKey = entry.Key as string;
				byte[] buffer = entry.Value as byte[];
				if (buffer != null)
				{
					string entryValue = Convert.ToBase64String(buffer);
					FuseAPI_SetGameDataKeyValue(entryKey, entryValue, true);
				}
				else
				{
					string entryValue = entry.Value.ToString();
					FuseAPI_SetGameDataKeyValue(entryKey, entryValue, false);
				}
			}
			
			return FuseAPI_SetGameDataEnd();
		}
		else
		{
			_GameDataSetAcknowledged(-1);
			
			return -1;
		}
	}
	
	private static void _GameDataError(int error, int requestId)
	{
		FuseLog("GameDataError(" + error + "," + requestId + ")");
		
		OnGameDataError(error, requestId);
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_GetGameDataStart(string key, string fuseId);
	[DllImport("__Internal")]
	private static extern void FuseAPI_GetGameDataKey(string key);
	[DllImport("__Internal")]
	private static extern int FuseAPI_GetGameDataEnd();
	
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
		FuseLog("GetGameData(" + fuseId + "," + key + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_GetGameDataStart(key, fuseId);
			
			foreach (string entry in keys)
			{
				FuseAPI_GetGameDataKey(entry);
			}
			
			return FuseAPI_GetGameDataEnd();
		}
		else
		{
			_GameDataReceivedStart(fuseId, key, -1);
			_GameDataReceivedEnd();
			
			return -1;
		}
	}
	
	private static void _GameDataSetAcknowledged(int requestId)
	{
		FuseLog("GameDataSetAcknowledged(" + requestId + ")");
		
		OnGameDataSetAcknowledged(requestId);
	}
	
	private static void _GameDataReceivedStart(string fuseId, string key, int requestId)
	{
		FuseLog("GameDataReceivedStart(" + fuseId + "," + key + ")");
		
		_gameDataFuseId = fuseId;
		_gameDataKey = key;
		_gameData = new Hashtable();
		_gameDataRequestId = requestId;
	}
	
	private static void _GameDataReceivedKeyValue(string key, string value, bool isBinary)
	{
		FuseLog("GameDataReceivedKeyValue(" + key + "," + value + "," + isBinary + ")");
		
		if (isBinary)
		{
			byte[] buffer = Convert.FromBase64String(value);
			_gameData.Add(key, buffer);
		}
		else
		{
			_gameData.Add(key, value);
		}
	}
	
	private static void _GameDataReceivedEnd()
	{
		FuseLog("GameDataReceivedEnd()");
		
		OnGameDataReceived(_gameDataFuseId, _gameDataKey, _gameData, _gameDataRequestId);
		_gameDataRequestId = -1;
		_gameData = null;
		_gameDataKey = "";
		_gameDataFuseId = "";
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_RefreshGameConfiguration();

	new public static Dictionary<string, string> GetGameConfig()
	{
		FuseLog("GetGameConfig()");
		
		// re-populate the game config
		FuseAPI_RefreshGameConfiguration();		
				
		return gameConfig;
	}
	
	private static void _GameConfigInit()
	{
		FuseLog("GameConfigInit()");
		
		// clear the dictionary
		gameConfig.Clear();
	}
	
	private static void _UpdateGameConfig(string key, string value)
	{
		FuseLog("UpdateGameConfig()");
		
		// add value for key here
		gameConfig.Add(key, value);
	}	
	
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetFuseId();
	
	new public static string GetFuseId()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string fuseId = FuseAPI_GetFuseId();
			
			FuseLog("GetFuseId()==" + fuseId);
			
			return fuseId;
		}
		else
		{
			FuseLog("GetFuseId()");
			
			return "";
		}
	}
	
	private static string _gameDataKey = "";
	private static string _gameDataFuseId = "";
	private static Hashtable _gameData = null;
	private static int _gameDataRequestId = -1;
    #endregion
	
    #region Friend List
	[DllImport("__Internal")]
	private static extern void FuseAPI_AddFriend(string fuseId);
	new public static void AddFriend(string fuseId)
	{
		FuseAPI_AddFriend(fuseId);
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_RemoveFriend(string fuseId);
	new public static void RemoveFriend(string fuseId)
	{
		FuseAPI_RemoveFriend(fuseId);
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_AcceptFriend(string fuseId);
	new public static void AcceptFriend(string fuseId)
	{
		FuseAPI_AcceptFriend(fuseId);
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_RejectFriend(string fuseId);
	new public static void RejectFriend(string fuseId)
	{
		FuseAPI_RejectFriend(fuseId);
	}
	
	private static void _FriendAdded(string fuseId, int error)
	{
		OnFriendAdded(fuseId, error);
	}
	private static void _FriendRemoved(string fuseId, int error)
	{
		OnFriendRemoved(fuseId, error);
	}
	private static void _FriendAccepted(string fuseId, int error)
	{
		OnFriendAccepted(fuseId, error);
	}
	private static void _FriendRejected(string fuseId, int error)
	{
		OnFriendRejected(fuseId, error);
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_MigrateFriends(string fuseId);
	
	new public static void MigrateFriends(string fuseId)
	{
		FuseAPI_MigrateFriends(fuseId);
	}
	
	private static void _FriendsMigrated(string fuseId, int error)
	{
		OnFriendsMigrated(fuseId, error);
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_UpdateFriendsListFromServer();
	
	new public static void UpdateFriendsListFromServer()
	{
		FuseLog("UpdateFriendsListFromServer()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_UpdateFriendsListFromServer();
		}
		else
		{
			_FriendsListUpdatedStart();
			_FriendsListUpdatedEnd();
		}
	}
	
	private static void _FriendsListUpdatedStart()
	{
		FuseLog("FriendsListUpdatedStart()");
		
		_friendsList = new List<Friend>();
	}
	
	private static void _FriendsListUpdatedFriend(string fuseId, string accountId, string alias, bool pending)
	{
		FuseLog("FriendsListUpdatedFriend(" + fuseId + "," + accountId + "," + alias + "," + pending + ")");
		
		Friend friend = new Friend();
		friend.fuseId = fuseId;
		friend.accountId = accountId;
		friend.alias = alias;
		friend.pending = pending;
		
		_friendsList.Add(friend);
	}
	
	private static void _FriendsListUpdatedEnd()
	{
		FuseLog("FriendsListUpdatedEnd()");
		
		OnFriendsListUpdated(_friendsList);
		
		_friendsList = null;
	}
	
	private static void _FriendsListError(int error)
	{
		FuseLog("FriendsListError(" + error + ")");
		
		OnFriendsListError(error);
	}
	
	private static List<Friend> _friendsList = null;
	
	[DllImport("__Internal")]
	private static extern int FuseAPI_GetFriendsListCount();
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetFriendsListFriendFuseId(int index);
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetFriendsListFriendAccountId(int index);
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetFriendsListFriendAlias(int index);
	[DllImport("__Internal")]
	private static extern bool FuseAPI_GetFriendsListFriendPending(int index);
	
	new public static List<Friend> GetFriendsList()
	{
		FuseLog("GetFriendsList()");
		
		List<Friend> friendsList = new List<Friend>();
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			int friendCount = FuseAPI_GetFriendsListCount();
			
			for (int index = 0; index < friendCount; index++)
			{
				Friend friend = new Friend();
				friend.fuseId = FuseAPI_GetFriendsListFriendFuseId(index);
				friend.accountId = FuseAPI_GetFriendsListFriendAccountId(index);
				friend.alias = FuseAPI_GetFriendsListFriendAlias(index);
				friend.pending = FuseAPI_GetFriendsListFriendPending(index);
				
				friendsList.Add(friend);
			}
		}
			
		return friendsList;
	}
    #endregion

    #region Chat List
    #endregion

    #region User-to-User Push Notifications
	[DllImport("__Internal")]
	private static extern void FuseAPI_UserPushNotification(string fuseId, string message);
	
	new public static void UserPushNotification(string fuseId, string message)
	{
		FuseLog("UserPushNotification(" + fuseId +"," + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_UserPushNotification(fuseId, message);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_FriendsPushNotification(string message);
	
	new public static void FriendsPushNotification(string message)
	{
		FuseLog("FriendsPushNotification(" + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_FriendsPushNotification(message);
		}
	}
    #endregion
	
    #region Gifting
	[DllImport("__Internal")]
	private static extern void FuseAPI_GetMailListFromServer();
	[DllImport("__Internal")]
	private static extern void FuseAPI_GetMailListFriendFromServer(string fuseId);
	
	new public static void GetMailListFromServer()
	{
		FuseLog("GetMailListFromServer()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_GetMailListFromServer();
		}
		else
		{
			_MailListReceivedStart("");
			_MailListReceivedEnd();
		}
	}
	
	new public static void GetMailListFriendFromServer(string fuseId)
	{
		FuseLog("GetMailListFriendFromServer(" + fuseId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_GetMailListFriendFromServer(fuseId);
		}
		else
		{
			_MailListReceivedStart(fuseId);
			_MailListReceivedEnd();
		}
	}
	
	private static void _MailListReceivedStart(string fuseId)
	{
		FuseLog("MailListReceivedStart()");
		
		_mailListFuseId = fuseId;
		_mailList = new List<Mail>();
	}
	
	private static void _MailListReceivedMail(int messageId, long timestamp, string alias, string message, int giftId, string giftName, int giftAmount)
	{
		FuseLog("MailListReceivedMail(" + messageId + "," + timestamp + "," + alias + "," + message + "," + giftId + "," + giftName + "," + giftAmount + ")");
		
		Mail mail = new Mail();
		mail.messageId = messageId;
		mail.timestamp = unixEpoch + TimeSpan.FromTicks(timestamp * TimeSpan.TicksPerSecond);
		mail.alias = alias;
		mail.message = message;
		mail.giftId = giftId;
		mail.giftName = giftName;
		mail.giftAmount = giftAmount;
		
		_mailList.Add(mail);
	}
	
	private static void _MailListReceivedEnd()
	{
		FuseLog("MailListReceivedEnd()");
		
		OnMailListReceived(_mailList, _mailListFuseId);
		
		_mailList = null;
		_mailListFuseId = "";
	}
	
	private static void _MailListError(int error)
	{
		FuseLog("MailListError(" + error + ")");
		
		OnMailListError(error);
	}
	
	private static List<Mail> _mailList = null;
	private static string _mailListFuseId = "";
	
	[DllImport("__Internal")]
	private static extern int FuseAPI_GetMailListCount(string fuseId);
	[DllImport("__Internal")]
	private static extern int FuseAPI_GetMailListMailMessageId(string fuseId, int index);
	[DllImport("__Internal")]
	private static extern long FuseAPI_GetMailListMailTimestamp(string fuseId, int index);
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetMailListMailAlias(string fuseId, int index);
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetMailListMailMessage(string fuseId, int index);
	[DllImport("__Internal")]
	private static extern int FuseAPI_GetMailListMailGiftId(string fuseId, int index);
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetMailListMailGiftName(string fuseId, int index);
	[DllImport("__Internal")]
	private static extern int FuseAPI_GetMailListMailGiftAmount(string fuseId, int index);
	
	new public static List<Mail> GetMailList(string fuseId)
	{
		FuseLog("GetMailList()");
		
		List<Mail> mailList = new List<Mail>();
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			int mailCount = FuseAPI_GetMailListCount(fuseId);
			
			for (int index = 0; index < mailCount; index++)
			{
				Mail mail = new Mail();
				mail.messageId = FuseAPI_GetMailListMailMessageId(fuseId, index);
				mail.timestamp = unixEpoch + TimeSpan.FromTicks(FuseAPI_GetMailListMailTimestamp(fuseId, index) * TimeSpan.TicksPerSecond);
				mail.alias = FuseAPI_GetMailListMailAlias(fuseId, index);
				mail.message = FuseAPI_GetMailListMailMessage(fuseId, index);
				mail.giftId = FuseAPI_GetMailListMailGiftId(fuseId, index);
				mail.giftName = FuseAPI_GetMailListMailGiftName(fuseId, index);
				mail.giftAmount = FuseAPI_GetMailListMailGiftAmount(fuseId, index);
				
				mailList.Add(mail);
			}
		}
			
		return mailList;
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_SetMailAsReceived(int messageId);
	
	new public static void SetMailAsReceived(int messageId)
	{
		FuseLog("SetMailAsReceived(" + messageId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_SetMailAsReceived(messageId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern int FuseAPI_SendMail(string fuseId, string message);
	
	new public static int SendMail(string fuseId, string message)
	{
		FuseLog("SendMail(" + fuseId + "," + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return FuseAPI_SendMail(fuseId, message);
		}
		else
		{
			_MailAcknowledged(-1, fuseId, -1);
			return -1;
		}
	}
	
	[DllImport("__Internal")]
	private static extern int FuseAPI_SendMailWithGift(string fuseId, string message, int giftId, int giftAmount);
	
	new public static int SendMailWithGift(string fuseId, string message, int giftId, int giftAmount)
	{
		FuseLog("SendMailWithGift(" + fuseId + "," + message + "," + giftId + "," + giftAmount + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return FuseAPI_SendMailWithGift(fuseId, message, giftId, giftAmount);
		}
		else
		{
			_MailAcknowledged(-1, fuseId, -1);
			return -1;
		}
	}
	
	private static void _MailAcknowledged(int messageId, string fuseId, int requestID)
	{
		FuseLog("MailAcknowledged()");
		
		OnMailAcknowledged(messageId, fuseId, requestID);
	}
	
	private static void _MailError(int error)
	{
		FuseLog("MailError(" + error + ")");
		
		OnMailError(error);
	}
	
    #endregion

    #region Game Configuration Data
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetGameConfigurationValue(string key);	
	
	new public static string GetGameConfigurationValue(string key)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string value = FuseAPI_GetGameConfigurationValue(key);
			
			FuseLog("GetGameConfigurationValue(" + key + ")==" + value + "");
			
			return value;
		}
		else
		{
			FuseLog("GetGameConfigurationValue(" + key + ")");
			
			return "";
		}
	}
	
	private static void _GameConfigurationReceived()
	{
		FuseLog("GameConfigurationReceived()");
		
		OnGameConfigurationReceived();
	}
	
    #endregion
	
    #region Specific Event Registration
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterLevel(int level);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterCurrency(int type, int balance);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterFlurryView();
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterFlurryClick();
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterTapjoyReward(int amount);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterAge(int age);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterBirthday(int year, int month, int day);
	
	new public static void RegisterLevel(int level)
	{
		FuseLog("RegisterLevel(" + level + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterLevel(level);
		}
	}
	
	new public static void RegisterCurrency(int type, int balance)
	{
		FuseLog("RegisterCurrency(" + type + "," + balance + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterCurrency(type, balance);
		}
	}
	
	new public static void RegisterFlurryView()
	{
		FuseLog("RegisterFlurryView()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterFlurryView();
		}
	}
	
	new public static void RegisterFlurryClick()
	{
		FuseLog("RegisterFlurryClick()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterFlurryClick();
		}
	}
	
	new public static void RegisterTapjoyReward(int amount)
	{
		FuseLog("RegisterTapjoyReward(" + amount + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterTapjoyReward(amount);
		}
	}
	
	new public static void RegisterAge(int age)
	{
		// Available in 1.35.2 and above
		FuseLog("RegisterAge(" + age + ")");
		FuseAPI_RegisterAge(age);
	}
	
	new public static void RegisterBirthday(int year, int month, int day)
	{
		// Available in 1.35.2 and above
		FuseLog("RegisterBirthday(" + year + ", " + month + ", " + day + ")");
		FuseAPI_RegisterBirthday(year, month, day);
	}
	
    #endregion
	
#endif
}