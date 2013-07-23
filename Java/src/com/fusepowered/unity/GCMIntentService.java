package com.fusepowered.unity;

import com.unity3d.player.UnityPlayer;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.util.Log;
import android.os.Bundle;
import android.os.Handler;
import com.fusepowered.fuseapi.Constants;

import com.fusepowered.fuseapi.FuseAPI;
import com.google.android.gcm.GCMBaseIntentService;

public class GCMIntentService extends GCMBaseIntentService
{

	private static final String TAG = "FuseUnityGCMIntentService";

	public static String message = "";
	
	private static String mainClass = "com.unity3d.player.UnityPlayerNativeActivity";

	private Handler handler = new Handler();

	/**
	 * Method called on device registered
	 **/
	@Override
	protected void onRegistered(Context context, String registrationId)
	{
		Log.i(TAG, "Device registered: regId = " + registrationId);
		UnityPlayer.UnitySendMessage("ECPNManager","RegisterAndroidDevice",registrationId);
		FuseAPI.registerGCM(registrationId);
	}

	/**
	 * Method called on device un registred
	 * */
	@Override
	protected void onUnregistered(Context context, String registrationId)
	{
		Log.i(TAG, "Device unregistered");
		UnityPlayer.UnitySendMessage("ECPNManager","UnregisterDevice",registrationId);
	}

	/**
	 * Method called on Receiving a new message
	 * */
	@Override
	protected void onMessage(Context context, Intent intent)
	{
		Log.i(TAG, "Received message");
		//String message = intent.getExtras().getString("price");
		
		// notifies user
		generateNotification(context, intent);
	}

	/**
	 * Method called on receiving a deleted message
	 * */
	@Override
	protected void onDeletedMessages(Context context, int total)
	{
		Log.i(TAG, "Received deleted messages notification");
	}

	/**
	 * Method called on Error
	 * */
	@Override
	public void onError(Context context, String errorId)
	{
		Log.i(TAG, "Received error: " + errorId);
	}

	@Override
	protected boolean onRecoverableError(Context context, String errorId)
	{
		// log message
		Log.i(TAG, "Received recoverable error: " + errorId);
		return super.onRecoverableError(context, errorId);
	}

	/**
	 * Issues a notification to inform the user that server has sent a message.
	 */
	private void generateNotification(Context context, final Intent m)
	{
		// get the class name for the native unity player
		// it can be overridden, so we need to parse the manifest for it
		try 
		{
			PackageInfo pInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), PackageManager.GET_ACTIVITIES | PackageManager.GET_META_DATA);
			for( int i = 0; i < pInfo.activities.length; i++ )
			{				
				Bundle meta = pInfo.activities[i].metaData;
				if( meta != null && meta.containsKey("unityplayer.ForwardNativeEventsToDalvik") )									
				{
					mainClass = pInfo.activities[i].name;
				}
			}
			
		} 
		catch (NameNotFoundException e) 
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		//message = m;
		handler.post(new Runnable()
		{
			@SuppressWarnings("deprecation")
			public void run()
			{
				String title = m.getStringExtra("title");
				String msg = m.getStringExtra("text");
				//final int NOTIF_ID = 123477884; 

				//int stringId = FuseAPI.getContext().getApplicationInfo().labelRes;
				//String AppName = FuseAPI.getContext().getString(stringId);
				Context context = GCMIntentService.this;
				int icon = getResources().getIdentifier("ic_launcher", "drawable", context.getPackageName());//R.drawable.ic_launcher;
				long when = System.currentTimeMillis();
				NotificationManager notificationManager = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);
				//Notification notification = new Notification(icon, message, when);
				Notification notification = new Notification(icon, msg, when);

				// attempt to use the Unity Native Player class as defined in the manifest file	
				Intent notificationIntent = null;
				try 
				{
					notificationIntent = new Intent(GCMIntentService.this, Class.forName(mainClass));
				} 
				catch (ClassNotFoundException e) 
				{
					Log.e(TAG, "Unity native player class not found: " + mainClass);
					e.printStackTrace();
				}

				// set intent so it does not start a new activity
				notificationIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP |
						Intent.FLAG_ACTIVITY_SINGLE_TOP);
				PendingIntent intent =
						PendingIntent.getActivity(context, 0, notificationIntent, 0);
				notification.setLatestEventInfo(context, title, msg, intent);

				// Flags
				// log the notification ID
				FuseAPI.notificationID = m.getStringExtra(Constants.PARAM_NOTIFICATION_ID);
				notification.flags |= Notification.FLAG_AUTO_CANCEL;
				//notification.defaults |= Notification.DEFAULT_SOUND;
				//notification.defaults |= Notification.DEFAULT_VIBRATE; // uncomment and add VIBRATE permissions on manifest to get vibrating notifications
				
				notificationManager.notify(Integer.parseInt(FuseAPI.notificationID), notification);
			}
		});        
	}

}