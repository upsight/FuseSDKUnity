
#import "FuseAPI.h"
#import "FuseUnityAPI.h"
#import "NSData-Base64.h"
#include "Mono.h"

#define FuseSafeRelease(obj) if (obj != nil){ \
[obj release]; \
obj = nil;}

static void* _FuseAPI_SessionStartReceived = NULL; 
static void* _FuseAPI_SessionLoginError = NULL;
static void* _FuseAPI_TimeUpdated = NULL;
static void* _FuseAPI_PurchaseVerification = NULL;
static void* _FuseAPI_AdAvailabilityResponse = NULL;
static void* _FuseAPI_AdWillClose = NULL;
static void* _FuseAPI_NotificationAction = NULL;
static void* _FuseAPI_OverlayWillClose = NULL;
static void* _FuseAPI_AccountLoginComplete = NULL;
static void* _FuseAPI_GameConfigurationReceived = NULL;
static void* _FuseAPI_GameDataSetAcknowledged = NULL;
static void* _FuseAPI_GameDataError = NULL;
static void* _FuseAPI_GameDataReceivedStart = NULL;
static void* _FuseAPI_GameDataReceivedKeyValue = NULL;
static void* _FuseAPI_GameDataReceivedEnd = NULL;
static void* _FuseAPI_FriendsListUpdatedStart = NULL;
static void* _FuseAPI_FriendsListUpdatedFriend = NULL;
static void* _FuseAPI_FriendsListUpdatedEnd = NULL;
static void* _FuseAPI_FriendsListError = NULL;
static void* _FuseAPI_FriendsMigrated = NULL;
static void* _FuseAPI_MailListReceivedStart = NULL;
static void* _FuseAPI_MailListReceivedMail = NULL;
static void* _FuseAPI_MailListReceivedEnd = NULL;
static void* _FuseAPI_MailListError = NULL;
static void* _FuseAPI_MailAcknowledged = NULL;
static void* _FuseAPI_MailError = NULL;
static void* _FuseAPI_GameConfigInit = NULL;
static void* _FuseAPI_UpdateGameConfig = NULL;

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
	
	Mono_Initialize();
	
	_FuseAPI_SessionStartReceived = Mono_GetMethod("FuseAPI:_SessionStartReceived");
	_FuseAPI_SessionLoginError = Mono_GetMethod("FuseAPI:_SessionLoginError");
	_FuseAPI_TimeUpdated = Mono_GetMethod("FuseAPI:_TimeUpdated");
	_FuseAPI_PurchaseVerification = Mono_GetMethod("FuseAPI:_PurchaseVerification");
	_FuseAPI_AdAvailabilityResponse = Mono_GetMethod("FuseAPI:_AdAvailabilityResponse");
	_FuseAPI_AdWillClose = Mono_GetMethod("FuseAPI:_AdWillClose");
	_FuseAPI_NotificationAction = Mono_GetMethod("FuseAPI:_NotificationAction");
	_FuseAPI_OverlayWillClose = Mono_GetMethod("FuseAPI:_OverlayWillClose");
	_FuseAPI_AccountLoginComplete = Mono_GetMethod("FuseAPI:_AccountLoginComplete");
	_FuseAPI_GameConfigurationReceived = Mono_GetMethod("FuseAPI:_GameConfigurationReceived");
	_FuseAPI_GameDataSetAcknowledged = Mono_GetMethod("FuseAPI:_GameDataSetAcknowledged");
	_FuseAPI_GameDataError = Mono_GetMethod("FuseAPI:_GameDataError");
	_FuseAPI_GameDataReceivedStart = Mono_GetMethod("FuseAPI:_GameDataReceivedStart");
	_FuseAPI_GameDataReceivedKeyValue = Mono_GetMethod("FuseAPI:_GameDataReceivedKeyValue");
	_FuseAPI_GameDataReceivedEnd = Mono_GetMethod("FuseAPI:_GameDataReceivedEnd");
	_FuseAPI_FriendsListUpdatedStart = Mono_GetMethod("FuseAPI:_FriendsListUpdatedStart");
	_FuseAPI_FriendsListUpdatedFriend = Mono_GetMethod("FuseAPI:_FriendsListUpdatedFriend");
	_FuseAPI_FriendsListUpdatedEnd = Mono_GetMethod("FuseAPI:_FriendsListUpdatedEnd");
	_FuseAPI_FriendsListError = Mono_GetMethod("FuseAPI:_FriendsListError");
    _FuseAPI_FriendsMigrated = Mono_GetMethod("FuseAPI:_FriendsMigrated");
	_FuseAPI_MailListReceivedStart = Mono_GetMethod("FuseAPI:_MailListReceivedStart");
	_FuseAPI_MailListReceivedMail = Mono_GetMethod("FuseAPI:_MailListReceivedMail");
	_FuseAPI_MailListReceivedEnd = Mono_GetMethod("FuseAPI:_MailListReceivedEnd");
	_FuseAPI_MailListError = Mono_GetMethod("FuseAPI:_MailListError");
	_FuseAPI_MailAcknowledged = Mono_GetMethod("FuseAPI:_MailAcknowledged");
	_FuseAPI_MailError = Mono_GetMethod("FuseAPI:_MailError");
    _FuseAPI_GameConfigInit = Mono_GetMethod("FuseAPI:_GameConfigInit");
    _FuseAPI_UpdateGameConfig = Mono_GetMethod("FuseAPI:_UpdateGameConfig");
	
	_FuseAPI_delegate = [FuseAPI_Delegate new];
}

void FuseAPI_RegisterCrash(NSException* exception)
{
	[FuseAPI registerCrash:exception];
}

#pragma mark - Session

void FuseAPI_StartSession(const char* gameId)
{
	FuseAPI_Initialize();
	[FuseAPI startSession:[NSString stringWithUTF8String:gameId] Delegate:_FuseAPI_delegate];
}

void FuseAPI_SessionStartReceived()
{
	Mono_CallMethod(_FuseAPI_SessionStartReceived, NULL);
}

void FuseAPI_SessionLoginError(int error)
{
	void *args[] = { &error };
	Mono_CallMethod(_FuseAPI_SessionLoginError, args);
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
    //FuseSafeRelease(_productIdentifier);
    [super dealloc];
}

-(void)setIdentifier:(const char*)identifier
{
    _productIdentifier = [NSString stringWithUTF8String:identifier];
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

void FuseAPI_PurchaseVerification(bool verified, const char* transactionId, const char* originalTransactionId)
{
	void *args[] = { &verified, Mono_NewString(transactionId), Mono_NewString(originalTransactionId) };
	Mono_CallMethod(_FuseAPI_PurchaseVerification, args);
}

#pragma mark - Ads

void FuseAPI_CheckAdAvailable()
{
	[FuseAPI checkAdAvailable];
}

void FuseAPI_ShowAd()
{
	[FuseAPI showAdWithDelegate:_FuseAPI_delegate];
}

void FuseAPI_AdAvailabilityResponse(int available, int error)
{
	void *args[] = { &available, &error };
	Mono_CallMethod(_FuseAPI_AdAvailabilityResponse, args);
}

void FuseAPI_AdWillClose()
{
	Mono_CallMethod(_FuseAPI_AdWillClose, NULL);
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

void FuseAPI_NotificationAction(const char* action)
{
	void *args[] = { Mono_NewString(action) };
	Mono_CallMethod(_FuseAPI_NotificationAction, args);
}

#pragma mark - More Games

void FuseAPI_DisplayMoreGames()
{
	[FuseAPI displayMoreGames:_FuseAPI_delegate];
}

void FuseAPI_OverlayWillClose()
{
	Mono_CallMethod(_FuseAPI_OverlayWillClose, NULL);
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

void FuseAPI_OpenFeintLogin(const char* openFeintId)
{
	[FuseAPI openFeintLogin:[NSString stringWithUTF8String:openFeintId]];
}

void FuseAPI_FuseLogin(const char* fuseId, const char* alias)
{
	[FuseAPI fuseLogin:[NSString stringWithUTF8String:fuseId] Alias:[NSString stringWithUTF8String:alias]];
}

void FuseAPI_GooglePlayLogin(const char* _id, const char* alias, const char* token)
{
    [FuseAPI googlePlayLogin:[NSString stringWithUTF8String:alias] AccessToken:[NSString stringWithUTF8String:token]];
}

const char* FuseAPI_GetOriginalAccountAlias()
{
    NSString* accountAlias = [FuseAPI getOriginalAccountAlias];
	
	const char* string = accountAlias.UTF8String;
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

const char* FuseAPI_GetOriginalAccountId()
{
	NSString* accountId = [FuseAPI getOriginalAccountID];
	
	const char* string = accountId.UTF8String;
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

int FuseAPI_GetOriginalAccountType()
{
	return [FuseAPI getOriginalAccountType];
}

void FuseAPI_AccountLoginComplete(int type, const char* accountId)
{
	void *args[] = { &type , Mono_NewString(accountId) };
	Mono_CallMethod(_FuseAPI_AccountLoginComplete, args);
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
	
	const char* string = [libraryVersion UTF8String];
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

void FuseAPI_TimeUpdated(long long timestamp)
{
	void *args[] = { &timestamp };
	Mono_CallMethod(_FuseAPI_TimeUpdated, args);
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
	
	const char* string = [value UTF8String];
	if (string)
	{
		char* copy = (char*)malloc(strlen(string) + 1);
		strcpy(copy, string);
		
		return copy;
	}
	else
	{
		return 0;
	}
}

void FuseAPI_GameConfigurationReceived()
{
	Mono_CallMethod(_FuseAPI_GameConfigurationReceived, NULL);
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

void FuseAPI_GameDataSetAcknowledged(int requestId)
{
	void *args[] = { &requestId };
	Mono_CallMethod(_FuseAPI_GameDataSetAcknowledged, args);
}

void FuseAPI_GameDataError(int error, int requestId)
{
	void *args[] = { &error, &requestId };
	Mono_CallMethod(_FuseAPI_GameDataError, args);
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

void FuseAPI_GameDataReceivedStart(const char* fuseId, const char* key, int requestId)
{
	void *args[] = { Mono_NewString(fuseId), Mono_NewString(key), &requestId };
	Mono_CallMethod(_FuseAPI_GameDataReceivedStart, args);
}

void FuseAPI_GameDataReceivedKeyValue(const char* key, const char* value, bool isBinary)
{
	void *args[] = { Mono_NewString(key), Mono_NewString(value), &isBinary };
	Mono_CallMethod(_FuseAPI_GameDataReceivedKeyValue, args);
}

void FuseAPI_GameDataReceivedEnd()
{
	Mono_CallMethod(_FuseAPI_GameDataReceivedEnd, NULL);
}

void FuseAPI_RefreshGameConfiguration()
{
    // get the game config table
    NSMutableDictionary* dict = [FuseAPI getGameConfiguration];
    if( dict != nil && [dict count] > 0 )
    {
        // initialize game config by clearing the old dictionary
        Mono_CallMethod(_FuseAPI_GameConfigInit, NULL);
        
        // iterate through each kvp in the dictionary and send them to mono
        NSArray* keys = [dict allKeys];
        for( int i = 0; i < [keys count]; i++ )
        {
            NSString* key = [keys objectAtIndex:i];
            NSString* value = [dict objectForKey:key];
            
            void* argsForDict[] = { Mono_NewString(key.UTF8String), Mono_NewString([value UTF8String]) };
            Mono_CallMethod(_FuseAPI_UpdateGameConfig, argsForDict);
        }
    }
}

#pragma mark - Friends List

void FuseAPI_UpdateFriendsListFromServer()
{
	[FuseAPI updateFriendsListFromServer];
}

void FuseAPI_FriendsListUpdatedStart()
{
	Mono_CallMethod(_FuseAPI_FriendsListUpdatedStart, NULL);
}

void FuseAPI_FriendsListUpdatedFriend(const char* fuseId, const char* accountId, const char* alias, bool pending)
{
	void *args[] = { Mono_NewString(fuseId), Mono_NewString(accountId), Mono_NewString(alias), &pending };
	Mono_CallMethod(_FuseAPI_FriendsListUpdatedFriend, args);
}

void FuseAPI_FriendsListUpdatedEnd()
{
	Mono_CallMethod(_FuseAPI_FriendsListUpdatedEnd, NULL);
}

void FuseAPI_FriendsListError(int error)
{
	void *args[] = { &error };
	Mono_CallMethod(_FuseAPI_FriendsListError, args);
}


void FuseAPI_MigrateFriends(const char* fuseId)
{
    [FuseAPI migrateFriends:[NSString stringWithUTF8String:fuseId]];
}

void FuseAPI_FriendsMigrated(const char* fuseId, int error)
{
    void *args[] = { Mono_NewString(fuseId), &error };
    Mono_CallMethod(_FuseAPI_FriendsMigrated, args);
}

int FuseAPI_GetFriendsListCount()
{
	return [[FuseAPI getFriendsList] count];
}

const char* FuseAPI_GetFriendsListFriendFuseId(int index)
{
	NSArray* keys = [[FuseAPI getFriendsList] allKeys];
	NSString* fuseId = [keys objectAtIndex:index];
	
	const char* string = [fuseId UTF8String];
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

const char* FuseAPI_GetFriendsListFriendAccountId(int index)
{
	NSArray* keys = [[FuseAPI getFriendsList] allKeys];
	NSString* fuseId = [keys objectAtIndex:index];
	NSDictionary* friend = (NSDictionary*)[[FuseAPI getFriendsList] objectForKey:fuseId];
	NSString* accountId = [friend objectForKey:@"account_id"];
	
	const char* string = accountId.UTF8String;
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

const char* FuseAPI_GetFriendsListFriendAlias(int index)
{
	NSArray* keys = [[FuseAPI getFriendsList] allKeys];
	NSString* fuseId = [keys objectAtIndex:index];
	NSDictionary* friend = (NSDictionary*)[[FuseAPI getFriendsList] objectForKey:fuseId];
	NSString* alias = [friend objectForKey:@"alias"];
	
	const char* string = alias.UTF8String;
	char* copy = (char*)malloc(strlen(string) + 1);
	strcpy(copy, string);
	
	return copy;
}

bool FuseAPI_GetFriendsListFriendPending(int index)
{
	NSArray* keys = [[FuseAPI getFriendsList] allKeys];
	NSString* fuseId = [keys objectAtIndex:index];
	NSDictionary* friend = (NSDictionary*)[[FuseAPI getFriendsList] objectForKey:fuseId];
	int pending = [[friend objectForKey:@"pending"] intValue];
	
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

void FuseAPI_MailListReceivedStart(const char* fuseId)
{
	void *args[] = { Mono_NewString(fuseId) };
	Mono_CallMethod(_FuseAPI_MailListReceivedStart, args);
}

void FuseAPI_MailListReceivedMail(int messageId, long long timestamp, const char* alias, const char* message, int giftId, const char* giftName, int giftAmount)
{
	void *args[] = { &messageId, &timestamp, Mono_NewString(alias), Mono_NewString(message), &giftId, Mono_NewString(giftName), &giftAmount };
	Mono_CallMethod(_FuseAPI_MailListReceivedMail, args);
}

void FuseAPI_MailListReceivedEnd()
{
	Mono_CallMethod(_FuseAPI_MailListReceivedEnd, NULL);
}

void FuseAPI_MailListError(int error)
{
	void *args[] = { &error };
	Mono_CallMethod(_FuseAPI_MailListError, args);
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
	
	const char* string = alias.UTF8String;
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
	
	const char* string = message.UTF8String;
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
	
	return [giftId intValue];
}

const char* FuseAPI_GetMailListMailGiftName(const char* fuseId, int index)
{
	NSArray* keys = [[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] allKeys];
	NSNumber* messageId = [keys objectAtIndex:index];
	NSDictionary* mail = (NSDictionary*)[[FuseAPI getMailList:[NSString stringWithUTF8String:fuseId]] objectForKey:messageId];
	NSString* giftName = [mail objectForKey:@"gift_name"];
	
	const char* string = giftName.UTF8String;
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
	
	return [giftAmount intValue];
}

void FuseAPI_SetMailAsReceived(int messageId)
{
	[FuseAPI setMailAsReceived:messageId];
}

void FuseAPI_SendMail(const char* fuseId, const char* message)
{
	[FuseAPI sendMail:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message]];
}

void FuseAPI_SendMailWithGift(const char* fuseId, const char* message, int giftId, int giftAmount)
{
	[FuseAPI sendMailWithGift:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message] GiftID:giftId GiftAmount:giftAmount];
}

void FuseAPI_MailAcknowledged(int messageId, const char* fuseId)
{
	void *args[] = { &messageId, Mono_NewString(fuseId) };
	Mono_CallMethod(_FuseAPI_MailAcknowledged, args);
}

void FuseAPI_MailError(int error)
{
	void *args[] = { &error };
	Mono_CallMethod(_FuseAPI_MailError, args);
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

void FuseAPI_RegisterFlurryView()
{
	[FuseAPI registerFlurryView];
}

void FuseAPI_RegisterFlurryClick()
{
	[FuseAPI registerFlurryClick];
}

void FuseAPI_RegisterTapjoyReward(int amount)
{
	[FuseAPI registerTapjoyReward:amount];
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
	FuseAPI_SessionStartReceived();
}

- (void)sessionLoginError:(NSNumber*)_error
{
	FuseAPI_SessionLoginError([_error integerValue]);
}

#pragma mark In-App Purchase Logging

- (void)puchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id
{
	FuseAPI_PurchaseVerification(_verified.intValue == 1, _tx_id.UTF8String, _o_tx_id.UTF8String);
}

#pragma mark Fuse Interstitial Ads

- (void) adAvailabilityResponse:(NSNumber*)_available Error:(NSNumber*)_error
{
	FuseAPI_AdAvailabilityResponse(_available.intValue, _error.intValue);
}

- (void)adWillClose
{
	FuseAPI_AdWillClose();
}

#pragma mark Notifications

-(void) notificationAction:(NSString*)_action
{
	FuseAPI_NotificationAction(_action.UTF8String);
}

#pragma mark More Games

- (void)overlayWillClose
{
	FuseAPI_OverlayWillClose();
}

#pragma mark Account Login

- (void)accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id
{
	FuseAPI_AccountLoginComplete(_type.intValue, _account_id.UTF8String);
}

#pragma mark Miscellaneous

- (void)timeUpdated:(NSNumber*)_timeStamp
{
	FuseAPI_TimeUpdated([_timeStamp longLongValue]);
}

#pragma mark User Game Data

-(void) gameDataReceived:(NSString*)_fuse_id ForKey:(NSString*)_key Data:(NSMutableDictionary*)_data
{
    [self gameDataReceived:_fuse_id ForKey:_key Data:_data RequestID:[NSNumber numberWithInt:0]];
}

- (void)gameDataReceived:(NSString *)_user_account_id ForKey:(NSString *)_key Data:(NSMutableDictionary *)_data RequestID:(NSNumber *)_request_id
{
	FuseAPI_GameDataReceivedStart(_user_account_id.UTF8String, _key.UTF8String, _request_id.intValue);
	for (NSString* key in _data)
	{
		NSObject* value = [_data objectForKey:key];
		
		if (value && [value isKindOfClass:[NSString class]] == YES)
		{
			NSString* string = (NSString*)value;
            
			FuseAPI_GameDataReceivedKeyValue(key.UTF8String, string.UTF8String, false);
		}
		else if (value && [value isKindOfClass:[NSData class]] == YES)
		{
			NSData* buffer = (NSData*)value;
			
			NSString* string = buffer.fuseUnityBase64EncodedString;
            
			FuseAPI_GameDataReceivedKeyValue(key.UTF8String, string.UTF8String, true);
		}
	}
	FuseAPI_GameDataReceivedEnd();
}

- (void)gameDataError:(NSNumber*)_error RequestID:(NSNumber*)_request_id
{
	FuseAPI_GameDataError(_error.intValue, _request_id.intValue);
}

- (void)gameDataSetAcknowledged:(NSNumber*)_request_id
{
	FuseAPI_GameDataSetAcknowledged(_request_id.intValue);
}

#pragma mark Friends List

-(void) friendsListUpdated:(NSDictionary*)_friendsList
{
	FuseAPI_FriendsListUpdatedStart();
	for (NSString* fuseId in _friendsList)
	{
		NSDictionary* friendEntry = (NSDictionary*)[_friendsList objectForKey:fuseId];
		
		NSString* accountId = [friendEntry objectForKey:@"account_id"];
		NSString* alias = [friendEntry objectForKey:@"alias"];
		int pending = [[friendEntry objectForKey:@"pending"] intValue];
		
		FuseAPI_FriendsListUpdatedFriend(fuseId.UTF8String, accountId.UTF8String, alias.UTF8String, pending);
	}
	FuseAPI_FriendsListUpdatedEnd();
}

-(void) friendsListError:(NSNumber*)_error
{
	FuseAPI_FriendsListError(_error.intValue);
}

-(void) friendsMigrated:(NSString*)_fuse_id Error:(NSNumber*)_error
{
    FuseAPI_FriendsMigrated(_fuse_id.UTF8String, _error.intValue);
}

#pragma mark Gifting

-(void) mailListRecieved:(NSDictionary*)_messages User:(NSString*)_fuse_id
{
	FuseAPI_MailListReceivedStart(_fuse_id.UTF8String);
	for (NSNumber* messageId in _messages)
	{
		NSDictionary* mail = (NSDictionary*)[_messages objectForKey:messageId];
		
		NSNumber* timestamp = [mail objectForKey:@"timestamp"];
		NSString* alias = [mail objectForKey:@"alias"];
		NSString* message = [mail objectForKey:@"message"];
		int giftId = [[mail objectForKey:@"gift_id"] intValue];
		NSString* giftName = [mail objectForKey:@"gift_name"];
		int giftAmount = [[mail objectForKey:@"gift_amount"] intValue];
		
		FuseAPI_MailListReceivedMail(messageId.intValue, timestamp.longLongValue, alias.UTF8String, message.UTF8String, giftId, giftName.UTF8String, giftAmount);
	}
	FuseAPI_MailListReceivedEnd();
}

-(void) mailListError:(NSNumber*)_error
{
	FuseAPI_MailListError(_error.intValue);
}

-(void) mailAcknowledged:(NSNumber*)_message_id User:(NSString*)_fuse_id
{
	FuseAPI_MailAcknowledged(_message_id.intValue, _fuse_id.UTF8String);
}

-(void) mailError:(NSNumber*)_error
{
	FuseAPI_MailError(_error.intValue);
}

#pragma mark Game Configuration Data

- (void)gameConfigurationReceived
{
	FuseAPI_GameConfigurationReceived();
}

@end
