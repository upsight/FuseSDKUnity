#if UNITY_IPHONE && !UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using FuseMisc;

public partial class FuseSDK
{
	private static Dictionary<string, string> _gameConfig = new Dictionary<string, string>();
	private static List<Friend> _friendsList = new List<Friend>();

	#region Extern definitions
	[DllImport("__Internal")]
	private static extern void Native_StartSession(string gameId, bool registerForPush);
	[DllImport("__Internal")]
	private static extern void Native_RegisterPushToken(byte[] token, int size);
	[DllImport("__Internal")]
	private static extern void Native_SetUnityGameObject(string gameObjectName);

	[DllImport("__Internal")]
	private static extern bool Native_RegisterEventVariable(string name, string paramName, string paramValue, string variableName, double variableValue);
	[DllImport("__Internal")]
	private static extern bool Native_RegisterEventWithDictionary(string message, string paramName, string paramValue, string[] keys, double[] values, int numEntries);

	[DllImport("__Internal")]
	private static extern void Native_RegisterInAppPurchaseList(string[] productId, string[] priceLocale, float[] price, int numEntries);
	[DllImport("__Internal")]
	private static extern void Native_RegisterInAppPurchase(string productId, string transactionId, byte[] transactionReceiptBuffer, int transactionReceiptLength, int transactionState);
	[DllImport("__Internal")]
	private static extern void Native_RegisterUnibillPurchase(string productID, byte[] receipt, int receiptLength);

	[DllImport("__Internal")]
	private static extern bool Native_IsAdAvailableForZoneID(string zoneId);
	[DllImport("__Internal")]
	private static extern bool Native_ZoneHasRewarded(string zoneId);
	[DllImport("__Internal")]
	private static extern bool Native_ZoneHasIAPOffer(string zoneId);
	[DllImport("__Internal")]
	private static extern bool Native_ZoneHasVirtualGoodsOffer(string zoneId);
	[DllImport("__Internal")]
	private static extern string Native_GetRewardedInfoForZone(string zoneId);
	[DllImport("__Internal")]
	private static extern void Native_ShowAdForZoneID(string zoneId, string[] optionKeys, string[] optionValues, int numOptions);
	[DllImport("__Internal")]
	private static extern void Native_PreloadAdForZone(string zoneId);
	[DllImport("__Internal")]
	private static extern void Native_DisplayMoreGames();

	[DllImport("__Internal")]
	private static extern void Native_DisplayNotifications();
	[DllImport("__Internal")]
	private static extern bool Native_IsNotificationAvailable();

	[DllImport("__Internal")]
	private static extern void Native_RegisterGender(int gender);
	[DllImport("__Internal")]
	private static extern void Native_RegisterLevel(int level);
	[DllImport("__Internal")]
	private static extern void Native_RegisterCurrency(int type, int balance);
	[DllImport("__Internal")]
	private static extern void Native_RegisterAge(int age);
	[DllImport("__Internal")]
	private static extern void Native_RegisterBirthday(int year, int month, int day);

	[DllImport("__Internal")]
	private static extern string Native_GetFuseId();
	[DllImport("__Internal")]
	private static extern string Native_GetOriginalAccountAlias();
	[DllImport("__Internal")]
	private static extern string Native_GetOriginalAccountId();
	[DllImport("__Internal")]
	private static extern int Native_GetOriginalAccountType();
	[DllImport("__Internal")]
	private static extern void Native_GameCenterLogin();
	[DllImport("__Internal")]
	private static extern void Native_FacebookLogin(string facebookId, string name, string accessToken);
	[DllImport("__Internal")]
	private static extern void Native_TwitterLogin(string twitterId, string alias);
	[DllImport("__Internal")]
	private static extern void Native_FuseLogin(string fuseId, string alias);
	[DllImport("__Internal")]
	private static extern void Native_EmailLogin(string email, string alias);
	[DllImport("__Internal")]
	private static extern void Native_DeviceLogin(string alias);
	[DllImport("__Internal")]
	private static extern void Native_GooglePlayLogin(string alias, string token);

	[DllImport("__Internal")]
	private static extern int Native_GamesPlayed();
	[DllImport("__Internal")]
	private static extern string Native_LibraryVersion();
	[DllImport("__Internal")]
	private static extern bool Native_Connected();
	[DllImport("__Internal")]
	private static extern void Native_TimeFromServer();

	[DllImport("__Internal")]
	private static extern void Native_EnableData();
	[DllImport("__Internal")]
	private static extern void Native_DisableData();
	[DllImport("__Internal")]
	private static extern bool Native_DataEnabled();

	[DllImport("__Internal")]
	private static extern void Native_UpdateFriendsListFromServer();
	[DllImport("__Internal")]
	private static extern void Native_AddFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_RemoveFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_AcceptFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_RejectFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_MigrateFriends(string fuseId);

	[DllImport("__Internal")]
	private static extern void Native_UserPushNotification(string fuseId, string message);
	[DllImport("__Internal")]
	private static extern void Native_FriendsPushNotification(string message);

	[DllImport("__Internal")]
	private static extern string Native_GetGameConfigurationValue(string key);
	#endregion

	#region Initialization

	private void Awake()
	{
		GameObject go = GameObject.Find("FuseSDK");
		if(go != null && go != gameObject && go.GetComponent<FuseSDK>() != null)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

		if(iosStoreKit && !GetComponent<FuseSDK_Prime31StoreKit>())
			gameObject.AddComponent<FuseSDK_Prime31StoreKit>().logging = logging;

		if(iosUnibill && !GetComponent<FuseSDK_Unibill_iOS>())
			gameObject.AddComponent<FuseSDK_Unibill_iOS>().logging = logging;


		_gameId = iOSAppID;
		_debugOutput = logging;
		_registerForPush = registerForPushNotifications;

		Native_SetUnityGameObject(gameObject.name);
	}

	private void Start()
	{
		if(!string.IsNullOrEmpty(iOSAppID) && StartAutomatically)
		{
			_StartSession(_gameId, _registerForPush);
		}
	}
	#endregion

#region Session Creation

	public static void StartSession()
	{
		_StartSession(_gameId, _registerForPush);
	}

	private static void _StartSession(string gameId, bool registerForPush)
	{
		if(_sessionStarted)
		{
			Debug.LogWarning("FuseSDK: Duplicate StartSession call. Ignoring.");
			return;
		}

		if(string.IsNullOrEmpty(gameId))
		{
			Debug.LogError("FuseSDK: Null or empty App ID. Make sure your App ID is entered in the FuseSDK prefab");
			return;
		}

		_sessionStarted = true;
		FuseLog("StartSession(" + gameId + ")");
		Native_StartSession(gameId, registerForPush);
	}
#endregion

#region Analytics Event

	[Obsolete("Registering events is deprecated and will be removed from future releases.")]
	public static bool RegisterEvent(string name, string paramName, string paramValue, Hashtable variables)
	{
		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + ", [variables])");

		if(variables == null)
			return false;

		try
		{
			var varKeys = variables.Keys.Cast<string>().ToArray();
			var varValues = variables.Values.Cast<object>().Select(o => Convert.ToDouble(o)).ToArray();
			return Native_RegisterEventWithDictionary(name, paramName, paramValue, varKeys, varValues, varKeys.Length);
		}
		catch(Exception e)
		{
			Debug.LogError("FuseSDK: Error parsing hashtable in RegisterEvent. Operation failed.");
			Debug.LogException(e);
			return false;
		}
	}

	[Obsolete("Registering events is deprecated and will be removed from future releases.")]
	public static bool RegisterEvent(string name, string paramName, string paramValue, string variableName, double variableValue)
	{
		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");
		return Native_RegisterEventVariable(name, paramName, paramValue, variableName, variableValue);
	}
#endregion

#region In-App Purchase Logging

	public static void RegisterIOSInAppPurchaseList(Product[] products)
	{
		FuseLog("RegisterInAppPurchaseList(" + products.Length + ")");
		var ids = products.Select(p => p.ProductId).ToArray();
		var locales = products.Select(p => p.PriceLocale).ToArray();
		var prices = products.Select(p => p.Price).ToArray();

		Native_RegisterInAppPurchaseList(ids, locales, prices, ids.Length);
	}

	public static void RegisterIOSInAppPurchase(string productId, string transactionId, byte[] transactionReceipt, IAPState transactionState)
	{
		FuseLog("RegisterInAppPurchase(" + productId + "," + transactionReceipt.Length + "," + transactionState + ")");

		Native_RegisterInAppPurchase(productId, transactionId, transactionReceipt, transactionReceipt.Length, (int)transactionState);
	}

	public static void RegisterUnibillPurchase(string productID, byte[] receipt)
	{
		FuseLog("Registering Unibill transaction with product ID: " + productID);
		Native_RegisterUnibillPurchase(productID, receipt, receipt.Length);
	}

	public static void RegisterAndroidInAppPurchase(IAPState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
	}

	public static void RegisterAndroidInAppPurchase(IAPState purchaseState, string purchaseToken, string productId, string orderId, long purchaseTime, string developerPayload, double price, string currency)
	{
	}
#endregion

#region Ads

	public static bool IsAdAvailableForZoneID(string zoneId)
	{
		FuseLog("IsAdAvailableForZoneID");
		return Native_IsAdAvailableForZoneID(zoneId);
	}

	public static bool ZoneHasRewarded(string zoneId)
	{
		FuseLog("ZoneHasRewarded");
		return Native_ZoneHasRewarded(zoneId);
	}

	public static bool ZoneHasIAPOffer(string zoneId)
	{
		FuseLog("ZoneHasIAPOffer");
		return Native_ZoneHasIAPOffer(zoneId);
	}

	public static bool ZoneHasVirtualGoodsOffer(string zoneId)
	{
		FuseLog("ZoneHasVirtualGoodsOffer");
		return Native_ZoneHasVirtualGoodsOffer(zoneId);
	}

	public static RewardedInfo GetRewardedInfoForZone(string zoneId)
	{
		FuseLog("GetRewardedInfoForZone");
		var infoStr = Native_GetRewardedInfoForZone(zoneId);
		try
		{
			RewardedInfo rInfo;
			var pars = infoStr.Split(',');

			rInfo.PreRollMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[0]));
			rInfo.RewardMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[1]));
			rInfo.RewardItem = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[2]));
			rInfo.RewardAmount = int.Parse(pars[3]);
			return rInfo;
		}
		catch(Exception e)
		{
			Debug.LogError("FuseSDK: Error parsing RewardInfo. Ignoring callback.");
			Debug.LogException(e);
			return default(RewardedInfo);
		}
	}

	public static void ShowAdForZoneID(String zoneId, Dictionary<string, string> options = null)
	{
		FuseLog("ShowAdForZoneID");
		var keys = options == null ? new string[0] : options.Keys.ToArray();
		var values = options == null ? new string[0] : options.Values.ToArray();
		Native_ShowAdForZoneID(zoneId, keys, values, keys.Length);
	}

	public static void PreloadAdForZoneID(string zoneId)
	{
		FuseLog("PreloadAdForZoneID");
		Native_PreloadAdForZone(zoneId);
	}

	public static void DisplayMoreGames()
	{
		FuseLog("DisplayMoreGames");
		Native_DisplayMoreGames();
	}

#endregion

#region Notifications

	public static void DisplayNotifications()
	{
		FuseLog("DisplayNotifications()");
		Native_DisplayNotifications();
	}

	public static bool IsNotificationAvailable()
	{
		FuseLog("IsNotificationAvailable()");
		return Native_IsNotificationAvailable();
	}
#endregion

#region User Info

	public static void RegisterGender(Gender gender)
	{
		FuseLog("RegisterGender()");
		Native_RegisterGender((int)gender);
	}

	public static void RegisterAge(int age)
	{
		FuseLog("RegisterAge()");
		Native_RegisterAge(age);
	}

	public static void RegisterBirthday(int year, int month, int day)
	{
		FuseLog("RegisterBirthday()");
		Native_RegisterBirthday(year, month, day);
	}

	public static void RegisterLevel(int level)
	{
		FuseLog("RegisterLevel()");
		Native_RegisterLevel(level);
	}

	public static void RegisterCurrency(int currencyType, int balance)
	{
		FuseLog("RegisterCurrency()");
		Native_RegisterCurrency(currencyType, balance);
	}
#endregion

#region Account Login

	public static string GetFuseId()
	{
		FuseLog("GetFuseId()");
		return Native_GetFuseId();
	}

	public static string GetOriginalAccountAlias()
	{
		FuseLog("GetOriginalAccountAlias()");
		return Native_GetOriginalAccountAlias();
	}

	public static string GetOriginalAccountId()
	{
		FuseLog("GetOriginalAccountId()");
		return Native_GetOriginalAccountId();
	}

	public static AccountType GetOriginalAccountType()
	{
		FuseLog("GetOriginalAccountType()");
		return (AccountType)Native_GetOriginalAccountType();
	}

	public static void GameCenterLogin()
	{
		FuseLog("GameCenterLogin()");
		Native_GameCenterLogin();
	}

	public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		FuseLog("FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		Native_FacebookLogin(facebookId, name, accessToken);
	}

	public static void TwitterLogin(string twitterId, string alias)
	{
		FuseLog("TwitterLogin(" + twitterId + ")");
		Native_TwitterLogin(twitterId, alias);
	}

	public static void FuseLogin(string fuseId, string alias)
	{
		FuseLog(" FuseLogin(" + fuseId + "," + alias + ")");
		Native_FuseLogin(fuseId, alias);
	}

	public static void EmailLogin(string email, string alias)
	{
		FuseLog(" EmailLogin(" + email + "," + alias + ")");
		Native_EmailLogin(email, alias);
	}

	public static void DeviceLogin(string alias)
	{
		FuseLog("DeviceLogin(" + alias + ")");
		Native_DeviceLogin(alias);
	}

	public static void GooglePlayLogin(string alias, string token)
	{
		FuseLog("GooglePlayLogin(" + alias + "," + token + ")");
		Native_GooglePlayLogin(alias, token);
	}
#endregion

#region Miscellaneous

	public static int GamesPlayed()
	{
		FuseLog("GamesPlayed()");
		return Native_GamesPlayed();
	}


	public static string LibraryVersion()
	{
		FuseLog("LibraryVersion()");
		return Native_LibraryVersion();
	}

	public static bool Connected()
	{
		FuseLog("Connected()");
		return Native_Connected();
	}


	public static void UTCTimeFromServer()
	{
		FuseLog("TimeFromServer()");
		Native_TimeFromServer();
	}

	public static void FuseLog(string str)
	{
		if(_debugOutput)
		{
			Debug.Log("FuseSDK: " + str);
		}
	}
#endregion

#region Data Opt In/Out

	public static void EnableData()
	{
		FuseLog("EnableData()");
		Native_EnableData();
	}

	public static void DisableData()
	{
		FuseLog("DisableData()");
		Native_DisableData();
	}

	public static bool DataEnabled()
	{
		FuseLog("DataEnabled()");
		return Native_DataEnabled();
	}
#endregion

#region Friend List

	public static void UpdateFriendsListFromServer()
	{
		FuseLog("UpdateFriendsListFromServer()");
		Native_UpdateFriendsListFromServer();
	}

	public static List<Friend> GetFriendsList()
	{
		FuseLog("GetFriendsList()");
		return _friendsList;
	}

	public static void AddFriend(string fuseId)
	{
		FuseLog("AddFriend(" + fuseId + ")");
		Native_AddFriend(fuseId);
	}

	public static void RemoveFriend(string fuseId)
	{
		FuseLog("RemoveFriend(" + fuseId + ")");
		Native_RemoveFriend(fuseId);
	}

	public static void AcceptFriend(string fuseId)
	{
		FuseLog("AcceptFriend(" + fuseId + ")");
		Native_AcceptFriend(fuseId);
	}

	public static void RejectFriend(string fuseId)
	{
		FuseLog("RejectFriend(" + fuseId + ")");
		Native_RejectFriend(fuseId);
	}

	public static void MigrateFriends(string fuseId)
	{
		FuseLog("MigrateFriends(" + fuseId + ")");
		Native_MigrateFriends(fuseId);
	}
#endregion

#region User-to-User Push Notifications

	public static void UserPushNotification(string fuseId, string message)
	{
		FuseLog("UserPushNotification(" + fuseId + "," + message + ")");
		Native_UserPushNotification(fuseId, message);
	}

	public static void FriendsPushNotification(string message)
	{
		FuseLog("FriendsPushNotification(" + message + ")");
		Native_FriendsPushNotification(message);
	}
#endregion

#region Game Configuration Data

	public static string GetGameConfigurationValue(string key)
	{
		FuseLog("GetGameConfigurationValue(" + key + ")");
		return Native_GetGameConfigurationValue(key);
	}

	public static Dictionary<string, string> GetGameConfiguration()
	{
		FuseLog("GetGameConfig()");
		return _gameConfig;
	}

#endregion


#region Callbacks
	private void _CB_SessionStartReceived(string param)
	{
		FuseLog("SessionStartReceived()");
		OnSessionStartReceived();
	}

	private void _CB_SessionLoginError(string param)
	{
		FuseLog("SessionLoginError(" + param + ")");
		int error;
		if(int.TryParse(param, out error))
			OnSessionLoginError(error);
		else
			Debug.LogError("FuseSDK: Parsing error in _SessionLoginError");
	}

	private void _CB_PurchaseVerification(string param)
	{
		FuseLog("PurchaseVerification(" + param + ")");
		int verified;

		var pars = param.Split(',');
		if(pars.Length == 3 && int.TryParse(pars[0], out verified))
			OnPurchaseVerification(verified, pars[1], pars[2]);
		else
			Debug.LogError("FuseSDK: Parsing error in _PurchaseVerification");
	}

	private void _CB_AdAvailabilityResponse(string param)
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

	private void _CB_AdWillClose(string param)
	{
		FuseLog("AdWillClose()");
		OnAdWillClose();
	}

	private void _CB_RewardedAdCompleted(string param)
	{
		FuseLog("RewardedAdCompleted(" + param + ")");

		try
		{
			RewardedInfo rInfo;
			var pars = param.Split(',');

			rInfo.PreRollMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[0]));
			rInfo.RewardMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[1]));
			rInfo.RewardItem = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[2]));
			rInfo.RewardAmount = int.Parse(pars[3]);
			OnRewardedAdCompleted(rInfo);
		}
		catch(Exception e)
		{
			Debug.LogError("FuseSDK: Error parsing RewardInfo. Ignoring callback.");
			Debug.LogException(e);
			return;
		}
	}

	private void _CB_IAPOfferAccepted(string param)
	{
		FuseLog("IAPOfferAccepted(" + param + ")");

		try
		{
			IAPOfferInfo oInfo;
			var pars = param.Split(',');

			oInfo.ProductId = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[0]));
			oInfo.ProductPrice = float.Parse(pars[1]);
			oInfo.ItemName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[2]));
			oInfo.ItemAmount = int.Parse(pars[3]);
			OnIAPOfferAccepted(oInfo);
		}
		catch(Exception e)
		{
			Debug.LogError("FuseSDK: Error parsing IAPOfferInfo. Ignoring callback.");
			Debug.LogException(e);
			return;
		}
	}

	private void _CB_VirtualGoodsOfferAccepted(string param)
	{
		FuseLog("VirtualGoodsOfferAccepted(" + param + ")");

		try
		{
			VGOfferInfo oInfo;
			var pars = param.Split(',');

			oInfo.PurchaseCurrency = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[0]));
			oInfo.PurchasePrice = float.Parse(pars[1]);
			oInfo.ItemName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(pars[2]));
			oInfo.ItemAmount = int.Parse(pars[3]);
			OnVirtualGoodsOfferAccepted(oInfo);
		}
		catch(Exception e)
		{
			Debug.LogError("FuseSDK: Error parsing VGOfferInfo. Ignoring callback.");
			Debug.LogException(e);
			return;
		}
	}

	private void _CB_NotificationAction(string param)
	{
		FuseLog("NotificationAction(" + param + ")");
		OnNotificationAction(param);
	}

	private void _CB_NotificationWillClose(string param)
	{
		FuseLog("NotificationAction()");
		OnNotificationWillClose();
	}

	private void _CB_AccountLoginComplete(string param)
	{
		FuseLog("AccountLoginComplete(" + param + ")");
		int type;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out type))
			OnAccountLoginComplete(type, pars[1]);
		else
			Debug.LogError("FuseSDK: Parsing error in _AccountLoginComplete");
	}

	private void _CB_AccountLoginError(string param)
	{
		FuseLog("_AccountLoginError(" + param + ")");

		var pars = param.Split(',');
		if(pars.Length == 2)
			OnAccountLoginError(pars[0], pars[1]);
		else
			Debug.LogError("FuseSDK: Parsing error in _AdAvailabilityResponse");
	}

	private void _CB_TimeUpdated(string param)
	{
		FuseLog("TimeUpdated(" + param + ")");

		long timestamp;
		if(long.TryParse(param, out timestamp))
			OnTimeUpdated(timestamp.ToDateTime());
		else
			Debug.LogError("FuseSDK: Parsing error in _TimeUpdated");
	}

	private void _CB_FriendAdded(string param)
	{
		FuseLog("_FriendAdded(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAdded(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}

	private void _CB_FriendRemoved(string param)
	{
		FuseLog("_FriendRemoved(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRemoved(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendRemoved");
	}

	private void _CB_FriendAccepted(string param)
	{
		FuseLog("_FriendAccepted(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAccepted(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAccepted");
	}

	private void _CB_FriendRejected(string param)
	{
		FuseLog("_FriendRejected(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRejected(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendRejected");
	}

	private void _CB_FriendsMigrated(string param)
	{
		FuseLog("_FriendsMigrated(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendsMigrated(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsMigrated");
	}

	private void _CB_FriendsListUpdated(string param)
	{
		FuseLog("FriendsListUpdated()");

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
					friend.FuseId = parts[0];
					friend.AccountId = parts[1];
					friend.Alias = parts[2];
					friend.Pending = parts[3] != "0";

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

		OnFriendsListUpdated(_friendsList);
	}

	private void _CB_FriendsListError(string param)
	{
		FuseLog("FriendsListError(" + param + ")");

		int error;

		if(int.TryParse(param, out error))
			OnFriendsListError(error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsListError");
	}

	private void _CB_GameConfigurationReceived(string param)
	{
		FuseLog("GameConfigurationReceived()");

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

		OnGameConfigurationReceived();
	}
#endregion
}
#endif