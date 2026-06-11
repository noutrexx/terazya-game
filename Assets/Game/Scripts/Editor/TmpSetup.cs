using System.IO;
using UnityEditor;
using UnityEngine;

namespace DengeGame.Editor
{
    /// <summary>
    /// TextMeshPro temel kaynaklarını (varsayılan font asset + TMP Settings) içe aktarır.
    /// ImportPackage asenkron olduğundan batchmode'da -quit YERİNE tamamlanma callback'inde
    /// EditorApplication.Exit ile çıkılır (timeout güvenliğiyle).
    /// </summary>
    public static class TmpSetup
    {
        private static double _timeoutAt;
        private static bool _batch;

        [MenuItem("Denge/TMP Temel Kaynaklarını İçe Aktar")]
        public static void ImportEssentials()
        {
            _batch = UnityEngine.Application.isBatchMode;

            if (Directory.Exists("Assets/TextMesh Pro"))
            {
                Debug.Log("[Denge] TMP temel kaynakları zaten mevcut.");
                Quit(0);
                return;
            }

            string pkg = FindEssentialsPackage();
            if (pkg == null)
            {
                Debug.LogError("[Denge] TMP Essential Resources.unitypackage bulunamadı.");
                Quit(1);
                return;
            }

            AssetDatabase.importPackageCompleted += OnCompleted;
            AssetDatabase.importPackageFailed += OnFailed;
            _timeoutAt = EditorApplication.timeSinceStartup + 180;
            EditorApplication.update += OnUpdate;

            AssetDatabase.ImportPackage(pkg, interactive: false);
        }

        private static void OnUpdate()
        {
            if (EditorApplication.timeSinceStartup > _timeoutAt)
            {
                Debug.LogError("[Denge] TMP içe aktarma zaman aşımı.");
                Cleanup();
                Quit(2);
            }
        }

        private static void OnCompleted(string packageName)
        {
            Debug.Log($"[Denge] TMP temel kaynakları içe aktarıldı: {packageName}");
            Cleanup();
            AssetDatabase.Refresh();
            Quit(0);
        }

        private static void OnFailed(string packageName, string error)
        {
            Debug.LogError($"[Denge] TMP içe aktarma başarısız: {error}");
            Cleanup();
            Quit(1);
        }

        private static void Cleanup()
        {
            AssetDatabase.importPackageCompleted -= OnCompleted;
            AssetDatabase.importPackageFailed -= OnFailed;
            EditorApplication.update -= OnUpdate;
        }

        private static void Quit(int code)
        {
            if (_batch) EditorApplication.Exit(code);
        }

        private static string FindEssentialsPackage()
        {
            foreach (var dir in Directory.GetDirectories("Library/PackageCache"))
            {
                if (!Path.GetFileName(dir).StartsWith("com.unity.textmeshpro")) continue;
                string candidate = Path.Combine(dir, "Package Resources", "TMP Essential Resources.unitypackage");
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }
    }
}
