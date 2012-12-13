package com.fusepowered.unity;

import com.fusepowered.fuseactivities.FuseApiAdBrowser;
import com.fusepowered.fuseactivities.FuseApiMoregamesBrowser;
import com.fusepowered.fuseapi.FuseAPI;
import com.fusepowered.util.VerifiedPurchase;
import com.fusepowered.util.GameKeyValuePairs;
import com.fusepowered.util.GameValue;
import com.fusepowered.util.Player;
import com.fusepowered.util.Mail;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.util.HashMap;
import java.util.ArrayList;
import java.util.List;

import android.os.Bundle;
import android.util.Log;
import android.app.AlertDialog;
import android.content.Intent;


public class FuseUnityAPI extends UnityPlayerActivity implements Thread.UncaughtExceptionHandler
{
	public void onCreate(Bundle savedInstanceState)
	{
		Log.d(_logTag, "onCreate()");
		super.onCreate(savedInstanceState);
		
		_this = this;
		_gameDataCallback = new FuseUnityGameDataCallback();
		_adCallback = new FuseUnityAdCallback();
		
//		Thread.currentThread().setUncaughtExceptionHandler(this);
//		Thread.setDefaultUncaughtExceptionHandler(this);

		FuseAPI.initializeFuseAPI(this, getApplicationContext());

//		getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LOW_PROFILE);
	}
	
	public void onRestart()
	{
		Log.d(_logTag, "onRestart()");
		super.onRestart();

		FuseAPI.initializeFuseAPI(this, getApplicationContext());

//		getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LOW_PROFILE);
	}
	
	public void onDestroy()
	{
		Log.d(_logTag, "onDestroy()");
		super.onDestroy();

		if (_sessionStarted)
		{
			FuseAPI.endSession();
			_sessionStarted = false;
		}
	}
	
	public void onPause()
	{
		Log.d(_logTag, "onPause()");
		super.onPause();
		
		if (_sessionStarted)
		{
			FuseAPI.suspendSession();
		}
	}
	
	public void onResume()
	{
		Log.d(_logTag, "onResume()");
		super.onResume();

		if (_sessionStarted)
		{
			FuseAPI.resumeSession(_gameDataCallback);
		}

//		getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LOW_PROFILE);
	}
	
	public void onBackPressed()
	{
		Log.d(_logTag, "onBackPressed()");
		super.onBackPressed();
	}

	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		Log.d(_logTag, "onActivityResult()");
		super.onActivityResult(requestCode, resultCode, data);
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
				UnityPlayer.UnitySendMessage("FuseCallback_Android", "_OverlayWillClose", "");
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
		Log.d(_logTag, "startSession(" + gameId + ")");
		FuseAPI.startSession(gameId, _this, _this.getApplicationContext(), _gameDataCallback);
		_sessionStarted = true;
	}



// +-----------------+
// | Analytics Event |
// +-----------------+

	public static void registerEvent(String message)
	{
		Log.d(_logTag, "startSession(" + message + ")");
		FuseAPI.registerEvent(message);
	}



// +-------------------------+
// | In-App Purchase Logging |
// +-------------------------+

	public static void registerInAppPurchase(String purchaseState, String notifyId, String productId, String orderId, int purchaseTime, String developerPayload)
	{
		Log.d(_logTag, "registerInAppPurchase(" + purchaseState + "," + notifyId + "," + productId + "," + orderId + "," + purchaseTime + "," + developerPayload + ")");
		VerifiedPurchase purchase = new VerifiedPurchase(purchaseState, notifyId, productId, orderId, purchaseTime, developerPayload);
		FuseAPI.registerInAppPurchase(purchase);
	}

	public static void registerInAppPurchase(String purchaseState, String notifyId, String productId, String orderId, int purchaseTime, String developerPayload, double price, String currency)
	{
		Log.d(_logTag, "registerInAppPurchase(" + purchaseState + "," + notifyId + "," + productId + "," + orderId + "," + purchaseTime + "," + developerPayload + "," + price + "," + currency + ")");
		VerifiedPurchase purchase = new VerifiedPurchase(purchaseState, notifyId, productId, orderId, purchaseTime, developerPayload);
		FuseAPI.registerInAppPurchase(purchase, price, currency);
	}



// +-----------------------+
// | Fuse Interstitial Ads |
// +-----------------------+

	public static void showAd()
	{
		Log.d(_logTag, "showAd()");

		_this.runOnUiThread(new Runnable() {
		    public void run() {
		    	FuseAPI.getAd(new FuseApiAdBrowser(), _adCallback); // TODO Change this to the new way when Fuse fix it
//				FuseAPI.showFuseApiAdBrowser(new FuseApiAdBrowser());
		    }
		});
	}



// +---------------+
// | Notifications |
// +---------------+

	public static void displayNotifications()
	{
		Log.d(_logTag, "displayNotifications()");
		FuseAPI.displayNotifications(new AlertDialog.Builder(_this));
	}



// +------------+
// | More games |
// +------------+

	public static void displayMoreGames()
	{
		Log.d(_logTag, "displayMoreGames()");
		FuseAPI.displayMoreGames(new FuseApiMoregamesBrowser());
	}



// +--------+
// | Gender |
// +--------+

	public static void registerGender(int gender)
	{
		Log.d(_logTag, "registerGender(" + gender + ")");
		FuseAPI.registerGender(gender);
	}



// +---------------+
// | Account Login |
// +---------------+
	
	public static void facebookLogin(String facebookId, String name, String accessToken)
	{
		Log.d(_logTag, "facebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		FuseAPI.facebookLogin(facebookId, name); // TODO Send the access token when the Fuse API supports it
	}

	public static void facebookLoginGender(String facebookId, String name, int gender, String accessToken) // TODO Finish this when the Fuse API supports the call
	{
		Log.d(_logTag, "*** NOT IMPLEMENTED *** facebookLoginGender(" + facebookId + "," + name + "," + gender + "," + accessToken + ")");
	}

	public static void twitterLogin(String twitterId)
	{
		Log.d(_logTag, "twitterLogin(" + twitterId + ")");
		FuseAPI.twitterLogin(twitterId, _gameDataCallback);
	}

	public static void openFeintLogin(String openFeintId)
	{
		Log.d(_logTag, "*** NOT IMPLEMENTED *** openFeintLogin(" + openFeintId + ")");
	}

	public static void fuseLogin(String fuseId, String alias)
	{
		Log.d(_logTag, "fuseLogin(" + fuseId + "," + alias + ")");
		FuseAPI.fuseLogin(fuseId, alias, _gameDataCallback);
	}

	public static String getOriginalAccountId() // TODO Return the account ID when the Fuse API supports this call
	{
		Log.d(_logTag, "*** NOT IMPLEMENTED *** getOriginalAccountId()");
		return "";
	}

	public static int getOriginalAccountType() // TODO Return the account type when the Fuse API supports this call
	{
		Log.d(_logTag, "*** NOT IMPLEMENTED *** getOriginalAccountType()");
		return 0;
	}



// +---------------+
// | Miscellaneous |
// +---------------+
		
	public static int gamesPlayed()
	{
		Log.d(_logTag, "gamesPlayed() = " + FuseAPI.getNumPlays());
		return FuseAPI.getNumPlays();
	}

	public static String libraryVersion()
	{
		Log.d(_logTag, "libraryVersion() = " + FuseAPI.libraryVersion());
		return FuseAPI.libraryVersion();
	}

	public static boolean connected()
	{
		Log.d(_logTag, "connected() = " + FuseAPI.connected());
		return FuseAPI.connected();
	}

	public static void timeFromServer()
	{
		Log.d(_logTag, "timeFromServer()");
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
		Log.d(_logTag, "enableData(" + enable + ")");
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
		Log.d(_logTag, "setGameDataStart()");
		_setGameData = new HashMap<String,GameValue>();
	}

	public static void setGameDataKeyValue(String entryKey, String entryValue, boolean isBinary)
	{
		Log.d(_logTag, "setGameDataKeyValue(" + entryKey + "," + entryValue + "," + isBinary + ")");
		_setGameData.put(entryKey, new GameValue(entryValue, isBinary));
	}

	public static void setGameDataEnd(String key, boolean isCollection, String fuseId)
	{
		Log.d(_logTag, "setGameDataEnd(" + key + "," + isCollection + "," + fuseId + ")");

		GameKeyValuePairs gameKeyValuePairs = new GameKeyValuePairs();
		gameKeyValuePairs.setMap(_setGameData);

		if (key != null && !key.equals(""))
		{
			FuseAPI.setGameData(fuseId, key, gameKeyValuePairs, _gameDataCallback);
		}
		else if (fuseId.equals(getFuseID()))
		{
			FuseAPI.setGameData(gameKeyValuePairs, _gameDataCallback);
		}

		_setGameData = null;
	}

	public static String getFuseID()
	{
		Log.d(_logTag, "getFuseID() = " + FuseAPI.getFuseID());
		return FuseAPI.getFuseID();
	}

	private static ArrayList<String> _getGameData;

	public static void getGameDataStart()
	{
		Log.d(_logTag, "getGameDataStart()");
		_getGameData = new ArrayList<String>();
	}

	public static void getGameDataKey(String entryKey)
	{
		Log.d(_logTag, "getGameDataKey(" + entryKey + ")");
		_getGameData.add(entryKey);
	}

	public static void getGameDataEnd(String key, String fuseId)
	{
		Log.d(_logTag, "getGameDataEnd(" + key + "," + fuseId + ")");

		if (fuseId != null && !fuseId.equals(""))
		{
			if (key != null && !key.equals(""))
			{
				FuseAPI.getFriendGameData(key, _getGameData, _gameDataCallback, fuseId);
			}
			else
			{
				FuseAPI.getFriendGameData(_getGameData, _gameDataCallback, fuseId, false);
			}
		}
		else
		{
			if (key != null && !key.equals(""))
			{
				FuseAPI.getGameData(key, _getGameData, _gameDataCallback);
			}
			else
			{
				FuseAPI.getGameData(_getGameData, _gameDataCallback, false);
			}
		}

		_getGameData = null;
	}



// +-------------+
// | Friend List |
// +-------------+

	public static void updateFriendsListFromServer()
	{
		Log.d(_logTag, "updateFriendsListFromServer()");
		FuseAPI.updateFriendsListFromServer(_gameDataCallback);
	}

	public static String getFriendsList()
	{
		Log.d(_logTag, "getFriendsList()");
		List<Player> friendsList = FuseAPI.getFriendsList();

		String returnValue = "";

		for (Player friend : friendsList)
		{
			returnValue += MakeReturnComponent(friend.getFuseId());
			returnValue += MakeReturnComponent("" /* friend.getAccountId() */); // TODO Return the account ID
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
		Log.d(_logTag, "getMailListFromServer()");
		FuseAPI.getMailListFromServer(_gameDataCallback);
	}

	public static void getMailListFriendFromServer(String fuseId)
	{
		Log.d(_logTag, "getMailListFriendFromServer(" + fuseId + ")");
		FuseAPI.getMailListFriendFromServer(fuseId, _gameDataCallback);
	}

	public static String getMailList(String fuseId)
	{
		Log.d(_logTag, "getMailList(" + fuseId + ")");
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
		Log.d(_logTag, "setMailAsReceived(" + messageId + ")");
		FuseAPI.setMailAsReceived(messageId);
	}

	public static void sendMailWithGift(String fuseId, String message, int giftId, int giftAmount)
	{
		Log.d(_logTag, "sendMailWithGift(" + fuseId + "," + message + "," + giftId + "," + giftAmount + ")");
		FuseAPI.sendMailWithGift(fuseId, message, giftId, giftAmount, _gameDataCallback);
	}

	public static void sendMail(String fuseId, String message)
	{
		Log.d(_logTag, "sendMailWithGift(" + fuseId + "," + message + ")");
		FuseAPI.sendMail(fuseId, message, _gameDataCallback);
	}



// +-------------------------+
// | Game Configuration Data |
// +-------------------------+

	public static String getGameConfigurationValue(String key)
	{
		Log.d(_logTag, "getGameConfigurationValue(" + key + ") = " + FuseAPI.getGameConfigurationValue(key));
		return FuseAPI.getGameConfigurationValue(key);
	}



// +--------------------+
// | API bridge helpers |
// +--------------------+

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

	private static final String _logTag = "FuseUnityAPI";
	private static UnityPlayerActivity _this;
	private static FuseUnityGameDataCallback _gameDataCallback;
	private static FuseUnityAdCallback _adCallback;
	private static boolean _sessionStarted = false;
}
