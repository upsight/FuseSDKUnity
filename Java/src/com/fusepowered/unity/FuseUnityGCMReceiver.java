package com.fusepowered.unity;

import android.content.Context;
import com.google.android.gcm.GCMBroadcastReceiver;
//import static com.google.android.gcm.GCMConstants.DEFAULT_INTENT_SERVICE_CLASS_NAME;

public class FuseUnityGCMReceiver extends GCMBroadcastReceiver
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
