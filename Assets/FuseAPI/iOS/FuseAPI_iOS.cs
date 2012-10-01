using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_IPHONE
public class FuseAPI_iOS
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
		
		if (SessionStartReceived != null)
		{
			SessionStartReceived();
		}
	}
	
	public static event Action SessionStartReceived;
	
	private static void _SessionLoginError(int error)
	{
//		Debug.Log("FuseAPI:SessionLoginError(" + error + ")");
		
		if (SessionLoginError != null)
		{
			SessionLoginError(error);
		}
	}
	
	public static event Action<int> SessionLoginError;
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
	
	public struct Product
	{
		public string productId;
		public string priceLocale;
		public float price;
	}
	
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
		
		if (PurchaseVerification != null)
		{
			PurchaseVerification(verified, transactionId, originalTransactionId);
		}
	}
	
	public static event Action<bool, string, string> PurchaseVerification;
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
		
		if (AdWillClose != null)
		{
			AdWillClose();
		}
	}
	
	public static event Action AdWillClose;
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
		
		if (OverlayWillClose != null)
		{
			OverlayWillClose();
		}
	}
	
	public static event Action OverlayWillClose;
	#endregion
	
	#region Gender
	[DllImport("__Internal")]
	private static extern void FuseAPI_RegisterGender(int gender);
	
	public enum Gender { UNKNOWN, MALE, FEMALE };
	
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
			_AccountLoginComplete(0, "");
		}
	}
	
	[DllImport("__Internal")]
	private static extern void FuseAPI_FacebookLogin(string facebookId);
	
	public static void FacebookLogin(string facebookId)
	{
//		Debug.Log ("FuseAPI:FacebookLogin(" + facebookId + ")");
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			FuseAPI_FacebookLogin(facebookId);
		}
		else
		{
			_AccountLoginComplete(2, facebookId);
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
			_AccountLoginComplete(0, twitterId);
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
			_AccountLoginComplete(0, openFeintId);
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
			_AccountLoginComplete(0, fuseId);
		}
	}
	
	private static void _AccountLoginComplete(int type, string accountId)
	{
//		Debug.Log("FuseAPI:AccountLoginComplete(" + type + "," + accountId + ")");
		
		if (AccountLoginComplete != null)
		{
			AccountLoginComplete(type, accountId);
		}
	}
	
	public static event Action<int, string> AccountLoginComplete;
	
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
			_TimeUpdated((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
		}
	}
	
	private static void _TimeUpdated(int timestamp)
	{
//		Debug.Log("FuseAPI:TimeUpdated(" + timestamp + ")");
		
		if (TimeUpdated != null)
		{
			TimeUpdated(timestamp);
		}
	}
	
	public static event Action<int> TimeUpdated;
	
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
	
	private static void _GameDataError(int error)
	{
//		Debug.Log("FuseAPI:GameDataError(" + error + ")");
		
		if (GameDataError != null)
		{
			GameDataError(error);
		}
	}
	
	public static event Action<int> GameDataError;
	
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
			_GameDataReceivedStart(fuseId, key);
			_GameDataReceivedEnd();
			
			return -1;
		}
	}
	
	private static void _GameDataSetAcknowledged(int requestId)
	{
//		Debug.Log("FuseAPI:GameDataSetAcknowledged(" + requestId + ")");
		
		if (GameDataSetAcknowledged != null)
		{
			GameDataSetAcknowledged(requestId);
		}
	}
	
	public static event Action<int> GameDataSetAcknowledged;
	
	private static void _GameDataReceivedStart(string fuseId, string key)
	{
//		Debug.Log("FuseAPI:_GameDataReceivedStart(" + fuseId + "," + key + ")");
		
		_gameDataFuseId = fuseId;
		_gameDataKey = key;
		_gameData = new Hashtable();
	}
	
	private static void _GameDataReceivedKeyValue(string key, string value, bool isBinary)
	{
//		Debug.Log("FuseAPI:_GameDataReceivedKeyValue(" + key + "," + value + "," + isBinary + ")");
		
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
//		Debug.Log("FuseAPI:_GameDataReceivedEnd()");
		
		if (GameDataReceived != null)
		{
			GameDataReceived(_gameDataFuseId, _gameDataKey, _gameData);
			_gameData = null;
			_gameDataKey = "";
			_gameDataFuseId = "";
		}
	}
	
	public static event Action<string, string, Hashtable> GameDataReceived;
	
	private static string _gameDataKey = "";
	private static string _gameDataFuseId = "";
	private static Hashtable _gameData = null;
	#endregion
	
	#region Friend List
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
		
		if (GameConfigurationReceived != null)
		{
			GameConfigurationReceived();
		}
	}
	
	public static event Action GameConfigurationReceived;
	#endregion
}
#endif
