using UnityEngine;

namespace DengeGame.Presentation.UI
{
    /// <summary>Asset gerektirmeyen, erişilebilir kontrastlı renk paleti (tek kaynak).</summary>
    public static class UiTheme
    {
        public static readonly Color Background = new Color(0.10f, 0.12f, 0.16f);
        public static readonly Color Panel = new Color(0.16f, 0.19f, 0.25f);
        public static readonly Color Card = new Color(0.93f, 0.93f, 0.90f);
        public static readonly Color CardText = new Color(0.12f, 0.13f, 0.16f);
        public static readonly Color TextLight = new Color(0.92f, 0.94f, 0.98f);
        public static readonly Color TextMuted = new Color(0.65f, 0.70f, 0.80f);
        public static readonly Color Accent = new Color(0.30f, 0.55f, 0.85f);

        public static readonly Color Positive = new Color(0.35f, 0.75f, 0.45f);
        public static readonly Color Negative = new Color(0.85f, 0.40f, 0.35f);
        public static readonly Color Warning = new Color(0.90f, 0.70f, 0.20f);
        public static readonly Color StatFill = new Color(0.45f, 0.65f, 0.90f);
        public static readonly Color StatCritical = new Color(0.85f, 0.35f, 0.30f);

        public const int FontTitle = 52;
        public const int FontBody = 34;
        public const int FontSmall = 26;
        public const int FontStat = 22;
    }

    /// <summary>Çalışma zamanı UI tercihleri (Faz 13 ayarlar sistemine bağlanacak).</summary>
    public static class UiPreferences
    {
        /// <summary>Animasyonları azalt (erişilebilirlik). true ise anlık geçişler kullanılır.</summary>
        public static bool ReduceMotion = false;

        /// <summary>Kaydırma yerine karar butonlarını göster.</summary>
        public static bool ShowDecisionButtons = true;
    }
}
