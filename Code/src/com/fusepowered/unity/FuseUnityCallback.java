package com.fusepowered.unity;

import java.util.ArrayList;

import android.util.Log;
import android.util.Base64;
import com.unity3d.player.UnityPlayer;
import com.fusepowered.*;
import com.fusepowered.util.*;

public class FuseUnityCallback implements FuseSDKListenerWithAdClick
{
	private static final String _logTag = "FuseUnityCallback";

// +------------------+
// | Session Creation |
// +------------------+

	public void sessionStartReceived()
	{
		FuseUnitySDK.SendMessage("_SessionStartReceived", "");
	}
	
	public void sessionLoginError(final FuseError error)
	{
		FuseUnitySDK.SendMessage("_SessionLoginError", Integer.toString(error.errorCode));
	}

    public void didRecieveGCMRegistrationToken(String newRegistrationID)
	{
	}
	

// +---------------+
// |      IAP      |
// +---------------+
    public void purchaseVerification(final int verified, final String transaction_id, final String originalTransactionID)
    {
        FuseUnitySDK.SendMessage("_PurchaseVerification", Integer.toString(verified) + "," + transaction_id + "," + originalTransactionID);
    }

	
// +---------------+
// |      Ads      |
// +---------------+

	public void adAvailabilityResponse(final boolean available, final int error)
	{
		FuseUnitySDK.SendMessage("_AdAvailabilityResponse", (available ? "1" : "0") + "," + Integer.toString(error));
	}

	public void adWillClose()
	{
		FuseUnitySDK.SendMessage("_AdWillClose", "");
	}

	public void adFailedToDisplay()
	{
		FuseUnitySDK.SendMessage("_AdFailedToDisplay", "");
	}

	public void handleAdClickWithUrl(final String url)
	{
		FuseUnitySDK.SendMessage("_AdClickedWithURL", url);
	}


// +---------------+
// | Rewarded Ads  |
// +---------------+

    public void rewardedAdCompleteWithObject(RewardedInfo rewardInfo)
    {
		if(rewardInfo == null)
		{
			FuseUnitySDK.SendMessage("_RewardedAdCompleted", "");
			return;
		}

		String pre = rewardInfo.preRollMessage == null ? "" : Base64.encodeToString(rewardInfo.preRollMessage.getBytes(), Base64.NO_WRAP);
		String post = rewardInfo.rewardMessage == null ? "" : Base64.encodeToString(rewardInfo.rewardMessage.getBytes(), Base64.NO_WRAP);
		String item = rewardInfo.rewardItem == null ? "" : Base64.encodeToString(rewardInfo.rewardItem.getBytes(), Base64.NO_WRAP);
		FuseUnitySDK.SendMessage("_RewardedAdCompleted", pre + "," + post + "," + item + "," + rewardInfo.rewardAmount);
    }


// +---------------+
// |     Offers    |
// +---------------+

    public void IAPOfferAcceptedWithObject(IAPOfferInfo offer)
    {
		if(offer == null)
		{
			FuseUnitySDK.SendMessage("_IAPOfferAccepted", "");
			return;
		}
		
		String id = offer.productID == null ? "" : Base64.encodeToString(offer.productID.getBytes(), Base64.NO_WRAP);
		String name = offer.itemName == null ? "" : Base64.encodeToString(offer.itemName.getBytes(), Base64.NO_WRAP);
		FuseUnitySDK.SendMessage("_IAPOfferAccepted", id + "," + offer.productPrice + "," + name + "," + offer.itemAmount);
    }

    public void virtualGoodsOfferAcceptedWithObject(VGOfferInfo offer)
    {
		if(offer == null)
		{
			FuseUnitySDK.SendMessage("_VirtualGoodsOfferAccepted", "");
			return;
		}
		
		String currency = offer.purchaseCurrency == null ? "" : Base64.encodeToString(offer.purchaseCurrency.getBytes(), Base64.NO_WRAP);
		String name = offer.itemName == null ? "" : Base64.encodeToString(offer.itemName.getBytes(), Base64.NO_WRAP);
		FuseUnitySDK.SendMessage("_VirtualGoodsOfferAccepted", currency + "," + offer.purchasePrice + "," + name + "," + offer.itemAmount);
    }


// +---------------+
// | Notifications |
// +---------------+
	
	public void notificationAction(final String action)
	{
		FuseUnitySDK.SendMessage("_NotificationAction", action);
	}

    public void notificationWillClose()
	{
		FuseUnitySDK.SendMessage("_NotificationWillClose", "");
	}



// +---------------+
// | Account Login |
// +---------------+

	public void accountLoginComplete(final int accountType, final String accountId)
	{
		FuseUnitySDK.SendMessage("_AccountLoginComplete", Integer.toString(accountType) + "," + accountId);
	}

    public void accountLoginError(final String accountId, final FuseError error)
	{
		FuseUnitySDK.SendMessage("_AccountLoginError", accountId + "," + error);
	}

	
// +---------------+
// | Miscellaneous |
// +---------------+

	public void timeUpdated(final int timestamp)
	{
		FuseUnitySDK.SendMessage("_TimeUpdated", Integer.toString(timestamp));
	}


// +-------------+
// | Friend List |
// +-------------+

	public void friendAdded(final String fuseId, final FuseError error)
    {
        FuseUnitySDK.SendMessage("_FriendAdded", fuseId + "," + Integer.toString(error.errorCode));
    }

    public void friendRemoved(final String fuseId, final FuseError error)
    {
        FuseUnitySDK.SendMessage("_FriendRemoved", fuseId + "," + Integer.toString(error.errorCode));
    }

    public void friendAccepted(final String fuseId, final FuseError error)
    {
        FuseUnitySDK.SendMessage("_FriendAccepted", fuseId + "," + Integer.toString(error.errorCode));
    }

    public void friendRejected(final String fuseId, final FuseError error)
    {
        FuseUnitySDK.SendMessage("_FriendRejected", fuseId + "," + Integer.toString(error.errorCode));
    }

    public void friendsMigrated(final String fuseId, final FuseError error)
    {
        FuseUnitySDK.SendMessage("_FriendsMigrated",  fuseId + "," + Integer.toString(error.errorCode));
    }

	public void friendsListUpdated(ArrayList<Player> friendsList)
	{
		FuseUnitySDK.SendMessage("_FriendsListUpdated", "");
	}

	public void friendsListError(final FuseError error)
	{
		FuseUnitySDK.SendMessage("_FriendsListError", Integer.toString(error.errorCode));
	}


// +-------------------------+
// | Game Configuration Data |
// +-------------------------+

	public void gameConfigurationReceived()
	{
		FuseUnitySDK.SendMessage("_GameConfigurationReceived", "");
	}
}
