//#define USING_PRIME31_ANDROID

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class FuseSDK_Prime31_IAB : MonoBehaviour
{
	public bool logging = false;
#if UNITY_ANDROID && USING_PRIME31_ANDROID
	
	public static bool debugOutput = false;
	
	private GooglePurchase savedPurchase = null;
	
	void Start () 
	{
		if(logging)
		{
			FuseSDK_Prime31_IAB.debugOutput = true;
		}

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
			//Debug.LogError("FuseSDK_Prime31_IAB::GetSkuInfo - savedPurchase was null!");
			return;
		}

		string priceString = skuInfo[0].price;
		double price = 0;
		try
		{
			price = double.Parse(priceString, NumberStyles.Currency);
		}
		catch
		{
			var re = new System.Text.RegularExpressions.Regex(@"\D*(?<num>[\d\s\.,]+?)(?<dec>([\.,]\s*\d?\d?)?)\D*$");
			var match = re.Match(priceString);
			if(match.Success)
			{
				string stripped = "";
				try
				{
					var dec = System.Text.RegularExpressions.Regex.Replace(match.Groups["dec"].Value, @"\s", "");
					stripped = System.Text.RegularExpressions.Regex.Replace(match.Groups["num"].Value, @"[^\d]", "") + dec;
					price = double.Parse(stripped, NumberStyles.Currency);
					
					//Just in case the culture info is broken and double.Parse can't parse the decimal part properly
					if(price % 1 == 0 && dec.Length > 1)
					{
						price /= 100;
					}
				}
				catch
				{
					Debug.LogError("FuseSDK_Prime31_IAB::GetSkuInfo - Error parsing " + priceString + " >> Unable to parse " + stripped);
				}
			}
			else
			{
				Debug.LogError("FuseSDK_Prime31_IAB::GetSkuInfo - Error parsing " + priceString + " >> String did not match regex");
			}
		}

		GooglePurchase purchase = savedPurchase;
		// purchaseTime from GoogleIAB is milliseconds since unix epoch
		DateTime time = new DateTime(purchase.purchaseTime*TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
		FuseSDK.RegisterAndroidInAppPurchase((FuseMisc.IAPState)purchase.purchaseState, purchase.purchaseToken, purchase.productId, purchase.orderId, time, purchase.developerPayload, price, null);		
		
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
		//FuseSDK.RegisterAndroidInAppPurchase(FuseSDK.PurchaseState.CANCELED, "", "", "", System.DateTime.Now, "");			
	}


	
	public static void FuseLog(string str)
	{
		if(debugOutput)
		{
			Debug.Log("FuseSDK: " + str);
		}
	}

#endif//UNITY_ANDROID && USING_PRIME31_ANDROIDs
}
