using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FuseAPI_iOS : FuseAPI
{

#if UNITY_IPHONE && !UNITY_EDITOR
	#region Session Creation
	[DllImport("__Internal")]
	private static extern void FuseAPI_StartSession(string gameId);
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterPushToken(Byte[] token, int size);
	[DllImport("__Internal")]
	private static extern void FuseAPI_SetUnityGameObject(string gameObjectName);
	
	private bool waitingForToken = false;	
	private static bool _registerForPushNotificationsCalled = false;
	private static Dictionary<string, string> _gameConfig = new Dictionary<string, string>();
	
	public static bool debugOutput = false;

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
			FuseAPI_iOS.debugOutput = true;
		}

		FuseAPI_SetUnityGameObject(gameObject.name);
	}
	
	[Obsolete("StartSession now called automatically. Set gameID in the FuseAPI Component.", true)]
	new public static void StartSession(string gameId)
	{
	}
	
	new protected static void _StartSession(string gameId)
	{
		if(string.IsNullOrEmpty(gameId))
			Debug.LogError("FuseSDK: Null or empty API Key. Make sure your API Key is entered in the FuseSDK prefab");

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
	
	void Start()
	{
		_StartSession(iOSGameID);
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
	
	private void _CB_SessionStartReceived(string param)
	{
		_SessionStartReceived();
	}

	private static void _SessionStartReceived()
	{
		FuseLog("SessionStartReceived()");
		
		OnSessionStartReceived();
	}

	private void _CB_SessionLoginError(string param)
	{
		int error;
		if(int.TryParse(param, out error))
			_SessionLoginError(error);
		else
			Debug.LogError("FuseSDK: Parsing error in _SessionLoginError");
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
			_PurchaseVerification(1, "", "");
		}
	}

	private void _CB_PurchaseVerification(string param)
	{
		int verified;

		var pars = param.Split(',');
		if(pars.Length == 3 && int.TryParse(pars[0], out verified))
			_PurchaseVerification(verified, pars[1], pars[2]);
		else
			Debug.LogError("FuseSDK: Parsing error in _PurchaseVerification");
	}
	
	private static void _PurchaseVerification(int verified, string transactionId, string originalTransactionId)
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
	
	#region Fuse Ads
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

	private void _CB_AdAvailabilityResponse(string param)
	{
		int available;
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out available) && int.TryParse(pars[1], out error))
			_AdAvailabilityResponse(available, error);
		else
			Debug.LogError("FuseSDK: Parsing error in _AdAvailabilityResponse");
	}

	private static void _AdAvailabilityResponse(int available, int error)
	{
		FuseLog("AdAvailabilityResponse(" + available + "," + error + ")");

		OnAdAvailabilityResponse(available, error);
	}

	private void _CB_AdWillClose(string param)
	{
		_AdWillClose();
	}

	private static void _AdWillClose()
	{
		FuseLog("AdWillClose()");
		
		OnAdWillClose();
	}

	private void _CB_RewardedVideoCompleted(string param)
	{
		_RewardedVideoCompleted(param);
	}

	private static void _RewardedVideoCompleted(string adZone)
	{
		FuseLog("RewardedVideoCompleted(" + adZone + ")");
		OnRewardedVideoCompleted(adZone);
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

	private void _CB_NotificationAction(string param)
	{
		_NotificationAction(param);
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

	private void _CB_OverlayWillClose(string param)
	{
		_OverlayWillClose();
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

	private void _CB_AccountLoginComplete(string param)
	{
		int type;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out type))
			_AccountLoginComplete((AccountType) type, pars[1]);
		else
			Debug.LogError("FuseSDK: Parsing error in _AccountLoginComplete");
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

	private void _CB_TimeUpdated(string param)
	{
		long timestamp;
		if(long.TryParse(param, out timestamp))
			_TimeUpdated(timestamp);
		else
			Debug.LogError("FuseSDK: Parsing error in _TimeUpdated");
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

	[DllImport("__Internal")]
	private static extern string FuseAPI_GetFuseId();

	new public static string GetFuseId()
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer)
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

	private void _CB_FriendAdded(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			_FriendAdded(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}

	private void _CB_FriendRemoved(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			_FriendRemoved(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendRemoved");
	}

	private void _CB_FriendAccepted(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			_FriendAccepted(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAccepted");
	}

	private void _CB_FriendRejected(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			_FriendRejected(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendRejected");
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

	private void _CB_FriendsMigrated(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			_FriendsMigrated(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsMigrated");
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
			_FriendsListUpdated();
		}
	}

	private void _CB_FriendsListUpdated(string param)
	{
		var path = Application.persistentDataPath + "/" + param;
		if(System.IO.File.Exists(path))
		{
			_friendsList.Clear();

			var lines = System.IO.File.ReadAllLines(path);
			foreach(var line in lines)
			{
				var parts = line.Split(',');
				if(parts.Length == 4)
				{
					Friend friend = new Friend();
					friend.fuseId = parts[0];
					friend.accountId = parts[1];
					friend.alias = parts[2];
					friend.pending = parts[3] != "0";

					_friendsList.Add(friend);
				}
				else
				{
					Debug.LogError("FuseSDK: Error reading FriendsList data. Invalid line: " + line);
				}
			}
		}
		else
		{
			Debug.LogError("FuseSDK: Error reading FriendsList data. " + path + " is not a valid file.");
		}
		
		_FriendsListUpdated();
	}
	
	private static void _FriendsListUpdated()
	{
		FuseLog("FriendsListUpdatedEnd()");
		
		OnFriendsListUpdated(_friendsList);
	}

	private void _CB_FriendsListError(string param)
	{
		int error;

		if(int.TryParse(param, out error))
			_FriendsListError(error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsListError");
	}
	
	private static void _FriendsListError(int error)
	{
		FuseLog("FriendsListError(" + error + ")");
		
		OnFriendsListError(error);
	}
	
	private static List<Friend> _friendsList = new List<Friend>();
	
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
			
		return _friendsList;
	}
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

	new public static Dictionary<string, string> GetGameConfig()
	{
		FuseLog("GetGameConfig()");

		return _gameConfig;
	}

	private void _CB_GameConfigurationReceived(string param)
	{
		var path = Application.persistentDataPath + "/" + param;
		if(System.IO.File.Exists(path))
		{
			_gameConfig.Clear();

			var lines = System.IO.File.ReadAllLines(path);
			foreach(var line in lines)
			{
				var parts = line.Split(',');
				if(parts.Length == 2)
				{
					_gameConfig.Add(parts[0], parts[1]);
				}
				else
				{
					Debug.LogError("FuseSDK: Error reading GameConfiguration data. Invalid line: " + line);
				}
			}
		}
		else
		{
			Debug.LogError("FuseSDK: Error reading GameConfiguration data. " + path + " is not a valid file.");
		}

		_GameConfigurationReceived();
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