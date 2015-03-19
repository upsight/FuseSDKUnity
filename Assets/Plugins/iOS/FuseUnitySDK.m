
#import "FuseSDK.h"
#import "FuseUnitySDK.h"
#import "NSData-Base64.h"

#define FuseSafeRelease(obj) if (obj != nil){ \
[obj release]; \
obj = nil;}

#define ReturnNSString(obj) { \
const char* ___utf8String___ = ""; \
if( obj != nil ) { \
___utf8String___ = [obj UTF8String]; } \
char* ___retString___ = (char*)malloc(strlen(___utf8String___) + 1); \
strcpy(___retString___, ___utf8String___); \
return ___retString___; }

static char *fuseGameObject = "FuseSDK";
static const char *nilString = "";

static FuseSDK_Delegate* _FuseSDK_delegate = nil;


#pragma mark - Initialization


void Native_SetUnityGameObject(const char* unityGameObject)
{
    char* fuseGameObject = (char*)malloc(strlen(unityGameObject) + 1);
    strcpy(fuseGameObject, unityGameObject);
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

void Native_StartSession(const char* gameId , bool registerPush)
{
    _FuseSDK_delegate = [FuseSDK_Delegate new];
    [FuseSDK setPlatform:@"unity-ios"];
    [FuseSDK startSession:[NSString stringWithUTF8String:gameId] delegate:_FuseSDK_delegate withOptions:@{kFuseSDKOptionKey_RegisterForPush: [NSNumber numberWithBool:registerPush]}];
}


#pragma mark - Analytics

bool Native_RegisterEventWithDictionary(const char* message, const char** keys, const char** attributes, int numValues)
{
    NSMutableDictionary* values = [[NSMutableDictionary alloc] initWithCapacity:numValues];
    for( int i = 0; i < numValues; i++ )
    {
        [values setObject:[NSString stringWithUTF8String:attributes[i]] forKey:[NSString stringWithUTF8String:keys[i]]];
    }
    bool ret = [FuseSDK registerEvent:[NSString stringWithUTF8String:message] withDict:values];
    FuseSafeRelease(values);
    return ret;
}

bool Native_RegisterEventVariable(const char* name, const char* paramName, const char* paramValue, const char* variableName, double variableValue)
{
    return [FuseSDK registerEvent:[NSString stringWithUTF8String:name] ParameterName:[NSString stringWithUTF8String:paramName] ParameterValue:[NSString stringWithUTF8String:paramValue] VariableName:[NSString stringWithUTF8String:variableName] VariableValue:[NSNumber numberWithDouble:variableValue]];
}


#pragma mark - In-App Purchase Logging

@implementation FuseSDK_Product

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

void Native_RegisterInAppPurchaseList(const char** productIds, const char** priceLocales, float* prices, int numValues)
{
    //TODO
}

@implementation FuseSDK_Payment

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

@implementation FuseSDK_PaymentTransaction

-(id)init
{
    self = [super init];
    
    if (self)
    {
        _payment = [[FuseSDK_Payment alloc] init];
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

void Native_RegisterInAppPurchase(const char* productId, const char* transactionId, const unsigned char* transactionReceiptBuffer, int transactionReceiptLength, int transactionState)
{
    FuseSDK_PaymentTransaction* paymentTransaction = [[FuseSDK_PaymentTransaction alloc] init];
    [paymentTransaction.payment setIdentifier:productId];
    [paymentTransaction setTransactionReceiptWithBuffer:(void*)transactionReceiptBuffer length:transactionReceiptLength];
    paymentTransaction.transactionState = transactionState;
    paymentTransaction.transactionIdentifier = transactionId != nil ? [NSString stringWithUTF8String:transactionId] : @"No Transaction ID";
    
    [FuseSDK registerInAppPurchase:(SKPaymentTransaction*)paymentTransaction];
    FuseSafeRelease(paymentTransaction);
}

SKProductsRequest* _UniBillRequest = nil;
FuseSDK_PaymentTransaction* _UniBillPayment = nil;
void Native_RegisterUnibillPurchase(const char* productId, Byte* receipt, int receiptLength)
{
    // request the product information for this purchase so we can get the price and currency code
    // we register the IAP when we get the product respnse
    if( [SKPaymentQueue canMakePayments] && _UniBillRequest == nil && _UniBillPayment == nil)
    {
        _UniBillRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:[NSSet setWithObjects:[NSString stringWithUTF8String:productId], nil]];
        _UniBillRequest.delegate = _FuseSDK_delegate;
        
        [_UniBillRequest start];
    }
    else
    {
        NSLog(@"FUSE ERROR: Could not register IAP because we could not connect to the store");
        return;
    }
    
    // store the payment information
    // this is released after we register the IAP
    _UniBillPayment = [[FuseSDK_PaymentTransaction alloc] init];
    [_UniBillPayment.payment setIdentifier:productId];
    [_UniBillPayment setTransactionReceiptWithBuffer:(void*)receipt length:receiptLength];
    _UniBillPayment.transactionState = SKPaymentTransactionStatePurchased;
    _UniBillPayment.transactionIdentifier = @"No Transaction ID";
}


#pragma mark - Ads

bool Native_IsAdAvailableForZoneID(const char* _zoneId)
{
    return [FuseSDK isAdAvailableForZoneID:[NSString stringWithUTF8String:_zoneId]];
}

void Native_ShowAdForZoneID(const char* _zoneId, const char** optionKeys, const char** optionValues, int numOptions)
{
    NSMutableDictionary * _options = [NSMutableDictionary new];
    for(int i = 0 ; i < numOptions ; i++)
    {
        NSString * key = [NSString stringWithUTF8String:optionKeys[i]];
        NSString * value = [NSString stringWithUTF8String:optionValues[i]];
        
        if([value caseInsensitiveCompare:@"true"] == NSOrderedSame)
        {
            [_options setObject:@YES forKey: key];
        }
        else if([value caseInsensitiveCompare:@"false"] == NSOrderedSame)
        {
            [_options setObject:@NO forKey: key];
        }
        else
        {
            [_options setObject:value forKey: key];
        }
        
    }
    
    [FuseSDK showAdForZoneID:[NSString stringWithUTF8String:_zoneId] options:_options];
}

void Native_PreloadAdForZone(const char* _zoneId)
{
    
    [FuseSDK preloadAdForZoneID:[NSString stringWithUTF8String:_zoneId]];
}

NSString * base64NSString(NSString *str)
{
    NSString * ret = NULL;
    if(!str)
    return [[NSString alloc] initWithUTF8String:""];
    
    const char * c_str = [str UTF8String];
    size_t outputLength;
    char * outStr = FuseUnityBase64Encode(c_str, strlen(c_str), NO, &outputLength);
    
    if(outStr)
    {
        ret =  [[NSString alloc] initWithUTF8String:outStr];
        free(outStr);
    }
    
    return ret;
}
NSString * base64NSNumber(NSNumber * num)
{
    return base64NSString([NSString stringWithFormat:@"%@",num]);
}

const char* Native_GetRewardedInfoForZone(const char* _zoneId)
{
    
    FuseRewardedObject* rewObj = [FuseSDK getRewardedInfoForZoneID:[NSString stringWithUTF8String:_zoneId]];
    if(rewObj == nil)
    {
        ReturnNSString(@"");
    }
    
    NSArray * values = nil;
    
    NSString * preRoll = base64NSString(rewObj.preRollMessage);
    NSString * message = base64NSString(rewObj.rewardMessage);
    NSString * rewardID = base64NSString(rewObj.rewardItem);
    NSString * rewardQty = [NSString stringWithFormat:@"%@",rewObj.rewardAmount];
    if(preRoll && message && rewardID && rewardQty)
    {
        values = @[preRoll,message,rewardID,rewardQty];
        FuseSafeRelease(preRoll);
        FuseSafeRelease(message);
        FuseSafeRelease(rewardID);
        ReturnNSString([values componentsJoinedByString:@","]);
    }
    ReturnNSString(@"");
}


bool Native_ZoneHasRewarded(const char* _zoneId)
{
    return [FuseSDK zoneHasRewarded:[NSString stringWithUTF8String:_zoneId]];
}

bool Native_ZoneHasIAPOffer(const char* _zoneId)
{
    return [FuseSDK zoneHasIAPOffer:[NSString stringWithUTF8String:_zoneId]];
}

bool Native_ZoneHasVirtualGoodsOffer(const char* _zoneId)
{
    return [FuseSDK zoneHasVirtualGoodsOffer:[NSString stringWithUTF8String:_zoneId]];
}

void Native_DisplayMoreGames()
{
    [FuseSDK displayMoreGames];
}

void Native_RegisterPushToken(Byte* token, int size)
{
    NSData * data = [[NSData alloc] initWithBytes:token length:size];
    [FuseSDK applicationdidRegisterForRemoteNotificationsWithDeviceToken:data];
    
}


#pragma mark - Notifications

void Native_DisplayNotifications()
{
    [FuseSDK displayNotifications];
    
}
bool Native_IsNotificationAvailable()
{
    return [FuseSDK isNotificationAvailable];
    
}


#pragma mark - Account Login

const char* Native_GetFuseId()
{
    NSString* fuseId = [FuseSDK getFuseID];
    
    ReturnNSString(fuseId);
}

const char* Native_GetOriginalAccountAlias()
{
    NSString* accountAlias = [FuseSDK getOriginalAccountAlias];
    
    ReturnNSString(accountAlias);
}

const char* Native_GetOriginalAccountId()
{
    NSString* accountId = [FuseSDK getOriginalAccountID];
    
    ReturnNSString(accountId);
}

int Native_GetOriginalAccountType()
{
    return [FuseSDK getOriginalAccountType];
}

void Native_GameCenterLogin()
{
    [FuseSDK gameCenterLogin:[GKLocalPlayer localPlayer]];
}

void Native_FacebookLogin(const char* facebookId, const char* name, const char* accessToken)
{
    [FuseSDK facebookLogin:[NSString stringWithUTF8String:facebookId] Name:[NSString stringWithUTF8String:name] withAccessToken:[NSString stringWithUTF8String:accessToken]];
}

void Native_TwitterLogin(const char* twitterId, const char* alias)
{
    [FuseSDK twitterLogin:[NSString stringWithUTF8String:twitterId]];
}

void Native_DeviceLogin(const char* alias)
{
    [FuseSDK deviceLogin:[NSString stringWithUTF8String:alias]];
}

void Native_FuseLogin(const char* fuseId, const char* alias)
{
    [FuseSDK fuseLogin:[NSString stringWithUTF8String:fuseId] Alias:[NSString stringWithUTF8String:alias]];
}

void Native_EmailLogin(const char* email, const char* alias)
{
    [FuseSDK emailLogin:[NSString stringWithUTF8String:email] Alias:[NSString stringWithUTF8String:alias]];
}

void Native_GooglePlayLogin(const char* alias, const char* token)
{
    [FuseSDK googlePlayLogin:[NSString stringWithUTF8String:alias] AccessToken:[NSString stringWithUTF8String:token]];
}


#pragma mark - Miscellaneous

int Native_GamesPlayed()
{
    return [FuseSDK gamesPlayed];
}

const char* Native_LibraryVersion()
{
    NSString* libraryVersion = [FuseSDK libraryVersion];
    
    ReturnNSString(libraryVersion);
}

bool Native_Connected()
{
    return [FuseSDK connected];
}

void Native_TimeFromServer()
{
    [FuseSDK utcTimeFromServer];
}


#pragma mark - Data Opt In/Out

void Native_EnableData()
{
    [FuseSDK enableData];
}

void Native_DisableData()
{
    [FuseSDK disableData];
}

bool Native_DataEnabled()
{
    return [FuseSDK dataEnabled];
}


#pragma mark - Game Configuration Data

const char* Native_GetGameConfigurationValue(const char* key)
{
    NSString* value = [FuseSDK getGameConfigurationValue:[NSString stringWithUTF8String:key]];
    
    ReturnNSString(value);
}


#pragma mark - Friends List

void Native_UpdateFriendsListFromServer()
{
    [FuseSDK updateFriendsListFromServer];
}

void Native_AddFriend(const char* fuseId)
{
    [FuseSDK addFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_RemoveFriend(const char* fuseId)
{
    [FuseSDK removeFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_AcceptFriend(const char* fuseId)
{
    [FuseSDK acceptFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_RejectFriend(const char* fuseId)
{
    [FuseSDK rejectFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_MigrateFriends(const char* fuseId)
{
    [FuseSDK migrateFriends:[NSString stringWithUTF8String:fuseId]];
}


#pragma mark - User-to-User Push Notifications

void Native_UserPushNotification(const char* fuseId, const char* message)
{
    [FuseSDK userPushNotification:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message]];
}

void Native_FriendsPushNotification(const char* message)
{
    [FuseSDK friendsPushNotification:[NSString stringWithUTF8String:message]];
}


#pragma mark - Specific Event Registration

void Native_RegisterGender(int gender)
{
    [FuseSDK registerGender:gender];
}

void Native_RegisterLevel(int level)
{
    [FuseSDK registerLevel:level];
}

void Native_RegisterCurrency(int type, int balance)
{
    [FuseSDK registerCurrency:type Balance:balance];
}

void Native_RegisterAge(int age)
{
    [FuseSDK registerAge:age];
}

void Native_RegisterBirthday(int year, int month, int day)
{
    [FuseSDK registerBirthday:year Month:month Day:day];
}



#pragma mark - Callback

@implementation FuseSDK_Delegate

#pragma mark Initialization

+ (void)load
{
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidFinishLaunching:) name:UIApplicationDidFinishLaunchingNotification object:nil];
}

+ (void)applicationDidFinishLaunching:(NSNotification *)notification
{
}

#pragma mark Session Creation

- (void)sessionStartReceived
{
    CallUnity("_CB_SessionStartReceived", NULL);
}

- (void)sessionLoginError:(NSError*)_error
{
    CallUnity("_CB_SessionLoginError", [NSString stringWithFormat:@"%ld",[_error code]].UTF8String);
}

#pragma mark In-App Purchase Logging

- (void)puchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id
{
    NSArray *values = @[_verified,_tx_id,_o_tx_id];
    
    CallUnity("_CB_PurchaseVerification", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark Notifications

-(void) notificationAction:(NSString*)_action
{
    CallUnity("_CB_NotificationAction", _action.UTF8String);
}

-(void) notficationWillClose
{
    CallUnity("_CB_NotificationWillClose", NULL);
}

#pragma mark Account Login

- (void)accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id
{
    NSArray *values = @[_type,_account_id];
    
    CallUnity("_CB_AccountLoginComplete", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

- (void)accountLoginError:(NSError*)_error Account:(NSString*)_account_id;
{
    NSArray *values = @[[NSNumber numberWithInteger:_error.code],_account_id];
    
    CallUnity("_CB_AccountLoginError", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
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

-(void) friendAdded:(NSString*)_fuse_id Error:(NSError*)_error
{
    NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
    
    CallUnity("_CB_FriendAdded", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) friendRemoved:(NSString*)_fuse_id Error:(NSError*)_error
{
    NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
    
    CallUnity("_CB_FriendRemoved", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) friendAccepted:(NSString*)_fuse_id Error:(NSError*)_error
{
    NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
    
    CallUnity("_CB_FriendAccepted", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) friendRejected:(NSString*)_fuse_id Error:(NSError*)_error
{
    NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
    
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

-(void) friendsListError:(NSError*)_error
{
    CallUnity("_CB_FriendsListError", [NSString stringWithFormat:@"%ld", _error.code].UTF8String);
}

-(void) friendsMigrated:(NSString*)_fuse_id Error:(NSError*)_error
{
    NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
    
    CallUnity("_CB_FriendsMigrated", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}



#pragma mark Ads

-(void) adAvailabilityResponse:(NSNumber*)_available Error:(NSError*)_error
{
    NSArray *values = @[_available, [NSNumber numberWithInteger:[ _error code]]];
    
    CallUnity("_CB_AdAvailabilityResponse", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void) rewardedAdCompleteWithObject:(FuseRewardedObject*) reward
{
    if(reward == nil)
    {
        return;
    }
    
    NSArray * values = nil;
    
    NSString * preRoll = base64NSString(reward.preRollMessage);
    NSString * message = base64NSString(reward.rewardMessage);
    NSString * rewardID = base64NSString(reward.rewardItem);
    NSString * rewardQty = [NSString stringWithFormat:@"%@",reward.rewardAmount];
    if(preRoll && message && rewardID && rewardQty)
    {
        values = @[preRoll,message,rewardID,rewardQty];
        CallUnity("_CB_RewardedAdCompleted", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
    }
    
    FuseSafeRelease(preRoll);
    FuseSafeRelease(message);
    FuseSafeRelease(rewardID);
}

-(void) IAPOfferAcceptedWithObject:(FuseIAPOfferObject*) _offer
{
    if(_offer == nil)
    {
        return;
    }
    
    NSArray * values = nil;
    
    NSString * productID = base64NSString(_offer.productID);
    NSString * productPrice = [NSString stringWithFormat:@"%@",_offer.productPrice];
    NSString * itemName = base64NSString(_offer.itemName);
    NSString * itemAmount = [NSString stringWithFormat:@"%@",_offer.itemAmount];
    
    if(productID && productPrice && itemName && itemAmount)
    {
        values = @[productID, productPrice, itemName, itemAmount];
        CallUnity("_CB_IAPOfferAccepted", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
    }
    
    FuseSafeRelease(productID);
    FuseSafeRelease(itemName);
}

-(void) virtualGoodsOfferAcceptedWithObject:(FuseVirtualGoodsOfferObject*) _offer
{
    
    if(_offer == nil)
    {
        return;
    }
    
    NSArray * values = nil;
    
    NSString * purchaseCurrency = base64NSString(_offer.purchaseCurrency);
    NSString * purchasePrice = [NSString stringWithFormat:@"%@",_offer.purchasePrice];
    NSString * itemName = base64NSString(_offer.itemName);
    NSString * itemAmount = [NSString stringWithFormat:@"%@",_offer.itemAmount];
    
    if(purchaseCurrency && purchasePrice && itemName && itemAmount)
    {
        values = @[purchaseCurrency, purchasePrice, itemName, itemAmount];
        CallUnity("_CB_VirtualGoodsOfferAccepted", [[values componentsJoinedByString:@","] cStringUsingEncoding:NSUTF8StringEncoding]);
    }
    
    FuseSafeRelease(purchaseCurrency);
    FuseSafeRelease(itemName);
}

-(void) adWillClose
{
    CallUnity("_CB_AdWillClose", NULL);
}

#pragma mark Game Configuration Data

- (void)gameConfigurationReceived
{
    NSMutableArray *configData = [NSMutableArray new];
    NSMutableDictionary* dict = [FuseSDK getGameConfiguration];
    
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
    [FuseSDK registerInAppPurchaseList:response];
    
    // register the IAP now that we have the product information
    [FuseSDK registerInAppPurchase:(SKPaymentTransaction*)_UniBillPayment];
    
    // release the payment object
    FuseSafeRelease(_UniBillPayment);
    
    // finally release the reqest we alloc/initÕed in requestProUpgradeProductData
    FuseSafeRelease(_UniBillRequest);
}

@end
