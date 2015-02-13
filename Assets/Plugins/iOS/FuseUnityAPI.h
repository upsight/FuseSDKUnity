
#pragma mark Initialization

void FuseAPI_Initialize();
void FuseAPI_RegisterCrash(NSException* exception);
void FuseAPI_SetUnityGameObject(const char* unityGameObject);

#pragma mark - Session Creation

void FuseAPI_StartSession(const char* gameId);

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
void FuseAPI_RegisterUnibillPurchase(const char* productId, const char* receipt, int receiptLength);

#pragma mark - Fuse Interstitial Ads

void FuseAPI_PreloadAdForZone(const char * _adZone);
void FuseAPI_CheckAdAvailable(const char * _adZone);
void FuseAPI_ShowAd(const char * _adZone);

#pragma mark - Notifications

void FuseAPI_RegisterPushToken(Byte* token, int size);
void FuseAPI_DisplayNotifications();
bool FuseAPI_IsNotificationAvailable();
void FuseAPI_NotificationAction(const char* action);

#pragma mark - More Games

void FuseAPI_DisplayMoreGames();

#pragma mark - Gender

void FuseAPI_RegisterGender(int gender);

#pragma mark - Account Login

void FuseAPI_GameCenterLogin();
void FuseAPI_FacebookLogin(const char* facebookId, const char* name, const char* accessToken);
void FuseAPI_FacebookLoginGender(const char* facebookId, const char* name, int gender, const char* accessToken);
void FuseAPI_TwitterLogin(const char* twitterId);
void FuseAPI_DeviceLogin(const char* alias);
void FuseAPI_FuseLogin(const char* fuseId, const char* alias);
const char* FuseAPI_GetOriginalAccountId();
int FuseAPI_GetOriginalAccountType();
void FuseAPI_GooglePlayLogin(const char* alias, const char* token);
const char* FuseAPI_GetOriginalAccountAlias();

#pragma mark - Miscellaneous

int FuseAPI_GamesPlayed();
const char* FuseAPI_LibraryVersion();
bool FuseAPI_Connected();
void FuseAPI_TimeFromServer();
bool FuseAPI_NotReadyToTerminate();

#pragma mark - Data Opt In/Out

void FuseAPI_EnableData(bool enable);
bool FuseAPI_DataEnabled();

const char* FuseAPI_GetFuseId();

#pragma mark - Friend List

void FuseAPI_AddFriend(const char* fuseId);
void FuseAPI_RemoveFriend(const char* fuseId);
void FuseAPI_AcceptFriend(const char* fuseId);
void FuseAPI_RejectFriend(const char* fuseId);
void FuseAPI_UpdateFriendsListFromServer();
void FuseAPI_MigrateFriends(const char* fuseId);
int FuseAPI_GetFriendsListCount();
const char* FuseAPI_GetFriendsListFriendFuseId(int index);
const char* FuseAPI_GetFriendsListFriendAccountId(int index);
const char* FuseAPI_GetFriendsListFriendAlias(int index);
bool FuseAPI_GetFriendsListFriendPending(int index);


#pragma mark - User-to-User Push Notifications

void FuseAPI_UserPushNotification(const char* fuseId, const char* message);
void FuseAPI_FriendsPushNotification(const char* message);

#pragma mark - Game Configuration Data

const char* FuseAPI_GetGameConfigurationValue(const char* key);

#pragma mark - Specific Event Registration

void FuseAPI_RegisterLevel(int level);
void FuseAPI_RegisterCurrency(int type, int balance);
void FuseAPI_RegisterAge(int age);
void FuseAPI_RegisterBirthday(int year, int month, int day);

#pragma mark - Callback

@interface FuseAPI_Delegate : NSObject<FuseDelegate, FuseAdDelegate, FuseOverlayDelegate, FuseGameDataDelegate, SKProductsRequestDelegate>
{
	
}

@end

