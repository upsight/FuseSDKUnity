//#define USING_PRIME31_ANDROID

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseAPI_Prime31_IAB : MonoBehaviour
{
#if UNITY_ANDROID && USING_PRIME31_ANDROID
	
	private GooglePurchase savedPurchase = null;
	
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
	
	private void GetSkuInfo( List<GooglePurchase> purchaseInfo, List<GoogleSkuInfo> skuInfo)
	{		
		GoogleIABManager.queryInventorySucceededEvent -= GetSkuInfo;
		if( savedPurchase == null )
		{
			//Debug.LogError("FuseAPI_Prime31_IAB::GetSkuInfo - savedPurchase was null!");
			return;
		}		
		
		// parse the price as a number value from the string in skuInfo
		// we can't get the currency symbol this way, but passing an empty string to RegisterInAppPurchase will cause it to check the current locale for the currency type		
		double price = double.Parse(skuInfo[0].price, System.Globalization.NumberStyles.Currency);
		GooglePurchase purchase = savedPurchase;
		DateTime time = FuseAPI.TimestampToDateTime(purchase.purchaseTime);
		FuseAPI.RegisterInAppPurchase((FuseAPI.PurchaseState)purchase.purchaseState, purchase.purchaseToken, purchase.productId, purchase.orderId, time, purchase.developerPayload, price, null);		
		
		savedPurchase = null;
	}
	
	void PurchaseSucceeded( GooglePurchase purchase )
	{
		// get the sku info in order to complete logging the transaction
		string[] skus = { purchase.productId };		
		GoogleIABManager.queryInventorySucceededEvent += GetSkuInfo;
		GoogleIAB.queryInventory(skus);
		
		// cache the purchase to use when the skuInfo is discovered
		savedPurchase = purchase;				
	}
	
	void PurchaseFailed( string error )
	{
		//FuseAPI.RegisterInAppPurchase(FuseAPI.PurchaseState.CANCELED, "", "", "", System.DateTime.Now, "");			
	}

#endif//UNITY_ANDROID && USING_PRIME31_ANDROIDs
}
