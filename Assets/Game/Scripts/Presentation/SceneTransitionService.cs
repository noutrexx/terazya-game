using DengeGame.Application;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DengeGame.Presentation
{
    /// <summary>
    /// ISceneTransitionService'in Unity implementasyonu. Sahnenin Build Settings'te kayıtlı
    /// olmadığı durumda anlaşılır hata üretir (sessizce başarısız olmaz).
    /// </summary>
    public sealed class SceneTransitionService : ISceneTransitionService
    {
        public void Load(GameScene scene)
        {
            string sceneName = scene.ToSceneName();

            if (!UnityEngine.Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"[Denge] '{sceneName}' sahnesi Build Settings'te bulunamadı. " +
                    "File > Build Settings ile sahneyi ekleyin veya 'Denge/Setup Project' aracını çalıştırın.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
