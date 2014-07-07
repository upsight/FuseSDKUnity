
//#define USING_UNIBILL_IOS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseAPI_Unibill_iOS : MonoBehaviour
{	
	public bool logging = false;
	// Uncomment the #define at the top of this file if you are using the Prime31 StoreKit plugin for iOS
#if UNITY_IPHONE && USING_UNIBILL_IOS
	public static bool debugOutput = false;

	public static bool ActionsRegistered = false;
	void Awake ()
	{
		if(logging)
		{
			FuseAPI_Unibill_iOS.debugOutput = true;
		}
	}
	
	void Start () 
	{
		//We are Assuming Unbiller.Initialise is called independantly.
		RegisterActions();
	}
	
	private void RegisterActions()
	{
		if (FuseAPI_Unibill_iOS.ActionsRegistered)
						return;
		FuseLog("Unbiller Actions Registered");
		FuseAPI_Unibill_iOS.ActionsRegistered = true;
		// add hooks for UniBiller events
		Unibiller.onBillerReady += onBillerReady;
		Unibiller.onPurchaseCancelled += purchaseCancelled;
		Unibiller.onPurchaseFailed += purchaseFailed;
		Unibiller.onPurchaseCompleteEvent += purchaseSuccessful;

	}
	
	private void UnregisterActions()
	{
		FuseLog("Unbiller Actions unRegistered");
		FuseAPI_Unibill_iOS.ActionsRegistered = false;
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
			byte[] recipt = { 0 };
			FuseAPI.RegisterInAppPurchase(item.Id, "", recipt, FuseAPI.TransactionState.FAILED);
		}
	}	
	
	void purchaseCancelled( PurchasableItem item )
	{

		if( item != null )
		{
			byte[] recipt = { 0 };
			FuseAPI.RegisterInAppPurchase(item.Id, "", recipt, FuseAPI.TransactionState.FAILED);	
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
			byte[] reciept =  GetBytes(e.Receipt);
			FuseLog(e.PurchasedItem.description + " " + e.PurchasedItem.localizedPriceString);
			FuseAPI.RegisterInAppPurchase(e.PurchasedItem.LocalID, "", reciept, FuseAPI.TransactionState.PURCHASED);		
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
	
#endif//UNITY_IPHONE && USING_UNIBILL_IOS
}