using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace AttSample.Editor
{
    public static class AttPostProcess
    {
        /// <summary>
        /// ビルドのとき、最後に実行される
        /// iOSプロジェクトの設定ファイル周りを書き換えている
        /// </summary>
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;

            // iOSプロジェクトにAppTrackingTransparency.frameworkを追加
            AddFrameworkToProject(path);

            // ATTダイアログの中に表示する文言を指定
            AddUserTrackingUsageDescription(path, "好みに合わせた広告を表示するために使用されます。");

            // SkAdNetworkIdを設定する場合はここに列挙
            // 参考: https://developers.google.com/admob/ios/ios14?hl=ja
            AddSkAdNetworkIdentifiers(path, new[]
            {
                "cstr6suwn9.skadnetwork" // ←例。GoogleのSkAdNetworkId
            });
        }

        private static void AddFrameworkToProject(string path)
        {
            var pbxPath = PBXProject.GetPBXProjectPath(path);

            var pbx = new PBXProject();
            pbx.ReadFromFile(pbxPath);

            var guid = pbx.GetUnityFrameworkTargetGuid();
            pbx.AddFrameworkToProject(guid, "AppTrackingTransparency.framework", true);

            pbx.WriteToFile(pbxPath);
        }

        private static void AddUserTrackingUsageDescription(string path, string val)
        {
            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();

            plist.ReadFromFile(plistPath);

            plist.root.SetString("NSUserTrackingUsageDescription", val);

            plist.WriteToFile(plistPath);
        }

        private static void AddSkAdNetworkIdentifiers(string path, string[] identifiers)
        {
            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();

            plist.ReadFromFile(plistPath);

            var count = plist.root.values
                .Where(kv => kv.Key == "SKAdNetworkItems")
                .Select(kv => kv.Value)
                .Count();

            var skAdNetworkItems = count == 0
                ? plist.root.CreateArray("SKAdNetworkItems")
                : plist.root.values["SKAdNetworkItems"].AsArray();

            foreach (var identifier in identifiers)
                skAdNetworkItems.AddDict().SetString("SKAdNetworkIdentifier", identifier);

            plist.WriteToFile(plistPath);
        }
    }
}