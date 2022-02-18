#import "UICKeyChainStore.h"
#import "KeyChainAccess.h"
#import "Security/Security.h"
#import <CommonCrypto/CommonCryptor.h>

@implementation KeyChainAccess

NSString* getKeys(NSString* serviceName)
{
    UICKeyChainStore *keychain = [UICKeyChainStore keyChainStoreWithService:serviceName];
    return [keychain.allKeys componentsJoinedByString:@","];
}

void deleteKey(NSString* serviceName, NSString* keyName)
{
    UICKeyChainStore *keychain = [UICKeyChainStore keyChainStoreWithService:serviceName];
    NSLog(@"removeKey with keyName: %@ from keyChain: %@", keyName, serviceName);
    [keychain removeItemForKey:keyName];
}

void getPrivateKey(NSString* serviceName, NSString* keyName, Byte* data, int size)
{
    NSLog(@"getPrivateKey with keyName: %@ from keyChain: %@", keyName, serviceName);
    
    if(size != kCCKeySizeAES128 && size != kCCKeySizeAES256) {
        NSLog(@"Invalid key size (%d) for AES symmetric key gen. Valid options: %d, %d", size, kCCKeySizeAES128, kCCKeySizeAES256);
        return;
    }
    
    // First check if keychain + key exists
    UICKeyChainStore *keychain = [UICKeyChainStore keyChainStoreWithService:serviceName];
    
    if(![keychain contains:keyName]) {
        NSLog(@"private key not found in keychain: %@", serviceName);
        NSData *key = [KeyChainAccess generateSymmetricKeyOfLength: size];
        [keychain setData:key forKey:keyName];
        key = nil;
    }
    
    int key_len = [[keychain dataForKey:keyName] length];
    if(key_len != size) {
        NSLog(@"stored key len != expected key len: %d != %d", key_len, size);
        return;
    }
    
    NSLog(@"Copying key of len %d (bytes) into array", key_len);
    // Copy keychain key data into data byte array
    [[keychain dataForKey:keyName] getBytes:data];
}

+(NSData *)generateSymmetricKeyOfLength:(size_t)length {
    // generate a unique AES key
    NSLog(@"generate symmetric key with length: %zu", length);
    NSMutableData *key = [NSMutableData dataWithLength:length];
    int result = SecRandomCopyBytes(kSecRandomDefault, length, (uint8_t*)key.mutableBytes);
    
    NSAssert(result == 0, @"Unable to generate random bytes: %d", errno);
    return key;
}

@end

char* cStringCopy(const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

extern "C" {
    void _deleteKey(char* serviceName, char* keyName) {
        deleteKey([[NSString alloc] initWithUTF8String:serviceName], [[NSString alloc] initWithUTF8String:keyName]);
    };
    
    void _getPrivateKey(char* serviceName, char* keyName, Byte* data, int size) {
        getPrivateKey([[NSString alloc] initWithUTF8String:serviceName], [[NSString alloc] initWithUTF8String:keyName], data, size);
    }
    
    const char* _getKeys(char* serviceName) {
        NSString *keyStr = [[NSString alloc] initWithUTF8String:serviceName];
        
        // NOTE: To avoid memory leaks remember to free this string again once bytes are turned into a managed string
        return cStringCopy([keyStr UTF8String]);
    }
}


