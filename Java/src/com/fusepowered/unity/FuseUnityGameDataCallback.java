package com.fusepowered.unity;

import com.fusepowered.util.FuseGameDataCallback;
import com.fusepowered.util.GameKeyValuePairs;
import com.fusepowered.util.GameValue;
import com.fusepowered.util.FuseAttackErrors;
import com.fusepowered.util.FuseEnemiesListError;
import com.fusepowered.util.FuseGameDataError;
import com.fusepowered.util.FuseFriendsListError;
import com.fusepowered.util.FuseMailError;
import com.fusepowered.util.UserTransactionLog;
import com.fusepowered.util.Player;
import com.fusepowered.util.Mail;
import java.util.Map;
import java.util.HashMap;
import java.util.ArrayList;
import com.unity3d.player.UnityPlayer;
import android.util.Log;

public class FuseUnityGameDataCallback extends FuseGameDataCallback
{
	public void callback()
	{
		Log.d(_logTag, "callback()");
	}



// +------------------+
// | Session Creation |
// +------------------+

	public void sessionStartReceived()
	{
		Log.d(_logTag, "sessionStartReceived()");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_SessionStartReceived", "");
	}
	
	public void sessionLoginError(int error)
	{
		Log.d(_logTag, "sessionLoginError(" + error + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_SessionLoginError", Integer.toString(error));
	}



// +---------------+
// | Notifications |
// +---------------+
	
	public void notificationAction(final String action)
	{
		Log.d(_logTag, "notificationAction(" + action + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_NotificationAction", action);
	}



// +---------------+
// | Account Login |
// +---------------+

	public void accountLoginComplete(final int accountType, final String accountId)
	{
		Log.d(_logTag, "accountLoginComplete(" + accountType + "," + accountId + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", Integer.toString(accountType));
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AccountLoginComplete",         accountId);
	}



// +---------------+
// | Miscellaneous |
// +---------------+

	public void timeUpdated(final int timestamp)
	{
		Log.d(_logTag, "timeUpdated(" + timestamp + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_TimeUpdated", Integer.toString(timestamp));
	}



// +----------------+
// | User Game Data |
// +----------------+

	public void gameDataReceived(String accountId, GameKeyValuePairs gameKeyValuePairs)
	{
		Log.d(_logTag, "gameDataReceived(" + accountId + ",[data])");

		gameDataReceived(accountId, gameKeyValuePairs, 0);
	}

	public void gameDataReceived(String accountId, GameKeyValuePairs gameKeyValuePairs, int requestId)
	{
		Log.d(_logTag, "gameDataReceived(" + accountId + ",[data]," + requestId + ")");

		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", accountId);

		HashMap<String,GameValue> hashMap = gameKeyValuePairs.getMap();
		for (Map.Entry<String,GameValue> entry : hashMap.entrySet())
		{
			boolean isBinary = entry.getValue().isBinary();
			String key = entry.getKey();
			String value = entry.getValue().getValue();

			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", isBinary ? "1" : "0");
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", key);
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", value);
		}

		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_GameDataReceived", Integer.toString(requestId));
	}

	public void gameDataError(FuseGameDataError fuseGameDataError)
	{
		Log.d(_logTag, "gameDataError(" + fuseGameDataError + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_ClearArgumentList", "");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_GameDataError", Integer.toString(fuseGameDataError.ordinal()));
	}

	public void gameDataError(FuseGameDataError fuseGameDataError, int requestId)
	{
		Log.d(_logTag, "gameDataError(" + fuseGameDataError + "," + requestId + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", Integer.toString(requestId));
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_GameDataError",                Integer.toString(fuseGameDataError.ordinal()));
	}

	public void gameDataSetAcknowledged(int requestId)
	{
		Log.d(_logTag, "gameDataSetAcknowledged(" + requestId + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_GameDataSetAcknowledged", Integer.toString(requestId));
	}



// +-------------+
// | Friend List |
// +-------------+

	public void friendsListUpdated(ArrayList<Player> friendsList)
	{
		Log.d(_logTag, "friendsListUpdated([data])");

		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_ClearArgumentList", "");

		for (Player friend : friendsList)
		{
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", friend.getFuseId());
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", "" /* friend.getAccountId() */); // TODO Return the account ID
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", friend.getAlias());
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(friend.getPending()));
		}

		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_FriendsListUpdated", "");
	}

	public void friendsListError(FuseFriendsListError fuseFriendsListError)
	{
		Log.d(_logTag, "friendsListError(" + fuseFriendsListError + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_FriendsListError", Integer.toString(fuseFriendsListError.ordinal()));
	}



// +---------+
// | Gifting |
// +---------+

	public void mailListReceived(ArrayList<Mail> mailList, String fuseId)
	{
		Log.d(_logTag, "mailListReceived([data]," + fuseId + ")");

		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_ClearArgumentList", "");

		for (Mail mail : mailList)
		{
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(mail.getId()));
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", mail.getDate());
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", mail.getAlias());
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", mail.getMessage());
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(mail.getGift().getId()));
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", mail.getGift().getName());
			UnityPlayer.UnitySendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(mail.getGift().getAmount()));
		}

		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_MailListReceived", fuseId);
	}

	public void mailListError(FuseMailError fuseMailError)
	{
		Log.d(_logTag, "mailListError(" + fuseMailError + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_MailListError", Integer.toString(fuseMailError.ordinal()));
	}

	public void mailAcknowledged(int messageId, String fuseId)
	{
		Log.d(_logTag, "mailAcknowledged(" + messageId + "," + fuseId + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", Integer.toString(messageId));
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_MailAcknowledged",             fuseId);
	}

	public void mailError(FuseMailError fuseMailError)
	{
		Log.d(_logTag, "mailError(" + fuseMailError + ")");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_MailError", Integer.toString(fuseMailError.ordinal()));
	}



// +-------------------------+
// | Game Configuration Data |
// +-------------------------+

	public void gameConfigurationReceived()
	{
		Log.d(_logTag, "gameConfigurationReceived()");
		UnityPlayer.UnitySendMessage("FuseAPI_Android", "_GameConfigurationReceived", "");
	}




	public void enemiesListError(FuseEnemiesListError a)
	{
	}

	public void enemiesListResult(ArrayList<Player> a)
	{
	}

	public void attackRobberyLogReceived(ArrayList<UserTransactionLog> a)
	{
	}

	public void attackRobberyLogError(FuseAttackErrors a)
	{
	}




	private static final String _logTag = "FuseUnityGameDataCallback";
}
