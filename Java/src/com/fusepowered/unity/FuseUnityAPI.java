package com.fusepowered.unity;

import java.util.ArrayList;
import java.util.Currency;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.text.TextUtils;
import android.util.Log;
import android.os.Handler;

import com.fusepowered.fuseactivities.FuseApiAdBrowser;
import com.fusepowered.fuseactivities.FuseApiMoregamesBrowser;
import com.fusepowered.fuseapi.*;
import com.fusepowered.util.GameKeyValuePairs;
import com.fusepowered.util.GameValue;
import com.fusepowered.util.Mail;
import com.fusepowered.util.Player;
import com.fusepowered.util.VerifiedPurchase;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

//@TargetApi(Build.VERSION_CODES.GINGERBREAD)
public class FuseUnityAPI implements Thread.UncaughtExceptionHandler
{	
	public FuseUnityAPI()
	{
		_this = this;
		_activity = UnityPlayer.currentActivity;
	}
	
	public static void setUnityActivity(UnityPlayerActivity activity)
	{
		_activity = activity;
	}
	
	public static FuseUnityAPI instance()
	{
		if( _this == null )
		{
			new FuseUnityAPI();
		}
		
		return _this;
	}
	
	public void Initialize()
	{
		//Log.d(_logTag, "Initialize()");
		
		_this = this;
		_gameDataCallback = new FuseUnityGameDataCallback();
		_adCallback = new FuseUnityAdCallback();
		
		FuseAPI.initializeFuseAPI(_activity, _activity.getApplicationContext());
	}
	
	public void onRestart()
	{
		//Log.d(_logTag, "onRestart()");

		FuseAPI.initializeFuseAPI(_activity, _activity.getApplicationContext());
	}
	
	public void onDestroy()
	{
		//Log.d(_logTag, "onDestroy()");

		if (_sessionStarted)
		{
			FuseAPI.endSession();
			_sessionStarted = false;
		}
	}
	
	public static void onPause()
	{
		//Log.d(_logTag, "onPause()");
		
		if (_sessionStarted)
		{
			FuseAPI.suspendSession();
		}
	}
	
	public static void onResume()
	{
		//Log.d(_logTag, "onResume()");

		if (_sessionStarted)
		{
			FuseAPI.resumeSession(_activity, _gameDataCallback);
		}
	}
	
	public void onBackPressed()
	{
		//Log.d(_logTag, "onBackPressed()");
	}

	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		//Log.d(_logTag, "onActivityResult()");
//		super.onActivityResult(requestCode, resultCode, data);
		/*
		if (resultCode == RESULT_OK)
		{
			switch (ActivityResults.valueOf(data.getData().toString()))
			{
			case AD_DISPLAYED:
				FuseAPI.displayNotifications();
				break;
			case AD_CLICKED:
				break;
			case MORE_GAMES_DISPLAYED:
				SendMessage("FuseCallback_Android", "_OverlayWillClose", "");
			}
		}
		*/
	}

	public void uncaughtException(Thread thread, Throwable ex)
	{
		Log.d(_logTag, "uncaughtException()");
		Log.d(_logTag, ex.toString());
		Log.d(_logTag, ex.getMessage());

		String crashInfo = ex.getMessage();
		String crashName = ex.toString();
		String stack = "";

		StackTraceElement[] stackTrace = ex.getStackTrace();
		for (int index = 0; index < stackTrace.length; ++index)
		{
			String stackLine = stackTrace[index].getFileName() + "(" + stackTrace[index].getLineNumber() + "): Class " + stackTrace[index].getClassName() + ", Method " + stackTrace[index].getMethodName();
			stack += stackLine + "\n";

			Log.d(_logTag, stackLine);			
		}

		FuseAPI.registerCrash(crashInfo, crashName, stack);
	}


// +------------------+
// | Session Creation |
// +------------------+

	public static void startSession(String gameId)
	{
		//Log.d(_logTag, "startSession(" + gameId + ")");
		FuseAPI.startSession(gameId, _activity, _activity.getApplicationContext(), _gameDataCallback);
		_sessionStarted = true;
	}
	
	public static void registerForPushNotifications(final String projectID)
	{
		//Log.d(_logTag, "registerForPushNotifications(" + projectID + ")");

//        Intent forGCM = new Intent(UnityPlayer.currentActivity.getApplicationContext(), FuseUnityAPI.class);
//        FuseAPI.setupGCM(projectID, forGCM, _activity, 0, 0);

        // Get a handler that can be used to post to the main thread
        Handler mainHandler = new Handler(UnityPlayer.currentActivity.getApplicationContext().getMainLooper());

        Runnable myRunnable = new Runnable()
        {
            @Override
            public void run()
            {
                Intent forGCM = new Intent(UnityPlayer.currentActivity.getApplicationContext(), FuseUnityAPI.class);
                FuseAPI.setupGCM(projectID, forGCM, _activity, 0, 0);
            }
        };
        mainHandler.post(myRunnable);
	}

    public static void testGCMSetup()
    {
        //Log.d(_logTag, "testing GCM Setup");
        FuseAPI.testGCMSetup(_activity);
    }
// +-----------------+
// | Analytics Event |
// +-----------------+
	
	//@SuppressWarnings("deprecation")
	public static void registerEvent(String message)
	{
		//Log.d(_logTag, "registerEvent(" + message + ")");
		//FuseAPI.registerEvent(message);
		//registerEventWithDictionary(message, null, null, 0);
		FuseAPI.registerEvent(message, "", "", "", null);
	}
	
	@SuppressWarnings("deprecation")
	public static void registerEventWithDictionary(String message, String[] keys, String[] attributes, int numValues)
	{
		//Log.d(_logTag, "registerEvent(" + message + " + [variables] )");
		HashMap<String, String> data = new HashMap<String, String>();
		for( int i = 0; i < numValues; i++ )
		{
			data.put(keys[i], attributes[i]);
		}
		FuseAPI.registerEvent(message, data);
	}

	private static HashMap<String,Number> _registerEventData;

	public static void registerEventStart()
	{
		//Log.d(_logTag, "registerEventStart()");
		_registerEventData = new HashMap<String,Number>();
	}

	public static void registerEventKeyValue(String entryKey, double entryValue)
	{
		//Log.d(_logTag, "registerEventKeyValue(" + entryKey + "," + entryValue + ")");
		_registerEventData.put(entryKey, entryValue);
	}

	public static int registerEventEnd(String name, String paramName, String paramValue)
	{
		//Log.d(_logTag, "registerEventEnd(" + name + "," + paramName + "," + paramValue + ")");
		int result = FuseAPI.registerEvent(name, paramName, paramValue, _registerEventData).ordinal();
		_registerEventData = null;
		return result;
	}

	public static int registerEvent(String name, String paramName, String paramValue, String variableName, double variableValue)
	{
		//Log.d(_logTag, "registerEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");
		return FuseAPI.registerEvent(name, paramName, paramValue, variableName, new Double(variableValue)).ordinal();
	}



// +-------------------------+
// | In-App Purchase Logging |
// +-------------------------+

	public static void registerInAppPurchase(String purchaseState, String purchaseToken, String productId, String orderId, long purchaseTime, String developerPayload)
	{
		//Log.d(_logTag, "registerInAppPurchase(" + purchaseState + "," + purchaseToken + "," + productId + "," + orderId + "," + purchaseTime + "," + developerPayload + ")");
		VerifiedPurchase purchase = new VerifiedPurchase(purchaseState, purchaseToken, productId, orderId, purchaseTime, developerPayload);
		FuseAPI.registerInAppPurchase(purchase, _gameDataCallback);
	}

	public static void registerInAppPurchase(String purchaseState, String purchaseToken, String productId, String orderId, long purchaseTime, String developerPayload, double price, String currency)
	{
		// If we haven't been passed a currency string, make a guess based on the current locale
		if( TextUtils.isEmpty(currency) ) 
		{
			Locale locale = Locale.getDefault();

			if (locale != null)
			{
				Currency c = Currency.getInstance(locale);

				if (c != null)
				{
					currency = c.getCurrencyCode();
				}
			}
		}

		//Log.d(_logTag, "registerInAppPurchase(" + purchaseState + "," + purchaseToken + "," + productId + "," + orderId + "," + purchaseTime + "," + developerPayload + "," + price + "," + currency + ")");
		VerifiedPurchase purchase = new VerifiedPurchase(purchaseState, purchaseToken, productId, orderId, purchaseTime, developerPayload);
		FuseAPI.registerInAppPurchase(purchase, price, currency, _gameDataCallback);
	}



// +-----------------------+
// | Fuse Interstitial Ads |
// +-----------------------+

	public static void checkAdAvailable()
	{
		_activity.runOnUiThread(new Runnable() {
		    public void run() {
				//Log.d(_logTag, "checkAdAvailable()");
		    	FuseAPI.checkAdAvailable(_adCallback);
   		    }
		});
	}

	public static void showAd()
	{
		//Log.d(_logTag, "showAd()");

		_activity.runOnUiThread(new Runnable() 
		{
		    public void run() 
		    {
		    	FuseAPI.displayAd(new FuseApiAdBrowser(), _adCallback);
		    }
		});
	}



// +---------------+
// | Notifications |
// +---------------+

	public static void displayNotifications()
	{
		//Log.d(_logTag, "displayNotifications()");

		_activity.runOnUiThread(new Runnable() {
		    public void run() {
		    	FuseAPI.displayNotifications(new AlertDialog.Builder(_activity));
		    }
		});
	}
	
	public static boolean isNotificationAvailable()
	{
		return FuseAPI.isNotificationAvailable();
	}
	
	public static void userPushNotification(String fuse_id, String message)
	{
		//Log.d(_logTag, "userPushNotification(" + fuse_id + ", " + message + ")");
		
		FuseAPI.userPushNotification(fuse_id, message);
	}
	
	public static void friendsPushNotification(String message)
	{
		//Log.d(_logTag, "friendsPushNotification(" + message + ")");
		
		FuseAPI.friendsPushNotification(message);
	}


// +------------+
// | More games |
// +------------+

	public static void displayMoreGames()
	{
		//Log.d(_logTag, "displayMoreGames()");

		_activity.runOnUiThread(new Runnable() {
		    public void run() {
		    	FuseAPI.displayMoreGames(new FuseApiMoregamesBrowser());
		    }
		});
	}



// +--------+
// | Gender |
// +--------+

	public static void registerGender(int gender)
	{
		//Log.d(_logTag, "registerGender(" + gender + ")");
		FuseAPI.registerGender(gender);
	}



// +---------------+
// | Account Login |
// +---------------+
	
	public static void facebookLogin(String facebookId, String name, String accessToken)
	{
		//Log.d(_logTag, "facebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		FuseAPI.facebookLogin(facebookId, name, accessToken, _gameDataCallback);
	}

	public static void facebookLoginGender(String facebookId, String name, int gender, String accessToken) // TODO Finish this when the Fuse API supports the call
	{
		Log.d(_logTag, "*** NOT IMPLEMENTED *** facebookLoginGender(" + facebookId + "," + name + "," + gender + "," + accessToken + ")");
	}

	public static void twitterLogin(String twitterId)
	{
		//Log.d(_logTag, "twitterLogin(" + twitterId + ")");
		FuseAPI.twitterLogin(twitterId, _gameDataCallback);
	}

	public static void openFeintLogin(String openFeintId)
	{
		//Log.d(_logTag, "*** NOT IMPLEMENTED *** openFeintLogin(" + openFeintId + ")");
	}

	public static void fuseLogin(String fuseId, String alias)
	{
		//Log.d(_logTag, "fuseLogin(" + fuseId + "," + alias + ")");
		FuseAPI.fuseLogin(fuseId, alias, _gameDataCallback);
	}
	
	public static void googlePlayLogin(String alias, String token)
	{
		//Log.d(_logTag, "googlePlayLogin(" + alias + "," + token + ")");		
		FuseAPI.googlePlayLogin(alias, token, _gameDataCallback);
	}
	
	public static void gamecenterLogin(String accountID, String alias)
	{
		//Log.d(_logTag, "gamecenterLogin(" + accountID + "," + alias + ")");
		FuseAPI.gamecenterLogin(accountID, alias, _gameDataCallback);
	}
	
	public static void deviceLogin(String alias)
	{
		//Log.d(_logTag, "deviceLogin(" + alias + ")");
		FuseAPI.deviceLogin(alias, _gameDataCallback);
	}
	
	public static String getFuseID()
	{
		//Log.d(_logTag, "getFuseID() = " + FuseAPI.getFuseID());
		return FuseAPI.getFuseID();
	}

	public static String getOriginalAccountId()
	{
		//Log.d(_logTag, "getOriginalAccountId()");
		return FuseAPI.getOriginalAccountId();
	}
	
	public static String getOriginalAccountAlias()
	{
		//Log.d(_logTag, "getOriginalAccountAlias()");
		return FuseAPI.getOriginalAccountAlias();
	}

	public static int getOriginalAccountType()
	{
		//Log.d(_logTag, "getOriginalAccountType()");
		return FuseAPI.getOriginalAccountType();
	}



// +---------------+
// | Miscellaneous |
// +---------------+
		
	public static int gamesPlayed()
	{
		//Log.d(_logTag, "gamesPlayed() = " + FuseAPI.gamesPlayed());
		return FuseAPI.gamesPlayed();
	}

	public static String libraryVersion()
	{
		//Log.d(_logTag, "libraryVersion() = " + FuseAPI.libraryVersion());
		return FuseAPI.libraryVersion();
	}

	public static boolean connected()
	{
		//Log.d(_logTag, "connected() = " + FuseAPI.connected());
		return FuseAPI.connected();
	}

	public static void timeFromServer()
	{
		//Log.d(_logTag, "timeFromServer()");
		FuseAPI.utcTimeFromServer(_gameDataCallback);
	}

	public static boolean notReadyToTerminate() // TODO Finish this when the Fuse API supports the call
	{
		Log.d(_logTag, "*** NOT IMPLEMENTED *** notReadyToTerminate()");
		return false;
	}


// +-----------------+
// | Data Opt In/Out |
// +-----------------+
			
	public static void enableData(boolean enable)
	{
		//Log.d(_logTag, "enableData(" + enable + ")");
		FuseAPI.userOptOut(enable ? 0 : 1);
	}

	public static boolean dataEnabled() // TODO Finish this when the Fuse API supports the call
	{
		Log.d(_logTag, "*** NOT IMPLEMENTED *** dataEnabled()");
		return true;
	}



// +----------------+
// | User Game Data |
// +----------------+
		
	private static HashMap<String,GameValue> _setGameData;

	public static void setGameDataStart()
	{
		//Log.d(_logTag, "setGameDataStart()");
		_setGameData = new HashMap<String,GameValue>();
	}

	public static void setGameDataKeyValue(String entryKey, String entryValue, boolean isBinary)
	{
		//Log.d(_logTag, "setGameDataKeyValue(" + entryKey + "," + entryValue + "," + isBinary + ")");
		_setGameData.put(entryKey, new GameValue(entryValue, isBinary));
	}

	public static int setGameDataEnd(String key, boolean isCollection, String fuseId)
	{
		//Log.d(_logTag, "setGameDataEnd(" + key + "," + isCollection + "," + fuseId + ")");

		GameKeyValuePairs gameKeyValuePairs = new GameKeyValuePairs();
		gameKeyValuePairs.setMap(_setGameData);

		FuseUnityGameDataCallback callback = new FuseUnityGameDataCallback();

		int requestID = 0;
		if (key != null && !key.equals(""))
		{
			requestID = FuseAPI.setGameData(fuseId, key, gameKeyValuePairs, callback);
		}
		else if (fuseId.equals(getFuseID()))
		{
			requestID = FuseAPI.setGameData(gameKeyValuePairs, callback);
		}

		_setGameData = null;

		return requestID;
	}
	
	private static ArrayList<String> _getGameData;
	public static void getGameDataStart()
	{
		//Log.d(_logTag, "getGameDataStart()");
		_getGameData = new ArrayList<String>();
	}

	public static void getGameDataKey(String entryKey)
	{
		//Log.d(_logTag, "getGameDataKey(" + entryKey + ")");
		_getGameData.add(entryKey);
	}

	public static int getGameDataEnd(String key, String fuseId)
	{
		//Log.d(_logTag, "getGameDataEnd(" + key + "," + fuseId + ")");

		FuseUnityGameDataCallback callback = new FuseUnityGameDataCallback();

		int requestID = 0;
		if (fuseId != null && !fuseId.equals(""))
		{
			if (key != null && !key.equals(""))
			{
				requestID = FuseAPI.getFriendGameData(key, _getGameData, callback, fuseId);
			}
			else
			{
				requestID = FuseAPI.getFriendGameData(_getGameData, callback, fuseId);
			}
		}
		else
		{
			if (key != null && !key.equals(""))
			{
				requestID = FuseAPI.getGameData(key, _getGameData, callback);
			}
			else
			{
				requestID = FuseAPI.getGameData(_getGameData, callback);
			}
		}

		_getGameData = null;

		return requestID;
	}



// +-------------+
// | Friend List |
// +-------------+

	public static void addFriend(String fuseId)
    {
        //Log.d(_logTag, "addFriend(" + fuseId + ")");
        FuseAPI.addFriend(fuseId, _gameDataCallback);
    }

	public static void removeFriend(String fuseId)
    {
        //Log.d(_logTag, "removeFriend(" + fuseId + ")");
        FuseAPI.removeFriend(fuseId, _gameDataCallback);
    }

	public static void acceptFriend(String fuseId)
    {
        //Log.d(_logTag, "acceptFriend(" + fuseId + ")");
        FuseAPI.acceptFriend(fuseId, _gameDataCallback);
    }

	public static void rejectFriend(String fuseId)
    {
        //Log.d(_logTag, "rejectFriend(" + fuseId + ")");
        FuseAPI.rejectFriend(fuseId, _gameDataCallback);
    }	

    public static void migrateFriends(String fuseId)
    {
        //Log.d(_logTag, "migrateFriends(" + fuseId + ")");
        FuseAPI.migrateFriends(fuseId, _gameDataCallback);
    }

	public static void updateFriendsListFromServer()
	{
		//Log.d(_logTag, "updateFriendsListFromServer()");
		FuseAPI.updateFriendsListFromServer(_gameDataCallback);
	}

	public static String getFriendsList()
	{
		//Log.d(_logTag, "getFriendsList()");
		List<Player> friendsList = FuseAPI.getFriendsList();
		String returnValue = "";		
		if( friendsList == null )
		{
			return returnValue;
		}

		for (Player friend : friendsList)
		{
			returnValue += MakeReturnComponent(friend.getFuseId());
			returnValue += MakeReturnComponent(friend.getAccountId());
			returnValue += MakeReturnComponent(friend.getAlias());
			returnValue += MakeReturnComponent(friend.getPending());
		}
		return returnValue;
	}



// +---------+
// | Gifting |
// +---------+

	public static void getMailListFromServer()
	{
		//Log.d(_logTag, "getMailListFromServer()");
		FuseAPI.getMailListFromServer(_gameDataCallback);
	}

	public static void getMailListFriendFromServer(String fuseId)
	{
		//Log.d(_logTag, "getMailListFriendFromServer(" + fuseId + ")");
		FuseAPI.getMailListFriendFromServer(fuseId, _gameDataCallback);
	}

	public static String getMailList(String fuseId)
	{
		//Log.d(_logTag, "getMailList(" + fuseId + ")");
		List<Mail> mailList = FuseAPI.getMailList(fuseId);

		String returnValue = "";

		for (Mail mail : mailList)
		{
			returnValue += MakeReturnComponent(mail.getId());
			returnValue += MakeReturnComponent(mail.getDate());
			returnValue += MakeReturnComponent(mail.getAlias());
			returnValue += MakeReturnComponent(mail.getMessage());
			returnValue += MakeReturnComponent(mail.getGift().getId());
			returnValue += MakeReturnComponent(mail.getGift().getName());
			returnValue += MakeReturnComponent(mail.getGift().getAmount());
		}

		return returnValue;
	}

	public static void setMailAsReceived(int messageId)
	{
		//Log.d(_logTag, "setMailAsReceived(" + messageId + ")");
		FuseAPI.setMailAsReceived(messageId);
	}

	public static int sendMailWithGift(String fuseId, String message, int giftId, int giftAmount)
	{
		//Log.d(_logTag, "sendMailWithGift(" + fuseId + "," + message + "," + giftId + "," + giftAmount + ")");
		return FuseAPI.sendMailWithGift(fuseId, message, giftId, giftAmount, _gameDataCallback);
	}

	public static int sendMail(String fuseId, String message)
	{
		//Log.d(_logTag, "sendMailWithGift(" + fuseId + "," + message + ")");
		return FuseAPI.sendMail(fuseId, message, _gameDataCallback);
	}



// +-------------------------+
// | Game Configuration Data |
// +-------------------------+

	public static String _stringConduit;
	public static void getGameConfigurationValue()
	{
//		Log.d(_logTag, "getGameConfigurationValue(" + _stringConduit + ") = " + FuseAPI.getGameConfigurationValue(_stringConduit));
		_stringConduit = FuseAPI.getGameConfigurationValue(_stringConduit);
	}		
	
	public static String[] getGameConfigKeys()
	{
		//Log.d(_logTag, "getGameConfigKeys()");		
		
		HashMap<String, String> gameConfig = FuseAPI.getGameConfiguration();
		if( gameConfig == null )
		{
			//Log.d(_logTag, "No gameConfig");
			return null;
		}
		Object[] keys = gameConfig.keySet().toArray();
		if( keys == null || keys.length == 0 )
		{
			//Log.d(_logTag, "No keys");
			return null;
		}
		String[] ret = new String[keys.length];		
		
		//Log.d(_logTag, "Number of keys: " + keys.length);
		for( int i = 0; i < keys.length; i++ )
		{
			ret[i]  = (String)keys[i];
		}
		return ret;
	}



// +-----------------------------+
// | Specific Event Registration |
// +-----------------------------+

	public static void registerLevel(int level)
	{
		//Log.d(_logTag, "registerLevel(" + level + ")");
		FuseAPI.registerLevel(level);
	}
	
	public static void registerCurrency(int type, int balance)
	{
		//Log.d(_logTag, "registerCurrency(" + type + "," + balance + ")");
		FuseAPI.registerCurrency(type, balance);
	}
	
	public static void registerFlurryView()
	{
		//Log.d(_logTag, "registerFlurryView()");
		FuseAPI.registerFlurryView();
	}
	
	public static void registerFlurryClick()
	{
		//Log.d(_logTag, "registerFlurryClick()");
		FuseAPI.registerFlurryClick();
	}
	
	public static void registerTapjoyReward(int amount)
	{
		//Log.d(_logTag, "registerTapjoyReward(" + amount + ")");
		FuseAPI.registerTapjoyReward(amount);
	}

	public static void registerAge(int age)
	{
		//Log.d(_logTag, "registerAge(" + age + ")");
		FuseAPI.registerAge(age);
	}

	public static void registerBirthday(int year, int month, int day)
	{
		//Log.d(_logTag, "registerBirthday(" + year + ", " + month + ", " + day + ")");
		FuseAPI.registerBirthday(year, month, day);
	}

// +--------------------+
// | API bridge helpers |
// +--------------------+

	public static void SetGameObjectCallback(String gameObject)
	{
		// initialize the unity plugin
		instance().Initialize();
		
		//Log.d(_logTag, "Callback object set to: " + gameObject);
		callbackObj = gameObject;
	}
	
	public static String MakeReturnComponent(int value)
	{
		return MakeReturnComponent(Integer.toString(value));
	}

	public static String MakeReturnComponent(boolean value)
	{
		return MakeReturnComponent(value ? "1" : "0");
	}

	public static String MakeReturnComponent(String value)
	{
		return value.length() + ":" + value;
	}

	public static void SendMessage(String gameObject, String methodName, String message)
	{
		//Log.d(_logTag, "Sending message: " + methodName + ": " + message);
		UnityPlayer.UnitySendMessage(callbackObj, methodName, message == null ? "" : message);
	}

	static String callbackObj = "FuseAPI";
	private static final String _logTag = "FuseUnityAPI";
	private static FuseUnityAPI _this = null;
	private static Activity _activity;
	private static FuseUnityGameDataCallback _gameDataCallback;
	private static FuseUnityAdCallback _adCallback;
	private static boolean _sessionStarted = false;
}
