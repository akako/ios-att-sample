using System.Collections;
using UnityEngine;

namespace AttSample.Scripts
{
    public class SampleSceneController : MonoBehaviour
    {
        private IEnumerator Start()
        {
#if UNITY_EDITOR || UNITY_IOS
            // iOSのATT対応
            if (AttPlugin.IsNotDetermined())
            {
                // TODO ATTダイアログの前に独自ダイアログを表示したい場合は、ここに書く

                // ATTダイアログのポップアップ
                yield return AttPlugin.RequestTrackingAuthorization();
            }
#endif
        
            // TODO ATTダイアログの表示が終わったら、広告SDKをイニシャライズ
            
        }
    }
}
