


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FuseAPI_Stub : FuseAPI
{
	public bool logging = false;
	public bool persistent = true;
	public bool registerForPushNotifications = true;	
#if !UNITY_ANDROID && !UNITY_IPHONE && !UNITY_EDITOR

	public static bool debugOutput = false;

	#region Session Creation

	public void Awake()
	{
		if(logging)
		{
			debugOutput = true;
		}
	}

	new public static void StartSession(string gameId)
	{
	}

	#endregion
	
	#region Analytics Event
	
	new public static void RegisterEvent(string message)
	{
	}
	
	new public static void RegisterEvent(string message, Hashtable values)
	{
	}
	
	new public static int RegisterEvent(string name, string paramName, string paramValue, Hashtable variables)
	{
		return 0;
	}
	
	new public static int RegisterEvent(string name, string paramName, string paramValue, string variableName, double variableValue)
	{
		return 0;
	}
	
	#endregion
	
	#region In-App Purchase Logging

	
	new public static void RegisterInAppPurchaseList(Product[] products)
	{
	}
	

	new public static void RegisterInAppPurchase(string productId, string transactionId, byte[] transactionReceipt, TransactionState transactionState)
	{

	}

	#endregion
	
	#region Fuse Interstitial Ads
	
	new public static void CheckAdAvailable()
	{
	}
	
	new public static void ShowAd()
	{
	}

	#endregion
	
	#region Notifications
	
	new public static void FuseAPI_RegisterForPushNotifications()
	{
	}
	
	new public static void DisplayNotifications()
	{
	}
	
	new public static bool IsNotificationAvailable()
	{
		return false;
	}

	#endregion
	
	#region More Games
	
	new public static void DisplayMoreGames()
	{

	}

	#endregion
	
	#region Gender
	
	new public static void RegisterGender(Gender gender)
	{
	}
	#endregion
	
	#region Account Login
	
	new public static void GameCenterLogin()
	{
	}
	
	new public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
	}
	
	new public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
	}
	
	new public static void TwitterLogin(string twitterId)
	{
	}
	
	new public static void DeviceLogin(string alias)
	{
	}
	
	new public static void OpenFeintLogin(string openFeintId)
	{
	}
	
	new public static void FuseLogin(string fuseId, string alias)
	{
	}
	
	new public static void GooglePlayLogin(string alias, string token)
	{
	}
	
	new public static string GetOriginalAccountAlias()
	{
		return "Fuse_Account_Alias";
	}
	
	new public static string GetOriginalAccountId()
	{
		return "Fuse_Account_ID";
	}
	
	new public static AccountType GetOriginalAccountType()
	{
		return 0;
	}
	
	#endregion
	
	#region Miscellaneous
	
	new public static int GamesPlayed()
	{
		return 0;
	}
	
	new public static string LibraryVersion()
	{

		return "0.00";
	}
	
	new public static bool Connected()
	{
		return false;
	}
	
	new public static void TimeFromServer()
	{

	}
	
	new public static bool NotReadyToTerminate()
	{
		return false;
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

	}
	
	new public static bool DataEnabled()
	{
		return false;
	}
	#endregion
	
	#region User Game Data
	
	new public static int SetGameData(Hashtable data)
	{
		return 0;
	}
	
	new public static int SetGameData(string key, Hashtable data)
	{
		return 0;
	}
	
	new public static int SetGameData(string key, Hashtable data, bool isCollection)
	{
		return 0;
	}
	
	new public static int SetGameData(string key, Hashtable data, bool isCollection, string fuseId)
	{
		return 0;
	}
	
	new public static int GetGameData(string[] keys)
	{
		return 0;
	}
	
	new public static int GetGameData(string key, string[] keys)
	{
		return 0;
	}
	
	new public static int GetFriendGameData(string fuseId, string[] keys)
	{
		return 0;
	}
	
	new public static int GetFriendGameData(string fuseId, string key, string[] keys)
	{
		return 0;
	}
	new public static string GetFuseId()
	{
		return "00000000";
	}
	
	#endregion
	
	#region Friend List

	
	new public static void AddFriend(string fuseId)
	{
	}
	new public static void RemoveFriend(string fuseId)
	{
	}
	new public static void AcceptFriend(string fuseId)
	{
	}
	new public static void RejectFriend(string fuseId)
	{
	}	

	
	new public static void MigrateFriends(string fuseId)
	{
	}
	
	new public static void UpdateFriendsListFromServer()
	{
	}

	
	public static readonly List<Friend>  emptyFriendList;

	new public static List<Friend> GetFriendsList()
	{
		return emptyFriendList;
	}
	#endregion
	
	#region Chat List
	#endregion
	
	#region User-to-User Push Notifications
	new public static void UserPushNotification(string fuseId, string message)
	{
	}
	
	new public static void FriendsPushNotification(string message)
	{

	}
	#endregion
	
	#region Gifting
	
	new public static void GetMailListFromServer()
	{

	}
	
	new public static void GetMailListFriendFromServer(string fuseId)
	{

	}
	
	public static readonly List<Mail>  emptyMailList;
	
	new public static List<Mail> GetMailList(string fuseId)
	{
		return emptyMailList;
	}
	
	new public static void SetMailAsReceived(int messageId)
	{
	}
	
	new public static int SendMailWithGift(string fuseId, string message, int giftId, int giftAmount)
	{
		return 0;
	}
	
	new public static int SendMail(string fuseId, string message)
	{
		return 0;
	}
	
	#endregion
	
	#region Game Configuration Data
	
	new public static string GetGameConfigurationValue(string key)
	{
		return "";
	}		
	public static readonly Dictionary<string,string>  emptyGameConfig;

	new public static Dictionary<string, string> GetGameConfig()
	{
		return emptyGameConfig;
	}
	
	#endregion
	
	#region Specific Event Registration
	new public static void RegisterLevel(int level)
	{
	}
	
	new public static void RegisterCurrency(int type, int balance)
	{
	}
	
	new public static void RegisterFlurryView()
	{
	}
	
	new public static void RegisterFlurryClick()
	{
	}
	
	new public static void RegisterTapjoyReward(int amount)
	{
	}

	new public static void RegisterAge(int age)
	{

	}
	
	new public static void RegisterBirthday(int year, int month, int day)
	{

	}
	#endregion
	
	#region Internal Event Triggers
	new static protected void OnSessionStartReceived()
	{
	}
	
	new static protected void OnSessionLoginError(int error)
	{
	}
	
	new static protected void OnPurchaseVerification(bool verified, string transactionId, string originalTransactionId)
	{
	}
	
	new static protected void OnAdAvailabilityResponse(int available, int error)
	{

	}
	
	new static protected void OnAdWillClose()
	{
	}
	
	new static protected void OnAdDisplayed()
	{
	}
	
	new static protected void OnAdClicked()
	{
	}
	
	new static protected void OnNotificationAction(string action)
	{
	}
	
	new static protected void OnOverlayWillClose()
	{
	}
	
	new static protected void OnAccountLoginComplete(AccountType type, string accountId)
	{
	}
	
	new static protected void OnTimeUpdated(DateTime time)
	{
	}
	
	new static protected void OnGameDataError(int error, int requestId)
	{
	}
	
	new static protected void OnGameDataSetAcknowledged(int requestId)
	{
	}
	
	new static protected void OnGameDataReceived(string fuseId, string dataKey, Hashtable data, int requestId)
	{
	}
	
	new static protected void OnFriendAdded(string fuseId, int error)
	{
	}
	
	new static protected void OnFriendRemoved(string fuseId, int error)
	{
	}
	
	new static protected void OnFriendAccepted(string fuseId, int error)
	{
	}
	
	new static protected void OnFriendRejected(string fuseId, int error)
	{
	}
	
	new static protected void OnFriendsMigrated(string fuseId, int error)
	{
	}
	
	new static protected void OnFriendsListUpdated(List<Friend> friends)
	{
	}
	
	new static protected void OnFriendsListError(int error)
	{
	}
	
	new static protected void OnMailListReceived(List<Mail> mailList, string mailFuseId)
	{
	}
	
	new static protected void OnMailListError(int error)
	{
	}
	
	new static protected void OnMailAcknowledged(int messageId, string fuseId, int requestID)
	{
	}
	
	new static protected void OnMailError(int error)
	{
	}
	
	new static protected void OnGameConfigurationReceived()
	{
	}
	#endregion
#endif//!UNITY_ANDROID && !UNITY_IPHONE && !UNITY_EDITOR
};