package com.fusepowered.unity;

import android.content.Context;
import com.fusepowered.push.FuseGCMBroadcastReceiver;

public class FuseUnityGCMReceiver extends FuseGCMBroadcastReceiver
{

	
    protected String getGCMIntentServiceClassName(Context context)
    {
        return getDefaultIntentServiceClassName(context);
    }

    /**
     * Gets the default class name of the intent service that will handle GCM
     * messages.
     */

    
    static final String getDefaultIntentServiceClassName(Context context)
    {
        return "com.fusepowered.unity.GCMIntentService";
    }
    
    
}
