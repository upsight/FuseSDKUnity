using System;
using System.Collections;
using System.Collections.Generic;

namespace FuseMisc
{
	public static class Constants
	{
		/// <summary>Use with <c>false.ToString()</c> value to disable pre rewarded ad alert.</summary>
		public static readonly string RewardedAdOptionKey_ShowPreRoll = "FuseRewardedAdOptionKey_ShowPreRoll";

		/// <summary>Use with <c>false.ToString()</c> value to disable post rewarded ad alert.</summary>
		public static readonly string RewardedAdOptionKey_ShowPostRoll = "FuseRewardedAdOptionKey_ShowPostRoll";

		/// <summary>Use to specify the pre rewarded ad alert accept text.</summary>
		public static readonly string RewardedOptionKey_PreRollYesButtonText = "FuseRewardedOptionKey_PreRollYesButtonText";

		/// <summary>Use to specify the pre rewarded ad alert decline text.</summary>
		public static readonly string RewardedOptionKey_PreRollNoButtonText = "FuseRewardedOptionKey_PreRollNoButtonText";

		/// <summary>Use to specify the post rewarded ad alert confirmation button text.</summary>
		public static readonly string RewardedOptionKey_PostRollContinueButtonText = "FuseRewardedOptionKey_PostRollContinueButtonText";
	}

	/// <summary>Representation of another player that is contained in the user's friend list.</summary>
	public struct Friend
	{
		public string FuseId;
		public string AccountId;
		public string Alias;
		public bool Pending;

#if UNITY_EDITOR
		public static implicit operator Friend(FuseSDKDotNET.Util.Friend f)
		{
			return new Friend() { FuseId = f.FuseId, AccountId = f.AccountId, Alias = f.Alias, Pending = f.Pending };
		}

		public static implicit operator FuseSDKDotNET.Util.Friend(Friend f)
		{
			return new FuseSDKDotNET.Util.Friend() { FuseId = f.FuseId, AccountId = f.AccountId, Alias = f.Alias, Pending = f.Pending };
		}
#endif
	}

	/// <summary>Representation of an item that the user is able to purchase from the App Store. IOS ONLY.</summary>
	public struct Product
	{
		public string ProductId;
		public string PriceLocale;
		public float Price;

#if UNITY_EDITOR
		public static implicit operator Product(FuseSDKDotNET.Util.Product p)
		{
			return new Product() { ProductId = p.ProductId, Price = p.Price, PriceLocale = p.PriceLocale };
		}

		public static implicit operator FuseSDKDotNET.Util.Product(Product p)
		{
			return new FuseSDKDotNET.Util.Product() { ProductId = p.ProductId, Price = p.Price, PriceLocale = p.PriceLocale };
		}
#endif
	}

	/// <summary>Representation of the reward that a player will recieve for watching a rewarded video.</summary>
	public struct RewardedInfo
	{
		public string PreRollMessage;
		public string RewardMessage;
		public string RewardItem;
		public int RewardAmount;

#if UNITY_EDITOR
		public static implicit operator RewardedInfo(FuseSDKDotNET.Util.RewardedInfo r)
		{
			return new RewardedInfo() { PreRollMessage = r.PreRollMessage, RewardMessage = r.RewardMessage, RewardAmount = r.RewardAmount, RewardItem = r.RewardItem };
		}

		public static implicit operator FuseSDKDotNET.Util.RewardedInfo(RewardedInfo r)
		{
			return new FuseSDKDotNET.Util.RewardedInfo() { PreRollMessage = r.PreRollMessage, RewardMessage = r.RewardMessage, RewardAmount = r.RewardAmount, RewardItem = r.RewardItem };
		}
#endif
	}

	/// <summary>Representation of an offer (IAP or VirtualGood) that can be presented to a player.</summary>
	public struct IAPOfferInfo
	{
		public string ProductId;
		public float ProductPrice;
		public string ItemName;
		public int ItemAmount;

#if UNITY_EDITOR
		public static implicit operator IAPOfferInfo(FuseSDKDotNET.Util.IAPOfferInfo o)
		{
			return new IAPOfferInfo() { ProductId = o.ProductId, ProductPrice = o.ProductPrice, ItemName = o.ItemName, ItemAmount = o.ItemAmount };
		}

		public static implicit operator FuseSDKDotNET.Util.IAPOfferInfo(IAPOfferInfo o)
		{
			return new FuseSDKDotNET.Util.IAPOfferInfo() { ProductId = o.ProductId, ProductPrice = o.ProductPrice, ItemName = o.ItemName, ItemAmount = o.ItemAmount };
		}
#endif
	}

	/// <summary>Representation of an offer (IAP or VirtualGood) that can be presented to a player.</summary>
	public struct VGOfferInfo
	{
		public string PurchaseCurrency;
		public float PurchasePrice;
		public string ItemName;
		public int ItemAmount;

#if UNITY_EDITOR
		public static implicit operator VGOfferInfo(FuseSDKDotNET.Util.VGOfferInfo o)
		{
			return new VGOfferInfo() { PurchaseCurrency = o.PurchaseCurrency, PurchasePrice = o.PurchasePrice, ItemName = o.ItemName, ItemAmount = o.ItemAmount };
		}

		public static implicit operator FuseSDKDotNET.Util.VGOfferInfo(VGOfferInfo o)
		{
			return new FuseSDKDotNET.Util.VGOfferInfo() { PurchaseCurrency = o.PurchaseCurrency, PurchasePrice = o.PurchasePrice, ItemName = o.ItemName, ItemAmount = o.ItemAmount };
		}
#endif
	}

	/// <summary>Error codes returned by the Fuse SDK.</summary>
	public enum FuseError
	{
		NONE = 0,			/// no error has occurred
		NOT_CONNECTED,		/// the user is not connected to the internet
		REQUEST_FAILED,		/// there was an error in establishing a connection with the server
		SERVER_ERROR,		/// data was received, but there was a problem parsing the xml
		BAD_DATA,           /// The server has indicated the data it received was not valid.
		SESSION_FAILURE,    /// The session has recieved an error and the operation did not complete due to this error.
		INVALID_REQUEST,    /// The request was not valid, and no action will be performed.
		UNDEFINED,			/// unknown error
	}

	/// <summary>The type of transaction being recorded.</summary>
	public enum IAPState
	{
#if UNITY_IOS
		PURCHASING, PURCHASED, FAILED, RESTORED,	// IOS Specific
#elif UNITY_ANDROID
		PURCHASED, CANCELED, REFUNDED,				// Android Specific
#endif
	}

	/// <summary>The user's gender.</summary>
	public enum Gender
	{
		UNKNOWN,
		MALE,
		FEMALE
	}

	/// <summary>Type of offer.</summary>
	public enum OfferType
	{
		DISCOUNT = 0,
		STANDARD = 1,
		BONUS = 2,
	}

	/// <summary>Type of account the player signed in with.</summary>
	public enum AccountType
	{
		NONE = 0,
		GAMECENTER = 1,
		FACEBOOK = 2,
		TWITTER = 3,
		OPENFEINT = 4,
		USER = 5,
		EMAIL = 6,
		DEVICE_ID = 7,
		GOOGLE_PLAY = 8,
	}

	/// <summary>Helpful extension functions.</summary>
	public static class FuseExtensions
	{
		public static long ToUnixTimestamp(this DateTime dateTime)
		{
			return (long)(dateTime - unixEpoch).TotalSeconds;
		}

		public static DateTime ToDateTime(this long timestamp)
		{
			return unixEpoch.AddSeconds(timestamp);
		}

		private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	}

#region Doxygen

	/*! \mainpage 
	 * \section Classes
	 * \ref FuseSDK
	 * 
	 * \section Namespaces
	 * \ref FuseMisc
	 * 
	 */
#endregion
}