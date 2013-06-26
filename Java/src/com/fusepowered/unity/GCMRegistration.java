package com.fusepowered.unity;

import android.content.Intent;
import android.content.Context;

public class GCMRegistration
{
  
  public static void RegisterDevice(String SENDER_ID)
  {
    Context context = StaticApplicationContext.getCustomAppContext();
    Intent i = new Intent(context,GCMJava.class);
    i.putExtra("isRegistration", true);
    i.putExtra("senderID",SENDER_ID);
    i.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
    context.startActivity(i);
  }
  
  public static void UnregisterDevice()
  {
    Context context = StaticApplicationContext.getCustomAppContext();
    Intent i = new Intent(context,GCMJava.class);
    i.putExtra("isRegistration", false);
    i.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
    context.startActivity(i);
  }
  
}
