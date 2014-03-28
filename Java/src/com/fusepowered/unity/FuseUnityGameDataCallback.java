package com.fusepowered.unity;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

import com.fusepowered.util.FuseAttackErrors;
import com.fusepowered.util.FuseEnemiesListError;
import com.fusepowered.util.FuseFriendsListError;
import com.fusepowered.util.FuseGameDataCallback;
import com.fusepowered.util.FuseGameDataError;
import com.fusepowered.util.FuseMailError;
import com.fusepowered.util.GameKeyValuePairs;
import com.fusepowered.util.GameValue;
import com.fusepowered.util.Mail;
import com.fusepowered.util.Player;
import com.fusepowered.util.UserTransactionLog;
import com.fusepowered.util.FuseMigrateFriendsError;
import com.fusepowered.util.FuseAddFriendError;
import com.fusepowered.util.FuseRemoveFriendError;
import com.fusepowered.util.FuseAcceptFriendError;
import com.fusepowered.util.FuseRejectFriendError;

import android.util.Log;

public class FuseUnityGameDataCallback extends FuseGameDataCallback
{
	public FuseUnityGameDataCallback()
	{
	}

	public void callback()
	{
		//Log.d(_logTag, "callback()");
	}



// +------------------+
// | Session Creation |
// +------------------+

	public void sessionStartReceived()
	{
		//Log.d(_logTag, "sessionStartReceived()");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_SessionStartReceived", "");
	}
	
	public void sessionLoginError(int error)
	{
		Log.d(_logTag, "sessionLoginError(" + error + ")");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_SessionLoginError", Integer.toString(error));
	}



// +---------------+
// | Notifications |
// +---------------+
	
	public void notificationAction(final String action)
	{
		//Log.d(_logTag, "notificationAction(" + action + ")");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_NotificationAction", action);
	}



// +---------------+
// | Account Login |
// +---------------+

	public void accountLoginComplete(final int accountType, final String accountId)
	{		
		synchronized(_sync)
		{
			//Log.d(_logTag, "START: accountLoginComplete(" + accountType + "," + accountId + ")");
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", Integer.toString(accountType));
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_AccountLoginComplete",         accountId);
			//Log.d(_logTag, "END: accountLoginComplete(" + accountType + "," + accountId + ")");
		}
	}



// +---------------+
// | Miscellaneous |
// +---------------+

	public void timeUpdated(final int timestamp)
	{
		//Log.d(_logTag, "timeUpdated(" + timestamp + ")");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_TimeUpdated", Integer.toString(timestamp));
	}



// +----------------+
// | User Game Data |
// +----------------+

	public void gameDataReceived(String accountId, GameKeyValuePairs gameKeyValuePairs)
	{
		gameDataReceived(accountId, gameKeyValuePairs, -1);
	}

	public void gameDataReceived(String accountId, GameKeyValuePairs gameKeyValuePairs, int requestId)
	{
		synchronized(_sync)
		{
			//Log.d(_logTag, "START: gameDataReceived(" + accountId + ",[data]," + requestId + ")");

			FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", accountId);

			HashMap<String,GameValue> hashMap = gameKeyValuePairs.getMap();
			for (Map.Entry<String,GameValue> entry : hashMap.entrySet())
			{
				boolean isBinary = entry.getValue().isBinary();
				String key = entry.getKey();
				String value = entry.getValue().getValue();
	
				FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", isBinary ? "1" : "0");
				FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", key);
				FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", value);
			}

			FuseUnityAPI.SendMessage("FuseAPI_Android", "_GameDataReceived", Integer.toString(requestId)); 

			//Log.d(_logTag, "END: gameDataReceived(" + accountId + ",[data]," + requestId + ")");
		}
	}

	public void gameDataError(FuseGameDataError fuseGameDataError)
	{
		gameDataError(fuseGameDataError, -1);
	}

	public void gameDataError(FuseGameDataError fuseGameDataError, int requestId)
	{
		synchronized(_sync)
		{
			//Log.d(_logTag, "START: gameDataError(" + fuseGameDataError + "," + requestId + ")");
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", Integer.toString(requestId));
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_GameDataError",                Integer.toString(fuseGameDataError.ordinal()));
			//Log.d(_logTag, "END: gameDataError(" + fuseGameDataError + "," + requestId + ")");
		}
	}

	public void gameDataSetAcknowledged(int requestId)
	{
		//Log.d(_logTag, "gameDataSetAcknowledged(" + requestId + ")");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_GameDataSetAcknowledged", Integer.toString(requestId));
	}



// +-------------+
// | Friend List |
// +-------------+

	public void friendAdded(final String fuse_id, final FuseAddFriendError error)
    {
       // Log.d(_logTag, "friendAdded: " + fuseId + ", " + migrateFriendsError);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentList", "");
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", fuseId);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(error.getErrorCode()));
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_FriendAdded", "");
    }

    public void friendRemoved(final String fuse_id, final FuseRemoveFriendError error)
    {
       // Log.d(_logTag, "friendRemoved: " + fuseId + ", " + migrateFriendsError);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentList", "");
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", fuseId);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(error.getErrorCode()));
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_FriendRemoved", "");
    }

    public void friendAccepted(final String fuse_id, final FuseAcceptFriendError error)
    {
       // Log.d(_logTag, "friendAccepted: " + fuseId + ", " + migrateFriendsError);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentList", "");
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", fuseId);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(error.getErrorCode()));
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_FriendAccepted", "");
    }

    public void friendRejected(final String fuse_id, final FuseRejectFriendError error)
    {
       // Log.d(_logTag, "friendRejected: " + fuseId + ", " + migrateFriendsError);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentList", "");
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", fuseId);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(error.getErrorCode()));
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_FriendRejected", "");
    }

    public void friendsMigrated(String fuseId, FuseMigrateFriendsError migrateFriendsError)
    {
       // Log.d(_logTag, "friendsMigrated: " + fuseId + ", " + migrateFriendsError);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentList", "");
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", fuseId);
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(migrateFriendsError.getErrorCode()));
        FuseUnityAPI.SendMessage("FuseAPI_Android", "_FriendsMigrated", "");
    }

	public void friendsListUpdated(ArrayList<Player> friendsList)
	{
		synchronized(_sync)
		{
			//Log.d(_logTag, "START: friendsListUpdated([data])");

			FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentList", "");

			for (Player friend : friendsList)
			{
				FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", friend.getFuseId());
				FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", friend.getAccountId()); 
				FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", friend.getAlias());
				FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(friend.getPending()));
			}

			FuseUnityAPI.SendMessage("FuseAPI_Android", "_FriendsListUpdated", "");

			//Log.d(_logTag, "END: friendsListUpdated([data])");
		}
	}

	public void friendsListError(FuseFriendsListError fuseFriendsListError)
	{
		//Log.d(_logTag, "friendsListError(" + fuseFriendsListError + ")");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_FriendsListError", Integer.toString(fuseFriendsListError.ordinal()));
	}



// +---------+
// | Gifting |
// +---------+

	public void mailListReceived(ArrayList<Mail> mailList, String fuseId)
	{
		synchronized(_sync)
		{
			//Log.d(_logTag, "START: mailListReceived([data]," + fuseId + ")");

			FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentList", "");

			if (mailList != null)
			{
				for (Mail mail : mailList)
				{
					FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(mail.getId()));
					FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", mail.getDate());
					FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", mail.getAlias());
					FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", mail.getMessage());
					if( mail.getGift() != null )
					{
						FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(mail.getGift().getId()));
						FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", mail.getGift().getName());
						FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(mail.getGift().getAmount()));
					}
					else
					{
						// No Gift
						FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", "0");
						FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", "");
						FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", "0");
					}
				}
			}

			FuseUnityAPI.SendMessage("FuseAPI_Android", "_MailListReceived", fuseId);

			//Log.d(_logTag, "END: mailListReceived([data]," + fuseId + ")");
		}
	}

	public void mailListError(FuseMailError fuseMailError)
	{
		//Log.d(_logTag, "mailListError(" + fuseMailError + ")");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_MailListError", Integer.toString(fuseMailError.ordinal()));
	}

	public void mailAcknowledged(int messageId, String fuseId, int requestID)
	{
		synchronized(_sync)
		{
			//Log.d(_logTag, "START: mailAcknowledged(" + messageId + "," + fuseId + "," + requestID + ")");
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_ClearArgumentListAndSetFirst", Integer.toString(messageId));
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", fuseId);
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_AddArgument", Integer.toString(requestID));
			FuseUnityAPI.SendMessage("FuseAPI_Android", "_MailAcknowledged", "");
			//Log.d(_logTag, "END: mailAcknowledged(" + messageId + "," + fuseId + "," + requestID + ")");
		}
	}

	public void mailError(FuseMailError fuseMailError)
	{
		Log.d(_logTag, "mailError(" + fuseMailError + ")");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_MailError", Integer.toString(fuseMailError.ordinal()));
	}



// +-------------------------+
// | Game Configuration Data |
// +-------------------------+

	public void gameConfigurationReceived()
	{
		//Log.d(_logTag, "gameConfigurationReceived()");
		FuseUnityAPI.SendMessage("FuseAPI_Android", "_GameConfigurationReceived", "");
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
	private static int _nextRequestId = 1;
	private static Object _sync = new Object();
}
