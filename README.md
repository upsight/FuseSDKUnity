# Fuse SDK Unity Wrapper

## Current Version

Version: 2.5.5.0

Released: April 1st, 2016

## Update Instructions
The location of our Unity packages has changed. The 2 packages in this repository simply contain a Updater that will fetch the package from Unity.  
Please use the links below to download the FuseSDK.


## To Download
[Unity 4 Package](https://github.com/fusepowered/FuseSDKUnity/releases/download/v2.5.5.0/FuseUnitySDK.unitypackage)  
[Unity 5 Package](https://github.com/fusepowered/FuseSDKUnity/releases/download/v2.5.5.0/FuseUnitySDK-Unity5.unitypackage)  
Once the package has been imported into your project, you will be able to update the FuseSDK Wrapper through the Unity Editor.

## Getting Started

Please review the [integration instructions](https://wiki.fusepowered.com/index.php?title=Unity) found here for more information on integrating the Fuse SDK.

## References

* [Integration Instructions](https://wiki.fusepowered.com/index.php?title=Unity)
* [Documentation] (http://fusepowered.github.io/FuseSDKUnity/)

## Need an Account?
Please visit [http://www.fusepowered.com](http://www.fusepowered.com) for an account to get started!

## Release Notes

### 2.5.5.0
April 1st, 2016
* Critical Android Bug fixes in 3rd party providers

### 2.5.4.0
March 29th, 2016
* iOS Bug fixes

### 2.5.3.0
March 23rd, 2016
* Removed Android InAppBilling library (included by Unity or other 3rd party billing plugins)
* Removed FuseSDK.NET-Stub.dll
* Added options to FuseSDK prefab to start Fuse Sessions in the Editor and Standalone builds
* iOS Bug fixes

### 2.5.2.0
March 11th, 2016
* Ad provider updates
* Price localization for offers
* Rich media pre and post rolls for cross promotional videos
* Optimized PostProcess scripts
* Removed support for Unity 3.5
* Added Soomla Store IAP tracking
* Added Unity IAP tracking to Extras folder
* Added new iOS Frameworks
* Bug fixes

### 2.4.2.1
January 20th, 2016
* Android Manifest bug fixes

### 2.4.2.0
November 26th, 2015
* Ad Provider optimizations

### 2.4.1.0
November 23rd, 2015
* Ad Provider updates
* VAST Improvements
* Custom End Cards
* Bug fixes

### 2.3.2.0
October 14th, 2015
* Custom Offer meta-data
* Custom Call to Action on campaign videos
* Ad provider updates
* Bug fixes
* Android 6.0 and iOS 9 compatibility
* Unity 5 package now uses a single .aar file for Android

### 2.2.1.0
August 13th, 2015
* Added new ad providers
* Added VAST support
* Added rewarded video authentication 
  * Added method FuseSDK.SetRewardedVideoUserID(string userID) to identify the user
  * The RewardedInfo struct now contains RewardItemId
* Added StartTime and EndTime to the IAPOfferInfo struct
* Added CurrencyID, VirtualGoodID, StartTime and EndTime to the VGOfferInfo struct
* Fix for game data get/set
* Bug fixes

### 2.1.4.0
June 24th, 2015
* Fixed warning caused by FuseSDK.NET-Stub.dll
* Bug fixes for Ad providers

### 2.1.1.0
June 4th, 2015
* Bug fixes for game configurations and ad adapters

### 2.1.0.0
May 28th, 2015
* Added new segmentation functionality
* Added parental consent toggle
* Added new gender enums
* Bug fixes 

### 2.0.5.1
May 20th, 2015
* Bug fixes for registering iOS IAPs

### 2.0.5.0
May 12th, 2015
* Bug fixes for Android rewarded callbacks and push notifications
* Optimizations for iOS game configurations and friends lists

### 2.0.4.0
April 29th, 2015
* Bug fixes for rewarded ads

### 2.0.3.1
April 23rd, 2015
* Bug fixes for external adapters, game configuration, and friends list

### 2.0.2.0
April 17th, 2015
* IAP and Virtual Good offers
* Rewarded video enhancements
* Interface updates
* FuseAPI class renamed - please use FuseSDK


## Legal Requirements
By downloading the Fuse Powered SDK, you are granted a limited, non-commercial license to use and review the SDK solely for evaluation purposes.  If you wish to integrate the SDK into any commercial applications, you must register an account with [Fuse Powered](https://www.fusepowered.com) and accept the terms and conditions on the Fuse Powered website.

## Contact Us
For more information, please visit [http://www.fusepowered.com](http://www.fusepowered.com). For questions or assistance, please email us at [support@fusepowered.com](mailto:support@fusepowered.com).
