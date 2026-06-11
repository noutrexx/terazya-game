using UnityEngine;

namespace DengeGame.Presentation.UI
{
    /// <summary>
    /// Bir RectTransform'u cihazın Screen.safeArea'sına oturtur (çentik/yuvarlak köşe desteği).
    /// Ekran değiştiğinde yeniden uygular. Farklı ekran oranlarında güvenli alanı korur.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake() => _rect = GetComponent<RectTransform>();

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreen.x || Screen.height != _lastScreen.y)
                Apply();
        }

        private void Apply()
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (Screen.width == 0 || Screen.height == 0) return;

            var safe = Screen.safeArea;
            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;

            _lastSafeArea = safe;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
