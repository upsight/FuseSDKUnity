package com.fusepowered.unity;

import java.util.ArrayList;
import java.util.Currency;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;

import android.content.Intent;
import android.util.Base64;

import com.unity3d.player.UnityPlayer;

import com.fusepowered.*;
import com.fusepowered.fuseapi.*;
import com.fusepowered.util.Player;
import com.fusepowered.util.VerifiedPurchase;
import com.fusepowered.util.Gender;

public class FuseUnitySDK
{	
	private static final String _logTag = "FuseUnitySDK";

	private static String callbackObj = "FuseSDK";
	private static boolean _sessionStarted = false;

	
	public static void onDestroy()
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.endSession();
				_sessionStarted = false;
			}
		});
	}
	
	public static void onPause()
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.pauseSession();
			}
		});
	}
	
	public static void onResume()
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.resumeSession(UnityPlayer.currentActivity);
			}
		});
	}
	



// +------------------+
// | Session Creation |
// +------------------+

	public static void startSession(final String gameId, final boolean handleAdURLs)
	{
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				HashMap<String, String> options = new HashMap<String, String>();
				options.put(FuseSDKConstants.FuseConfigurationKey_HandleAdUrls, Boolean.toString(handleAdURLs));

				FuseSDK.setPlatform("unity-android");
				FuseSDK.startSession(gameId, UnityPlayer.currentActivity, new FuseUnityCallback(), options);
				_sessionStarted = true;
			}
		});
	}
	
	public static void registerForPushNotifications(final String projectID)
	{
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				Intent forGCM = new Intent(UnityPlayer.currentActivity.getApplicationContext(), FuseUnitySDK.class);
				FuseSDK.setupPushNotifications(projectID, forGCM, 0, 0);
			}
		});
	}

// +-----------------+
// | Analytics Event |
// +-----------------+
	
	public static boolean registerEvent(final String message, final String paramName, final String paramValue, String[] dataKeys, double[] dataValues)
	{
		if(!_sessionStarted)
			return false;
		
		HashMap<String, Number> data = new HashMap<String, Number>();
		try
		{
			if(dataKeys == null || dataValues == null || dataKeys.length == 0 || dataValues.length == 0)
			{
				data = null;
			}
			else
			{
				for( int i = 0; i < dataKeys.length; i++ )
				{
					data.put(dataKeys[i], new Double(dataValues[i]));
				}
			}
		}
		catch(Exception e)
		{
			return false;
		}
		
		final HashMap<String, Number> finalData = data;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseAPI.registerEvent(message, paramName, paramValue, finalData);
			}
		});
		return true;
	}
	
	public static boolean registerEvent(final String name, final String paramName, final String paramValue, final String variableName, final double variableValue)
	{
		if(!_sessionStarted)
			return false;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseAPI.registerEvent(name, paramName, paramValue, variableName, (variableName == null || variableName.compareTo("") == 0) ? null : new Double(variableValue));
			}
		});
		return true;
	}


// +-------------------------+
// | In-App Purchase Logging |
// +-------------------------+

	public static void registerVirtualGoodsPurchase(final int virtualgoodID, final int currencyAmount, final int currencyID)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.registerVirtualGoodsPurchase(virtualgoodID, currencyAmount, currencyID);
			}
		});
	}

	public static void registerInAppPurchase(String purchaseState, String purchaseToken, String productId, String orderId, long purchaseTime, String developerPayload, final double price, String currency)
	{
		if(!_sessionStarted)
			return;

		// If we haven't been passed a currency string, make a guess based on the current locale
		if(currency == null || currency.length() == 0) 
		{
			Locale locale = Locale.getDefault();

			if(locale != null)
			{
				Currency c = Currency.getInstance(locale);

				if(c != null)
				{
					currency = c.getCurrencyCode();
				}
			}
		}
		
		final String cur = currency;
		final VerifiedPurchase purchase = new VerifiedPurchase(purchaseState, purchaseToken, productId, orderId, purchaseTime, developerPayload);
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.registerInAppPurchase(purchase, price, cur);
			}
		});
	}



// +-----------------------+
// | Fuse Interstitial Ads |
// +-----------------------+

	public static boolean isAdAvailableForZoneID(final String zoneId)
	{
		if(!_sessionStarted)
			return false;

		return FuseSDK.isAdAvailableForZoneID(zoneId);
	}

	public static boolean zoneHasRewarded(final String zoneId)
	{
		if(!_sessionStarted)
			return false;

		return FuseSDK.zoneHasRewarded(zoneId);
	}

	public static boolean zoneHasIAPOffer(final String zoneId)
	{
		if(!_sessionStarted)
			return false;

		return FuseSDK.zoneHasIAPOffer(zoneId);
	}

	public static boolean zoneHasVirtualGoodsOffer(final String zoneId)
	{
		if(!_sessionStarted)
			return false;

		return FuseSDK.zoneHasVirtualGoodsOffer(zoneId);
	}

	public static String getRewardedInfoForZoneID(final String zoneId)
	{
		if(!_sessionStarted)
			return "";

		RewardedInfo reward = FuseSDK.getRewardedInfoForZoneID(zoneId);
		String preRoll = Base64.encodeToString(reward.preRollMessage.getBytes(), Base64.NO_WRAP);
		String postRoll = Base64.encodeToString(reward.rewardMessage.getBytes(), Base64.NO_WRAP);
		String rewardItem = Base64.encodeToString(reward.rewardItem.getBytes(), Base64.NO_WRAP);
		return preRoll + "," + postRoll + "," + rewardItem + "," + reward.rewardAmount;
	}

	public static void showAdForZoneID(final String zoneId, String[] optionsKeys, String[] optionValues)
	{
		if(!_sessionStarted)
			return;

		final HashMap<String, String> options = new HashMap<String, String>();
		for( int i = 0; i < optionsKeys.length; i++ )
		{
			options.put(optionsKeys[i], optionValues[i]);
		}

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.showAdForZoneID(zoneId, options);
			}
		});
	}

	public static void preloadAdForZoneID(final String zoneId)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.preloadAdForZoneID(zoneId);
			}
		});
	}

	public static void displayMoreGames()
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.displayMoreGames();
			}
		});
	}


// +---------------+
// | Notifications |
// +---------------+

	public static boolean isNotificationAvailable()
	{
		if(!_sessionStarted)
			return false;

		return FuseSDK.isNotificationAvailable();
	}

	public static void displayNotifications()
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.displayNotifications();
			}
		});
	}


// +-----------+
// | User Info |
// +-----------+

	public static void registerGender(final int gender)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.registerGender(Gender.getGenderByCode(gender));
			}
		});
	}

	public static void registerAge(final int age)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.registerAge(age);
			}
		});
	}

	public static void registerBirthday(final int year, final int month, final int day)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.registerBirthday(year, month, day);
			}
		});
	}
		
	public static void registerLevel(final int level)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.registerLevel(level);
			}
		});
	}
	
	public static boolean registerCurrency(final int type, final int balance)
	{
		if(!_sessionStarted)
			return false;

		return FuseSDK.registerCurrency(type, balance);
	}
	
	public static void registerParentalConsent(final boolean consentGranted)
	{
		if(!_sessionStarted)
			return;
			
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.registerParentalConsent(consentGranted);
			}
		});
	}

	public static boolean registerCustomEventString(final int eventNumber, final String value)
	{
		if(!_sessionStarted)
			return false;
			
		return FuseSDK.registerCustomEvent(eventNumber, value);
	}

	public static boolean registerCustomEventInt(final int eventNumber, final int value)
	{
		if(!_sessionStarted)
			return false;
			
		return FuseSDK.registerCustomEvent(eventNumber, value);
	}


// +---------------+
// | Account Login |
// +---------------+
	
	public static String getFuseID()
	{
		if(!_sessionStarted)
			return "";

		return FuseSDK.getFuseID();
	}

	public static String getOriginalAccountId()
	{
		if(!_sessionStarted)
			return "";

		return FuseSDK.getOriginalAccountID();
	}

	public static int getOriginalAccountType()
	{
		if(!_sessionStarted)
			return -1;

		return FuseSDK.getOriginalAccountType().getAccountNumber();
	}
	
	public static String getOriginalAccountAlias()
	{
		if(!_sessionStarted)
			return "";

		return FuseSDK.getOriginalAccountAlias();
	}

	public static void facebookLogin(final String facebookId, final String name, final String accessToken)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.facebookLogin(facebookId, name, accessToken);
			}
		});
	}

	public static void twitterLogin(final String twitterId, final String alias)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.twitterLogin(twitterId, alias);
			}
		});
	}

	public static void fuseLogin(final String fuseId, final String alias)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.fuseLogin(fuseId, alias);
			}
		});
	}

	public static void emailLogin(final String email, final String alias)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.emailLogin(email, alias);
			}
		});
	}
	
	public static void deviceLogin(final String alias)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.deviceLogin(alias);
			}
		});
	}
	
	public static void googlePlayLogin(final String alias, final String token)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.googlePlayLogin(alias, token);
			}
		});
	}


// +---------------+
// | Miscellaneous |
// +---------------+
		
	public static int gamesPlayed()
	{
		if(!_sessionStarted)
			return -1;

		return FuseSDK.gamesPlayed();
	}

	public static String libraryVersion()
	{
		return FuseSDK.libraryVersion();
	}

	public static boolean connected()
	{
		return FuseSDK.connected();
	}

	public static void utcTimeFromServer()
	{
		if(!_sessionStarted)
			return;

		FuseSDK.utcTimeFromServer();
	}


// +-----------------+
// | Data Opt In/Out |
// +-----------------+
			
	public static void disableData()
	{
		if(!_sessionStarted)
			return;

		FuseSDK.disableData();
	}

	public static void enableData()
	{
		if(!_sessionStarted)
			return;

		FuseSDK.disableData();
	}

	public static boolean dataEnabled()
	{
		if(!_sessionStarted)
			return false;

		return FuseSDK.dataEnabled();
	}


// +-------------+
// | Friend List |
// +-------------+

	public static void updateFriendsListFromServer()
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.updateFriendsListFromServer();
			}
		});
	}

	public static String[] getFriendsList()
	{
		if(!_sessionStarted)
			return new String[0];

		List<Player> friendsList = FuseSDK.getFriendsList();
		if(friendsList == null)
		{
			return new String[0];
		}

		String[] returnValue = new String[friendsList.size()];

		for(int i = 0; i < returnValue.length; i++)
		{
			Player friend = friendsList.get(i);
			StringBuilder sb = new StringBuilder();

			sb.append(friend.getFuseId()).append(",")
				.append(friend.getAccountId()).append(",")
				.append(friend.getAlias()).append(",")
				.append(friend.getPending());

			returnValue[i] = sb.toString();
		}
		return returnValue;
	}

	public static void addFriend(String fuseId)
	{
		if(!_sessionStarted)
			return;

		FuseSDK.addFriend(fuseId);
	}

	public static void removeFriend(String fuseId)
	{
		if(!_sessionStarted)
			return;

		FuseSDK.removeFriend(fuseId);
	}

	public static void acceptFriend(String fuseId)
	{
		if(!_sessionStarted)
			return;

		FuseSDK.acceptFriend(fuseId);
	}

	public static void rejectFriend(String fuseId)
	{
		if(!_sessionStarted)
			return;

		FuseSDK.rejectFriend(fuseId);
	}	

	public static void migrateFriends(String fuseId)
	{
		if(!_sessionStarted)
			return;

		FuseSDK.migrateFriends(fuseId);
	}
	

// +------------------------+
// | P2P Push Notifications |
// +------------------------+

	public static void userPushNotification(final String friendID, final String message)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.userPushNotification(friendID, message);
			}
		});
	}

	public static void friendsPushNotification(final String message)
	{
		if(!_sessionStarted)
			return;

		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				FuseSDK.friendsPushNotification(message);
			}
		});
	}



// +-------------------------+
// | Game Configuration Data |
// +-------------------------+

	public static String getGameConfigurationValue(String key)
	{
		if(!_sessionStarted)
			return "";

		return FuseSDK.getGameConfigurationValue(key);
	}
	
	public static String[] getGameConfigurationKeys()
	{
		if(!_sessionStarted)
			return new String[0];

		HashMap<String, String> gameConfig = FuseSDK.getGameConfiguration();
		if(gameConfig == null || gameConfig.size() == 0)
		{
			return new String[0];
		}

		String[] keys = gameConfig.keySet().toArray(new String[0]);
		if( keys == null || keys.length == 0 )
		{
			return new String[0];
		}

		return keys;
	}



// +--------------------+
// | API bridge helpers |
// +--------------------+

	public static void SetGameObjectCallback(String gameObject)
	{
		callbackObj = gameObject;
	}

	public static void SendMessage(final String methodName, final String message)
	{
		UnityPlayer.UnitySendMessage(callbackObj, methodName, message == null ? "" : message);
	}
}
