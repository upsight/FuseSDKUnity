package com.fusepowered.unity;

import android.util.Log;
import com.fusepowered.util.FuseAdCallback;
import com.unity3d.player.UnityPlayer;

public class FuseUnityAdCallback extends FuseAdCallback
{
	public void adAvailabilityResponse(int available, int error)
	{
		//Log.d(_logTag, "adAvailabilityResponse(" + available + "," + error + ")");
		UnityPlayer.UnitySendMessage(FuseUnityAPI.callbackObj, "_ClearArgumentListAndSetFirst", Integer.toString(error));
		UnityPlayer.UnitySendMessage(FuseUnityAPI.callbackObj, "_AdAvailabilityResponse",       Integer.toString(available));
	}

	public void adDisplayed()
	{
		//Log.d(_logTag, "adDisplayed()");
		UnityPlayer.UnitySendMessage(FuseUnityAPI.callbackObj, "_AdDisplayed", "");
	}

	public void adClicked()
	{
		//Log.d(_logTag, "adClicked()");
		UnityPlayer.UnitySendMessage(FuseUnityAPI.callbackObj, "_AdClicked", "");
	}

	public void adWillClose()
	{
		//Log.d(_logTag, "adWillClose()");
		UnityPlayer.UnitySendMessage(FuseUnityAPI.callbackObj, "_AdWillClose", "");
	}

    public void adFailedToLoad()
    {
        //TODO
    }

    public void adWillLeaveApp()
    {
        //TODO
    }
    
    public void videoCompleted(String adZone)
    {
		//Log.d(_logTag, "videoCompleted()");
		UnityPlayer.UnitySendMessage(FuseUnityAPI.callbackObj, "_VideoCompleted", adZone);
    }
    
    public void rewardedVideoCompleted(String adZone)
    {
		//Log.d(_logTag, "rewardedVideoCompleted()");
		UnityPlayer.UnitySendMessage(FuseUnityAPI.callbackObj, "_RewardedVideoCompleted", adZone);
    }

	private static final String _logTag = "FuseUnityAdCallback";
}
