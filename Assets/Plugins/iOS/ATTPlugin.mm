// 参考: http://blog.be-style.jpn.com/article/188329627.html#03

#import <string.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * ATT承認状態を取得(同期)
 **/
int Sge_Att_getTrackingAuthorizationStatus()
{
    if (@available(iOS 14, *)) {
        return (int)ATTrackingManager.trackingAuthorizationStatus;
    } else {
        return -1;
    }
}

/**
 * ATT承認を要求(非同期)
 **/
typedef void (*Callback)(int status);
void Sge_Att_requestTrackingAuthorization(Callback callback)
{
    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            if (callback != nil) {
                if (status == ATTrackingManagerAuthorizationStatusNotDetermined) {
                    NSLog(@"ATT status = NotDetermined(未設定) -> 端末設定でOK、アプリで未選択");
                } else if (status == ATTrackingManagerAuthorizationStatusRestricted) {
                    NSLog(@"ATT status = Restricted(制限あり)");
                } else if (status == ATTrackingManagerAuthorizationStatusDenied) {
                    NSLog(@"ATT status = Denied(不許可) -> 端末設定orアプリで不許可");
                } else if (status == ATTrackingManagerAuthorizationStatusAuthorized) {
                    NSLog(@"ATT status = Authorized(許可)");
                } else {
                    NSLog(@"ATT status = Other(その他)");
                }
                callback((int)status);
            }
        }];
    } else {
        NSLog(@"ATT status = iOS14 未満(非対応)");
        callback(-1);
    }
}
#ifdef __cplusplus
}
#endif