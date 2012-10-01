
#import "FuseAPI.h"
#import "FuseUnityAPI.h"
#import "NSData-Base64.h"
#include "Mono.h"

static void* _FuseAPI_SessionStartReceived = NULL;
static void* _FuseAPI_SessionLoginError = NULL;
static void* _FuseAPI_TimeUpdated = NULL;
static void* _FuseAPI_PurchaseVerification = NULL;
static void* _FuseAPI_AdWillClose = NULL;
static void* _FuseAPI_OverlayWillClose = NULL;
static void* _FuseAPI_AccountLoginComplete = NULL;
static void* _FuseAPI_GameConfigurationReceived = NULL;
static void* _FuseAPI_GameDataSetAcknowledged = NULL;
static void* _FuseAPI_GameDataError = NULL;
static void* _FuseAPI_GameDataReceivedStart = NULL;
static void* _FuseAPI_GameDataReceivedKeyValue = NULL;
static void* _FuseAPI_GameDataReceivedEnd = NULL;

static FuseAPI_ProductsResponse* _FuseAPI_productsResponse = nil;

static NSString* _FuseAPI_gameDataKey = nil;
static NSString* _FuseAPI_gameDataFuseId = nil;
static BOOL _FuseAPI_gameDataIsCollection = NO;
static NSMutableDictionary* _FuseAPI_gameData = nil;
static NSMutableArray* _FuseAPI_gameDataKeys = nil;

static FuseAPI_Delegate* _FuseAPI_delegate = nil;

#pragma mark Initialization

void FuseAPI_Initialize()
{
	NSSetUncaughtExceptionHandler(&FuseAPI_RegisterCrash);
	
	Mono_Initialize();
	
	_FuseAPI_SessionStartReceived = Mono_GetMethod("FuseAPI:_SessionStartReceived");
	_FuseAPI_SessionLoginError = Mono_GetMethod("FuseAPI:_SessionLoginError(int)");
	_FuseAPI_TimeUpdated = Mono_GetMethod("FuseAPI:_TimeUpdated(int)");
	_FuseAPI_PurchaseVerification = Mono_GetMethod("FuseAPI:_PurchaseVerification");
	_FuseAPI_AdWillClose = Mono_GetMethod("FuseAPI:_AdWillClose");
	_FuseAPI_OverlayWillClose = Mono_GetMethod("FuseAPI:_OverlayWillClose");
	_FuseAPI_AccountLoginComplete = Mono_GetMethod("FuseAPI:_AccountLoginComplete");
	_FuseAPI_GameConfigurationReceived = Mono_GetMethod("FuseAPI:_GameConfigurationReceived");
	_FuseAPI_GameDataSetAcknowledged = Mono_GetMethod("FuseAPI:_GameDataSetAcknowledged");
	_FuseAPI_GameDataError = Mono_GetMethod("FuseAPI:_GameDataError");
	_FuseAPI_GameDataReceivedStart = Mono_GetMethod("FuseAPI:_GameDataReceivedStart");
	_FuseAPI_GameDataReceivedKeyValue = Mono_GetMethod("FuseAPI:_GameDataReceivedKeyValue");
	_FuseAPI_GameDataReceivedEnd = Mono_GetMethod("FuseAPI:_GameDataReceivedEnd");
	
	_FuseAPI_delegate = [FuseAPI_Delegate new];
}

void FuseAPI_RegisterCrash(NSException* exception)
{
	[FuseAPI registerCrash:exception];
}

#pragma mark - Session

void FuseAPI_StartSession(const char* gameId)
{
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

#pragma mark - Time

void FuseAPI_TimeFromServer()
{
	[FuseAPI utcTimeFromServer];
}

void FuseAPI_TimeUpdated(int timestamp)
{
	void *args[] = { &timestamp };
	Mono_CallMethod(_FuseAPI_TimeUpdated, args);
}

#pragma mark - Analytics

void FuseAPI_RegisterEvent(const char* message)
{
	[FuseAPI registerEvent:[NSString stringWithUTF8String:message]];
}

#pragma mark - In-App Purchase Logging

@implementation FuseAPI_Product

@end

@implementation FuseAPI_ProductsResponse

@end

void FuseAPI_RegisterInAppPurchaseListStart()
{
	_FuseAPI_productsResponse = [FuseAPI_ProductsResponse alloc];
}

void FuseAPI_RegisterInAppPurchaseListProduct(const char* productId, const char* priceLocale, float price)
{
	FuseAPI_Product* product = [FuseAPI_Product alloc];
	product.productIdentifier = [NSString stringWithUTF8String:productId];
	product.priceLocale = [[NSLocale alloc] initWithLocaleIdentifier:[NSString stringWithUTF8String:priceLocale]];
	product.price = [NSDecimalNumber decimalNumberWithDecimal:[[NSNumber numberWithFloat:price] decimalValue]];
	
	[_FuseAPI_productsResponse.products addObject:product];
}

void FuseAPI_RegisterInAppPurchaseListEnd()
{
	[FuseAPI registerInAppPurchaseList:(SKProductsResponse*)_FuseAPI_productsResponse];
	
	[_FuseAPI_productsResponse release];
}

@implementation FuseAPI_Payment

@end

@implementation FuseAPI_PaymentTransaction

@end

void FuseAPI_RegisterInAppPurchase(const char* productId, const unsigned char* transactionReceiptBuffer, int transactionReceiptLength, int transactionState)
{
	FuseAPI_PaymentTransaction* paymentTransaction = [FuseAPI_PaymentTransaction alloc];
	paymentTransaction.payment = [FuseAPI_Payment alloc];
	paymentTransaction.payment.productIdentifier = [NSString stringWithUTF8String:productId];
	paymentTransaction.transactionReceipt = [[NSData alloc] initWithBytesNoCopy:(void*)transactionReceiptBuffer length:transactionReceiptLength];
	paymentTransaction.transactionState = transactionState;
	
	[FuseAPI registerInAppPurchase:(SKPaymentTransaction*)paymentTransaction];
}

void FuseAPI_PurchaseVerification(bool verified, const char* transactionId, const char* originalTransactionId)
{
	void *args[] = { &verified, Mono_NewString(transactionId), Mono_NewString(originalTransactionId) };
	Mono_CallMethod(_FuseAPI_PurchaseVerification, args);
}

#pragma mark - Ads

void FuseAPI_ShowAd()
{
	[FuseAPI showAdWithDelegate:_FuseAPI_delegate];
}

void FuseAPI_AdWillClose()
{
	Mono_CallMethod(_FuseAPI_AdWillClose, NULL);
}

#pragma mark - Notifications

void FuseAPI_DisplayNotifications()
{
	[FuseAPI displayNotifications];
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

void FuseAPI_FacebookLogin(const char* facebookId)
{
	[FuseAPI facebookLogin:[NSString stringWithUTF8String:facebookId]];
}

void FuseAPI_TwitterLogin(const char* twitterId)
{
	[FuseAPI twitterLogin:[NSString stringWithUTF8String:twitterId]];
}

void FuseAPI_OpenFeintLogin(const char* openFeintId)
{
	[FuseAPI openFeintLogin:[NSString stringWithUTF8String:openFeintId]];
}

void FuseAPI_FuseLogin(const char* fuseId, const char* alias)
{
	[FuseAPI fuseLogin:[NSString stringWithUTF8String:fuseId] Alias:[NSString stringWithUTF8String:alias]];
}

void FuseAPI_AccountLoginComplete(int type, const char* accountId)
{
	void *args[] = { &type , Mono_NewString(accountId) };
	Mono_CallMethod(_FuseAPI_AccountLoginComplete, args);
}

const char* FuseAPI_GetFuseId()
{
	NSString* fuseId = [FuseAPI getFuseID];
	
	const char* string = [fuseId UTF8String];
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
    char* copy = (char*)malloc(strlen(string) + 1);
    strcpy(copy, string);
	
	return copy;
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
		NSData* buffer = [NSData dataFromBase64String:[NSString stringWithUTF8String:value]];
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
	[_FuseAPI_gameData release];
	_FuseAPI_gameDataKey = nil;
	
	return requestId;
}

void FuseAPI_GameDataSetAcknowledged(int requestId)
{
	void *args[] = { &requestId };
	Mono_CallMethod(_FuseAPI_GameDataSetAcknowledged, args);
}

void FuseAPI_GameDataError(int error)
{
	void *args[] = { &error };
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
	[_FuseAPI_gameDataKeys release];
	_FuseAPI_gameDataFuseId = nil;
	_FuseAPI_gameDataKey = nil;
	
	return requestId;
	
}

void FuseAPI_GameDataReceivedStart(const char* fuseId, const char* key)
{
	void *args[] = { Mono_NewString(fuseId), Mono_NewString(key) };
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

#pragma mark - User-to-User Push Notifications

void FuseAPI_UserPushNotification(const char* fuseId, const char* message)
{
	[FuseAPI userPushNotification:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message]];
}

void FuseAPI_FriendsPushNotification(const char* message)
{
	[FuseAPI friendsPushNotification:[NSString stringWithUTF8String:message]];
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
	FuseAPI_Initialize();
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

- (void)adWillClose
{
	FuseAPI_AdWillClose();
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
	FuseAPI_TimeUpdated([_timeStamp integerValue]);
}

#pragma mark User Game Data

- (void)gameDataReceived:(NSString *)_user_account_id ForKey:(NSString *)_key Data:(NSMutableDictionary *)_data
{
	FuseAPI_GameDataReceivedStart(_user_account_id.UTF8String, _key.UTF8String);
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
			
			NSString* string = buffer.base64EncodedString;
            
			FuseAPI_GameDataReceivedKeyValue(key.UTF8String, string.UTF8String, true);
		}
	}
	FuseAPI_GameDataReceivedEnd();
}

- (void)gameDataError:(NSNumber *)_error
{
	FuseAPI_GameDataError(_error.intValue);
}

- (void)gameDataSetAcknowledged:(NSNumber*)_request_id
{
	FuseAPI_GameDataSetAcknowledged(_request_id.intValue);
}

#pragma mark Game Configuration Data

- (void)gameConfigurationReceived
{
	FuseAPI_GameConfigurationReceived();
}

@end
