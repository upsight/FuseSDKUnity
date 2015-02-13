#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseAPI_UnityEditor : FuseAPI
{
	public static bool debugOutput = false;

#region Session Creation
	protected override void Init()
	{
		if (logging)
		{
			FuseAPI_UnityEditor.debugOutput = true;
		}
	}

	void Start()
	{
		_StartSession(iOSGameID);
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
		OnSessionStartReceived();
	}

#if UNITY_ANDROID
	new public static void SetupPushNotifications(string gcmProjectID)
	{
		FuseLog("SetupPushNotifications(" + gcmProjectID + ")");
	}
#endif

#endregion

#region Analytics Event

	new public static void RegisterEvent(string message)
	{
		FuseLog("RegisterEvent(" + message + ")");
	}

	new public static void RegisterEvent(string message, Hashtable values)
	{
		FuseLog("RegisterEvent(" + message + ", [variables])");
	}

	new public static int RegisterEvent(string name, string paramName, string paramValue, Hashtable variables)
	{
		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + ", [variables])");

		foreach (DictionaryEntry entry in variables)
		{
			string entryKey = entry.Key as string;
			try
			{
				//double entryValue =
				Convert.ToDouble(entry.Value);
			}
			catch
			{
				string entryString = (entry.Value == null) ? "" : entry.Value.ToString();
				Debug.LogWarning("Key/value pairs in FuseAPI::RegisterEvent must be String/Number");
				Debug.LogWarning("For Key: " + entryKey + " and Value: " + entryString);
			}
		}

		return -1;
	}

	new public static int RegisterEvent(string name, string paramName, string paramValue, string variableName, double variableValue)
	{
		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");

		return -1;
	}
#endregion

#region In-App Purchase Logging

	new public static void RegisterInAppPurchaseList(Product[] products)
	{
		FuseLog("RegisterInAppPurchaseList(" + products.Length + ")");
	}

#if UNITY_IPHONE
	new public enum TransactionState { PURCHASING, PURCHASED, FAILED, RESTORED }

	public static void RegisterInAppPurchase(string productId, string transactionId, byte[] transactionReceipt, TransactionState transactionState)
	{
		FuseLog("RegisterInAppPurchase(" + productId + "," + transactionReceipt.Length + "," + transactionState + ")");

		_PurchaseVerification(1, "", "");
	}
#elif UNITY_ANDROID
	// Android purchase notification
	new public static void RegisterInAppPurchase(PurchaseState purchaseState, string notifyId, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
		FuseLog("RegisterInAppPurchase");
	}
	new public static void RegisterInAppPurchase(PurchaseState purchaseState, string notifyId, string productId, string orderId, long purchaseTime, string developerPayload, double price, string currency)
	{
		FuseLog("RegisterInAppPurchase");
	}
#endif

	new public static void RegisterUnibillPurchase(string productID, byte[] receipt)
	{
		// do nothing.  This method is for iOS only at this point.  Unibill Android can be handled with current methods
	}

	private static void _PurchaseVerification(int verified, string transactionId, string originalTransactionId)
	{
		FuseLog("PurchaseVerification(" + verified + "," + transactionId + "," + originalTransactionId + ")");

		OnPurchaseVerification(verified, transactionId, originalTransactionId);
	}

#endregion

#region Fuse Ads

	new public static void PreLoadAd(string adZone)
	{
		Debug.Log("In editor");
	}

	new public static void CheckAdAvailable(string adZone)
	{
		FuseLog("CheckAdAvailable()");

		_AdAvailabilityResponse(0,0);
	}

	new public static void ShowAd(string adZone)
	{
		FuseLog("ShowAd()");

		_AdWillClose();
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
	
	private void _RewardedVideoCompleted(string adZone)
	{
		FuseLog("RewardedVideoCompleted(" + adZone + ")");
		OnRewardedVideoCompleted(adZone);
	}

#endregion

#region Notifications

	new public static void FuseAPI_RegisterForPushNotifications()
	{
		FuseLog("RegisterForNotifications()");
	}

	new public static void DisplayNotifications()
	{
		FuseLog("DisplayNotifications()");
	}

    new public static bool IsNotificationAvailable()
    {
        return false;
    }

	private static void _NotificationAction(string action)
	{
		FuseLog("NotificationAction(" + action + ")");

		OnNotificationAction(action);
	}

#endregion

#region More Games

	new public static void DisplayMoreGames()
	{
		FuseLog("DisplayMoreGames()");
		_OverlayWillClose();
	}

	private static void _OverlayWillClose()
	{
		FuseLog("OverlayWillClose()");
		OnOverlayWillClose();
	}

    #endregion

#region Gender

	new public static void RegisterGender(Gender gender)
	{
		FuseLog("RegisterGender(" + gender + ")");
	}
#endregion

#region Account Login

	new public static void GameCenterLogin()
	{
		FuseLog("GameCenterLogin()");

		_AccountLoginComplete(AccountType.GAMECENTER, "");
	}

	new public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		FuseLog("FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");

		_AccountLoginComplete(AccountType.FACEBOOK, facebookId);
	}

	new public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
		FuseLog("FacebookLogin(" + facebookId + "," + name + "," + gender + "," + accessToken + ")");

		_AccountLoginComplete(AccountType.FACEBOOK, facebookId);
	}

	new public static void TwitterLogin(string twitterId)
	{
		FuseLog("TwitterLogin(" + twitterId + ")");

		_AccountLoginComplete(AccountType.TWITTER, twitterId);
	}

	new public static void DeviceLogin(string alias)
	{
		FuseLog("DeviceLogin(" + alias + ")");

		_AccountLoginComplete(AccountType.DEVICE_ID, alias);
	}

	new public static void FuseLogin(string fuseId, string alias)
	{
		FuseLog("FuseLogin(" + fuseId + "," + alias + ")");

		_AccountLoginComplete(AccountType.USER, fuseId);
	}

	new public static void GooglePlayLogin(string alias, string token)
	{
		FuseLog("GooglePlayLogin(" + alias + "," + token + ")");

		_AccountLoginComplete(AccountType.GOOGLE_PLAY, alias);
	}

	new public static string GetOriginalAccountAlias()
	{
		FuseLog("GetOriginalAccountAlias()");

		return "";
	}

	new public static string GetOriginalAccountId()
	{
		FuseLog("GetOriginalAccountId()");
		return "";
	}

	new public static AccountType GetOriginalAccountType()
	{
		FuseLog("GetOriginalAccountType()");

		return AccountType.NONE;
	}

	private static void _AccountLoginComplete(AccountType type, string accountId)
	{
		FuseLog("AccountLoginComplete(" + type + "," + accountId + ")");

		OnAccountLoginComplete(type, accountId);
	}

    #endregion

#region Miscellaneous

	new public static int GamesPlayed()
	{
		FuseLog("GamesPlayed()");
		return 0;
	}

	new public static string LibraryVersion()
	{
		FuseLog("LibraryVersion()");

		return System.IO.File.ReadAllText(Application.dataPath + "/FuseAPI/version");
	}

	new public static bool Connected()
	{
		FuseLog("Connected()");
		
		return true;
	}

	new public static void TimeFromServer()
	{
		FuseLog("TimeFromServer()");

		_TimeUpdated((DateTime.UtcNow - unixEpoch).Ticks / TimeSpan.TicksPerSecond);
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

	new public static bool NotReadyToTerminate()
	{
		FuseLog("NotReadyToTerminate()");
		return false;
	}

	new public static void FuseLog(string str)
	{
		if(debugOutput)
		{
			Debug.Log(" " + str);
		}
	}

	new public static string GetFuseId()
	{
		FuseLog("GetFuseId()");

		return "";
	}

    #endregion

#region Data Opt In/Out

	new public static void EnableData(bool enable)
	{
		FuseLog("EnableData(" + enable + ")");
	}


	new public static bool DataEnabled()
	{
		FuseLog("DataEnabled()");
		return false;
	}
    #endregion

#region Friend List
	new public static void AddFriend(string fuseId)
	{
		OnFriendAdded(fuseId, (int)FuseAPI.FriendErrors.FUSE_FRIEND_NO_ERROR);
	}
	new public static void RemoveFriend(string fuseId)
	{
		OnFriendRemoved(fuseId, (int)FuseAPI.FriendErrors.FUSE_FRIEND_NO_ERROR);
	}
	new public static void AcceptFriend(string fuseId)
	{
		OnFriendAccepted(fuseId, (int)FuseAPI.FriendErrors.FUSE_FRIEND_NO_ERROR);
	}
	new public static void RejectFriend(string fuseId)
	{
		OnFriendRejected(fuseId, (int)FuseAPI.FriendErrors.FUSE_FRIEND_NO_ERROR);
	}
	
	new public static void MigrateFriends(string fuseId)
	{
		OnFriendsMigrated(fuseId, (int)FuseAPI.MigrateFriendErrors.FUSE_MIGRATE_FRIENDS_NOT_CONNECTED);
	}

	new public static void UpdateFriendsListFromServer()
	{
		FuseLog("UpdateFriendsListFromServer()");

		_FriendsListUpdated();
	}

	private static void _FriendsListUpdated()
	{
		FuseLog("FriendsListUpdatedEnd()");

		OnFriendsListUpdated(new List<Friend>());
	}

	private static void _FriendsListError(int error)
	{
		FuseLog("FriendsListError(" + error + ")");

		OnFriendsListError(error);
	}

	new public static List<Friend> GetFriendsList()
	{
		FuseLog("GetFriendsList()");

		return new List<Friend>();
	}
    #endregion

#region User-to-User Push Notifications

	new public static void UserPushNotification(string fuseId, string message)
	{
		FuseLog("UserPushNotification(" + fuseId +"," + message + ")");
	}

	new public static void FriendsPushNotification(string message)
	{
		FuseLog("FriendsPushNotification(" + message + ")");
	}
    #endregion

#region Game Configuration Data

	new public static string GetGameConfigurationValue(string key)
	{
		FuseLog("GetGameConfigurationValue(" + key + ")");

		return "";
	}

	public static readonly Dictionary<string, string> emptyGameConfig = new Dictionary<string,string>();
	new public static Dictionary<string, string> GetGameConfig()
	{
		FuseLog("GetGameConfig()");

		return emptyGameConfig;
	}
    #endregion

#region Specific Event Registration

	new public static void RegisterLevel(int level)
	{
		FuseLog("RegisterLevel(" + level + ")");
	}

	new public static void RegisterCurrency(int type, int balance)
	{
		FuseLog("RegisterCurrency(" + type + "," + balance + ")");
	}

	new public static void RegisterAge(int age)
	{
		FuseLog("RegisterAge(" + age + ")");
	}
	
	new public static void RegisterBirthday(int year, int month, int day)
	{
		FuseLog("RegisterBirthday(" + year + ", " + month + ", " + day + ")");
	}
#endregion

}
#endif
