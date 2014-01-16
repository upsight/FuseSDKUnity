//#define USING_PRIME31_IOS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseAPI_Prime31StoreKit : MonoBehaviour
{	
	// Uncomment the #define at the top of this file if you are using the Prime31 StoreKit plugin for iOS
#if UNITY_IPHONE && USING_PRIME31_IOS
	
	// cached in order to send failed and cancelled messages
	private StoreKitTransaction currentTransaction = null;
	private static string transactionIDPurchasing = "";
	private static string transactionIDPurchased = ""; 
	
	void Start () 
	{
		RegisterActions();
	}
	
	private void RegisterActions()
	{
		// add hooks for Prime31 StoreKit events
		StoreKitManager.productPurchaseAwaitingConfirmationEvent += productPurchaseAwaitingConfirmationEvent;
		StoreKitManager.purchaseSuccessfulEvent += purchaseSuccessful;
		StoreKitManager.purchaseCancelledEvent += purchaseCancelled;
		StoreKitManager.purchaseFailedEvent += purchaseFailed;
		StoreKitManager.productListReceivedEvent += productListReceivedEvent;
	}
	
	private void UnregisterActions()
	{
		// remove all hooks for Prime31 StoreKit events
		StoreKitManager.productPurchaseAwaitingConfirmationEvent -= productPurchaseAwaitingConfirmationEvent;
		StoreKitManager.purchaseSuccessfulEvent -= purchaseSuccessful;
		StoreKitManager.purchaseCancelledEvent -= purchaseCancelled;
		StoreKitManager.purchaseFailedEvent -= purchaseFailed;
		StoreKitManager.productListReceivedEvent -= productListReceivedEvent;
	}
	
	void productListReceivedEvent( List<StoreKitProduct> productList )
	{	
		//Debug.Log( "productListReceivedEvent. total products received: " + productList.Count );
		
		// create an array of product info to pass into the Fuse API
		FuseAPI.Product[] products = new FuseAPI.Product[productList.Count];
		int numItems = 0;
		foreach( StoreKitProduct product in productList )
		{
			FuseAPI.Product currentProduct = new FuseAPI.Product();
			currentProduct.productId = product.productIdentifier;
			currentProduct.priceLocale = product.currencyCode;
			currentProduct.price = float.Parse(product.price);
			products.SetValue(currentProduct, numItems++);
			//Debug.Log( product.ToString() + "\n" );
		}
		FuseAPI.RegisterInAppPurchaseList(products);
	}
	
	void productPurchaseAwaitingConfirmationEvent( StoreKitTransaction transaction )
	{
		//Debug.Log( "productPurchaseAwaitingConfirmationEvent: " + transaction );
		
		if( transactionIDPurchasing == transaction.transactionIdentifier )
		{
			//Debug.Log("Duplicate transaction " + transactionID);
			return;
		}
		transactionIDPurchasing = transaction.transactionIdentifier;
		currentTransaction = transaction;
		byte[] reciept = Convert.FromBase64String(transaction.base64EncodedTransactionReceipt);
		FuseAPI.RegisterInAppPurchase(transaction.productIdentifier, transaction.transactionIdentifier, reciept, FuseAPI.TransactionState.PURCHASING);		
	}
	
	void purchaseFailed( string error )
	{
		//Debug.Log( "purchase failed with error: " + error );
		
		if( currentTransaction != null )
		{
			byte[] reciept = Convert.FromBase64String(currentTransaction.base64EncodedTransactionReceipt);
			FuseAPI.RegisterInAppPurchase(currentTransaction.productIdentifier, currentTransaction.transactionIdentifier, reciept, FuseAPI.TransactionState.FAILED);			
			currentTransaction = null;
		}
	}	

	void purchaseCancelled( string error )
	{
		//Debug.Log( "purchase cancelled with error: " + error );
		if( currentTransaction != null )
		{
			byte[] reciept = Convert.FromBase64String(currentTransaction.base64EncodedTransactionReceipt);
			FuseAPI.RegisterInAppPurchase(currentTransaction.productIdentifier, currentTransaction.transactionIdentifier, reciept, FuseAPI.TransactionState.FAILED);			
			currentTransaction = null;			
		}
	}
	
	void purchaseSuccessful( StoreKitTransaction transaction )
	{
		if( transactionIDPurchased == transaction.transactionIdentifier )
		{
			//Debug.Log("Duplicate transaction " + transactionIDPurchased);
			return;
		}
		transactionIDPurchased = transaction.transactionIdentifier;
		
		//Debug.Log( "purchased product: " + transaction );
		
		currentTransaction = null;
		byte[] reciept = Convert.FromBase64String(transaction.base64EncodedTransactionReceipt);
		FuseAPI.RegisterInAppPurchase(transaction.productIdentifier, transaction.transactionIdentifier, reciept, FuseAPI.TransactionState.PURCHASED);		
	}
	
	void OnDestroy()
	{
		UnregisterActions();
	}
	
	
#endif//UNITY_IPHONE && USING_PRIME31_IOS
}