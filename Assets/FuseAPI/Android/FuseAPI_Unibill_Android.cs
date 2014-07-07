//#define USING_UNIBILL_ANDROID

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class FuseAPI_Unibill_Android : MonoBehaviour
{	
	public bool logging = false;
	// Uncomment the #define at the top of this file if you are using the Prime31 StoreKit plugin for iOS
	#if UNITY_ANDROID && USING_UNIBILL_ANDROID
	public static bool debugOutput = false;
	
	public static bool ActionsRegistered = false;
	
	void Awake ()
	{
		if(logging)
		{
			FuseAPI_Unibill_Android.debugOutput = true;
		}
	}
	
	void Start () 
	{
		//We are Assuming Unbiller.Initialise is called independantly.
		RegisterActions();
	}
	
	private void RegisterActions()
	{
		
		if (FuseAPI_Unibill_Android.ActionsRegistered)
			return;
		
		FuseAPI_Unibill_Android.ActionsRegistered = true;

		// add hooks for UniBiller events
		Unibiller.onBillerReady += onBillerReady;
		Unibiller.onPurchaseCancelled += purchaseCancelled;
		Unibiller.onPurchaseFailed += purchaseFailed;
		Unibiller.onPurchaseCompleteEvent += purchaseSuccessful;
		
	}
	
	private void UnregisterActions()
	{
		
		FuseAPI_Unibill_Android.ActionsRegistered = false;

		// remove all hooks UniBiller events
		Unibiller.onBillerReady -= onBillerReady;
		Unibiller.onPurchaseCancelled -= purchaseCancelled;
		Unibiller.onPurchaseFailed -= purchaseFailed;
		Unibiller.onPurchaseCompleteEvent -= purchaseSuccessful;
	}
	
	private static void onBillerReady(UnibillState state) 
	{
		FuseLog (" UniBiller State " + state);
	}
	
	void purchaseFailed( PurchasableItem item )
	{
		if( item != null )
		{
			FuseLog( "purchase failed for item " + item.Id );
		//	FuseAPI.RegisterInAppPurchase(item.Id, "", recipt, FuseAPI.TransactionState.FAILED);
		}
	}	
	
	void purchaseCancelled( PurchasableItem item )
	{
		
		if( item != null )
		{
			FuseLog( "purchase cancelled for item : " + item.Id );
	//		FuseAPI.RegisterInAppPurchase(item.Id, "", recipt, FuseAPI.TransactionState.FAILED);	
		}
	}
	static byte[] GetBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}
	void purchaseSuccessful( PurchaseEvent e )
	{
		if(e != null)
		{
			//The Recipt is a json with the contents we need to pass in, parse it here
			JSONObject jObj = new JSONObject(e.Receipt);
			JSONObject json = new JSONObject(jObj.GetField("json").ToString().Replace("\\\"","\"").Trim('"'));
			JSONObject sig = jObj.GetField("signature");
			string sigstr = sig.ToString();

		
			FuseLog("json = " + json.Print(true));
			FuseLog("sig = " + sigstr);

			int purchaseState = 0;
			string purchaseToken = "";
			string productId = "";
			string orderId = "";
			double purchaseTime = 0;

			if(json.HasField("purchaseState"))
				json.GetField(ref purchaseState,"purchaseState");
			if(json.HasField("purchaseToken"))
				json.GetField(ref purchaseToken,"purchaseToken");
			if(json.HasField("orderId"))
				json.GetField(ref orderId,"orderId");
			if(json.HasField("productId"))
				json.GetField(ref productId,"productId");
			if(json.HasField("purchaseTime"))
				json.GetField(ref purchaseTime,"purchaseTime");

			double price = double.Parse(e.PurchasedItem.localizedPriceString, NumberStyles.Currency);

			FuseAPI.RegisterInAppPurchase((FuseAPI.PurchaseState)purchaseState,purchaseToken, 
			                              productId, orderId, (long)purchaseTime,
			                              sigstr, price , null);		

		}
	}
	
	void OnDestroy()
	{
		UnregisterActions();
	}
	
	
	public static void FuseLog(string str)
	{
		if(debugOutput)
		{
			Debug.Log("FuseAPI: " + str);
		}
	}
	
	#endif//UNITY_ANDROID && USING_UNIBILL_ANDROID
}