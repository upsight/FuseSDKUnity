/////////////////////////////////////////////////////////////////////////////
//
//  Copyright 2009-2012 Fuse Powered, Inc.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//  	http://www.apache.org/licenses/LICENSE-2.0
//  	
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
/////////////////////////////////////////////////////////////////////////////

#import <StoreKit/StoreKit.h>
#import <GameKit/GKLocalPlayer.h>

enum kFuseGender
{
    FUSE_GENDER_UNKNOWN = 0,        /// gender unknown
    FUSE_GENDER_MALE,               /// gender male
    FUSE_GENDER_FEMALE,             /// gender female
};

enum kFuseGameDataErrors
{
    FUSE_GD_ERROR_NONE = 0,         /// no error has occurred
    FUSE_GD_ERROR_NO_ACCOUNT,       /// the user has not signed in to an online account
    FUSE_GD_ERROR_NOT_CONNECTED,    /// the user is not connected to the internet
    FUSE_GD_ERROR_REQUEST_FAILED,   /// there was an error in establishing a connection with the server
    FUSE_GD_ERROR_XML_PARSE_ERROR,  /// data was received, but there was a problem parsing the xml
};

enum kFuseErrors
{
    FUSE_ERROR_NONE = 0,            /// no error has occurred
    FUSE_ERROR_NOT_CONNECTED,       /// the user is not connected to the internet
    FUSE_ERROR_REQUEST_FAILED,      /// there was an error in establishing a connection with the server
    FUSE_ERROR_XML_PARSE_ERROR,     /// data was received, but there was a problem parsing the xml
};

enum kFuseChatErrors
{
    FUSE_CHAT_NONE = 0,             /// no error has occurred
    FUSE_CHAT_NOT_CONNECTED,        /// the user is not connected to the internet
    FUSE_CHAT_REQUEST_FAILED,       /// there was an error in establishing a connection with the server
};

enum kFuseAddFriendErrors
{
    FUSE_ADD_FRIEND_NO_ERROR = 0,
    FUSE_ADD_FRIEND_BAD_ID,
    FUSE_ADD_FRIEND_NOT_CONNECTED,
    FUSE_ADD_FRIEND_REQUEST_FAILED
};

enum kFuseRemoveFriendErrors
{
    FUSE_REMOVE_FRIEND_NO_ERROR = 0,
    FUSE_REMOVE_FRIEND_BAD_ID,
    FUSE_REMOVE_FRIEND_NOT_CONNECTED,
    FUSE_REMOVE_FRIEND_REQUEST_FAILED
};

enum kFuseAcceptFriendErrors
{
    FUSE_ACCEPT_FRIEND_NO_ERROR = 0,
    FUSE_ACCEPT_FRIEND_BAD_ID,
    FUSE_ACCEPT_FRIEND_NOT_CONNECTED,
    FUSE_ACCEPT_FRIEND_REQUEST_FAILED
};

enum kFuseRejectFriendErrors
{
    FUSE_REJECT_FRIEND_NO_ERROR = 0,
    FUSE_REJECT_FRIEND_BAD_ID,
    FUSE_REJECT_FRIEND_NOT_CONNECTED,
    FUSE_REJECT_FRIEND_REQUEST_FAILED
};

enum kFuseFriendsListErrors
{
    FUSE_FRIENDS_LIST_NO_ERROR = 0,
    FUSE_FRIENDS_LIST_SERVER_ERROR,
    FUSE_FRIENDS_LIST_NOT_CONNECTED,
    FUSE_FRIENDS_LIST_REQUEST_FAILED
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 * @brief This is main Fuse API delegate
 * @details This delegate is optional.  However, relevant information might be passed to an object registered as a FuseAPI delegate (application specific).  A \<FuseDelegate\> is registered in FuseAPI::startSession:Delegate:.
 */
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@protocol FuseDelegate <NSObject>
@optional
/*!
 * @brief This function indicates when the server has acknowledged that a session has been established by the client device
 * @details When a delegate is registered with the FuseAPI::startSession:Delegate: function, and the acknowledgement of the connection has been received, this function is invoked.  This function is optional and is only intended to help in cases where this information is relevant to the application.
 * @see FuseAPI::startSession:Delegate: for more information on starting a session with a \<FuseDelegate\>
 */
-(void) sessionStartReceived;

/*!
 * @brief This function is invoked when an error has occurred when trying to start a session
 * @details To avoid cases where operations are continuing as if everything was being handled normally by the FuseAPI, this can be listened to and actions taken as a result.  This function is optional.
 * @param _error [NSNumber*] The error value corresponding to a value in kFuseErrors
 * @see FuseAPI::startSession:Delegate: for more information on starting a session with a \<FuseDelegate\>
 * @see kFuseErrors for more information on all possible error values
 */
-(void) sessionLoginError:(NSNumber*)_error;

/*!
 * @brief This function indicates that the server UTC time has been received by the client device
 * @details This function can be called both as a result of FuseAPI::utcTimeFromServer or automatically after a time event has been sent from the Fuse system (generally only happens when a session is started or resumed).  The time should only be treated a psuedo-accurate.  There are delays in propagating the time from the server to the device.  
 
 @code
 
 -(void) timeUpdated:(NSNumber*)_utcTimeStamp
 {
    int utc_unix_timestamp = [utcTimeStamp intValue];
 }
 
 @endcode
 
 * @param _utcTimeStamp [int] 
 * @see FuseAPI::utcTimeFromServer for more information on directly requesting the UTC time from the server
 * @see http://en.wikipedia.org/wiki/Unix_time for more information on Unix time
 * @see http://en.wikipedia.org/wiki/Coordinated_Universal_Time for more information on UTC time
 */
-(void) timeUpdated:(NSNumber*)_utcTimeStamp;

/*!
 * @brief This function notifies the device that an account login request has been received by the server.
 * @details When a user logs in using one of the available FuseAPI account function, for instance FuseAPI::gameCenterLogin:, the server will send the client a notification once received.  This is to prevent any action being taken by the client before this has been received.
 * @param _type [int] Account type
 * @param _account_id [int] The account ID of the user logged in
 * @see FuseAPI::gameCenterLogin: for a sample login function
 */
-(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;

/*!
 * @brief This function indicates that an action should be taken as a result of a notification being closed
 * @details To handle any possible action as a result of an in-game notification being displayed, a custom string can be configured as a part of the notification setup process in the Fuse dashboard.  This string can be any value that is recognized by the application.  For instance, a sample of this in action would be:
 
 @code
 
 -(void) notificationAction:(NSString*)_action
 {
    if ([_action isEqualToString:@"GO_TO_LEVEL_2"])
    {
        // Take the user to level 2 for instance
    }
    else if ([_action isEqualToString:@"GO_TO_SALE"])
    {
        // Take the user to the sale section of your store
    }
 }
 
 @endcode
 
 * @param _action [NSString*] The string value that indicates what action to take.  Custom and application specific.
 * @see FuseAPI::displayNotitications for more information on displaying in-game notifications
 */
-(void) notificationAction:(NSString*)_action;

/*!
 * @brief This function indicates that the game configuration values have been received by the client.
 * @details To avoid polling values before they are sent from the server, this callback can be used to determine when they are valid.  These values are update when a user starts or resumes a session.
 * @see FuseAPI::getGameConfigurationValue: for more information on reading game configuration values once valid
 */
-(void) gameConfigurationReceived;

/*!
 * @brief This function indicates the result of an addition of a friend to the Fuse friends list system
 * @details This function is optional, and only needs to be handled if the application needs to respond to errors in inviting friends. An example implementation would be:
 
 @code
 
 -(void) friendAdded:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    if ([_error intValue] == FUSE_ADD_FRIEND_NO_ERROR)
    {
        // no error has occurred in adding a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was added to
 @param _error [int] The error value corresponding to the a value in kFuseAddFriendErrors
 @see kFuseAddFriendErrors for all possible error codes
 @see FuseAPI::addFriend: for more information on adding a friend
 @since Fuse API version 1.22
 */
-(void) friendAdded:(NSString*)_fuse_id Error:(NSNumber*)_error;

/*!
 * @brief This function indicates the result of a removal of a friend to the Fuse friends list system
 * @details This function is optional, and only needs to be handled if the application needs to respond to errors in removing friends. An example implementation would be:
 
 @code
 
 -(void) friendRemoved:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    if ([_error intValue] == FUSE_REMOVE_FRIEND_NO_ERROR)
    {
        // no error has occurred in removing a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was removed
 @param _error [int] The error value corresponding to the a value in kFuseRemoveFriendErrors
 @see kFuseRemoveFriendErrors for all possible error codes
 @see FuseAPI::removeFriend: for more information on removing a friend
 @since Fuse API version 1.22
 */
-(void) friendRemoved:(NSString*)_fuse_id Error:(NSNumber*)_error;

/*!
 * @brief This function indicates the result of a acceptance of a friend to the Fuse friends list system
 * @details This function is optional, and only needs to be handled if the application needs to respond to errors in accepting friends. An example implementation would be:
 
 @code
 
 -(void) friendAccepted:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    if ([_error intValue] == FUSE_ACCEPT_FRIEND_NO_ERROR)
    {
        // no error has occurred in accepting a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was accepted
 @param _error [int] The error value corresponding to the a value in kFuseAcceptFriendErrors
 @see kFuseAcceptFriendErrors for all possible error codes
 @see FuseAPI::acceptFriend: for more information on accepting a friend
 @since Fuse API version 1.22
 */
-(void) friendAccepted:(NSString*)_fuse_id Error:(NSNumber*)_error;

/*!
 * @brief This function indicates the result of a rejection of a friend to the Fuse friends list system
 * @details This function is optional, and only needs to be handled if the application needs to respond to errors in rejecting friends. An example implementation would be:
 
 @code
 
 -(void) friendRejected:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    if ([_error intValue] == FUSE_REJECT_FRIEND_NO_ERROR)
    {
        // no error has occurred in accepting a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was rejected
 @param _error [int] The error value corresponding to the a value in kFuseRejectFriendErrors
 @see kFuseRejectFriendErrors for all possible error codes
 @see FuseAPI::rejectFriend: for more information on rejecting a friend
 @since Fuse API version 1.22
 */
-(void) friendRejected:(NSString*)_fuse_id Error:(NSNumber*)_error;

/*!
 * @brief This function indicates when the friends list on the client has been updated from the server
 * @details Either on the first fetch or subsequent updates of the friends list, this function is called every time the friends list is updated on the client.  A friends list is requested from the server using FuseAPI::updateFriendsListFromServer and the process invokes this function with a dictionary of friends when complete.  A sample implementation would be:
 
 @code
 
 -(void) friendsListUpdated:(NSDictionary*)_friendsList
 {
    // The friends list has returned to the device
 
    // sample code to parse the dictionary
    NSArray *fuse_ids = [_friendsList allKeys];
 
    for (int i = 0; i < [fuse_ids count]; i++)
    {
        NSString *fuse_id = [fuse_ids objectAtIndex:i];
 
        NSDictionary *friendEntry = (NSDictionary*)[_friendsList objectForKey:fuse_id];
 
        NSLog(@"Friend is Pending: %d", [[friendEntry objectForKey:@"pending"] intValue]);
        NSLog(@"Friend Alias: %@", [friendEntry objectForKey:@"alias"]);
        NSLog(@"Friend Fuse ID: %@", [friendEntry objectForKey:@"fuse_id"]);
    }
 }
 
 @endcode
 
 * @param _friendsList
 * @see FuseAPI::updateFriendsListFromServer or more information on requesting the friends list from the server
 * @see FuseAPI::getFriendsList for more information on requesting the local copy of the friends list
 * @see friendsListError: to handle errors involved in requesting the friends list from the Fuse server
 * @since Fuse API version 1.22
 */
-(void) friendsListUpdated:(NSDictionary*)_friendsList;

/*!
 * @brief This function indicates when an error has occurred in fetching the friends list from the server
 * @details This function is a result of an error originating from the invocation of FuseAPI::updateFriendsListFromServer and indicates an error has occurred somewhere in the process.
 
 @code
 
 -(void) friendsListError:(NSNumber*)_error
 {
    if ([_error intValue] != FUSE_FRIENDS_LIST_NO_ERROR)
    {
        // An error has occurred
    }
 }
 
 @endcode
 
 * @param _error [int] The error value corresponding to the a value in kFuseFriendsListErrors
 * @see FuseAPI::updateFriendsListFromServer or more information on requesting the friends list from the server
 * @since Fuse API version 1.22
 */
-(void) friendsListError:(NSNumber*)_error;

/*!
 * @brief This function is called as a result of requesting a user's chat list from the server
 * @details When invoked, this function will include the data payload of the chat list in the form of a dictionary.  The structure of the dictionary is as follows:
 
 @code
 
 -(void) chatListReceived:(NSDictionary*)_messages User:(NSString*)_fuse_id
 {
    // Sample code for parsing the dictionary returned
    if (_messages != nil && [_messages count] > 0)
    {
        NSArray *keys = [_messages allKeys];
        NSArray *sortedKeys = [keys sortedArrayUsingSelector:@selector(compare:)];
 
        for (int i = 0; i < [sortedKeys count]; i++)
        {
            NSString *message_id = [sortedKeys objectAtIndex:i];
            NSMutableDictionary *entry = (NSMutableDictionary*)[_messages objectForKey:message_id];
 
            NSLog(@"%d [%@]: %@ -> %@", [message_id intValue], [entry objectForKey:@"timestamp"], [entry objectForKey:@"alias"], [entry objectForKey:@"message"]);
        }
    }
 }
 
 @endcode
 
 * @param _messages [NSDictionary*] The message list
 * @param _fuse_id [NSString*] The fuse ID for which the message list belongs
 * @see FuseAPI::getUserChatListFromServer: for more information on requesting the chat list and triggering this callback
 * @see chatListError: for more information on handling an error in requesting the chat list
 */
-(void) chatListReceived:(NSDictionary*)_messages User:(NSString*)_fuse_id;

/*!
 @brief This function will be called in response to an error occurring for any \<FuseAPI\> chat list functions
 @param _error [NSNumber*] The error code corresponding to a value in kFuseChatErrors
 @see FuseAPI::postUserChatMessage:TargetUser: for more information on sending a chat message for a user
 @see kFuseChatErrors for more information on all of the error values
 */
-(void) chatListError:(NSNumber*)_error;

/*!
 * @brief This function indicates whether the registered in-app purchase has been validated by Apple's servers
 * @details This function is optional can indicates via the _verified bit if the in-app purchase was valid.  The Fuse servers initiate communication with Apple's in-app purchase verification servers once a call to FuseAPI::registerInAppPurchase: is invoked.  This callback is the result of that process.  If the _validated bit comes back as a '1', it can be safely concluded that the purchase is definitely valid.  However, if the bit comes back as '0', the purchase should only be treated as suspect as it is not definitive at this point whether it was invalid or an error occurred in the process.
 
 @code
 
 -(void) puchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id
 {
    BOOL is_valid = [_verified boolValue];
 
    if (is_valid)
    {
        // valid transaction
    }
    else
    {
        // suspect transaction
    }
 }
 
 @endcode
 
 * @param _verified [NSSNumber*] This is the _verified bit: 1 indicates that the transaction was valid.  0 indicates that the transaction should be treated with suspicion.
 * @param _tx_id [NSString*] The transaction ID specified by Apple
 * @param _o_tx_id [NSString*] The original transaction ID specified by Apple (can be different than the transaction ID because the transaction could be a reinstatement of a previous purchase)
 * @see FuseAPI::registerInAppPurchase: for more information on how to invoke this process
 */
-(void) puchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id;

-(void) giftListRecieved:(NSDictionary*)_gifts User:(NSString*)_fuse_id;
-(void) giftListError:(NSNumber*)_error;
-(void) giftAcknowledged:(NSNumber*)_gift_id User:(NSString*)_fuse_id;
-(void) giftError:(NSNumber*)_error;

@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 * @brief This is Fuse API overlay delegate
 * @details This delegate handles any event from a fuse overlay.  Most commonly, an overlay will occur in the form of the "More Games" section.
 */
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@protocol FuseOverlayDelegate <NSObject>
@optional
/*!
 * @brief This function indicates when a Fuse overlay will close
 * @details When an overlay is being displayed by the application, most commonly the "More Games" overlay, and has been dismissed by the user, this function will be called to indicate that the application can continue execution of the user flow or application (if applicable).  Often, the "More Games" section will be displayed over a menu, so the when the overlay is dismissed the menu will still be showing, thus not requiring the implementation of this function.
 * @see FuseAPI::displayMoreGames: for more information on calling the More Games overlay
 */
-(void) overlayWillClose;
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 * @brief This is Fuse API ad delegate
 * @details Handles all notifications involved with showing an ad and responding to an ad closing.
 */
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@protocol FuseAdDelegate <NSObject>
@required
/*!
 * @brief This function indicates when a full-screen (interstitial) ad is closing
 * @details When an ad is being dismissed by the user and control is to be returned to the application, this function will be called.  Once called, the application can continue execution of the user flow or application.
 * @see FuseAPI::showAdWithDelegate: for more information on displaying an ad with a \<FuseAdDelegate\>
 * @since FuseAPI version 1.12
 */
-(void) adWillClose;
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 * @brief This is the game data delegate which receives notifications from getting and setting user data.
 * @details Handles all notification events involved in sending and receiving per-user game data.  This is not required unless the functions FuseAPI::setGameData:Delegate: or FuseAPI::getGameData:Delegate: (or others of the same family) are implemented
 */
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@protocol FuseGameDataDelegate <NSObject>
@required
/*!
 * @brief This function is called when the request for game data has returned to the device.
 * @details The requested data is given as an input to the function.
 * @param _fuse_id [NSString*] The Fuse ID of the user for which the data was requested.  Can be different that the user signed in on the device if the data requested was for a friend or other user.
 * @param _key [NSString*] The master object key (if specified) for the data returning.  Can be 'nil'.
 * @param _data [NSMutableDictionary*] The data payload.
 * @see FuseAPI::getGameData:Delegate: for more information on retrieving game data
 * @see gameDataError: for more information on error cases involved with retrieving game data
 */
-(void) gameDataReceived:(NSString*)_fuse_id ForKey:(NSString*)_key Data:(NSMutableDictionary*)_data;
/*!
 * @brief This function indicates when an error has occurred when sending or receiving per-user game data information.
 * @details When an error occurs when trying to set or retrieve game data, this function will be thrown with an error code.
 * @param _error [NSNumber*] The error number corresponding to a value in kFuseGameDataErrors
 * @see FuseAPI::setGameData:Delegate: for more information on setting game data
 * @see FuseAPI::getGameData:Delegate: for more information on retrieving game data
 * @see kFuseGameDataErrors for information on all of the possible error cases
 */
-(void) gameDataError:(NSNumber*)_error;
@optional
/*!
 * @brief This function indicates the server has acknowledged a save of per-user game data
 * @details When this function is called, it indicates that the server has successfully saved data sent from the client device.  
 * @param _request_id [int] The request ID that corresponds to the ID returned by a FuseAPI::setGameData:Delegate: function variant.
 * @see FuseAPI::setGameData:Delegate: for more information on setting game data
 */
 -(void) gameDataSetAcknowledged:(NSNumber*)_request_id;
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 * @brief This is the main Fuse API static class
 * @details 
 */
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface FuseAPI : NSObject 
{
} 

#pragma mark Session Creation
/*!
 * @brief This function is used to initiate all communication with the Fuse system.
 * @details The startSession function is used to bootstrap all communications with the Fuse system. This should be called early in the applicationDidFinishLaunching function in your application delegate.  There is a second version of this function, startSession:Delegate which performs the same operation as this function except it registers a \<FuseDelegate\> delegate if you choose to listen for certain events.
 
 An example call would be:
 
 @code
 - (void)applicationDidFinishLaunching:(UIApplication *)application
 {
    [FuseAPI startSession: @"YOUR API KEY"]; 
 
    ...
 }
 @endcode
 
 * @param _game_id [NSString*] This is the 36-character API key assigned by the Fuse system.  Your API key is generated when you add your App to the Fuse dashboard system.  It can be found in the configuration tab in a specific game, or in the "Integrate API" section of the dashboard.  The API key is a 36-digit unique ID of the form 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'.
 * @see FuseDelegate::sessionLoginError: for more information on handling errors occurred when trying to start a session
 */
+(void) startSession:(NSString *)_game_id;

/*!
 * @brief This function is used to initiate all communication with the Fuse system (and register a \<FuseDelegate\>)
 * @details The startSession function is used to bootstrap all communications with the Fuse system. This should be called early in the applicationDidFinishLaunching function in your application delegate.  There is a second version of this function, startSession:Delegate: which performs the same operation as this function except it does not register a \<FuseDelegate\> delegate.
 
 An example call would be:
 
 @code
 - (void)applicationDidFinishLaunching:(UIApplication *)application
 {
    [FuseAPI startSession: @"YOUR API KEY" Delegate:DELEGATE_REFERENCE]; 
 
    ...
 }
 @endcode
 
 When a session has been established by Fuse system, a callback will be sent to the registered \<FuseDelegate\> object using the following function:

 @code
 -(void) sessionStartReceived;
 @endcode
 
 * @param _game_id [NSString*] This is the 36-character API key assigned by the Fuse system.  Your API key is generated when you add your App to the Fuse dashboard system.  It can be found in the configuration tab in a specific game, or in the "Integrate API" section of the dashboard.  The API key is a 36-digit unique ID of the form 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'.
 * @param _delegate [id] The \<FuseDelegate\> object to be registered to receive protocol callbacks (optional - see startSession: without this parameter if registering a delegate is not needed)
 * @see FuseDelegate::sessionStartReceived for more information on the delegate function
 * @see FuseDelegate::sessionLoginError: for more information on handling errors occurred when trying to start a session
 */
+(void) startSession:(NSString *)_game_id Delegate:(id)_delegate;

#pragma mark Application Hooks
/*!
 * @brief This function is used to pass the registered Apple push token to the Fuse servers for future push notification messaging.
 * @details This function should be called from your application delegate file application:didRegisterForRemoteNotificationsWithDeviceToken function:
 
 @code
 
 - (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken 
 {
    [FuseAPI applicationdidRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
 }
 
 @endcode
 
 * @param deviceToken [NSData*] The device token passed to application:didRegisterForRemoteNotificationsWithDeviceToken
 */
+(void) applicationdidRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken;

/*!
 * @brief This function is used to capture when the registration of a device token has failed
 * @param error [NSError*] This function should be called from your application delegate file application:didFailToRegisterForRemoteNotificationsWithError function:
 
 @code
 
 - (void)application:(UIApplication *)application didFailToRegisterForRemoteNotificationsWithError:(NSError *)error 
 {
    [FuseAPI applicationdidFailToRegisterForRemoteNotificationsWithError:error];
 }
 
 @endcode
 
 @param error [NSError*] The error passed to application:didFailToRegisterForRemoteNotificationsWithError
 */
+(void) applicationdidFailToRegisterForRemoteNotificationsWithError:(NSError *)error;

/*!
 * @brief This function is used to capture when a user either receives an Apple push notification when the application is running or chooses to re-enter the application because of a click on an on-of-application notification .
 * @details This function is very important in determining the effectiveness of a push notification in terms of winning back users to the application.  This function should be called from your application delegate file application:didFailToRegisterForRemoteNotificationsWithError function:
 
 @code
 
 - (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo
 {
    [FuseAPI applicationdidReceiveRemoteNotification:userInfo Application:application];	
 }
 
 @endcode

 * @param userInfo [NSDictionary*] The information dictionary passed to application:didReceiveRemoteNotification
 * @param application [UIApplication*] The initiating UIApplication instance
 * @see applicationdidRegisterForRemoteNotificationsWithDeviceToken: for more information on collecting tokens.
 */
+(void) applicationdidReceiveRemoteNotification:(NSDictionary *)userInfo Application:(UIApplication *)application;

/*!
 * @brief This function is optional and can collect launch options 
 * @details This function is optional and should be called from your application delegate file application:didFinishLaunchingWithOptions function (if implemented):
 
 @code
 
 - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
 {
    [FuseAPI respondToApplicationLaunchOptions:launchOptions Application:application];
 }
 
 @endcode
 
 * @param launchOptions [NSDictionary*] The dictionary of launch options passed to application:didFinishLaunchingWithOptions
 * @param application [UIApplication*] The initiating UIApplication instance
 * @deprecated This function is deprecated and will be removed in version 1.23
 */
+(void) respondToApplicationLaunchOptions:(NSDictionary *)launchOptions Application:(UIApplication *)application;

#pragma mark Analytic Event Function
/*!
 * @brief This function is used to register an named event in the Fuse system.
 * @details This function logs the time and frequency of an event within a given game session.  The input string can be anything that is relevant to the design of the game but should be easily understandable when read by users in the Fuse system.
 
 It is advisable to avoid recording events at a high rate as this could negatively impact both application and server performance.  For example, a good practice would be to issue an event at the start of a level (i.e. 'Level 1') or when a purchase is made.  It would be unadvisable to issue any event in each draw loop as this would create a tremendous amount of overhead and server traffic.
 
 The maximum length of a registered event is 256 characters, and each application is limited to a maximum 256 separate named events.
 
 An example call would be:
 
 @code
 [FuseAPI registerEvent:@"Level 1 Started"];
 @endcode
 
 * @param _message [NSString*] The event name to be logged
 */

+(void) registerEvent:(NSString *)_message;
/*!
 * @brief This function is used to register an named event in the Fuse system.
 * @details This function logs the time and frequency of an event within a given game session.  The input string can be anything that is relevant to the design of the game but should be easily understandable when read by users in the Fuse system.
 
 It is advisable to avoid recording events at a high rate as this could negatively impact both application and server performance.  For example, a good practice would be to issue an event at the start of a level (i.e. 'Level 1') or when a purchase is made.  It would be unadvisable to issue any event in each draw loop as this would create a tremendous amount of overhead and server traffic.
 
 The maximum length of a registered event is 256 characters, and each application is limited to a maximum 256 separate named events.
 
 An example call would be:
 
 @code
 [FuseAPI registerEvent:@"Level 1 Started" withDict:myValues];
 @endcode
 
 * @param _message [NSString*] The event name to be logged
 * @param _dict [NSDictionary*] A dictionary of values associated with the event
 */
+(void) registerEvent:(NSString *)_message withDict:(NSDictionary*)_dict;

#pragma mark Game Crash Registration
/*!
 * @brief This function is used to catch crashes within an app.
 * @details The registerCrash function should be used within a declare exception handler to send information in the case of a crash.  This function will only be able to catch certain types of failures, as it is dependent upon where the crash happens whether the application will use this handler.
 
 Add the following line to register an uncaught exception listener in your application delegate:
 
 @code
 -(void) applicationDidFinishLaunching:(UIApplication *)application
 {
    ...
 
    NSSetUncaughtExceptionHandler(&uncaughtExceptionHandler);
 
    ...
 }
 @endcode

 Now create a function that corresponds to the registered uncaught exception listener and put it at the top of the application delegate (outside of the class declaration).
 
 @code
 void uncaughtExceptionHandler(NSException *exception)
 {
    [FuseAPI registerCrash:exception];
 }
 @endcode
 
 @param _exception [NSException *] The exception object passed to the exception handler function by the system.
 
 */ 
+(void) registerCrash:(NSException *)_exception;

#pragma mark In-App Purchase Logging
/*!
 * @brief This function is used to register the price and currency that a user is using to make an in-app purchase.
 * @details After receiving the list of in-app purchases from the Apple system, this function can be called to record the localized item information.  To do this, place the following function call in productsRequest delegate function in your SKProducsRequestDelegate delegate:
 
 @code
 - (void) productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
 {
    [FuseAPI registerInAppPurchaseList:response];
 
    ...
 }
 @endcode
 
 @param _response [SKProductsResponse *] The response object from Apple's StoreKit request to get item availability and information.
 
 */
+(void) registerInAppPurchaseList:(SKProductsResponse *)_response;

/*!
 @brief This function records in-app purchases in the Fuse system.
 
 @details Call this function directly after an in-app purchase is made once it has been confirmed that the transaction has occurred successfully.  Optimally, this should be done in the recordTransaction function in your SKPaymentTransactionObserver delegate.  A callback is sent to the \<FuseDelegate\> delegate indicating whether the transaction was confirmed by Apple's in-app purchase system or whether it should be treated as suspect (optional).
 
 @code
 -(void) recordTransaction:(SKPaymentTransaction *)transaction
 {
    [FuseAPI registerInAppPurchase:transaction];
 
    ...
 }
 @endcode
 
 @param _transaction [SKPaymentTransaction *] The transaction object sent to the delegate once a purchase has been completed.
 @see FuseDelegate::puchaseVerification:TransactionID:OriginalTransactionID: for more information on the \<FuseDelegate\> callback indicating whether the transaction was verified by Apple's servers
 */
+(void) registerInAppPurchase:(SKPaymentTransaction *)_transaction;

#pragma mark Fuse Interstitial Ads
/*!
 @brief This function is used to display a Fuse interstitial ad within the application.
 @details The Fuse interstitial ad system can be used to deliver high-quality, full-screen ads within your application.  To call an ad, simply call the following function:
 
 @code
 [FuseAPI showAdWithDelegate:FuseAdDelegate_Reference];
 @endcode
 
 The object passed to the function must be an object that is subscribed to the \<FuseAdDelegate\> protocol.  This object will receive a callback when the ad is closing, signalling to the application that it has been handed back control.  To create a \<FuseAdDelegate\> object, change the interface declaration to add the \<FuseAdDelegate\> protocol:
 
 @code
 @interface YourAdObject : NSObject <FuseAdDelegate> 
 {
 }
 @end
 
 @endcode
 
 In the implementation of the delegate object, add this delegate function:
 
 @code
 
 @implementation YourAdObject
 
 -(void) adWillClose
 {
    // Continue execution flow of your application
 }
 
 @end
 
 @endcode
 
 Note that it is best to not perform other actions that require UI access while an ad is being displayed.  This includes:
 
 - Signing in to Game Center
 - Displaying a Fuse in-game notification
 
 See the Fuse dashboard to configure advertisements for your game.
 
 @param _delegate [id] The \<FuseAdDelegate\> object that will handle receiving callbacks in response to ad actions.
 */
+(void) showAdWithDelegate:(id)_delegate;

#pragma mark Notifications
/*!
 @brief This function is used to display in-game Fuse notifications
 @details The Fuse notification system can be used to deliver textual system notifications to your users, promoting features of your application for example or promoting another application.  In addition, the Fuse system automatically configures notifications to rate your application in the App Store as well as upgrade your application when a new version is released.  It is best to call this function early in the application flow of your game, preferably on your main menu.  Optionally, an action can be assigned to the closing of the dialog to notify the application that an internal action should be taken.  In this case, the FuseDelegate::notificationAction: function would be called when the dialog is closing (only if the affirmative button is pressed).
 
 To display notifications:
 
 @code
 [FuseAPI displayNotifications];
 @endcode
 
 @see FuseDelegate::notificationAction: for more information on handling internal actions
 */
+(void) displayNotifications;

#pragma mark More Games 
/*!
 @brief This function is use to display the "More Games" section
 @details The "More Games" section can be used to showcase your own games or all games within the Fuse network (including yours!).  To call the "More Games" overlay, simply call:
 
 @code
 [FuseAPI displayMoreGames:FuseOverlayDelegate_Reference];
 @endcode
 
 The delegate reference is optional - it is only required to be registered if you want to take action on a callback (specifically, when the section is closing).  Often, this overlay is displayed on top of a main menu screen, and once closed the main menu will already be present so no action on close is required.  To create a \<FuseOverlayDelegate\> object, change the interface declaration to add the \<FuseOverlayDelegate\> protocol:
 
 @code
 @interface YourMoreGamesObject : NSObject <FuseOverlayDelegate> 
 {
 }
 @end
 
 @endcode
 
 In the implementation of the delegate object, add this delegate function:
 
 @code
 
 @implementation YourMoreGamesObject
 
 -(void) overlayWillClose
 {
    // more games overlay has closed
 }
 
 @end
 
 @endcode
 
 @param _delegate [id] The \<FuseAdDelegate\> object that will handle receiving callbacks in response to the more games section signalling the application [optional - pass 'nil' if not required]
 @see FuseOverlayDelegate::overlayWillClose for more information on the delegate function call
 */
+(void) displayMoreGames:(id)_delegate;

#pragma mark Gender
/*!
 * @brief This function registers a gender for the user
 * @details If a gender is known or suspected for a user, call the following function to assign a gender to the user:
 
 @code
 
 [FuseAPI registerGender:FUSE_GENDER_FEMALE];
 
 // The enumerated type definition is as follows:
 enum kFuseGender
 {
    FUSE_GENDER_UNKNOWN = 0,
    FUSE_GENDER_MALE,
    FUSE_GENDER_FEMALE,
 };
 
 @endcode
 
 @param _gender [int] The enumerated gender of the user
 */
+(void) registerGender:(int)_gender;

#pragma mark Account Login Functions
/*!
 * @brief Game Center account registration
 * @details Uniquely track a user across devices by passing Game Center login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To register the account information, pass the Game Center object as follows as soon as the user has been confirmed to have logged in.  This example shows the Fuse API function being called in sample Game Center login code:
 
 @code
 
 GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
 
 [localPlayer authenticateWithCompletionHandler:^(NSError *error) 
 {
    if (localPlayer.isAuthenticated)
    {
        [FuseAPI gameCenterLogin:localPlayer];
    }
 }];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _lp [GKLocalPlayer*] This is the returned Game Center object from the Game Center completion handler
 @since Fuse API version 1.14
 @see startSession:Delegate: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 */
+(void) gameCenterLogin:(GKLocalPlayer*)_lp;

/*!
 * @brief Facebook account registration
 * @details Uniquely track a user across devices by passing Facebook login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To call this function:
 
 @code

 [FuseAPI facebookLogin:@"facebook_id"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _facebook_id [NSString*] This is the account id of the user signed in to Facebook
 @since Fuse API version 1.14
 @see startSession:Delegate: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 */
+(void) facebookLogin:(NSString*)_facebook_id;

/*!
 * @brief Twitter account registration
 * @details Uniquely track a user across devices by passing Twitter login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To call this function:
 
 @code
 
 [FuseAPI twitterLogin:@"twit_id"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _twitter_id [NSString*] This is the account id of the user signed in to Twitter
 @since Fuse API version 1.14
 @see startSession:Delegate: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 */
+(void) twitterLogin:(NSString*)_twitter_id;

/*!
 * @brief OpenFeint account registration
 * @details Uniquely track a user across devices by passing OpenFeint login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To call this function:
 
 @code
 
 [FuseAPI openFeintLogin:@"of_id"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _openfeint_id [NSString*] This is the account id of the user signed in to OpenFeint
 @since Fuse API version 1.14
 @see startSession:Delegate: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 */
+(void) openFeintLogin:(NSString*)_openfeint_id;

/*!
 * @brief Fuse account registration
 * @details Uniquely track a user across devices by passing Fuse login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 The Fuse ID is a nine-digit numeric value that is unique to every signed-in player (but not unique to device).  Note that this function required UI elements to allow a user to provide credentials to log in, and is currently not implemented.
 
 To call this function:
 
 @code
 
 [FuseAPI fuseLogin:@"012345678"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _fuse_id [NSString*] This is the account id of the user signed in to the Fuse system
 @param _alias [NSString*] The alias or 'handle' of the user
 @since Fuse API version 1.14
 @see startSession:Delegate: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @see getFuseID for more information on retrieving the user's Fuse ID once signed in
 */
+(void) fuseLogin:(NSString*)_fuse_id Alias:(NSString*)_alias;

#pragma mark Miscellaneous
/*!
 * @brief This function returns the amount of times the user has opened the application
 * @details Call this function to get the number of times the application has been opened either from the Springboard of system tray (minimized)
 
 @code
 
 int played = [FuseAPI gamesPlayed];
 
 @endcode
 
 * @retval [int] The number of times the application has been opened
 */
+(int) gamesPlayed;

/*!
 * @brief This function returns the Fuse API version
 * @details Call this function if it is required to know the Fuse API version.  
 
 @code
 
 NSString *api_ver = [FuseAPI libraryVersion];
 
 @endcode
 
 * @retval [NSString*] The API version of the form '1.22'
 */
+(NSString*) libraryVersion;

/*!
 * @brief This function returns the App API key
 * @details This returns the 36-character Fuse API key that was used when called startSession: or startSession:Delegate:
 
 @code
 
 NSString *api_key = [FuseAPI getID];
 
 @endcode
 
 * @retval [NSString*] The 36-character Fuse API key
 * @see startSession: to see how the API key was set
 * @deprecated This is deprecated as of version 1.22 because it is not really useful and is taking up binary size.  This will be removed in version 1.23.
 */
+(NSString*) getID __attribute__((deprecated));

/*! 
 * @brief This function indicates whether the application is connected to the internet
 * @details This function indicates if the application is connected via wifi or cellular network and connected to the internet. To use this function:
 
 @code
 
 BOOL is_connected = [FuseAPI connected];
 
 @endcode
 
 @retval [BOOL] The connected status of the application
 */
+(BOOL) connected;

/*!
 * @brief This function gets the UTC time from the server
 * @details To help determine the psuedo-accurate real-world time (i.e. not device time), this function can be called to get the UTC time from the Fuse servers.  The date is returned in unix time format (i.e. seconds elapsed since January 1, 1970).  The returned value is only psuedo-accurate in that it does not account for request time and delays - so it is the time on the server when the request was received but not the time when the value returns to the device.  This is generally used to prevent time exploits in games where such situations could occur (by a user changing their device time).
 
 To get the time, it is a two step process.  First a request is made to the API:
 
 @code

 [FuseAPI utcTimeFromServer];

 @endcode
 
 Then, a callback is triggered in the \<FuseDelegate\> with the result:
 
 @code
 
 -(void) timeUpdated:(NSNumber*)_utcTimeStamp
 {
    int server_time = [utcTimeStamp intValue];
 }
 
 @endcode
 
 @see startSession: or startSession:Delegate: to see how to register a \<FuseDelegate\>
 @see FuseDelegate::timeUpdated: to understand more about the \<FuseDelegate\> callback
 @see http://en.wikipedia.org/wiki/Unix_time for more information on Unix time
 @see http://en.wikipedia.org/wiki/Coordinated_Universal_Time for more information on UTC time
 */
+(void) utcTimeFromServer;

/*!
 * @brief This function indicates whether the Fuse API has concluded all necessary work before being able to be closed.
 * @details 
 
 @code
 
 BOOL is_not_done = [FuseAPI notReadyToTerminate];
 
 @endcode
 
 @retval [BOOL] Indicates wether the Fuse API is still working on sending out final requests.
 */
+(BOOL) notReadyToTerminate;

#pragma mark Data Opt In/Out
/*!
 * @brief This function opts a user out of data being collected by the API.
 * @details In accordance with Apple's terms of service, a user should always have the option to not have data collected on their play usage.  To allow a user to opt out, call the following function:
 
 @code 
 [FuseAPI disableData];
 @endcode
 
 While it is necessary to allow a user to opt in and out of data collection, the implementation of this function is optional as there is another way to allow a user to stop data collection.  By using a settings bundle, which appears in the "Settings" menu for the application, data collection can be toggled without adding any code in the binary.  Many developers find this an easier and less intrusive way to integrate this feature.  This file can be found on the dashboard in the "Integrate API" section, or at this link:
 
[https://dashboard.fusepowered.com/api/Settings.bundle.zip](https://dashboard.fusepowered.com/api/Settings.bundle.zip)
 
 @see enableData to understand how to enable collecting data
 @see Download https://dashboard.fusepowered.com/api/Settings.bundle.zip to integrate the settings bundle and avoid having to implement this function
 */
+(void) disableData;

/*!
 * @brief This function opts a user in so that data is collected by the API.
 * @details To allow a user to opt in, call the following function:
 
 @code 
 [FuseAPI enableData];
 @endcode

 @see disableData to understand how to disable collecting data and more information on using the settings bundle
 @see Download https://dashboard.fusepowered.com/api/Settings.bundle.zip to integrate the settings bundle and avoid having to implement this function
 */
+(void) enableData;

/*!
 * @brief This function indicates whether the user is opted-in to collecting data.
 * @details To see if a user has indicated whether they want data collected:
 
 @code
 BOOL is_opted_in = [FuseAPI dataEnabled];
 @endcode
 
 @retval [BOOL] Indicates if the user has enabled data to be collected
 */
+(BOOL) dataEnabled;

#pragma mark User Game Data
/*!
 @brief This function is used to store per-user persistent game data on the Fuse servers while specifying a mater key value for the key->value pairs in the dictionary.
 @details User information is stored in key->value pairs.  A dictionary is used to store the key value pairs that are passed in to the function.  Keys in the dictionary must be strings, while value data types fall in to two categories: the first being variables that can be cast to strings (int, bool, float, string), while the second is binary data in the form of a NSData* object.  There is currently no restriction on the length of the value passed to the server, however if requests become so large they could create problems and this policy will have to be changed.  To receive information on whether the call was made successfully, a \<FuseGameDataDelegate\> delegate can be optionally passed to the function (pass 'nil' if not required).  An example of how to create a dictionary and send it to the Fuse system is as follows:
 
 @code
 
 NSMutableDictionary *dict = [[NSMutableDictionary alloc] initWithCapacity:4];
 
 [dict setValue:[NSNumber numberWithInt:currentPoints] forKey:@"points"];
 [dict setValue:[NSNumber numberWithBool:hasPlayed] forKey:@"played"];
 [dict setValue:userName forKey:@"name"];
 
 NSData *binaryData = [[NSData alloc] initWithBytes:ptr length:1024];
 
 [dict setValue:binaryData forKey:@"binary_blob"];
 
 [FuseAPI setGameData:dict Delegate:self];
 
 @endcode
 
 To register a \<FuseGameDataDelegate\> delegate, create a class with the following interface:
 
 @code
 @interface YourDataObject : NSObject <FuseGameDataDelegate> 
 {
 }
 @end
 
 @endcode
 
 In the implementation of the delegate object, add this delegate function to receive set data callbacks:
 
 @code
 
 @implementation YourDataObject
 
 -(void) gameDataError:(NSNumber*)_error
 {
    // An error has occurred in setting game data
    // see kFuseGameDataErrors for all error values
    int error = [_error intValue];
 }
 
 -(void) gameDataSetAcknowledged:(NSNumber*)_request_id
 {
    // An acknowledgement has been received by the device from the server indicating that the data has been updated
    int request_id = [_request_id intValue];
 }
 
 @end
 
 @endcode 
 
 @param _data [NSMutableDictionary*] The data to be stored on the server
 @param _delegate [id] The \<FuseGameDataDelegate\> delegate to receive callbacks (optional - pass 'nil' if not required)
 @retval [int] Request number.  This is the request ID that can be correlated in gameDataSetAcknowledged: to know whether the request was successful.
 @see setGameData:Key:Delegate: for more information on setting data with a parent key defining the set of key->value pairs
 @see kFuseGameDataErrors for all possible game data error values
 @since Fuse API version 1.15
 */
+(int) setGameData:(NSMutableDictionary*)_data Delegate:(id)_delegate;

/*!
 * @brief This function is used to store per-user persistent game data on the Fuse servers while specifying a master key value for the key->value pairs in the dictionary.
 * @details This function is similar to setGameData:Delegate: except that a parent key value can be sent along with the dictionary to group data.  For instance, if you have a game with cars and you need to send information on each car that a user has, a dictionary could be sent for each car using a differentiating parent key value to separate the dictionary from others.  In addition, this function *can not* be used to send binary data unlike  setGameData:Delegate:.
 
 A sample implementation of this function call would be:
 
 @code
 
 NSMutableDictionary *dict = [[NSMutableDictionary alloc] initWithCapacity:2];
 
 [dict setValue:[NSNumber numberWithInt:speed] forKey:@"max_speed"];
 [dict setValue:[NSNumber numberWithBool:power] forKey:@"total_power"];
 
 [FuseAPI setGameData:dict Key:@"car_3" Delegate:self];
 
 @endcode
 
 @param _data [NSMutableDictionary*] The data to be stored on the server
 @param _key [NSString*] The parent key of the data set
 @param _delegate [id] The \<FuseGameDataDelegate\> delegate to receive callbacks (optional - pass 'nil' if not required)
 @retval [int] Request number.  This is the request ID that can be correlated in gameDataSetAcknowledged: to know whether the request was successful.
 @see setGameData:Delegate: for more information on configuring a \<FuseGameDataDelegate\> delegate
 @see kFuseGameDataErrors for all possible game data error values
 @since Fuse API version 1.15
 */
+(int) setGameData:(NSMutableDictionary*)_data Key:(NSString*)_key Delegate:(id)_delegate;

/*!
 * @brief This function is used to store per-user persistent game data on the Fuse servers while indicating whether the dictionary is a dictionary of dictionaries.
 * @details In the case where a great amount of data is to be sent at once, dictionaries can be grouped into a master dictionary that can be sent to the server in an atomic transaction.  This is useful in reducing server overhead as well as ensuring data is received by the server when the app is closing.
 
 A sample of this would be:
 
 @code
 
 NSMutableDictionary *dict1 = [[NSMutableDictionary alloc] initWithCapacity:2];
 
 [dict1 setValue:[NSNumber numberWithInt:speed] forKey:@"max_speed"];
 [dict1 setValue:[NSNumber numberWithBool:power] forKey:@"total_power"];
 
 NSMutableDictionary *dict2 = [[NSMutableDictionary alloc] initWithCapacity:2];
 
 [dict2 setValue:[NSNumber numberWithInt:level] forKey:@"level"];
 [dict2 setValue:[NSNumber numberWithBool:points] forKey:@"points"];
 
 NSMutableDictionary *masterDict = [[NSMutableDictionary alloc] initWithCapacity:2];

 [masterDict setValue:dict1 forKey:@"car_3"];
 [masterDict setValue:dict2 forKey:@"level_info"];
 
 [FuseAPI setGameData:masterDict Key:@"" Delegate:self IsCollection:YES];
 
 @endcode

 Note that this function is equavalent to setGameData:Key:Delegate: if IsCollection == NO.
 
 @param _data [NSMutableDictionary*] The data to be stored on the server
 @param _key [NSString*] The parent key of the data set - ignored if IsCollection == YES)
 @param _delegate [id] The \<FuseGameDataDelegate\> delegate to receive callbacks (optional - pass 'nil' if not required)
 @param _collection [BOOL] Indicates whether the dictionary being passed to the function is a dictionary of dictionaries (YES), or a dictionary of key->value pairs (NO)
 @retval [int] Request number.  This is the request ID that can be correlated in gameDataSetAcknowledged: to know whether the request was successful.
 @see setGameData:Delegate: for more information on configuring a \<FuseGameDataDelegate\> delegate
 @since Fuse API version 1.18
 */
+(int) setGameData:(NSMutableDictionary*)_data Key:(NSString*)_key Delegate:(id)_delegate IsCollection:(BOOL)_collection;

/*!
 @brief This function is used to store per-user persistent game data on the Fuse servers for another user.
 @details Use this function to set data for another user, not the user currently logged in.  This is a rare case in terms of user data in that generally a game session would only set data for the user playing.  Use this function carefully.
 
 @param _data [NSMutableDictionary*] The data to be stored on the server
 @param _fuse_id [NSString*] The public Fuse ID of the user.
 @param _key [NSString*] The parent key of the data set
 @param _delegate [id] The \<FuseGameDataDelegate\> delegate to receive callbacks (optional - pass 'nil' if not required)
 @param _collection [BOOL] Indicates whether the dictionary being passed to the function is a dictionary of dictionaries (YES), or a dictionary of key->value pairs (NO)
 @retval [int] Request number.  This is the request ID that can be correlated in gameDataSetAcknowledged: to know whether the request was successful.
 @see setGameData:Delegate: for more information on configuring a \<FuseGameDataDelegate\> delegate
 @see setGameData:Key:Delegate:IsCollection: for information on how to use the function
 @since Fuse API version 1.15
 */
+(int) setGameData:(NSMutableDictionary*)_data FuseID:(NSString*)_fuse_id Key:(NSString*)_key Delegate:(id)_delegate IsCollection:(BOOL)_collection;

/*!
 @brief This function is used to retrieve per-user persistent game data on the Fuse servers.
 @details This function will retrieve data that is not grouped by master key (see getGameData:Key:Delegate for how to get data grouped by master key).  Data can be retrieved in two different methods: by specifying an array of keys (i.e. a subset of the entire data set), or by passing 'nil' for the _keys value which will return the entire data set.  To request data, call the following function:
 
 @code
 
 // With specifying a subset of information
 [FuseAPI getGameData:[NSMutableArray arrayWithObjects:@"score", @"level", nil] Delegate:self]; 
 
 // Getting all data
 [FuseAPI getGameData:nil Delegate:self]; 
 
 @endcode
 
 The data is returned via a callback to the \<FuseGameDataDelegate\> object.  The delegate object *must* passed in to this function in order to get the data returned.  For information on how to create a delegate, please see the definition of setGameData:Delegate:.
 
 The delegate function called in the case of a callback is as follows:
 
 @code
 
 @implementation YourDataObject
 
 -(void) gameDataError:(NSNumber*)_error
 {
    // An error has occurred in getting game data
    // see kFuseGameDataErrors for all error values
    int error = [_error intValue];
 }
 
 -(void) gameDataReceived:(NSString*)_fuse_id ForKey:(NSString*)_key Data:(NSMutableDictionary*)_data
 {
    // Data has returned for the user specified by '_fuse_id'.
    // Key can optionally by 'nil' or an empty string
 }
 
 @end
 
 @endcode
 
 @param _keys [NSArray*] The subset of keys to request from the server.  If 'nil' all key->value pairs will be returned.
 @param _delegate [id] The \<FuseGameDataDelegate\> to which the callback will be made when data has returned to the device.
 @retval [int] This is the request ID.
 @see getGameData:Key:Delegate: for how to receive data grouped by master key
 @see setGameData:Delegate: for more information on configuring a \<FuseGameDataDelegate\> delegate
 @see gameDataReceived:ForKey:Data: for more information on the delegate callback
 @since Fuse API version 1.15
 */
+(int) getGameData:(NSArray*)_keys Delegate:(id)_delegate;

/*!
 @brief This function is used to retrieve per-user persistent game data on the Fuse servers while specifying a master key value for the key->value pairs in the dictionary.
 @details Similar to getGameData:Delegate, this function allows the specification of a master key to get grouped data.
 
 @code
 
 // With specifying a subset of information with a key specified
 [FuseAPI getGameData:[NSMutableArray arrayWithObjects:@"max_speed", @"total_power", nil] Key:@"car_info" Delegate:self]; 
 
 // Getting all data without a key specified
 [FuseAPI getGameData:nil Key:@"car_info" Delegate:self];  
 
 @endcode
 
 @param _keys [NSArray*] The subset of keys to request from the server.  If 'nil' all key->value pairs will be returned.
 @param _key [NSString*] This is the master key name.
 @param _delegate [id] The \<FuseGameDataDelegate\> to which the callback will be made when data has returned to the device.
 @retval [int] This is the request ID.
 @see getGameData:Delegate: for more information on make a request, setting up the delegate and receiving a request
 @since Fuse API version 1.15
 */
+(int) getGameData:(NSArray*)_keys Key:(NSString*)_key Delegate:(id)_delegate;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 @brief This function retrieves persistent data for a friend of the current user from the Fuse server.
 @details Similar to getGameData:Delegate: except with a parameter to specify "Fuse ID", this function retrieves information on another user (not the user signed in on the device).  Typically, this is used for social features such as visiting another user's "world".
 
 @code
 
 [FuseAPI getGameData:[NSMutableArray arrayWithObjects:@"max_speed", @"total_power", nil] FuseID:@"012345678" Delegate:self]; 
 
 @endcode
 
 @param _keys [NSArray*] The subset of keys to request from the server.  If 'nil' all key->value pairs will be returned.
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player for which the data is being fetched.
 @param _delegate [id] The \<FuseGameDataDelegate\> to which the callback will be made when data has returned to the device.
 @retval [int] This is the request ID.
 @see getGameData:Delegate: to understand how to use the game data aspects of the function, and how to register the \<FuseGameDataDelegate\> delegate.
 @since Fuse API version 1.15
 *///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
+(int) getFriendGameData:(NSArray*)_keys FuseID:(NSString*)_fuse_id Delegate:(id)_delegate;

/*!
 @brief This function retrieves persistent data for a friend of the current user from the Fuse server while specifying a master key value for the key->value pairs in the dictionary.
 @details Similar to getFriendGameData:FuseID:Delegate:, this function allows for the specification of a master key.
 
 @code
 
 // Example with Key == nil to get all information for the "car_info" master key
 [FuseAPI getGameData:nil Key:@"car_info" FuseID:@"012345678" Delegate:self]; 
 
 @endcode
 
 @param _keys [NSArray*] The subset of keys to request from the server.  If 'nil' all key->value pairs will be returned.
 @param _key [NSString*] This is the master key name.
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player for which the data is being fetched.
 @param _delegate [id] The \<FuseGameDataDelegate\> to which the callback will be made when data has returned to the device.
 @retval [int] This is the request ID.
 @see getGameData:Delegate: to understand how to use the game data aspects of the function, and how to register the \<FuseGameDataDelegate\> delegate.
 @see 
 @since Fuse API version 1.15
 */
+(int) getFriendGameData:(NSArray*)_keys Key:(NSString*)_key FuseID:(NSString*)_fuse_id Delegate:(id)_delegate;

/*!
 @brief This function returns the public 'Fuse ID'.
 @details After a user has registered a login for one of the supported services (i.e. Game Center, etc), a 9-digit 'Fuse ID' is generated that uniquely identifies the user.  This ID can be passed between users as a public ID for the Fuse system so that user's can interact (i.e. invite as friends, etc.) without exposing confidential account information.
 
 @code
 
 NSString *my_fuse_id = [FuseAPI getFuseID];
 
 @endcode
 
 @see gameCenterLogin: for more information on how to register a login with a Game Center ID
 @see facebookLogin: for more information on how to register a login with a Facebook account ID
 @see twitterLogin: for more information on how to register a login with a Twitter account ID
 @see openFeintLogin: for more information on how to register a login with an OpenFeint account ID
 @see fuseLogin:Alias: for more information on how to register a login with a Fuse ID
 @retval [NSString*] The 9-digit Fuse ID.  This ID is strictly comprised of integers, but *do not* cast this value to an integer because a valid ID could have leading zeroes.
 @since Fuse API version 1.21
 */
+(NSString*) getFuseID;

#pragma mark Friends List Actions
/*!
 * @brief Get a the user's friends list
 * @details Once a user has signed in with one of the supported account services (for instance Game Center using gameCenterLogin:), they will have a a friends list which is retrievable from the server.  This list is composed of any friends that they have invited.  This process is asynchronous in that it is required to go the the Fuse servers to retrieve the list.  Therefore, the result is returned in a callback (FuseDelegate::friendsListUpdated:) to the \<FuseDelegate\> delegate registered in startSession:Delegate:.
 
 To request the list:
 
 @code
 
 [FuseAPI updateFriendsListFromServer];
 
 @endcode
 
 The result is returned to the \<FuseDelegate\> callback:
 
 @code
 
 -(void) friendsListUpdated:(NSDictionary*)_friendsList
 {
    // The friends list has returned to the device
 
    // sample code to parse the dictionary
    NSArray *fuse_ids = [_friendsList allKeys];
 
    for (int i = 0; i < [fuse_ids count]; i++)
    {
        NSString *fuse_id = [fuse_ids objectAtIndex:i];
 
        NSDictionary *friendEntry = (NSDictionary*)[_friendsList objectForKey:fuse_id];
 
        NSLog(@"Friend is Pending: %d", [[friendEntry objectForKey:@"pending"] intValue]);
        NSLog(@"Friend Alias: %@", [friendEntry objectForKey:@"alias"]);
        NSLog(@"Friend Fuse ID: %@", [friendEntry objectForKey:@"fuse_id"]);
    }
 }
 
 -(void) friendsListError:(NSNumber*)_error
 {
    // An error has occurred in getting the friends list
    // See kFuseFriendsListErrors for more information on all of the possible error codes
 }
 
 @endcode
 
 @see getFriendsList for more information on getting the local version of the friends list
 @see startSession:Delegate: for information on setting up the \<FuseDelegate\>
 @see FuseDelegate::friendsListError: for more information of the delegate function
 @since Fuse API version 1.22
 */
+(void) updateFriendsListFromServer;
 
/*!
 * @brief This function returns the local friends list of the logged in user
 * @details Similar to updateFriendsListFromServer, this function merely returns the local copy of the friends list.  The local version of the list can differ from the server version in two ways.  Firstly, friends could accept an invite in another device, clearing both users to show up in each other's list.  This server updated is not signaled to the client devices.  Secondly, given that there are propagation delays and HTTP request ordering issues, any "action" (adding a friend, accepting or deleting) will take a few seconds to reach the server. A request to get the server version very close to one of these actions could result in the list not being representative of the final list.
 
 To get the local friends list:
 
 @code
 
 NSMutableDictionary *local_friends_list = [FuseAPI getFriendsList];
 
 @endcode
 
 @retval [NSMutableDictionary*] The local version of the user's friends list
 @see updateFriendsListFromServer for more information on retrieving the list from the Fuse servers and the format of the friends list dictionary
 @since Fuse API version 1.22
 */
+(NSMutableDictionary*) getFriendsList;

/*!
 * @brief This function is used to invite (add) a friend to the logged in user's friends list
 * @details A friend is not added right away to the inviting user's list.  Instead, there is a handshaking mechanism whereby the invited user needs to agree to the invite (see acceptFriend:) before both users are shown in each others list.  If a \<FuseDelegate\> has been registered using startSession:Delegate:, a callback can be made once the friend is added (in case a notification is required by the application).
 
 To add a friend:
 
 @code
 
 [FuseAPI addFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code
 
 -(void) friendAdded:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    // A friend has been added
    // If [error intValue] != 0, an error has occurred
    // Please see kFuseAddFriendErrors for more information on all of the possible error codes
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being invited
 @see startSession:Delegate: for information on setting up the \<FuseDelegate\>
 @see FuseDelegate::friendAdded:Error: for more information on the delegate function
 @since Fuse API version 1.22
 */
+(void) addFriend:(NSString*)_fuse_id;

/*!
 * @brief This function is used to delete a friend from the logged in user's friends list
 * @details Once a friend is removed by a user, both the target and source user will not show in each other's friends list.  If a \<FuseDelegate\> has been registered using startSession:Delegate:, a callback can be made once the friend is removed (in case a notification is required by the application).
 
 To remove a friend:
 
 @code
 
 [FuseAPI removeFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code
 
 -(void) friendRemoved:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    // A friend has been removed
    // If [error intValue] != 0, an error has occurred
    // Please see kFuseRemoveFriendErrors for more information on all of the possible error codes
 }

 @endcode
  
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being deleted
 @see startSession:Delegate: for information on setting up the \<FuseDelegate\>
 @see FuseDelegate::friendRemoved:Error: for more information on the delegate function
 @since Fuse API version 1.22
 */
+(void) removeFriend:(NSString*)_fuse_id;

/*!
 * @brief This function is used to accept a friend request
 * @details The inviting of a friend is a two-step process.  The first step is to actually invite the user (source user) using addFriend:, and the second step is the acceptance by the target user using this function.  If a \<FuseDelegate\> has been registered using startSession:Delegate:, a callback can be made once the friend is accepted (in case a notification is required by the application).

 To accept a user:
 
 @code
 
 [FuseAPI acceptFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code
 
 -(void) friendAccepted:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    // A friend has been marked as accepted on the server
    // If [error intValue] != 0, an error has occurred
    // Please see kFuseAcceptFriendErrors for more information on all of the possible error codes
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being accepted
 @see startSession:Delegate: for information on setting up the \<FuseDelegate\>
 @see addFriend: for more information on adding a friend and the handshaking process
 @see FuseDelegate::friendAccepted:Error: for more information on the delegate function
 @since Fuse API version 1.22
 */
+(void) acceptFriend:(NSString*)_fuse_id;

/*!
 * @brief This function is used to reject a friend request.
 * @details In the second half of the inviting a friend process, the user can optionally accept or reject a friend request.  If this user decides that they do not want the source user as a friend, call this function to remove it from the list of pending requests.  If a \<FuseDelegate\> has been registered using startSession:Delegate:, a callback can be made once the friend is accepted (in case a notification is required by the application).  A friend is in the second phase of the process if it is indicated as 'pending' in the updateFriendsListFromServer callback friendsListUpdated:.
 
 To reject a friend:
 
 @code
 
    [FuseAPI rejectFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code

 -(void) friendRejected:(NSString*)_fuse_id Error:(NSNumber*)_error
 {
    // A friend has been marked as rejected on the server
    // If [error intValue] != 0, an error has occurred
    // Please see kFuseRejectFriendErrors for more information on all of the possible error codes
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being rejected
 @see startSession:Delegate: for information on setting up the \<FuseDelegate\>
 @see addFriend: for more information on adding a friend and the handshaking process
 @see FuseDelegate::friendRejected:Error: for more information on the delegate function
 @since Fuse API version 1.22
 */
+(void) rejectFriend:(NSString*)_fuse_id;

#pragma mark Chat List Actions
/*!
 * @brief Post a chat message to another user
 * @details Using this system, users can communicate with one another inside the game.  Messages are posted to the Fuse system and requested by game clients when needed.  The user must have logged in to one of the supported account services (for instance Game Center using gameCenterLogin:) in order to have an assigned Fuse ID.  A message can be a maximum of 256 characters.  This system would most likely be used in conjunction with another social tool, such as the friend's list (see getFriendsList), where a list of users and their associated Fuse IDs would be known. A friend is in the second phase of the process if it is indicated as 'pending' in the updateFriendsListFromServer callback friendsListUpdated:.
 
 To post a message:
 
 @code
 
 [FuseAPI postUserChatMessage:@"Hey - I am chatting to you right now" TargetUser:@"012345678"];
 
 @endcode
 

 @param _message [NSString*] The message to be sent to the user (max 256 characters)
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player to which the message will be sent
 @since Fuse API version 1.22
 */
+(void) postUserChatMessage:(NSString*)_message TargetUser:(NSString*)_fuse_id; 

/*!
 * @brief Post a chat message to another user indicating the user's level
 * @details Similar to postUserChatMessage:TargetUser:, this function also adds the ability to send the user's current level along with the data.  This would only be done if it is desired to show a user level along with the message.
 
 To post a message:
 
 @code
 
 [FuseAPI postUserChatMessage:@"Hey - I am chatting to you right now" TargetUser:@"012345678" Level:5];
 
 @endcode
 
 @param _message [NSString*] The message to be sent to the user (max 256 characters)
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player to which the message will be sent
 @param _level [int] The level of the user in the game
 @see postUserChatMessage:TargetUser: for more information on posting a message
 @since Fuse API version 1.22
 */
+(void) postUserChatMessage:(NSString*)_message TargetUser:(NSString*)_fuse_id Level:(int)_level; 

/*! 
 * @brief Get the authoritative chat list from the server.
 * @details This function ignores the current local chat list and returns the server-side version of the chat list.  When the list arrives to the client, the old local chat list will be overwritten.  This function is asynchronous in that a list will not be returned to the user until it is received by the client from the server, and at that point in the form of a callback to the \<FuseDelegate\>.
 
 To perform the request:
 
 @code
 
 // To get the chat list for the signed in user (local)
 [FuseAPI getUserChatListFromServer:[FuseAPI getFuseID]];
 
 // To get the chat list for another user
 [FuseAPI getUserChatListFromServer:@"0123456789"];
 
 @endcode
 
 This function will return the data in the form of a NSDictionary* to the chatListReceived:User: function on the registered \<FuseDelegate\>:
 
 @code
 
 -(void) chatListReceived:(NSDictionary*)_messages User:(NSString*)_fuse_id
 {
    // Sample code for parsing the dictionary returned
    if (_messages != nil && [_messages count] > 0)
    {
        NSArray *keys = [_messages allKeys];
        NSArray *sortedKeys = [keys sortedArrayUsingSelector:@selector(compare:)];
 
        for (int i = 0; i < [sortedKeys count]; i++)
        {
            NSString *message_id = [sortedKeys objectAtIndex:i];
            NSMutableDictionary *entry = (NSMutableDictionary*)[_messages objectForKey:message_id];
 
            NSLog(@"%d [%@]: %@ -> %@", [message_id intValue], [entry objectForKey:@"timestamp"], [entry objectForKey:@"alias"], [entry objectForKey:@"message"]);
        }
    }
 }
 
 -(void) chatListError:(NSNumber*)_error
 {
    // An error has occurred
    // Refer to kFuseChatErrors for information of the error cases possible 
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player for which the chat list is desired
 @see getFuseID for more information on getting the local users Fuse ID
 @see startSession:Delegate: for information on setting up the \<FuseDelegate\>
 @see getUserChatList: for information on requesting the local copy of the chat list
 @see chatListError: for more information on the delegate function
 @since Fuse API version 1.22
 */
+(void) getUserChatListFromServer:(NSString*)_fuse_id;

/*!
 * @brief Get the local chat list stored in the client
 * @details Similar to getUserChatListFromServer:, this function directly returns the local version of the chat list.  The difference from the local and client version is twofold.  The first difference is that the client does not receive asynchronous update events to update it's list (i.e. if another user has messaged and that message has been recorded on the server, the server does not message the client about the update).  The second difference is that if a user posts to their chat list, the change takes a few seconds to be reflected on the server.  In this case, if a read is initiated directly after posting a message, due to the non-guaranteed order of HTTP requests, the returned list might exclude the recently posted message.  If getUserChatListFromServer: has not been called, there will be no local copy of the list.
 
 To request the local list:
 
 @code
 
 // To request the chat list for the signed in user (local)
 NSMutableDictionary* chat_list = [FuseAPI getUserChatList:[FuseAPI getFuseID]];
 
 // To request another user's chat list
 NSMutableDictionary* chat_list = [FuseAPI getUserChatList:@"0123456768"];
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player for which the chat list is desired
 @retval [NSMutableDictionary*] The local copy of the chat list for the desired player
 @see getUserChatListFromServer: for information on requesting the local message list
 @see getFuseID for more information on getting the local users Fuse ID
 @since Fuse API version 1.22
 */
+(NSMutableDictionary*) getUserChatList:(NSString*)_fuse_id;

/*!
 * @brief Delete a chat message.
 * @details This function will delete a chat message for a specified user.  The "message ID" must be specified (obtained when fetching a message list - see getUserChatListFromServer:).
 
 To delete a message:
 
 @code
 
 [FuseAPI deleteUserChatMessage:2 TargetUser:@"012345678"];
 
 @endcode
 
 @param _message_id [int] The ID of the message to be deleted
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player for which the message will be deleted
 @see getUserChatListFromServer: for information of fetching a request list from the server
 @since Fuse API version 1.22
 */
+(void) deleteUserChatMessage:(int)_message_id TargetUser:(NSString*)_fuse_id;

#pragma mark User-to-User Push Notifications
/*!
 * @brief Send an Apple push notification to another user
 * @details Use this function to send a push notification to another user, using their Fuse ID to each of their devices that have logged in with that ID (and that have registered to receive notifications).  This system is dependent upon both the sender and the recipient to have logged in with either a Game Center account (see gameCenterLogin:) or similar function.  This system would most likely be used in conjunction with another social tool, such as the friend's list (see getFriendsList), where a list of users and their associated Fuse IDs would be known.  Messages can be no longer than 256 characters in length.
 
 To send a push notification:
 
 @code
 
 [FuseAPI userPushNotification:@"012345678" Message:@"Test Push Message"];
 
 @endcode
 
 Please ensure that your game is registered to collect device tokens and receive push notifications to be compatible with this feature.  The can be up to a five minute delay or longer before a message is received by a user.
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player to which the message will be sent
 @param _message [NSString*] The message to be sent to the user (max 256 characters)
 @see gameCenterLogin: for more information on how to register a login with a Game Center ID
 @see facebookLogin: for more information on how to register a login with a Facebook account ID
 @see twitterLogin: for more information on how to register a login with a Twitter account ID
 @see openFeintLogin: for more information on how to register a login with an OpenFeint account ID
 @see fuseLogin:Alias: for more information on how to register a login with a Fuse ID 
 @since Fuse API version 1.22
 */
+(void) userPushNotification:(NSString*)_fuse_id Message:(NSString*)_message;

/*!
 * @brief Send an Apple push notification to a user's entire friends list
 * @details Similar to userPushNotification:Message:, this function sends the same message to each user in the source user's friends list.

 To send this type of push notification:
 
 @code
 
 [FuseAPI friendsPushNotification:@"Test Push Message Friends List"];
 
 @endcode
 
 @param _message [NSString*] The message to be sent to the user (max 256 characters)
 @see userPushNotification:Message: for more information on sending a push notification 
 @since Fuse API version 1.22
 */
+(void) friendsPushNotification:(NSString*)_message;

#pragma mark Game Configuration Data
/*!
 @brief This function retrieves server configuration values (deprecated: see getGameConfigurationValue:)
 
 @deprecated This is deprecated as of version 1.22 because it is conflicting with the definition in NSValue.  Will be removed in 1.23.  See getGameConfigurationValue: for the updated function usage.
 */
+(NSString*) getValue:(NSString*)_key __attribute__((deprecated));

/*!
 @brief This function retrieves server configuration values.
 @details The Fuse API provides a method to store game configuration variables that are provided to the application on start.  These are different than "Game Data" values since they are stored on a per-game basis, and not a per-user basis.
 
 In the Fuse dashboard, navigate to the 'configuration' tab in your game view.  You can edit the "Game Data" section by adding keys and associated data values.  Values can be 256 characters in length and support UTF-8 characters.
 
 @code
 
 NSString *my_val = [FuseAPI getGameConfigurationValue:@"my_key"];
 
 if (my_val != nil)
 {
    // always check against 'nil' before using the value
 }
 
 @endcode
 
 Values are update in the client each time a session is started from the Springboard or system tray. To find out when values are valid in the device, you can use the following \<FuseDelegate\> callback function that indicates when the values are ready to be inspected.
 
 @code
 
 BOOL has_game_config_returned = NO;
 
 -(void) gameConfigurationReceived
 {
    has_game_config_returned = YES;
 
    // You can now access your server-side data, either here or somewhere else in your code
    NSString *funny_val = [FuseAPI getGameConfigurationValue:@"not_funny"];
 }
 
 @endcode
 
 It is recommended that a default value be present on the device in case the user has not or never connects to the Internet.
 
 @param _key [NSString*] This is the key for which the value is requested.
 @retval [NSString*] This is the value for the corresponding key.
 @see startSession:Delegate: for information on setting up the \<FuseDelegate\>
 */
+(NSString*) getGameConfigurationValue:(NSString*)_key;

#pragma mark Public Methods
/*!
 * @brief Singleton accessor for the FuseAPI class
 * @retval [FuseAPI*] The Fuse API singleton instance
 */
+(FuseAPI *) get;

@end
