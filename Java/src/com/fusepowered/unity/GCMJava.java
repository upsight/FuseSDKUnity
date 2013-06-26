package com.fusepowered.unity;

import com.google.android.gcm.GCMRegistrar;
import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

public class GCMJava extends Activity
{
  
    String DISPLAY_MESSAGE_ACTION = "";
    String EXTRA_MESSAGE = "message";
    
    static Boolean isRegistration;
    
    @Override
    public void onCreate(Bundle savedInstance)
    {
        super.onCreate(savedInstance);
        Intent i = getIntent();
        Boolean isRegistration = getIntent().getExtras().getBoolean("isRegistration");
        if(!isRegistration) 
        {
          // Unregister device
           GCMRegistrar.unregister(StaticApplicationContext.getCustomAppContext());
        } 
        else
        {
          // Carry on with registration
          String SENDER_ID = i.getStringExtra("senderID");
          
          // Checking device and manifest dependencies
          GCMRegistrar.checkDevice(this);
          GCMRegistrar.checkManifest(this);
          
          // Get GCM registration id
          final String regId = GCMRegistrar.getRegistrationId(this);
          
          // Check if regid already presents
          if (regId.equals(""))
          {
            // Registration is not present, register now with GCM
            GCMRegistrar.register(this, SENDER_ID);
          }
          else
          {
            // Send ID to Unity
            sendConfirmRegistration(regId);
            // if registeredOnServer flag is not set, send info to Unity
            if (!GCMRegistrar.isRegisteredOnServer(this))
            {
              GCMRegistrar.setRegisteredOnServer(this, true);
            }
          }
        }
        
        finish();
        return;
  }
    
    public void sendConfirmRegistration(String id)
    {
      //UnityPlayer.UnitySendMessage("ECPNManager","RegisterAndroidDevice",id);
    }
}