using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_IPHONE && !UNITY_EDITOR
public class FuseAPI_iOS : FuseAPI
{
	#region Session Creation
	[DllImport("__Internal")]
	private static extern void FuseAPI_StartSession(string gameId);
	
	public static void StartSession(string gameId)
	{
//		Debug.Log("FuseAPI:StartSession(" + gameId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_StartSession(gameId);
		}
		else
		{
			_SessionStartReceived();
		}
	}
	
	private static void _SessionStartReceived()
	{
//		Debug.Log("FuseAPI:SessionStartReceived()");
		
		OnSessionStartReceived();
	}
	
	private static void _SessionLoginError(int error)
	{
//		Debug.Log("FuseAPI:SessionLoginError(" + error + ")");
		
		OnSessionLoginError(error);
	}
	
	#endregion
	
	#region Analytics Event
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterEvent(string message);
	
	public static void RegisterEvent(string message)
	{
//		Debug.Log("FuseAPI:RegisterEvent(" + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterEvent(message);
		}
	}
	#endregion
	
	#region In-App Purchase Logging
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterInAppPurchaseListStart();
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterInAppPurchaseListProduct(string productId, string priceLocale, float price);
	[DllImport("__Internal")]
	private static extern int FuseAPI_RegisterInAppPurchaseListEnd();
	
	public static void RegisterInAppPurchaseList(Product[] products)
	{
//		Debug.Log ("FuseAPI:RegisterInAppPurchaseList(" + products.Length + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterInAppPurchaseListStart();
			
			foreach (Product product in products)
			{
				FuseAPI_RegisterInAppPurchaseListProduct(product.productId, product.priceLocale, product.price);
			}
			
			FuseAPI_RegisterInAppPurchaseListEnd();
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterInAppPurchase(string productId, byte[] transactionReceiptBuffer, int transactionReceiptLength, int transactionState);
	
	public enum TransactionState { PURCHASING, PURCHASED, FAILED, RESTORED }
	
	public static void RegisterInAppPurchase(string productId, byte[] transactionReceipt, TransactionState transactionState)
	{
//		Debug.Log("FuseAPI:RegisterInAppPurchase(" + productId + "," + transactionReceipt.Length + "," + transactionState + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterInAppPurchase(productId, transactionReceipt, transactionReceipt.Length, (int)transactionState);
		}
		else
		{
			_PurchaseVerification(true, "", "");
		}
	}
	
	private static void _PurchaseVerification(bool verified, string transactionId, string originalTransactionId)
	{
//		Debug.Log("FuseAPI:PurchaseVerification(" + verified + "," + transactionId + "," + originalTransactionId + ")");
		
		OnPurchaseVerification(verified, transactionId, originalTransactionId);
	}
	
	#endregion
	
	#region Fuse Interstitial Ads
	[DllImport("__Internal")]
	private static extern void FuseAPI_ShowAd();
	
	public static void ShowAd()
	{
//		Debug.Log("FuseAPI:ShowAd()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_ShowAd();
		}
		else
		{
			_AdWillClose();
		}
	}
	
	private static void _AdWillClose()
	{
//		Debug.Log("FuseAPI:AdWillClose()");
		
		OnAdWillClose();
	}
	
	#endregion

	#region Notifications
	[DllImport("__Internal")]
	private static extern void FuseAPI_DisplayNotifications();
	
	public static void DisplayNotifications()
	{
//		Debug.Log("FuseAPI:DisplayNotifications()");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_DisplayNotifications();
		}
	}
	
	private static void _NotificationAction(string action)
	{
//		Debug.Log("FuseAPI:NotificationAction(" + action + ")");
		
		OnNotificationAction(action);
	}
	
	#endregion

	#region More Games
	[DllImport("__Internal")]
	private static extern void FuseAPI_DisplayMoreGames();
	
	public static void DisplayMoreGames()
	{
//		Debug.Log("FuseAPI:DisplayMoreGames()");
		
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
//		Debug.Log("FuseAPI:OverlayWillClose()");
		OnOverlayWillClose();
	}
	
	#endregion
	
	#region Gender
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterGender(int gender);
	
	public static void RegisterGender(Gender gender)
	{
//		Debug.Log("FuseAPI:RegisterGender(" + gender + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_RegisterGender((int)gender);
		}
	}
	#endregion
	
	#region Account Login
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_GameCenterLogin();
	
	public static void GameCenterLogin()
	{
//		Debug.Log ("FuseAPI:GameCenterLogin()");
		
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
	
	public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
//		Debug.Log ("FuseAPI:FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		
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
	
	public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
//		Debug.Log ("FuseAPI:FacebookLogin(" + facebookId + "," + name " "," + gender + "," + accessToken + ")");
		
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
	
	public static void TwitterLogin(string twitterId)
	{
//		Debug.Log ("FuseAPI:TwitterLogin(" + twitterId + ")");
		
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
	private static extern void FuseAPI_OpenFeintLogin(string openFeintId);
	
	public static void OpenFeintLogin(string openFeintId)
	{
//		Debug.Log ("FuseAPI:OpenFeintLogin(" + openFeintId + ")");
		
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
	
	public static void FuseLogin(string fuseId, string alias)
	{
//		Debug.Log ("FuseAPI:FuseLogin(" + fuseId + "," + alias + ")");
		
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
	private static extern string FuseAPI_GetOriginalAccountId();
	
	public static string GetOriginalAccountId()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string accountId = FuseAPI_GetOriginalAccountId();
			
//			Debug.Log("FuseAPI:GetOriginalAccountId()==" + accountId);
			
			return accountId;
		}
		else
		{
//			Debug.Log("FuseAPI:GetOriginalAccountId()");
			
			return "";
		}
	}
	
	[DllImport("__Internal")]
	private static extern AccountType FuseAPI_GetOriginalAccountType();
	
	public static AccountType GetOriginalAccountType()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			AccountType accountType = FuseAPI_GetOriginalAccountType();
			
//			Debug.Log("FuseAPI:GetOriginalAccountType()==" + accountType);
			
			return accountType;
		}
		else
		{
//			Debug.Log("FuseAPI:GetOriginalAccountType()");
			
			return AccountType.NONE;
		}
	}
	
	private static void _AccountLoginComplete(AccountType type, string accountId)
	{
//		Debug.Log("FuseAPI:AccountLoginComplete(" + type + "," + accountId + ")");
		
		OnAccountLoginComplete(type, accountId);
	}
	
	#endregion
	
	#region Miscellaneous
	[DllImport("__Internal")]
	private static extern int FuseAPI_GamesPlayed();
	
	public static int GamesPlayed()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			int gamesPlayed = FuseAPI_GamesPlayed();
			
//			Debug.Log("FuseAPI:GamesPlayed()==" + gamesPlayed);
		
			return gamesPlayed;
		}
		else
		{
			return 0;
		}
	}
	
	[DllImport("__Internal")]
	private static extern string FuseAPI_LibraryVersion();
	
	public static string LibraryVersion()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string libraryVersion = FuseAPI_LibraryVersion();
			
//			Debug.Log("FuseAPI:LibraryVersion()==" + libraryVersion);
		
			return libraryVersion;
		}
		else
		{
			return "1.22";
		}
	}
	[DllImport("__Internal")]
	private static extern bool FuseAPI_Connected();
	
	public static bool Connected()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			bool connected = FuseAPI_Connected();
			
//			Debug.Log("FuseAPI:Connected()==" + connected);
		
			return connected;
		}
		else
		{
			return true;
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_TimeFromServer();
	
	public static void TimeFromServer()
	{
//		Debug.Log("FuseAPI:TimeFromServer()");
		
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
//		Debug.Log("FuseAPI:TimeUpdated(" + timestamp + ")");
		
		if (timestamp >= 0)
		{
			OnTimeUpdated(unixEpoch + TimeSpan.FromTicks(timestamp * TimeSpan.TicksPerSecond));
		}
	}
	
	private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	
	[DllImport("__Internal")]
	private static extern bool FuseAPI_NotReadyToTerminate();
	
	public static bool NotReadyToTerminate()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			bool notReadyToTerminate = FuseAPI_NotReadyToTerminate();
			
//			Debug.Log("FuseAPI:NotReadyToTerminate()==" + notReadyToTerminate);
		
			return notReadyToTerminate;
		}
		else
		{
			return false;
		}
	}
	#endregion
	
	#region Data Opt In/Out
	[DllImport("__Internal")]
	private static extern void FuseAPI_EnableData(bool enable);
	
	public static void EnableData(bool enable)
	{
//		Debug.Log("FuseAPI:EnableData(" + enable + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_EnableData(enable);
		}
	}
	
	[DllImport("__Internal")]
	private static extern bool FuseAPI_DataEnabled();
	
	public static bool DataEnabled()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			bool enabled = FuseAPI_DataEnabled();
			
//			Debug.Log("FuseAPI:DataEnabled()==" + enabled);
		
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
	
	public static int SetGameData(Hashtable data)
	{
		return SetGameData("", data, false, GetFuseId());
	}
	
	public static int SetGameData(string key, Hashtable data)
	{
		return SetGameData(key, data, false, GetFuseId());
	}
	
	public static int SetGameData(string key, Hashtable data, bool isCollection)
	{
		return SetGameData(key, data, isCollection, GetFuseId());
	}
	
	public static int SetGameData(string key, Hashtable data, bool isCollection, string fuseId)
	{
//		Debug.Log ("FuseAPI:SetGameData(" + key + "," + isCollection + "," + fuseId + ")");
		
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
//		Debug.Log("FuseAPI:GameDataError(" + error + "," + requestId + ")");
		
		OnGameDataError(error, requestId);
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_GetGameDataStart(string key, string fuseId);
	[DllImport("__Internal")]
	private static extern void FuseAPI_GetGameDataKey(string key);
	[DllImport("__Internal")]
	private static extern int FuseAPI_GetGameDataEnd();
	
	public static int GetGameData(string[] keys)
	{
		return GetFriendGameData("", "", keys);
	}
	
	public static int GetGameData(string key, string[] keys)
	{
		return GetFriendGameData("", key, keys);
	}
	
	public static int GetFriendGameData(string fuseId, string[] keys)
	{
		return GetFriendGameData(fuseId, "", keys);
	}
	
	public static int GetFriendGameData(string fuseId, string key, string[] keys)
	{
//		Debug.Log ("FuseAPI:GetGameData(" + fuseId + "," + key + ")");
		
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
//		Debug.Log("FuseAPI:GameDataSetAcknowledged(" + requestId + ")");
		
		OnGameDataSetAcknowledged(requestId);
	}
	
	private static void _GameDataReceivedStart(string fuseId, string key, int requestId)
	{
//		Debug.Log("FuseAPI:GameDataReceivedStart(" + fuseId + "," + key + ")");
		
		_gameDataFuseId = fuseId;
		_gameDataKey = key;
		_gameData = new Hashtable();
		_gameDataRequestId = requestId;
	}
	
	private static void _GameDataReceivedKeyValue(string key, string value, bool isBinary)
	{
//		Debug.Log("FuseAPI:GameDataReceivedKeyValue(" + key + "," + value + "," + isBinary + ")");
		
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
//		Debug.Log("FuseAPI:GameDataReceivedEnd()");
		
		OnGameDataReceived(_gameDataFuseId, _gameDataKey, _gameData, _gameDataRequestId);
		_gameDataRequestId = -1;
		_gameData = null;
		_gameDataKey = "";
		_gameDataFuseId = "";
	}
	
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetFuseId();
	
	public static string GetFuseId()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string fuseId = FuseAPI_GetFuseId();
			
//			Debug.Log("FuseAPI:GetFuseId()==" + fuseId);
			
			return fuseId;
		}
		else
		{
//			Debug.Log("FuseAPI:GetFuseId()");
			
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
	private static extern void FuseAPI_UpdateFriendsListFromServer();
	
	public static void UpdateFriendsListFromServer()
	{
//		Debug.Log("FuseAPI:UpdateFriendsListFromServer()");
		
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
//		Debug.Log("FuseAPI:FriendsListUpdatedStart()");
		
		_friendsList = new List<Friend>();
	}
	
	private static void _FriendsListUpdatedFriend(string fuseId, string accountId, string alias, bool pending)
	{
//		Debug.Log("FuseAPI:FriendsListUpdatedFriend(" + fuseId + "," + accountId + "," + alias + "," + pending + ")");
		
		Friend friend = new Friend();
		friend.fuseId = fuseId;
		friend.accountId = accountId;
		friend.alias = alias;
		friend.pending = pending;
		
		_friendsList.Add(friend);
	}
	
	private static void _FriendsListUpdatedEnd()
	{
//		Debug.Log("FuseAPI:FriendsListUpdatedEnd()");
		
		OnFriendsListUpdated(_friendsList);
		
		_friendsList = null;
	}
	
	private static void _FriendsListError(int error)
	{
//		Debug.Log("FuseAPI:FriendsListError(" + error + ")");
		
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
	
	public static List<Friend> GetFriendsList()
	{
//		Debug.Log("FuseAPI:GetFriendsList()");
		
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
	
	public static void UserPushNotification(string fuseId, string message)
	{
//		Debug.Log("FuseAPI:UserPushNotification(" + fuseId +"," + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_UserPushNotification(fuseId, message);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_FriendsPushNotification(string message);
	
	public static void FriendsPushNotification(string message)
	{
//		Debug.Log("FuseAPI:FriendsPushNotification(" + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_FriendsPushNotification(message);
		}
	}
	#endregion
	
	#region Gifting
	[DllImport("__Internal")]
	private static extern void FuseAPI_GetMailListFriendFromServer(string fuseId);
	
	public static void GetMailListFromServer()
	{
		GetMailListFriendFromServer("");
	}
	
	public static void GetMailListFriendFromServer(string fuseId)
	{
//		Debug.Log("FuseAPI:GetMailListFriendFromServer(" + fuseId + ")");
		
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
		Debug.Log("FuseAPI:MailListReceivedStart()");
		
		_mailListFuseId = fuseId;
		_mailList = new List<Mail>();
	}
	
	private static void _MailListReceivedMail(int messageId, long timestamp, string alias, string message, int giftId, string giftName, int giftAmount)
	{
		Debug.Log("FuseAPI:MailListReceivedMail(" + messageId + "," + timestamp + "," + alias + "," + message + "," + giftId + "," + giftName + "," + giftAmount + ")");
		
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
		Debug.Log("FuseAPI:MailListReceivedEnd()");
		
		OnMailListReceived(_mailList, _mailListFuseId);
		
		_mailList = null;
		_mailListFuseId = "";
	}
	
	private static void _MailListError(int error)
	{
		Debug.Log("FuseAPI:MailListError(" + error + ")");
		
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
	
	public static List<Mail> GetMailList(string fuseId)
	{
		Debug.Log("FuseAPI:GetMailList()");
		
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
	
	public static void SetMailAsReceived(int messageId)
	{
		Debug.Log("FuseAPI:SetMailAsReceived(" + messageId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_SetMailAsReceived(messageId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_SendMail(string fuseId, string message);
	
	public static void SendMail(string fuseId, string message)
	{
		Debug.Log("FuseAPI:SendMail(" + fuseId + "," + message + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_SendMail(fuseId, message);
		}
		else
		{
			_MailAcknowledged(-1, fuseId);
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_SendMailWithGift(string fuseId, string message, int giftId, int giftAmount);
	
	public static void SendMailWithGift(string fuseId, string message, int giftId, int giftAmount)
	{
		Debug.Log("FuseAPI:SendMailWithGift(" + fuseId + "," + message + "," + giftId + "," + giftAmount + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_SendMailWithGift(fuseId, message, giftId, giftAmount);
		}
		else
		{
			_MailAcknowledged(-1, fuseId);
		}
	}
	
	private static void _MailAcknowledged(int messageId, string fuseId)
	{
		Debug.Log("FuseAPI:MailAcknowledged()");
		
		OnMailAcknowledged(messageId, fuseId);
	}
	
	private static void _MailError(int error)
	{
		Debug.Log("FuseAPI:MailError(" + error + ")");
		
		OnMailError(error);
	}
	
	#endregion

	#region Game Configuration Data
	[DllImport("__Internal")]
	private static extern string FuseAPI_GetGameConfigurationValue(string key);
	
	public static string GetGameConfigurationValue(string key)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string value = FuseAPI_GetGameConfigurationValue(key);
			
//			Debug.Log("FuseAPI:GetGameConfigurationValue(" + key + ")==" + value + "");
			
			return value;
		}
		else
		{
//			Debug.Log("FuseAPI:GetGameConfigurationValue(" + key + ")");
			
			return "";
		}
	}
	
	private static void _GameConfigurationReceived()
	{
//		Debug.Log("FuseAPI:GameConfigurationReceived()");
		
		OnGameConfigurationReceived();
	}
	
	#endregion
}
#endif
