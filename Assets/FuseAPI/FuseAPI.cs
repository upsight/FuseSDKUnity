
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using FusePlatformAPI = FuseAPI_UnityEditor;
#elif UNITY_IPHONE
using FusePlatformAPI = FuseAPI_iOS;
#elif UNITY_ANDROID
using FusePlatformAPI = FuseAPI_Android;
#else
using FusePlatformAPI = FuseAPI_Stub;
#endif



public class FuseAPI : MonoBehaviour
{
	public string AndroidGameID;
	public string iOSGameID;

	public string GCM_SenderID = "";
	public bool registerForPushNotifications = false;
	public bool logging = true;
	public bool persistent = true;

	public bool androidIAB = false;
	public bool androidUnibill = false;

	public bool iosStoreKit = false;
	public bool iosUnibill = false;

	enum Errors
	{
    	NONE = 0,            /// no error has occurred
    	NOT_CONNECTED,       /// the user is not connected to the internet
    	REQUEST_FAILED,      /// there was an error in establishing a connection with the server
    	XML_PARSE_ERROR,     /// data was received, but there was a problem parsing the xml
	};
#region Session Setup

	void Awake()
	{
		FuseAPI fuse = this;
		if(!this.GetType().IsSubclassOf(typeof(FuseAPI)))
		{
			if(androidIAB && !GetComponent<FuseAPI_Prime31_IAB>())
				gameObject.AddComponent<FuseAPI_Prime31_IAB>().logging = this.logging;

			if(androidUnibill && !GetComponent<FuseAPI_Unibill_Android>())
				gameObject.AddComponent<FuseAPI_Unibill_Android>().logging = this.logging;
			
			if(iosStoreKit && !GetComponent<FuseAPI_Prime31StoreKit>())
				gameObject.AddComponent<FuseAPI_Prime31StoreKit>().logging = this.logging;

			if(iosUnibill && !GetComponent<FuseAPI_Unibill_iOS>())
				gameObject.AddComponent<FuseAPI_Unibill_iOS>().logging = this.logging;


			if(Application.platform == RuntimePlatform.Android)
			{
				fuse = gameObject.AddComponent<FuseAPI_Android>();
				fuse.AndroidGameID = this.AndroidGameID;
				fuse.GCM_SenderID = this.GCM_SenderID;
				fuse.registerForPushNotifications = this.registerForPushNotifications;
				fuse.logging = this.logging;
				fuse.persistent = this.persistent;
			}
			else if(Application.platform == RuntimePlatform.IPhonePlayer)
			{
				fuse = gameObject.AddComponent<FuseAPI_iOS>();
				fuse.iOSGameID = this.iOSGameID;
				fuse.registerForPushNotifications = this.registerForPushNotifications;
				fuse.logging = this.logging;
				fuse.persistent = this.persistent;
			}
			else
			{
#if UNITY_EDITOR
				fuse = gameObject.AddComponent<FuseAPI_UnityEditor>();
#else
				fuse = gameObject.AddComponent<FuseAPI_Stub>();
#endif
				fuse.AndroidGameID = this.AndroidGameID;
				fuse.iOSGameID = this.iOSGameID;
				fuse.GCM_SenderID = this.GCM_SenderID;
				fuse.registerForPushNotifications = this.registerForPushNotifications;
				fuse.logging = this.logging;
				fuse.persistent = this.persistent;
			}

			DestroyImmediate(this);
		}

		if(fuse is FuseAPI_Android)
		{
			if(!string.IsNullOrEmpty(fuse.AndroidGameID))
			{
				fuse.Init();
			}
		}
		else if(!string.IsNullOrEmpty(fuse.iOSGameID))
		{
			fuse.Init();
		}
	}

	virtual protected void Init()
	{
	}

#endregion

#region Session Creation

	public static void FuseLog(string str)
	{
		FusePlatformAPI.FuseLog (str);
	}

	[Obsolete("StartSession now called automatically. Set gameID in the FuseAPI Component.", true)]
	public static void StartSession(string gameId)
	{
	}

	protected static void _StartSession(string gameId)
	{
		FuseLog(" Session Started");
		FusePlatformAPI._StartSession(gameId);
	}
	
	public static event Action SessionStartReceived;
	public static event Action<int> SessionLoginError; 
	
	#if UNITY_ANDROID
	public static void SetupPushNotifications(string gcmProjectID)
	{
		FusePlatformAPI.SetupPushNotifications(gcmProjectID);
	}
	#endif
	
#endregion
	
#region Analytics Event
	
	public static void RegisterEvent(string message)
	{
		FusePlatformAPI.RegisterEvent(message);
	}
	
	public static void RegisterEvent(string message, Hashtable values)
	{
		FusePlatformAPI.RegisterEvent(message, values);
	}

	public static int RegisterEvent(string name, string paramName, string paramValue, Hashtable variables)
	{
		return FusePlatformAPI.RegisterEvent(name, paramName, paramValue, variables);
	}

	public static int RegisterEvent(string name, string paramName, string paramValue, string variableName, double variableValue)
	{
		return FusePlatformAPI.RegisterEvent(name, paramName, paramValue, variableName, variableValue);
	}

#endregion
	
#region In-App Purchase Logging
	
	public struct Product
	{
		public string productId;
		public string priceLocale;
		public float price;
	}
	
	public static void RegisterInAppPurchaseList(Product[] products)
	{
		FusePlatformAPI.RegisterInAppPurchaseList(products);
	}
	
	#if UNITY_ANDROID
	public enum PurchaseState { PURCHASED, CANCELED, REFUNDED }

	public static void RegisterInAppPurchase(PurchaseState purchaseState, string notifyId, string productId, string orderId, DateTime purchaseTime, string developerPayload)
	{
		FusePlatformAPI.RegisterInAppPurchase(purchaseState, notifyId, productId, orderId, purchaseTime, developerPayload);
	}
	
	public static void RegisterInAppPurchase(PurchaseState purchaseState, string notifyId, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
		FusePlatformAPI.RegisterInAppPurchase(purchaseState, notifyId, productId, orderId, purchaseTime, developerPayload, price, currency);
	}

	public static void RegisterInAppPurchase(PurchaseState purchaseState, string notifyId, string productId, string orderId, long purchaseTime, string developerPayload, double price, string currency)
	{
		FusePlatformAPI.RegisterInAppPurchase(purchaseState, notifyId, productId, orderId, purchaseTime, developerPayload, price, currency);
	}
	#else
	public enum TransactionState { PURCHASING, PURCHASED, FAILED, RESTORED }	
		
	public static void RegisterInAppPurchase(string productId, string transactionId, byte[] transactionReceipt, TransactionState transactionState)
	{
		FusePlatformAPI.RegisterInAppPurchase(productId, transactionId, transactionReceipt, transactionState);
	}
	#endif

	public static void RegisterUnibillPurchase(string productID, byte[] receipt)
	{
		FusePlatformAPI.RegisterUnibillPurchase(productID, receipt);
	}
	
	public static event Action<int, string, string> PurchaseVerification;
#endregion
	
#region Fuse Ads

	
	public static void PreLoadAd()
	{
		PreLoadAd("");
	}
	public static void PreLoadAd(string adZone)
	{
		FusePlatformAPI.PreLoadAd(adZone);
	}

	public static void CheckAdAvailable()
	{
		CheckAdAvailable("");
	}
	public static void CheckAdAvailable(string adZone)
	{
		FusePlatformAPI.CheckAdAvailable(adZone);
	}

	public static void ShowAd()
	{
		ShowAd("");
	}
	public static void ShowAd(string adZone)
	{
		FusePlatformAPI.ShowAd(adZone);
	}
	
	public static event Action<int, int> AdAvailabilityResponse;
	public static event Action AdWillClose;
	public static event Action AdDisplayed;
	public static event Action AdClicked;
	public static event Action<string> VideoCompleted;
	public static event Action<string> RewardedVideoCompleted;

#endregion

#region Notifications
	
	public static void FuseAPI_RegisterForPushNotifications()
	{
		FusePlatformAPI.FuseAPI_RegisterForPushNotifications();
	}
	
	public static void DisplayNotifications()
	{
		FusePlatformAPI.DisplayNotifications();
	}

    public static bool IsNotificationAvailable()
    {
        return FusePlatformAPI.IsNotificationAvailable();
    }
	
	public static event Action<string> NotificationAction;
#endregion

#region More Games
	
	public static void DisplayMoreGames()
	{
		FusePlatformAPI.DisplayMoreGames();
	}
	
	public static event Action OverlayWillClose;
#endregion
	
#region Gender
	public enum Gender { UNKNOWN, MALE, FEMALE };
	
	public static void RegisterGender(Gender gender)
	{
		FusePlatformAPI.RegisterGender(gender);
	}
#endregion
	
#region Account Login
	public enum AccountType
	{
		NONE = 0,
		GAMECENTER = 1,
		FACEBOOK = 2,
		TWITTER = 3,
		OPENFEINT = 4,
		USER = 5,
		EMAIL = 6,
		DEVICE_ID = 7,
		GOOGLE_PLAY = 8,
	}
	
	public static void GameCenterLogin()
	{
		FusePlatformAPI.GameCenterLogin();
	}
	
	public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		FusePlatformAPI.FacebookLogin(facebookId, name, accessToken);
	}
	
	public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
		FusePlatformAPI.FacebookLogin(facebookId, name, gender, accessToken);
	}
	
	public static void TwitterLogin(string twitterId)
	{
		FusePlatformAPI.TwitterLogin(twitterId);
	}
	
	public static void DeviceLogin(string alias)
	{
		FusePlatformAPI.DeviceLogin(alias);
	}
	
	public static void OpenFeintLogin(string openFeintId)
	{
		FusePlatformAPI.OpenFeintLogin(openFeintId);
	}
	
	public static void FuseLogin(string fuseId, string alias)
	{
		FusePlatformAPI.FuseLogin(fuseId, alias);
	}
	
	public static void GooglePlayLogin(string alias, string token)
	{
		FusePlatformAPI.GooglePlayLogin(alias, token);
	}
	
	public static string GetOriginalAccountAlias()
	{
		return FusePlatformAPI.GetOriginalAccountAlias();
	}
	
	public static string GetOriginalAccountId()
	{
		return FusePlatformAPI.GetOriginalAccountId();
	}
	
	public static AccountType GetOriginalAccountType()
	{
		return FusePlatformAPI.GetOriginalAccountType();
	}
	
	public static event Action<AccountType, string> AccountLoginComplete;
#endregion
	
#region Miscellaneous
	
	public static int GamesPlayed()
	{
		return FusePlatformAPI.GamesPlayed();
	}
	
	public static string LibraryVersion()
	{
		return FusePlatformAPI.LibraryVersion();
	}

	public static bool Connected()
	{
		return FusePlatformAPI.Connected();
	}
	
	public static void TimeFromServer()
	{
		FusePlatformAPI.TimeFromServer();
	}
	
	public static event Action<DateTime> TimeUpdated;
	
	public static bool NotReadyToTerminate()
	{
		return FusePlatformAPI.NotReadyToTerminate();
	}
#endregion
	
#region Data Opt In/Out
	
	public static void EnableData(bool enable)
	{
		FusePlatformAPI.EnableData(enable);
	}
	
	public static bool DataEnabled()
	{
		return FusePlatformAPI.DataEnabled();
	}
#endregion
	
#region User Game Data
	
	public static int SetGameData(Hashtable data)
	{
		return FusePlatformAPI.SetGameData(data);
	}
	
	public static int SetGameData(string key, Hashtable data)
	{
		return FusePlatformAPI.SetGameData(key, data);
	}
	
	public static int SetGameData(string key, Hashtable data, bool isCollection)
	{
		return FusePlatformAPI.SetGameData(key, data, isCollection);
	}
	
	public static int SetGameData(string key, Hashtable data, bool isCollection, string fuseId)
	{
		return FusePlatformAPI.SetGameData(key, data, isCollection, fuseId);
	}
	
	public static int GetGameData(string[] keys)
	{
		return FusePlatformAPI.GetGameData(keys);
	}
	
	public static int GetGameData(string key, string[] keys)
	{
		return FusePlatformAPI.GetGameData(key, keys);
	}
	
	public static int GetFriendGameData(string fuseId, string[] keys)
	{
		return FusePlatformAPI.GetFriendGameData(fuseId, keys);
	}
	
	public static int GetFriendGameData(string fuseId, string key, string[] keys)
	{
		return FusePlatformAPI.GetFriendGameData(fuseId, key, keys);
	}
	
	public static event Action<int, int> GameDataError;
	public static event Action<int> GameDataSetAcknowledged;
	public static event Action<string, string, Hashtable, int> GameDataReceived;
	
	public static string GetFuseId()
	{
		return FusePlatformAPI.GetFuseId();
	}
	
	#endregion
	
	#region Friend List
	public enum FriendErrors
	{
		FUSE_FRIEND_NO_ERROR = 0,
    	FUSEE_FRIEND_BAD_ID,
    	FUSE_FRIEND_NOT_CONNECTED,
    	FUSE_FRIEND_REQUEST_FAILED,
	}	
	public static event Action<string, int> FriendAdded;
	public static event Action<string, int> FriendRemoved;
	public static event Action<string, int> FriendAccepted;
	public static event Action<string, int> FriendRejected;
	
	public static void AddFriend(string fuseId)
	{
		FusePlatformAPI.AddFriend(fuseId);
	}
	public static void RemoveFriend(string fuseId)
	{
		FusePlatformAPI.RemoveFriend(fuseId);
	}
	public static void AcceptFriend(string fuseId)
	{
		FusePlatformAPI.AcceptFriend(fuseId);
	}
	public static void RejectFriend(string fuseId)
	{
		FusePlatformAPI.RejectFriend(fuseId);
	}	
	
	public enum FuseMigrateFriendErrors
	{
	    FUSE_MIGRATE_FRIENDS_NO_ERROR = 0,
	    FUSE_MIGRATE_FRIENDS_BAD_ID,
	    FUSE_MIGRATE_FRIENDS_NOT_CONNECTED,
	    FUSE_MIGRATE_FRIENDS_REQUEST_FAILED
	};

	public static event Action<string, int> FriendsMigrated;
	
	public static void MigrateFriends(string fuseId)
	{
		FusePlatformAPI.MigrateFriends(fuseId);
	}
	
	public static void UpdateFriendsListFromServer()
	{
		FusePlatformAPI.UpdateFriendsListFromServer();
	}
	
	public struct Friend
	{
		public string fuseId;
		public string accountId;
		public string alias;
		public bool pending;
	}
	
	public static event Action<List<Friend>> FriendsListUpdated;
	public static event Action<int> FriendsListError;
	
	public static List<Friend> GetFriendsList()
	{
		return FusePlatformAPI.GetFriendsList();
	}
	#endregion

	#region Chat List
	#endregion

#region User-to-User Push Notifications
	public static void UserPushNotification(string fuseId, string message)
	{
		FusePlatformAPI.UserPushNotification(fuseId, message);
	}
	
	public static void FriendsPushNotification(string message)
	{
		FusePlatformAPI.FriendsPushNotification(message);
	}
#endregion
	
#region Gifting
	
	public static void GetMailListFromServer()
	{
		FusePlatformAPI.GetMailListFromServer();
	}
	
	public static void GetMailListFriendFromServer(string fuseId)
	{
		FusePlatformAPI.GetMailListFriendFromServer(fuseId);
	}
	
	public struct Mail
	{
		public int messageId;
		public DateTime timestamp;
		public string alias;
		public string message;
		public int giftId;
		public string giftName;
		public int giftAmount;
	}
	
	public static event Action<List<Mail>, string> MailListReceived;
	public static event Action<int> MailListError;
	
	public static List<Mail> GetMailList(string fuseId)
	{
		return FusePlatformAPI.GetMailList(fuseId);
	}
	
	public static void SetMailAsReceived(int messageId)
	{
		FusePlatformAPI.SetMailAsReceived(messageId);
	}
	
	public static int SendMailWithGift(string fuseId, string message, int giftId, int giftAmount)
	{
		return FusePlatformAPI.SendMailWithGift(fuseId, message, giftId, giftAmount);
	}
	
	public static int SendMail(string fuseId, string message)
	{
		return FusePlatformAPI.SendMail(fuseId, message);
	}
	
	public static event Action<int, string, int> MailAcknowledged;
	public static event Action<int> MailError;
	
#endregion

#region Game Configuration Data

	public static string GetGameConfigurationValue(string key)
	{
		return FusePlatformAPI.GetGameConfigurationValue(key);
	}		
	
	public static Dictionary<string, string> GetGameConfig()
	{
		return FusePlatformAPI.GetGameConfig();
	}
	
	public static event Action GameConfigurationReceived;
#endregion
	
#region Specific Event Registration
	public static void RegisterLevel(int level)
	{
		FusePlatformAPI.RegisterLevel(level);
	}
	
	public static void RegisterCurrency(int type, int balance)
	{
		FusePlatformAPI.RegisterCurrency(type, balance);
	}
	
	public static void RegisterFlurryView()
	{
		FusePlatformAPI.RegisterFlurryView();
	}
	
	public static void RegisterFlurryClick()
	{
		FusePlatformAPI.RegisterFlurryClick();
	}
	
	public static void RegisterTapjoyReward(int amount)
	{
		FusePlatformAPI.RegisterTapjoyReward(amount);
	}
	
	public static void RegisterAge(int age)
	{
		FusePlatformAPI.RegisterAge(age);
	}
	
	public static void RegisterBirthday(int year, int month, int day)
	{
		FusePlatformAPI.RegisterBirthday(year, month, day);
	}
#endregion
	
#region Internal Event Triggers
	static protected void OnSessionStartReceived()
	{
		if (SessionStartReceived != null)
		{
			SessionStartReceived();
		}
	}
	
	static protected void OnSessionLoginError(int error)
	{
		if (SessionLoginError != null)
		{
			SessionLoginError(error);
		}
	}
	
	static protected void OnPurchaseVerification(int verified, string transactionId, string originalTransactionId)
	{
		if (PurchaseVerification != null)
		{
			PurchaseVerification(verified, transactionId, originalTransactionId);
		}
	}
	
	static protected void OnAdAvailabilityResponse(int available, int error)
	{
		if (AdAvailabilityResponse != null)
		{
			AdAvailabilityResponse(available, error);
		}
	}
	
	static protected void OnAdWillClose()
	{
		if (AdWillClose != null)
		{
			AdWillClose();
		}
	}
	
	static protected void OnAdDisplayed()
	{
		if (AdDisplayed != null)
		{
			AdDisplayed();
		}
	}

	static protected void OnAdClicked()
	{
		if (AdClicked != null)
		{
			AdClicked();
		}
	}
	
	static protected void OnVideoCompleted(string adZone)
	{
		if (VideoCompleted != null)
		{
			VideoCompleted(adZone);
		}
	}
	
	static protected void OnRewardedVideoCompleted(string adZone)
	{
		if (RewardedVideoCompleted != null)
		{
			RewardedVideoCompleted(adZone);
		}
	}

	static protected void OnNotificationAction(string action)
	{
		if (NotificationAction != null)
		{
			NotificationAction(action);
		}
	}
	
	static protected void OnOverlayWillClose()
	{
		if (OverlayWillClose != null)
		{
			OverlayWillClose();
		}
	}
	
	static protected void OnAccountLoginComplete(AccountType type, string accountId)
	{
		if (AccountLoginComplete != null)
		{
			AccountLoginComplete(type, accountId);
		}
	}
	
	static protected void OnTimeUpdated(DateTime time)
	{
		if (TimeUpdated != null)
		{
			TimeUpdated(time);
		}
	}
	
	static protected void OnGameDataError(int error, int requestId)
	{
		if (GameDataError != null)
		{
			GameDataError(error, requestId);
		}
	}
	
	static protected void OnGameDataSetAcknowledged(int requestId)
	{
		if (GameDataSetAcknowledged != null)
		{
			GameDataSetAcknowledged(requestId);
		}
	}
	
	static protected void OnGameDataReceived(string fuseId, string dataKey, Hashtable data, int requestId)
	{
		if (GameDataReceived != null)
		{
			GameDataReceived(fuseId, dataKey, data, requestId);
		}
	}
	
	static protected void OnFriendAdded(string fuseId, int error)
	{
		if( FriendAdded != null )
		{
			FriendAdded(fuseId, error);
		}
	}
	
	static protected void OnFriendRemoved(string fuseId, int error)
	{
		if( FriendAdded != null )
		{
			FriendRemoved(fuseId, error);
		}
	}
	
	static protected void OnFriendAccepted(string fuseId, int error)
	{
		if( FriendAdded != null )
		{
			FriendAccepted(fuseId, error);
		}
	}
	
	static protected void OnFriendRejected(string fuseId, int error)
	{
		if( FriendAdded != null )
		{
			FriendRejected(fuseId, error);
		}
	}
	
	static protected void OnFriendsMigrated(string fuseId, int error)
	{
		if( FriendsMigrated != null )
		{
			FriendsMigrated(fuseId, error);
		}
	}
	
	static protected void OnFriendsListUpdated(List<Friend> friends)
	{
		if (FriendsListUpdated != null)
		{
			FriendsListUpdated(friends);
		}
	}
	
	static protected void OnFriendsListError(int error)
	{
		if (FriendsListError != null)
		{
			FriendsListError(error);
		}
	}
	
	static protected void OnMailListReceived(List<Mail> mailList, string mailFuseId)
	{
		if (MailListReceived != null)
		{
			MailListReceived(mailList, mailFuseId);
		}
	}
	
	static protected void OnMailListError(int error)
	{
		if (MailListError != null)
		{
			MailListError(error);
		}
	}
	
	static protected void OnMailAcknowledged(int messageId, string fuseId, int requestID)
	{
		if (MailAcknowledged != null)
		{
			MailAcknowledged(messageId, fuseId, requestID);
		}
	}
	
	static protected void OnMailError(int error)
	{
		if (MailError != null)
		{
			MailError(error);
		}
	}

	static protected void OnGameConfigurationReceived()
	{
		if (GameConfigurationReceived != null)
		{
			GameConfigurationReceived();
		}
	}
	#endregion

	#region Conversions
	static public long DateTimeToTimestamp(DateTime dateTime)
	{
		return (dateTime - unixEpoch).Ticks / TimeSpan.TicksPerSecond;
	}

	static public DateTime TimestampToDateTime(long timestamp)
	{
		return unixEpoch + TimeSpan.FromTicks(timestamp * TimeSpan.TicksPerSecond);
	}

	private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	
#endregion
}
