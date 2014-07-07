
#pragma mark Initialization

void FuseAPI_Initialize();
void FuseAPI_RegisterCrash(NSException* exception);

#pragma mark - Session Creation

void FuseAPI_StartSession(const char* gameId);
void FuseAPI_SessionStartReceived();
void FuseAPI_SessionLoginError(int error);

#pragma mark - Analytic Event

void FuseAPI_RegisterEventWithDictionary(const char* message, const char** keys, const char** attributes, int numValues);
void FuseAPI_RegisterEvent(const char* message);
void FuseAPI_RegisterEventStart();
void FuseAPI_RegisterEventKeyValue(const char* entryKey, double entryValue);
int FuseAPI_RegisterEventEnd(const char* name, const char* paramName, const char* paramValue);
int FuseAPI_RegisterEventVariable(const char* name, const char* paramName, const char* paramValue, const char* variableName, double variableValue);

#pragma mark - In-App Purchase Logging

@interface FuseAPI_Product : NSObject
{
	
}

@property(nonatomic, copy) NSString* productIdentifier;
@property(nonatomic, assign) NSLocale *priceLocale;
@property(nonatomic, assign) NSDecimalNumber *price;

-(void)setFloatPrice:(float)price;
-(void)setLocale:(const char*)locale;

@end

@interface FuseAPI_ProductsResponse : NSObject
{
	
}

@property(nonatomic, assign) NSMutableArray *products;

@end

void FuseAPI_RegisterInAppPurchaseListStart();
void FuseAPI_RegisterInAppPurchaseListProduct(const char* productId, const char* priceLocale, float price);
void FuseAPI_RegisterInAppPurchaseListEnd();

@interface FuseAPI_Payment : NSObject
{
	
}

@property(nonatomic, copy) NSString* productIdentifier;

-(void)setIdentifier:(const char*)identifier;

@end

@interface FuseAPI_PaymentTransaction : NSObject
{
	
}

@property(nonatomic, assign) FuseAPI_Payment* payment;
@property(nonatomic, assign) NSData* transactionReceipt;
@property(nonatomic, assign) SKPaymentTransactionState transactionState;
@property(nonatomic, assign) NSString* transactionIdentifier;

-(void)setTransactionReceiptWithBuffer:(void*)transactionReceiptBuffer length:(int)transactionReceiptLength;

@end

void FuseAPI_RegisterInAppPurchase(const char* productId, const char* transactionId, const unsigned char* transactionReceiptBuffer, int transactionReceiptLength, int transactionState);
void FuseAPI_PurchaseVerification(bool verified, const char* transactionId, const char* originalTransactionId);

#pragma mark - Fuse Interstitial Ads

void FuseAPI_CheckAdAvailable();
void FuseAPI_ShowAd();
void FuseAPI_AdAvailabilityResponse(int available, int error);
void FuseAPI_AdWillClose();

#pragma mark - Notifications

void FuseAPI_RegisterPushToken(Byte* token, int size);
void FuseAPI_DisplayNotifications();
bool FuseAPI_IsNotificationAvailable();
void FuseAPI_NotificationAction(const char* action);

#pragma mark - More Games

void FuseAPI_DisplayMoreGames();
void FuseAPI_OverlayWillClose();

#pragma mark - Gender

void FuseAPI_RegisterGender(int gender);

#pragma mark - Account Login

void FuseAPI_GameCenterLogin();
void FuseAPI_FacebookLogin(const char* facebookId, const char* name, const char* accessToken);
void FuseAPI_FacebookLoginGender(const char* facebookId, const char* name, int gender, const char* accessToken);
void FuseAPI_TwitterLogin(const char* twitterId);
void FuseAPI_DeviceLogin(const char* alias);
void FuseAPI_OpenFeintLogin(const char* openFeintId);
void FuseAPI_FuseLogin(const char* fuseId, const char* alias);
const char* FuseAPI_GetOriginalAccountId();
int FuseAPI_GetOriginalAccountType();
void FuseAPI_AccountLoginComplete(int type, const char* accountId);
void FuseAPI_GooglePlayLogin(const char* _id, const char* alias, const char* token);
const char* FuseAPI_GetOriginalAccountAlias();

#pragma mark - Miscellaneous

int FuseAPI_GamesPlayed();
const char* FuseAPI_LibraryVersion();
bool FuseAPI_Connected();
void FuseAPI_TimeFromServer();
void FuseAPI_TimeUpdated(long long timestamp);
bool FuseAPI_NotReadyToTerminate();

#pragma mark - Data Opt In/Out

void FuseAPI_EnableData(bool enable);
bool FuseAPI_DataEnabled();

#pragma mark - User Game Data

void FuseAPI_SetGameDataStart(const char* key, bool isCollection, const char* fuseId);
void FuseAPI_SetGameDataKeyValue(const char* key, const char* value, bool isBinary);
int FuseAPI_SetGameDataEnd();
void FuseAPI_GameDataSetAcknowledged(int requestId);

void FuseAPI_GameDataError(int error, int requestId);

void FuseAPI_GetGameDataStart(const char* key, const char* fuseId);
void FuseAPI_GetGameDataKey(const char* key);
int FuseAPI_GetGameDataEnd();
void FuseAPI_GameDataReceivedStart(const char* fuseId, const char* key, int requestId);
void FuseAPI_GameDataReceivedKeyValue(const char* key, const char* value, bool isBinary);
void FuseAPI_GameDataReceivedEnd();
void FuseAPI_RefreshGameConfiguration();


const char* FuseAPI_GetFuseId();

#pragma mark - Friend List

void FuseAPI_AddFriend(const char* fuseId);
void FuseAPI_RemoveFriend(const char* fuseId);
void FuseAPI_AcceptFriend(const char* fuseId);
void FuseAPI_RejectFriend(const char* fuseId);
void FuseAPI_FriendAdded(const char* fuseId, int error);
void FuseAPI_FriendRemoved(const char* fuseId, int error);
void FuseAPI_FriendAccepted(const char* fuseId, int error);
void FuseAPI_FriendRejected(const char* fuseId, int error);
void FuseAPI_UpdateFriendsListFromServer();
void FuseAPI_FriendsListUpdatedStart();
void FuseAPI_FriendsListUpdatedFriend(const char* fuseId, const char* accountId, const char* alias, bool pending);
void FuseAPI_FriendsListUpdatedEnd();
void FuseAPI_FriendsListError(int error);
void FuseAPI_MigrateFriends(const char* fuseId);
void FuseAPI_FriendsMigrated(const char* fuseId, int error);
int FuseAPI_GetFriendsListCount();
const char* FuseAPI_GetFriendsListFriendFuseId(int index);
const char* FuseAPI_GetFriendsListFriendAccountId(int index);
const char* FuseAPI_GetFriendsListFriendAlias(int index);
bool FuseAPI_GetFriendsListFriendPending(int index);

#pragma mark - Chat List

#pragma mark - User-to-User Push Notifications

void FuseAPI_UserPushNotification(const char* fuseId, const char* message);
void FuseAPI_FriendsPushNotification(const char* message);

#pragma mark - Gifting

void FuseAPI_GetMailListFriendFromServer(const char* fuseId);
void FuseAPI_MailListReceivedStart(const char* fuseId);
void FuseAPI_MailListReceivedMail(int messageId, long long timestamp, const char* alias, const char* message, int giftId, const char* giftName, int giftAmount);
void FuseAPI_MailListReceivedEnd();
void FuseAPI_MailListError(int error);
int FuseAPI_GetMailListCount(const char* fuseId);
int FuseAPI_GetMailListMailMessageId(const char* fuseId, int index);
long long FuseAPI_GetMailListMailTimestamp(const char* fuseId, int index);
const char* FuseAPI_GetMailListMailAlias(const char* fuseId, int index);
const char* FuseAPI_GetMailListMailMessage(const char* fuseId, int index);
int FuseAPI_GetMailListMailGiftId(const char* fuseId, int index);
const char* FuseAPI_GetMailListMailGiftName(const char* fuseId, int index);
int FuseAPI_GetMailListMailGiftAmount(const char* fuseId, int index);
void FuseAPI_SetMailAsReceived(int messageId);
int FuseAPI_SendMail(const char* fuseId, const char* message);
int FuseAPI_SendMailWithGift(const char* fuseId, const char* message, int giftId, int giftAmount);
void FuseAPI_MailAcknowledged(int messageId, const char* fuseId, int requestId);
void FuseAPI_MailError(int error);

#pragma mark - Game Configuration Data

const char* FuseAPI_GetGameConfigurationValue(const char* key);
void FuseAPI_GameConfigurationReceived();

#pragma mark - Specific Event Registration

void FuseAPI_RegisterLevel(int level);
void FuseAPI_RegisterCurrency(int type, int balance);
void FuseAPI_RegisterFlurryView();
void FuseAPI_RegisterFlurryClick();
void FuseAPI_RegisterTapjoyReward(int amount);
void FuseAPI_RegisterAge(int age);
void FuseAPI_RegisterBirthday(int year, int month, int day);

#pragma mark - Callback

@interface FuseAPI_Delegate : NSObject<FuseDelegate, FuseAdDelegate, FuseOverlayDelegate, FuseGameDataDelegate>
{
	
}

@end

