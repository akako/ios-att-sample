// 参考: http://blog.be-style.jpn.com/article/188329627.html#03

#if UNITY_EDITOR || UNITY_IOS
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using UnityEngine;

namespace AttSample.Scripts
{
    public enum AtTrackingManagerAuthorizationStatus
    {
        Empty = -999,
        NotSupported = -1,
        NotDetermined = 0,
        Restricted = 1,
        Denied = 2,
        Authorized = 3
    }

    public static class AttPlugin
    {
        private const string DLLName = "__Internal";
        private static SynchronizationContext _context;
        private static Action<AtTrackingManagerAuthorizationStatus> _onComplete;

        /// <summary>
        /// ATTダイアログ表示済みかどうか判定します
        /// </summary>
        /// <returns></returns>
        public static bool IsNotDetermined()
        {
            return GetTrackingAuthorizationStatus() == AtTrackingManagerAuthorizationStatus.NotDetermined;
        }

        /// <summary>
        /// ATTの承認ステータスを取得します
        /// </summary>
        /// <returns></returns>
        public static AtTrackingManagerAuthorizationStatus GetTrackingAuthorizationStatus()
        {
            if (Application.isEditor)
            {
                // Editorの場合のダミー処理
                return AtTrackingManagerAuthorizationStatus.NotDetermined;
            }
            return (AtTrackingManagerAuthorizationStatus) Sge_Att_getTrackingAuthorizationStatus();
        }
        
        /// <summary>
        /// ATTの承認ダイアログを表示します
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerator RequestTrackingAuthorization()
        {
            if (Application.isEditor)
            {
                // Editorの場合のダミー処理。一応書いてるだけで意味はない
                Debug.Log("EditorなのでATTダイアログはスキップ");
                yield return new WaitForSeconds(1);
                yield break;
            }

            if (_onComplete != null)
            {
                // ATT承認のコールバックを待たずに連続コールはNGとする
                throw new Exception("連続コールはできません");
            }

            var gotStatus = AtTrackingManagerAuthorizationStatus.Empty;
            
            // コールバックを仕込んでATT承認要求
            _context = SynchronizationContext.Current;
            _onComplete = status =>
            {
                gotStatus = status;
            };
            Sge_Att_requestTrackingAuthorization(OnRequestComplete);

            while (gotStatus == AtTrackingManagerAuthorizationStatus.Empty)
            {
                // 承認が終わるのを待つ
                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// ATT承認状態を取得する処理
        /// Plugins/iOS/ATTPlugin.mm の同名のメソッドが呼ばれる
        /// </summary>
        /// <returns></returns>
        [DllImport(DLLName)]
        private static extern int Sge_Att_getTrackingAuthorizationStatus();

        /// <summary>
        /// ネイティブにATT承認を要求する処理
        /// Plugins/iOS/ATTPlugin.mm の同名のメソッドが呼ばれる
        /// </summary>
        /// <param name="callback"></param>
        [DllImport(DLLName)]
        private static extern void Sge_Att_requestTrackingAuthorization(OnCompleteCallback callback);

        /// <summary>
        /// ATT承認のコールバック
        /// </summary>
        /// <param name="status"></param>
        [MonoPInvokeCallback(typeof(OnCompleteCallback))]
        private static void OnRequestComplete(int status)
        {
            if (_onComplete != null)
                _context.Post(_ =>
                {
                    _onComplete?.Invoke((AtTrackingManagerAuthorizationStatus) status);
                    _onComplete = null;
                }, null);
        }

        private delegate void OnCompleteCallback(int status);
    }
}
#endif