//#define USING_PRIME31_ANDROID

using UnityEngine;
using System;
using System.Collections;

public class FuseAPI_Prime31_IAB : MonoBehaviour
{
#if UNITY_ANDROID && USING_PRIME31_ANDROID	
	void Start () 
	{
		RegisterActions();
	}
	
	private void RegisterActions()
	{
		// add hooks for Prime31 IAB events
		GoogleIABManager.purchaseSucceededEvent += PurchaseSucceeded;
		GoogleIABManager.purchaseFailedEvent += PurchaseFailed;
	}
	
	void OnDestroy()
	{
		UnregisterActions();		
	}
	
	private void UnregisterActions()
	{
		// remove all hooks for Prime31 StoreKit events
		GoogleIABManager.purchaseSucceededEvent -= PurchaseSucceeded;
		GoogleIABManager.purchaseFailedEvent -= PurchaseFailed;
	}
	
	void PurchaseSucceeded( GooglePurchase purchase )
	{
		DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTime purchaseTime = start.AddMilliseconds(purchase.purchaseTime).ToLocalTime();
		FuseAPI.RegisterInAppPurchase((FuseAPI.PurchaseState)purchase.purchaseState, purchase.purchaseToken, purchase.productId, purchase.orderId, purchaseTime, purchase.developerPayload);
	}
	
	void PurchaseFailed( string error )
	{
		FuseAPI.RegisterInAppPurchase(FuseAPI.PurchaseState.CANCELED, "", "", "", System.DateTime.Now, "");			
	}

#endif//UNITY_ANDROID && USING_PRIME31_ANDROIDs
}
