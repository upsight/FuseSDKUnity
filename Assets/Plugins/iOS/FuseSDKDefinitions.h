//
//  FuseSDKDefinitions.h
//  FuseSDK
//
//  Created by fuse on 2015-01-21.
//  Copyright (c) 2015 Fuse Powered Inc. All rights reserved.
//

#ifndef FuseSDK_FuseSDKDefinitions_h
#define FuseSDK_FuseSDKDefinitions_h


#import <StoreKit/StoreKit.h>
#import <GameKit/GKLocalPlayer.h>

extern NSString * kFuseErrorDomain;

enum kFuseGender
{
    FUSE_GENDER_UNKNOWN = 0,        /// gender unknown
    FUSE_GENDER_MALE,               /// gender male
    FUSE_GENDER_FEMALE,             /// gender female
};

enum kFuseErrors
{
    FUSE_ERROR_NO_ERROR = 0,        /// No error has occurred.
    FUSE_ERROR_NOT_CONNECTED,       /// The user is not connected to the internet.
    FUSE_ERROR_REQUEST_FAILED,      /// There was an error in establishing a connection with the server.
    FUSE_ERROR_SERVER_ERROR,        /// The request was processed, but an error occured during processing.
    FUSE_ERROR_BAD_DATA,            /// The server has indicated the data it received was not valid.
    FUSE_ERROR_SESSION_FAILURE,     /// The session has recieved an error and the operation did not complete due to this error.
    FUSE_ERROR_INVALID_REQUEST,      /// The request was not valid, and no action will be performed.
    FUSE_ERROR_UNDEFINED            /// An Error occured, but there is no information about it
};

//Fuse SDK options
#pragma mark SDK Options
/*!
 //Keys For FuseSDK options
 kFuseSDKOptionKey_RegisterForPush - register for push option: default @YES
 kFuseSDKOptionKey_DisableCrashReporting - Disable crash reporting: default @NO
 
 
 @code
 - (void)applicationDidFinishLaunching:(UIApplication *)application
 {
 
 [FuseSDK startSession: @"YOUR APP ID" Delegate:self withOptions:[NSDictionary dictionaryWithObject:@NO forKey:kFuseSDKOptionRegisterForPush];
 
 ...
 }
 @endcode
 */

extern NSString* const kFuseSDKOptionKey_RegisterForPush;
extern NSString* const kFuseSDKOptionKey_DisableCrashReporting;

/*!
//Keys For showAdForZoneID:  options:
kFuseRewardedAdOptionKey_ShowPreRoll - (NSNumber*) Optional Show dialog before rewarded video, allowing the user to cancel (default @YES with dashboard set message)
kFuseRewardedAdOptionKey_ShowPostRoll - (NSNumber*) Optional Show reward dialog, letting the user know a reward was earned (default @YES with dashboard set message)
kFuseRewardedOptionKey_PreRollYesButtonText - (NSString*) Override String for the affirmative option on preroll dialog
kFuseRewardedOptionKey_PreRollNoButtonText - (NSString*) Override String for the reject option on preroll dialog
kFuseRewardedOptionKey_PostRollContinueButtonText - (NSString*) Override string for the Continue button on the postRoll Dialog
 
@code
- (void)functionForShowingFuseAd
{
     [FuseSDK showAdForZoneID:@"zoneID" options:
        @{kFuseRewardedAdOptionKey_ShowPreRoll:@YES,kFuseRewardedAdOptionKey_ShowPostRoll:@NO,kFuseRewardedOptionKey_PreRollYesButtonText:@"I want it!"}];
}
@endcode
     */
//Show Ad Options
extern NSString * const kFuseRewardedAdOptionKey_ShowPreRoll;
extern NSString * const kFuseRewardedAdOptionKey_ShowPostRoll;
extern NSString * const kFuseRewardedOptionKey_PreRollYesButtonText;
extern NSString * const kFuseRewardedOptionKey_PreRollNoButtonText;
extern NSString * const kFuseRewardedOptionKey_PostRollContinueButtonText;


@interface FuseRewardedObject : NSObject

@property (nonatomic , copy , readwrite) NSString* preRollMessage;
@property (nonatomic , copy , readwrite) NSString* rewardMessage;

@property (nonatomic , copy , readwrite) NSString* rewardItem;
@property (nonatomic , copy , readwrite) NSNumber* rewardAmount;

@end


@interface FuseIAPOfferObject : NSObject


@property (nonatomic , copy , readwrite) NSString* productID;
@property (nonatomic , copy , readwrite) NSNumber* productPrice; ///iapPrice is 0 or the value in local currency if available eg. 2.99

@property (nonatomic , copy , readwrite) NSString* itemName;
@property (nonatomic , copy , readwrite) NSNumber* itemAmount;

@end

@interface FuseVirtualGoodsOfferObject : NSObject

@property (nonatomic , copy , readwrite) NSString* purchaseCurrency;
@property (nonatomic , copy , readwrite) NSNumber* purchasePrice;

@property (nonatomic , copy , readwrite) NSString* itemName;
@property (nonatomic , copy , readwrite) NSNumber* itemAmount;

@end


#endif
