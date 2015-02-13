
#import "FuseSDK.h"
#import "FuseUnityAPI.h"
#import "NSData-Base64.h"

#define FuseSafeRelease(obj) if (obj != nil){ \
[obj release]; \
obj = nil;}

static char *fuseGameObject = "FuseSDK";
static const char *nilString = "";
static FuseAPI_ProductsResponse* _FuseAPI_productsResponse = nil;

static NSString* _FuseAPI_gameDataKey = nil;
static NSString* _FuseAPI_gameDataFuseId = nil;
static BOOL _FuseAPI_gameDataIsCollection = NO;
static NSMutableDictionary* _FuseAPI_gameData = nil;
static NSMutableArray* _FuseAPI_gameDataKeys = nil;
static NSMutableDictionary* _FuseAPI_registerEventData = nil;

static FuseAPI_Delegate* _FuseAPI_delegate = nil;

#pragma mark Initialization

void FuseAPI_Initialize()
{
	NSSetUncaughtExceptionHandler(&FuseAPI_RegisterCrash);
	
	_FuseAPI_delegate = [FuseAPI_Delegate new];
}

void FuseAPI_RegisterCrash(NSException* exception)
{
	[FuseAPI registerCrash:exception];
}

void FuseAPI_SetUnityGameObject(const char* unityGameObject)
{
    char* fuseGameObject = (char*)malloc(strlen(unityGameObject) + 1);
    strcpy(fuseGameObject, unityGameObject);
//	fuseGameObject = unityGameObject;
}

void CallUnity(const char* methodName, const char* param)
{
	if(param == nil)
	{
		param = nilString;
	}
	UnitySendMessage(fuseGameObject, methodName, param);
}

#pragma mark - Session

void FuseAPI_StartSession(const char* gameId)
{
	FuseAPI_Initialize();
	[FuseAPI setPlatform:@"unity-ios"];
	[FuseAPI startSession:[NSString stringWithUTF8String:gameId] Delegate:_FuseAPI_delegate AutoRegisterForPush:NO];    
}

#pragma mark - Analytics

void FuseAPI_RegisterEventWithDictionary(const char* message, const char** keys, const char** attributes, int numValues)
{
    // printf("RegisterEventWithDictionary\n");
    // printf("Message: %s\n", message);
    // printf("Num Values: %i\n", numValues);
    NSMutableDictionary* values = [[NSMutableDictionary alloc] initWithCapacity:numValues];
    for( int i = 0; i < numValues; i++ )
    {
        // printf("%i) key: %s, value: %s", i, keys[i], attributes[i]);
        [values setObject:[NSString stringWithUTF8String:attributes[i]] forKey:[NSString stringWithUTF8String:keys[i]]];
    }
    [FuseAPI registerEvent:[NSString stringWithUTF8String:message] withDict:values];
    FuseSafeRelease(values);
}

void FuseAPI_RegisterEvent(const char* message)
{
	[FuseAPI registerEvent:[NSString stringWithUTF8String:message]];
}

void FuseAPI_RegisterEventStart()
{
	_FuseAPI_registerEventData = [[NSMutableDictionary alloc] init];
}

void FuseAPI_RegisterEventKeyValue(const char* entryKey, double entryValue)
{
	[_FuseAPI_registerEventData setValue:[NSNumber numberWithDouble:entryValue] forKey:[NSString stringWithUTF8String:entryKey]];
}

int FuseAPI_RegisterEventEnd(const char* name, const char* paramName, const char* paramValue)
{
	int result = [FuseAPI registerEvent:[NSString stringWithUTF8String:name] ParameterName:[NSString stringWithUTF8String:paramName] ParameterValue:[NSString stringWithUTF8String:paramValue] Variables:_FuseAPI_registerEventData];
    FuseSafeRelease(_FuseAPI_registerEventData);
	return result;
}

int FuseAPI_RegisterEventVariable(const char* name, const char* paramName, const char* paramValue, const char* variableName, double variableValue)
{
	return [FuseAPI registerEvent:[NSString stringWithUTF8String:name] ParameterName:[NSString stringWithUTF8String:paramName] ParameterValue:[NSString stringWithUTF8String:paramValue] VariableName:[NSString stringWithUTF8String:variableName] VariableValue:[NSNumber numberWithDouble:variableValue]];
}

#pragma mark - In-App Purchase Logging

@implementation FuseAPI_Product

-(id)init
{
    self = [super init];
    
	if (self)
    {
        _price = NULL;
        _priceLocale = NULL;
    }
    
    return self;
}

-(void)setFloatPrice:(float)price
{
    if( _price == NULL )
    {
        _price = [NSDecimalNumber decimalNumberWithDecimal:[[NSNumber numberWithFloat:price] decimalValue]];
    }
}

-(void)setLocale:(const char *)locale
{
    if( _priceLocale == NULL )
    {
        NSString* formattedLocale = [NSString stringWithFormat:@"en_CA@currency=%@", [NSString stringWithUTF8String:locale]];
        _priceLocale = [[NSLocale alloc] initWithLocaleIdentifier:formattedLocale];
    }
}

-(void)dealloc
{
    [super dealloc];
}

@end

@implementation FuseAPI_ProductsResponse

-(id)init
{
    self = [super init];
    
	if (self)
    {
        _products = [[NSMutableArray alloc] init];
    }
    
    return self;
}

-(void)dealloc
{
    FuseSafeRelease(_products);
    [super dealloc];
}

@end

bool g_bRegistering = false;
void FuseAPI_RegisterInAppPurchaseListStart()
{
    if( g_bRegistering )
    {
        //NSLog(@"FuseAPI_productsResponse is already in use!");
        return;
    }
    g_bRegistering = true;
	_FuseAPI_productsResponse = [[FuseAPI_ProductsResponse alloc] init];
}

void FuseAPI_RegisterInAppPurchaseListProduct(const char* productId, const char* priceLocale, float price)
{
	FuseAPI_Product* product = [[FuseAPI_Product alloc] init];
	product.productIdentifier = [NSString stringWithUTF8String:productId];
    [product setLocale:priceLocale];
    [product setFloatPrice:price];
	
	[_FuseAPI_productsResponse.products addObject:product];
    FuseSafeRelease(product);
}

void FuseAPI_RegisterInAppPurchaseListEnd()
{
	[FuseAPI registerInAppPurchaseList:(SKProductsResponse*)_FuseAPI_productsResponse];
    g_bRegistering = false;
    FuseSafeRelease(_FuseAPI_productsResponse);
}

@implementation FuseAPI_Payment

-(id)init
{
    self = [super init];
    
	if (self)
    {
        _productIdentifier = NULL;
    }
    
    return self;
}

-(void)dealloc
{
    FuseSafeRelease(_productIdentifier);
    [super dealloc];
}

-(void)setIdentifier:(const char*)identifier
{
    NSString* temp = [NSString stringWithUTF8String:identifier];
    _productIdentifier = [temp copy];
}

@end

@implementation FuseAPI_PaymentTransaction

-(id)init
{
    self = [super init];
    
	if (self)
    {
        _payment = [[FuseAPI_Payment alloc] init];
        _transactionReceipt = NULL;
    }
    
    return self;
}

-(void)dealloc
{
    FuseSafeRelease(_payment);
    //    FuseSafeRelease(_transactionReceipt);
    [super dealloc];
}

-(void)setTransactionReceiptWithBuffer:(void*)transactionReceiptBuffer length:(int)transactionReceiptLength
{
    if( _transactionReceipt == NULL )
    {
        _transactionReceipt = [[NSData alloc] initWithBytes:(void*)transactionReceiptBuffer length:transactionReceiptLength];
    }
}


@end

void FuseAPI_RegisterInAppPurchase(const char* productId, const char* transactionId, const unsigned char* transactionReceiptBuffer, int transactionReceiptLength, int transactionState)
{
	FuseAPI_PaymentTransaction* paymentTransaction = [[FuseAPI_PaymentTransaction alloc] init];
	[paymentTransaction.payment setIdentifier:productId];
	[paymentTransaction setTransactionReceiptWithBuffer:(void*)transactionReceiptBuffer length:transactionReceiptLength];
	paymentTransaction.transactionState = transactionState;
    paymentTransaction.transactionIdentifier = transactionId != nil ? [NSString stringWithUTF8String:transactionId] : @"No Transaction ID";
	
	[FuseAPI registerInAppPurchase:(SKPaymentTransaction*)paymentTransaction];
    FuseSafeRelease(paymentTransaction);
}

SKProductsRequest* _UniBillRequest = nil;
FuseAPI_PaymentTransaction* _UniBillPayment = nil;
void FuseAPI_RegisterUnibillPurchase(const char* productId, const char* receipt, int receiptLength)
{
    // request the product information for this purchase so we can get the price and currency code
    // we register the IAP when we get the product respnse
    if( [SKPaymentQueue canMakePayments] && _UniBillRequest == nil && _UniBillPayment == nil)
    {
        _UniBillRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:[NSSet setWithObjects:[NSString stringWithUTF8String:productId], nil]];
        _UniBillRequest.delegate = _FuseAPI_delegate;

        [_UniBillRequest start];
    }
    else
    {
        NSLog(@"FUSE ERROR: Could not register IAP because we could not connect to the store");
        return;
    }

    // store the payment information
    // this is released after we register the IAP
    _UniBillPayment = [[FuseAPI_PaymentTransaction alloc] init];
	[_UniBillPayment.payment setIdentifier:productId];
	[_UniBillPayment setTransactionReceiptWithBuffer:(void*)receipt length:receiptLength];
	_UniBillPayment.transactionState = SKPaymentTransactionStatePurchased;
    _UniBillPayment.transactionIdentifier = @"No Transaction ID";
}

#pragma mark - Ads

void FuseAPI_PreloadAdForZone(const char * _adZone)
{
    [FuseAPI preLoadAdForZone:[NSString stringWithUTF8String:_adZone]];
}

void FuseAPI_CheckAdAvailable(const char * _adZone)
{
	[FuseAPI checkAdAvailableWithDelegate:_FuseAPI_delegate withAdZone:[NSString stringWithUTF8String:_adZone]];
}

void FuseAPI_ShowAd(const char * _adZone)
{
	[FuseAPI showAdWithDelegate:_FuseAPI_delegate adZone:[NSString stringWithUTF8String:_adZone]];
}

#pragma mark - Notifications

void FuseAPI_RegisterPushToken(Byte* token, int size)
{
    NSData* deviceToken = [NSData dataWithBytes:token length:size];
    [FuseAPI applicationdidRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
}

void FuseAPI_DisplayNotifications()
{
	[FuseAPI displayNotifications];
}

bool FuseAPI_IsNotificationAvailable()
{
	return [FuseAPI isNotificationAvailable];
}

#pragma mark - More Games

void FuseAPI_DisplayMoreGames()
{
	[FuseAPI displayMoreGames:_FuseAPI_delegate];
}

#pragma mark - Gender

void FuseAPI_RegisterGender(int gender)
{
	[FuseAPI registerGender:gender];
}

#pragma mark - Account Login

void FuseAPI_GameCenterLogin()
{
	[FuseAPI gameCenterLogin:[GKLocalPlayer localPlayer]];
}

void FuseAPI_FacebookLogin(const char* facebookId, const char* name, const char* accessToken)
{
	[FuseAPI facebookLogin:[NSString stringWithUTF8String:facebookId] Name:[NSString stringWithUTF8String:name] withAccessToken:[NSString stringWithUTF8String:accessToken]];
}

void FuseAPI_FacebookLoginGender(const char* facebookId, const char* name, int gender, const char* accessToken)
{
	[FuseAPI facebookLogin:[NSString stringWithUTF8String:facebookId] Name:[NSString stringWithUTF8String:name] Gender:gender withAccessToken:[NSString stringWithUTF8String:accessToken]];
}

void FuseAPI_TwitterLogin(const char* twitterId)
{
	[FuseAPI twitterLogin:[NSString stringWithUTF8String:twitterId]];
}

void FuseAPI_DeviceLogin(const char* alias)
{
	[FuseAPI deviceLogin:[NSString stringWithUTF8String:alias]];
}

void FuseAPI_FuseLogin(const char* fuseId, const char* alias)
{
	[FuseAPI fuseLogin:[NSString stringWithUTF8String:fuseId] Alias:[NSString stringWithUTF8String:alias]];
}

void FuseAPI_GooglePlayLogin(const char* alias, const char* token)
{
    [FuseAPI googlePlayLogin:[NSString stringWithUTF8String:alias] AccessToken:[NSString stringWithUTF8String:token]];
}

const char* FuseAPI_GetOriginalAccountAlias()
{
    NSString* accountAlias = [FuseAPI getOriginalAccountAlias];
	
    const char* string = "";
    if( accountAlias != nil )
    {
        string = [accountAlias UTF8String];
    }

	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

const char* FuseAPI_GetOriginalAccountId()
{
	NSString* accountId = [FuseAPI getOriginalAccountID];
    
	const char* string = "";
    if( accountId != nil )
    {
        string = [accountId UTF8String];
    }

	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

int FuseAPI_GetOriginalAccountType()
{
	return [FuseAPI getOriginalAccountType];
}

const char* FuseAPI_GetFuseId()
{
	NSString* fuseId = [FuseAPI getFuseID];
	
    const char* string = "";
    if( fuseId != nil )
    {
        string = [fuseId UTF8String];
    }
	
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

#pragma mark - Miscellaneous

int FuseAPI_GamesPlayed()
{
	return [FuseAPI gamesPlayed];
}

const char* FuseAPI_LibraryVersion()
{
	NSString* libraryVersion = [FuseAPI libraryVersion];
	
	const char* string = "";
    if(libraryVersion)
    {
        string = [libraryVersion UTF8String];
    }
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

bool FuseAPI_Connected()
{
	return [FuseAPI connected];
}

void FuseAPI_TimeFromServer()
{
	[FuseAPI utcTimeFromServer];
}

bool FuseAPI_NotReadyToTerminate()
{
	return [FuseAPI notReadyToTerminate];
}

#pragma mark - Data Opt In/Out

void FuseAPI_EnableData(bool enable)
{
	if (enable)
	{
		[FuseAPI enableData];
	}
	else
	{
		[FuseAPI disableData];
	}
}

bool FuseAPI_DataEnabled()
{
	return [FuseAPI dataEnabled];
}

#pragma mark - Game Configuration Data

const char* FuseAPI_GetGameConfigurationValue(const char* key)
{
	NSString* value = [FuseAPI getGameConfigurationValue:[NSString stringWithUTF8String:key]];
	
	const char* string = "";
    if(value)
    {
        string = [value UTF8String];
		
	}
    char* copy = (char*)malloc(strlen(string) + 1);
    strcpy(copy, string);
    
    return copy;
}

#pragma mark - Game Data

void FuseAPI_SetGameDataStart(const char* key, bool isCollection, const char* fuseId)
{
	_FuseAPI_gameDataKey = [NSString stringWithUTF8String:key];
	_FuseAPI_gameData = [[NSMutableDictionary alloc] init];
	_FuseAPI_gameDataIsCollection = isCollection;
	_FuseAPI_gameDataFuseId = [NSString stringWithUTF8String:fuseId];
}

void FuseAPI_SetGameDataKeyValue(const char* key, const char* value, bool isBinary)
{
	if (isBinary)
	{
		NSData* buffer = [NSData fuseUnityDataFromBase64String:[NSString stringWithUTF8String:value]];
		[_FuseAPI_gameData setValue:buffer forKey:[NSString stringWithUTF8String:key]];
	}
	else
	{
		[_FuseAPI_gameData setValue:[NSString stringWithUTF8String:value] forKey:[NSString stringWithUTF8String:key]];
	}
}

int FuseAPI_SetGameDataEnd()
{
	int requestId = -1;
	if (_FuseAPI_gameDataKey.length != 0)
	{
		requestId = [FuseAPI setGameData:_FuseAPI_gameData FuseID:_FuseAPI_gameDataFuseId Key:_FuseAPI_gameDataKey Delegate:_FuseAPI_delegate IsCollection:_FuseAPI_gameDataIsCollection];
	}
	else
	{
		requestId = [FuseAPI setGameData:_FuseAPI_gameData Delegate:_FuseAPI_delegate];
	}
	
	_FuseAPI_gameDataFuseId = nil;
	_FuseAPI_gameDataIsCollection = NO;
    FuseSafeRelease(_FuseAPI_gameData);
	_FuseAPI_gameDataKey = nil;
	
	return requestId;
}

void FuseAPI_GetGameDataStart(const char* key, const char* fuseId)
{
	_FuseAPI_gameDataKey = [NSString stringWithUTF8String:key];
	_FuseAPI_gameDataFuseId = [NSString stringWithUTF8String:fuseId];
	_FuseAPI_gameDataKeys = [[NSMutableArray alloc] init];
}

void FuseAPI_GetGameDataKey(const char* key)
{
	[_FuseAPI_gameDataKeys addObject:[NSString stringWithUTF8String:key]];
}

int FuseAPI_GetGameDataEnd()
{
	int requestId = -1;
	if (_FuseAPI_gameDataKey.length != 0)
	{
		if (_FuseAPI_gameDataFuseId.length != 0)
		{
			requestId = [FuseAPI getFriendGameData:_FuseAPI_gameDataKeys Key:_FuseAPI_gameDataKey FuseID:_FuseAPI_gameDataFuseId Delegate:_FuseAPI_delegate];
		}
		else
		{
			requestId = [FuseAPI getGameData:_FuseAPI_gameDataKeys Key:_FuseAPI_gameDataKey Delegate:_FuseAPI_delegate];
		}
	}
	else
	{
		if (_FuseAPI_gameDataFuseId.length != 0)
		{
			requestId = [FuseAPI getFriendGameData:_FuseAPI_gameDataKeys FuseID:_FuseAPI_gameDataFuseId Delegate:_FuseAPI_delegate];
		}
		else
		{
			requestId = [FuseAPI getGameData:_FuseAPI_gameDataKeys Delegate:_FuseAPI_delegate];
		}
	}
    FuseSafeRelease(_FuseAPI_gameDataKeys);
	_FuseAPI_gameDataFuseId = nil;
	_FuseAPI_gameDataKey = nil;
	
	return requestId;
	
}

#pragma mark - Friends List

void FuseAPI_UpdateFriendsListFromServer()
{
	[FuseAPI updateFriendsListFromServer];
}

void FuseAPI_AddFriend(const char* fuseId)
{
    [FuseAPI addFriend:[NSString stringWithUTF8String:fuseId]];
}

void FuseAPI_RemoveFriend(const char* fuseId)
{
    [FuseAPI removeFriend:[NSString stringWithUTF8String:fuseId]];
}

void FuseAPI_AcceptFriend(const char* fuseId)
{
    [FuseAPI acceptFriend:[NSString stringWithUTF8String:fuseId]];
}

void FuseAPI_RejectFriend(const char* fuseId)
{
    [FuseAPI rejectFriend:[NSString stringWithUTF8String:fuseId]];
}

void FuseAPI_MigrateFriends(const char* fuseId)
{
    [FuseAPI migrateFriends:[NSString stringWithUTF8String:fuseId]];
}

int FuseAPI_GetFriendsListCount()
{
	return [[FuseAPI getFriendsList] count];
}

const char* FuseAPI_GetFriendsListFriendFuseId(int index)
{
	NSArray* keys = [[FuseAPI getFriendsList] allKeys];
    
    const char* string = "";
    
    if(keys)
    {
        NSString* fuseId = [keys objectAtIndex:index];
	
    
        if( fuseId != nil )
        {
            string = [fuseId UTF8String];
        }
    }
    
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

const char* FuseAPI_GetFriendsListFriendAccountId(int index)
{
    
    const char* string = "";
	NSArray* keys = [[FuseAPI getFriendsList] allKeys];
    if(keys)
    {
        NSString* fuseId = [keys objectAtIndex:index];
        if(fuseId)
        {
            NSDictionary* friend = (NSDictionary*)[[FuseAPI getFriendsList] objectForKey:fuseId];
            if(friend)
            {
                
                NSString* accountId = [friend objectForKey:@"account_id"];
                
                if(accountId)
                {
                    string = [accountId UTF8String];
                }
            }
        }
    }
	
    
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

const char* FuseAPI_GetFriendsListFriendAlias(int index)
{
    const char* string = "";
	NSArray* keys = [[FuseAPI getFriendsList] allKeys];
    if(keys)
    {
        NSString* fuseId = [keys objectAtIndex:index];
        if(fuseId)
        {
            NSDictionary* friend = (NSDictionary*)[[FuseAPI getFriendsList] objectForKey:fuseId];
            if(friend)
            {
                
                NSString* accountId = [friend objectForKey:@"alias"];
                
                if(accountId)
                {
                    string = [accountId UTF8String];
                }
            }
        }
    }

	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

bool FuseAPI_GetFriendsListFriendPending(int index)
{
    int pending = false;
NSArray* keys = [[FuseAPI getFriendsList] allKeys];
	if(keys)
    {
        NSString* fuseId = [keys objectAtIndex:index];
        if(fuseId)
        {
            NSDictionary* friend = (NSDictionary*)[[FuseAPI getFriendsList] objectForKey:fuseId];
            if(friend)
            {
                pending = [[friend objectForKey:@"pending"] intValue];
            }
        }
    }
    
	return pending;
}

#pragma mark - User-to-User Push Notifications

void FuseAPI_UserPushNotification(const char* fuseId, const char* message)
{
	[FuseAPI userPushNotification:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message]];
}

void FuseAPI_FriendsPushNotification(const char* message)
{
	[FuseAPI friendsPushNotification:[NSString stringWithUTF8String:message]];
}

#pragma mark - Gifting

void FuseAPI_GetMailListFromServer()
{
	[FuseAPI getMailListFromServer];
}

void FuseAPI_GetMailListFriendFromServer(const char* fuseId)
{
	[FuseAPI getMailListFriendFromServer:[NSString stringWithUTF8String:fuseId]];
}

int FuseAPI_GetMailListCount(const char* fuseId)
{
	return [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] count];
}

int FuseAPI_GetMailListMailMessageId(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	
	return messageId.intValue;
}

long long FuseAPI_GetMailListMailTimestamp(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	NSDictionary* mail = (NSDictionary*)[[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] objectForKey:messageId];
	NSNumber* timestamp = [mail objectForKey:@"timestamp"];
	
	return timestamp.longLongValue;
}

const char* FuseAPI_GetMailListMailAlias(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	NSDictionary* mail = (NSDictionary*)[[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] objectForKey:messageId];
	NSString* alias = [mail objectForKey:@"alias"];
	
    const char * string = "";
    if(alias)
    {
        string = alias.UTF8String;
    }
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

const char* FuseAPI_GetMailListMailMessage(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	NSDictionary* mail = (NSDictionary*)[[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] objectForKey:messageId];
	NSString* message = [mail objectForKey:@"message"];
	
	const char* string = "";
    if(message)
    {
        string = message.UTF8String;
    }
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

int FuseAPI_GetMailListMailGiftId(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	NSDictionary* mail = (NSDictionary*)[[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] objectForKey:messageId];
	NSNumber* giftId = [mail objectForKey:@"gift_id"];
	if(giftId)
        return [giftId intValue];
    return 0;
}

const char* FuseAPI_GetMailListMailGiftName(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	NSDictionary* mail = (NSDictionary*)[[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] objectForKey:messageId];
	NSString* giftName = [mail objectForKey:@"gift_name"];
	const char* string = "";
    if(giftName)
    {
        string = giftName.UTF8String;
    }
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

int FuseAPI_GetMailListMailGiftAmount(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	NSDictionary* mail = (NSDictionary*)[[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] objectForKey:messageId];
	NSNumber* giftAmount = [mail objectForKey:@"gift_amount"];
	if(giftAmount)
        return [giftAmount intValue];
    return 0;
}

void FuseAPI_SetMailAsReceived(int messageId)
{
	[FuseAPI setMailAsReceived:messageId];
}

int FuseAPI_SendMail(const char* fuseId, const char* message)
{
	return [FuseAPI sendMail:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message]];
}

int FuseAPI_SendMailWithGift(const char* fuseId, const char* message, int giftId, int giftAmount)
{
	return [FuseAPI sendMailWithGift:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message] GiftID:giftId GiftAmount:giftAmount];
}

#pragma mark - Specific Event Registration

void FuseAPI_RegisterLevel(int level)
{
	[FuseAPI registerLevel:level];
}

void FuseAPI_RegisterCurrency(int type, int balance)
{
	[FuseAPI registerCurrency:type Balance:balance];
}

void FuseAPI_RegisterAge(int age)
{
	[FuseAPI registerAge:age];
}

void FuseAPI_RegisterBirthday(int year, int month, int day)
{
	[FuseAPI registerBirthday:year Month:month Day:day];
}

#pragma mark - Callback

@implementation FuseAPI_Delegate

#pragma mark Initialization

+ (void)load
{
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidFinishLaunching:) name:UIApplicationDidFinishLaunchingNotification object:nil];
}

+ (void)applicationDidFinishLaunching:(NSNotification *)notification
{
	// This no longer works under Unity 4.1 due to a mono_mutex_lock error,
	// so it is now called from StartSession until mono under Unity 4.1 is investigated further.
	//FuseAPI_Initialize();
}

#pragma mark Session Creation

- (void)sessionStartReceived
{
	CallUnity("_CB_SessionStartReceived", NULL);
}

- (void)sessionLoginError:(NSNumber*)_error
{
	CallUnity("_CB_SessionLoginError", [_error stringValue].UTF8String);
}

#pragma mark In-App Purchase Logging

- (void)puchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id
{
    NSArray *values = @[_verified,_tx_id,_o_tx_id];
    
	CallUnity("_CB_PurchaseVerification", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark Fuse Interstitial Ads

- (void) adAvailabilityResponse:(NSNumber*)_available Error:(NSNumber*)_error
{
	NSArray *values = @[_available,_error];

	CallUnity("_CB_AdAvailabilityResponse", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) rewardedVideoCompleted:(NSString*) _zoneID
{
    CallUnity("_CB_RewardedVideoCompleted", _zoneID.UTF8String);
}

- (void)adWillClose
{
	CallUnity("_CB_AdWillClose", NULL);
}

#pragma mark Notifications

-(void) notificationAction:(NSString*)_action
{
	CallUnity("_CB_NotificationAction", _action.UTF8String);
}

#pragma mark More Games

- (void)overlayWillClose
{
	CallUnity("_CB_OverlayWillClose", NULL);
}

#pragma mark Account Login

- (void)accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id
{
	NSArray *values = @[_type,_account_id];

	CallUnity("_CB_AccountLoginComplete", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark Miscellaneous

- (void)timeUpdated:(NSNumber*)_timeStamp
{
	CallUnity("_CB_TimeUpdated", [_timeStamp stringValue].UTF8String);
}

-(bool) writeText:(NSString*)text toFile:(NSString*)fileName
{
    NSError *err = nil;
    //get the documents directory:
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    
    //make a file name to write the data to using the documents directory:
    NSString *fName = [NSString stringWithFormat:@"%@/%@",
                          documentsDirectory,fileName];
    //create content - four lines of text
    //save content to the documents directory
	bool stuff =  [text writeToFile:fName
              atomically:NO
                encoding:NSStringEncodingConversionAllowLossy
                   error:&err];
    if(err)
    {
        NSLog(@"error writing string %@,%@",[err localizedFailureReason],[err domain]);
    }
    return stuff;
}


#pragma mark Friends List

-(void) friendAdded:(NSString*)_fuse_id Error:(NSNumber*)_error
{
	NSArray *values = @[_fuse_id,_error];

	CallUnity("_CB_FriendAdded", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) friendRemoved:(NSString*)_fuse_id Error:(NSNumber*)_error
{
	NSArray *values = @[_fuse_id,_error];

	CallUnity("_CB_FriendRemoved", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) friendAccepted:(NSString*)_fuse_id Error:(NSNumber*)_error
{
	NSArray *values = @[_fuse_id,_error];

	CallUnity("_CB_FriendAccepted", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) friendRejected:(NSString*)_fuse_id Error:(NSNumber*)_error
{
	NSArray *values = @[_fuse_id,_error];

	CallUnity("_CB_FriendRejected", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) friendsListUpdated:(NSDictionary*)_friendsList
{
    NSMutableArray *allFriends = [NSMutableArray new];
    
	for (NSString* fuseId in _friendsList)
	{
		NSDictionary* friendEntry = (NSDictionary*)[_friendsList objectForKey:fuseId];
		
		NSString* accountId = [friendEntry objectForKey:@"account_id"];
		NSString* alias = [friendEntry objectForKey:@"alias"];
		int pending = [[friendEntry objectForKey:@"pending"] intValue];
		
        [allFriends addObject:[ @[fuseId,accountId,alias,[NSNumber numberWithInt:pending]] componentsJoinedByString:@","]];
	}
    
	NSString *filename = @"FuseSDK-friendsList.dat";
    bool saved = [self writeText:[allFriends componentsJoinedByString:@"\n" ]  toFile:filename];
    if(saved)
    {
       CallUnity("_CB_FriendsListUpdated", filename.UTF8String);
    }
	[allFriends release];
}

-(void) friendsListError:(NSNumber*)_error
{
	CallUnity("_CB_FriendsListError", [_error stringValue].UTF8String);
}

-(void) friendsMigrated:(NSString*)_fuse_id Error:(NSNumber*)_error
{
	NSArray *values = @[_fuse_id,_error];

	CallUnity("_CB_FriendsMigrated", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark Game Configuration Data

- (void)gameConfigurationReceived
{
    NSMutableArray *configData = [NSMutableArray new];
	NSMutableDictionary* dict = [FuseAPI getGameConfiguration];
	
	if( dict != nil && [dict count] > 0 )
    {
	    NSArray* keys = [dict allKeys];
        for( int i = 0; i < [keys count]; i++ )
        {
            NSString* key = [keys objectAtIndex:i];
            NSString* value = [dict objectForKey:key];
			
			[configData addObject:[NSString stringWithFormat:@"%@,%@", key,value]];
		}
	}
    
	NSString *filename = @"FuseSDK-gameConfig.dat";
    bool saved = [self writeText:[configData componentsJoinedByString:@"\n" ]  toFile:filename];
    if(saved)
    {
		CallUnity("_CB_GameConfigurationReceived", filename.UTF8String);
    }
	
	[configData release];
}

#pragma mark SKProductRequestDelegate methods
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
    NSArray *products = response.products;
    SKProduct* unibillProduct = [products count] > 0 ? [products objectAtIndex:0] : nil;

    // Logging the response from Apple
//    if (unibillProduct)
//    {
//        NSLog(@"Product title: %@" , unibillProduct.localizedTitle);
//        NSLog(@"Product description: %@" , unibillProduct.localizedDescription);
//        NSLog(@"Product price: %@" , unibillProduct.price);
//        NSLog(@"Product id: %@" , unibillProduct.productIdentifier);
//    }
//
//    for (NSString *invalidProductId in response.invalidProductIdentifiers)
//    {
//        NSLog(@"Invalid product id: %@" , invalidProductId);
//    }

    // register the product list so we can track the IAP price and currency
    [FuseAPI registerInAppPurchaseList:response];

    // register the IAP now that we have the product information
    [FuseAPI registerInAppPurchase:(SKPaymentTransaction*)_UniBillPayment];

    // release the payment object
    FuseSafeRelease(_UniBillPayment);

    // finally release the reqest we alloc/initÕed in requestProUpgradeProductData
    FuseSafeRelease(_UniBillRequest);
}

@end
