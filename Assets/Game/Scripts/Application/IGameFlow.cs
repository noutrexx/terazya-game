namespace DengeGame.Application
{
    /// <summary>
    /// Üst düzey oyun akışı (port): yeni oyun, yönetimi bitirme, menüye dönüş.
    /// Sahne geçişlerini ve oturum yaşam döngüsünü koordine eder. Implementasyonu Presentation'dadır.
    /// </summary>
    public interface IGameFlow
    {
        /// <summary>Şu an aktif oturum (yoksa null).</summary>
        GameSession CurrentSession { get; }

        /// <summary>Yeni bir yönetim başlatır ve oyun sahnesine geçer. seed null ise üretilir.</summary>
        void StartNewGame(int? seed = null);

        /// <summary>Aktif yönetimi sonlandırır ve sonuç sahnesine geçer.</summary>
        void EndCurrentGame(string reason);

        /// <summary>Ana menüye döner.</summary>
        void ReturnToMenu();
    }
}
