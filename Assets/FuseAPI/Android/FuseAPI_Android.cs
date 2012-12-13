
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID && !UNITY_EDITOR
public class FuseAPI_Android : FuseAPI
{
	void Awake()
	{
		_callback = GameObject.Find("FuseAPI_Android");

		if (Application.platform == RuntimePlatform.Android)
		{
			_fusePlugin = new AndroidJavaClass("com.fusepowered.fuseapi.FuseAPI");
			Debug.Log("FuseAPI:FusePlugin " + (_fusePlugin == null ? "NOT FOUND" : "FOUND"));
			_fuseUnityPlugin = new AndroidJavaClass("com.fusepowered.unity.FuseUnityAPI");
			Debug.Log("FuseAPI:FuseUnityPlugin " + (_fuseUnityPlugin == null ? "NOT FOUND" : "FOUND"));
		}
	}
	
#region Session Creation
	public static void StartSession(string gameId)
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
#endregion

#region Analytics Event
	public static void RegisterEvent(string message)
	{
		Debug.Log("FuseAPI:RegisterEvent(" + message + ")");
		_fuseUnityPlugin.CallStatic("registerEvent", message);
	}
#endregion

#region In-App Purchase Logging
	public static void RegisterInAppPurchase(PurchaseState purchaseState, string notifyId, string productId, string orderId, DateTime purchaseTime, string developerPayload)
	{
		Debug.Log("FuseAPI:RegisterInAppPurchase(" + purchaseState.ToString() + "," + notifyId + "," + productId + "," + orderId + "," + DateTimeToTimestamp(purchaseTime) + "," + developerPayload + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), notifyId, productId, orderId, DateTimeToTimestamp(purchaseTime), developerPayload);
	}

	public static void RegisterInAppPurchase(PurchaseState purchaseState, string notifyId, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
		Debug.Log("FuseAPI:RegisterInAppPurchase(" + purchaseState.ToString() + "," + notifyId + "," + productId + "," + orderId + "," + DateTimeToTimestamp(purchaseTime) + "," + developerPayload + "," + price + "," + currency + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), notifyId, productId, orderId, DateTimeToTimestamp(purchaseTime), developerPayload, price, currency);
	}
#endregion

#region Fuse Interstitial Ads
	public static void ShowAd()
	{
		Debug.Log("FuseAPI:ShowAd()");
		_fuseUnityPlugin.CallStatic("showAd");
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
	public static void DisplayNotifications()
	{
		Debug.Log("FuseAPI:DisplayNotifications()");
		_fusePlugin.CallStatic("displayNotifications");
	}

	private void _NotificationAction(string action)
	{
		Debug.Log("FuseAPI:AdWillClose()");
		OnNotificationAction(action);
	}
#endregion

#region More Games
	public static void DisplayMoreGames()
	{
		Debug.Log("FuseAPI:DisplayMoreGames()");
		_fusePlugin.CallStatic("displayMoreGames");
	}
#endregion
	
#region Gender
	public static void RegisterGender(Gender gender)
	{
		Debug.Log("FuseAPI:RegisterGender()");
		_fusePlugin.CallStatic("registerGender", (int)gender);
	}
#endregion

#region Account Login
	public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		Debug.Log("FuseAPI:FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		_fuseUnityPlugin.CallStatic("facebookLogin", facebookId, name, accessToken);
	}

	public static void FacebookLogin(string facebookId, string name, Gender gender, string accessToken)
	{
		Debug.Log("FuseAPI:FacebookLogin(" + facebookId + "," + name + "," + gender + "," + accessToken + ")");
		_fuseUnityPlugin.CallStatic("facebookLoginGender", facebookId, name, (int)gender, accessToken);
	}

	public static void TwitterLogin(string twitterId)
	{
		Debug.Log("FuseAPI:TwitterLogin(" + twitterId + ")");
		_fuseUnityPlugin.CallStatic("twitterLogin", twitterId);
	}
	
	public static void OpenFeintLogin(string openFeintId)
	{
		Debug.Log("FuseAPI:OpenFeintLogin(" + openFeintId + ")");
		_fuseUnityPlugin.CallStatic("openFeintLogin", openFeintId);
	}
	
	public static void FuseLogin(string fuseId, string alias)
	{
		Debug.Log("FuseAPI:FuseLogin(" + fuseId + "," + alias + ")");
		_fuseUnityPlugin.CallStatic("fuseLogin", fuseId, alias);
	}
	
	public static string GetOriginalAccountId()
	{
		Debug.Log("FuseAPI:GetOriginalAccountId()");
		return _fuseUnityPlugin.CallStatic<string>("getOriginalAccountId");
	}
	
	public static AccountType GetOriginalAccountType()
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
	public static int GamesPlayed()
	{
		Debug.Log("FuseAPI:GamesPlayed()");
		return _fuseUnityPlugin.CallStatic<int>("gamesPlayed");
	}
	
	public static string LibraryVersion()
	{
		Debug.Log("FuseAPI:LibraryVersion()");
		return _fuseUnityPlugin.CallStatic<string>("libraryVersion");
	}

	public static bool Connected()
	{
		Debug.Log("FuseAPI:Connected()");
		return _fuseUnityPlugin.CallStatic<bool>("connected");
	}

	public static void TimeFromServer()
	{
		Debug.Log("FuseAPI:TimeFromServer()");
		_fuseUnityPlugin.CallStatic("timeFromServer");
	}

	private void _TimeUpdated(string timestamp)
	{
		Debug.Log("FuseAPI:TimeUpdated(" + timestamp + ")");
		OnTimeUpdated(TimestampToDateTime(long.Parse(timestamp)));
	}

	public static bool NotReadyToTerminate()
	{
		Debug.Log("FuseAPI:NotReadyToTerminate()");
		return _fuseUnityPlugin.CallStatic<bool>("notReadyToTerminate");
	}
#endregion

#region Data Opt In/Out
	public static void EnableData(bool enable)
	{
		Debug.Log("FuseAPI:EnableData(" + enable + ")");
		_fuseUnityPlugin.CallStatic("enableData", enable);
	}

	public static bool DataEnabled()
	{
		Debug.Log("FuseAPI:DataEnabled()");
		return _fuseUnityPlugin.CallStatic<bool>("dataEnabled");
	}
#endregion

#region User Game Data
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

		_fuseUnityPlugin.CallStatic("setGameDataEnd", key, isCollection, fuseId);
		return 0; //  fuseUnityPlugin.CallStatic<int>("setGameDataEnd", key, isCollection, fuseId);
	}

	public static string GetFuseId()
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
		if (_args.Count == 0)
		{
			Debug.Log("FuseAPI:GameDataError(" + fuseGameDataError + ")");
			OnGameDataError(int.Parse(fuseGameDataError), 0);
		}
		else
		{
			Debug.Log("FuseAPI:GameDataError(" + fuseGameDataError + "," + _args[0] + ")");
			OnGameDataError(int.Parse(fuseGameDataError), int.Parse(_args[0]));
			_args.Clear();
		}
	}

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
		Debug.Log ("FuseAPI:GetGameData(" + fuseId + "," + key + ",[keys])");
		
		_fuseUnityPlugin.CallStatic("getGameDataStart");
			
		foreach (string entry in keys)
		{
			_fuseUnityPlugin.CallStatic("getGameDataKey", entry);
		}

		_fuseUnityPlugin.CallStatic("getGameDataEnd", key, fuseId);
		return 0; // _fuseUnityPlugin.CallStatic<int>("getGameDataEnd", key, fuseId);
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
	public static void UpdateFriendsListFromServer()
	{
		Debug.Log("FuseAPI:UpdateFriendsListFromServer()");
		_fuseUnityPlugin.CallStatic("updateFriendsListFromServer");
	}
	
	public static List<Friend> GetFriendsList()
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

#region Gifting
	public static void GetMailListFromServer()
	{
		Debug.Log("FuseAPI:GetMailListFromServer()");
		_fuseUnityPlugin.CallStatic("getMailListFromServer");
	}
	
	public static void GetMailListFriendFromServer(string fuseId)
	{
		Debug.Log("FuseAPI:GetMailListFriendFromServer(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("getMailListFriendFromServer", fuseId);
	}
	
	public static List<Mail> GetMailList(string fuseId)
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

	public static void SetMailAsReceived(int messageId)
	{
		Debug.Log("FuseAPI:SetMailAsReceived(" + messageId + ")");
		_fuseUnityPlugin.CallStatic("setMailAsReceived", messageId);
	}
	
	public static void SendMailWithGift(string fuseId, string message, int giftId, int giftAmount)
	{
		Debug.Log("FuseAPI:SendMailWithGift(" + fuseId + "," + message + "," + giftId + "," + giftAmount + ")");
		_fuseUnityPlugin.CallStatic("sendMailWithGift", fuseId, message, giftId, giftAmount);
	}
	
	public static void SendMail(string fuseId, string message)
	{
		Debug.Log("FuseAPI:SendMail(" + fuseId + "," + message + ")");
		_fuseUnityPlugin.CallStatic("sendMail", fuseId, message);
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

	private void _MailAcknowledged(string messageId, string fuseId)
	{
		Debug.Log("FuseAPI:MailAcknowledged(" + messageId + "," + fuseId + ")");
		OnMailAcknowledged(int.Parse(messageId), fuseId);
	}

	private void _MailError(string error)
	{
		Debug.Log("FuseAPI:MailError(" + error + ")");
		OnMailError(int.Parse(error));
	}
#endregion

#region Game Configuration Data
	public static string GetGameConfigurationValue(string key)
	{
		Debug.Log("FuseAPI:GetGameConfigurationValue(" + key + ")");
		return _fuseUnityPlugin.CallStatic<string>("getGameConfigurationValue", key);
	}

	private void _GameConfigurationReceived(string dummy)
	{
		Debug.Log("FuseAPI:GameConfigurationReceived()");
		OnGameConfigurationReceived();
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
}
#endif
