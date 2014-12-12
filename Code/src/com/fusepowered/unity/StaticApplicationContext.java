package com.fusepowered.unity;

import android.content.Context;
 
public class StaticApplicationContext extends android.app.Application 
{
   private static Context context;
   public void onCreate()
   {
     context=getApplicationContext();
   }

   public static Context getCustomAppContext()
   {
     return context;
   }
 }