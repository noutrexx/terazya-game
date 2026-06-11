namespace DengeGame.Domain
{
    /// <summary>
    /// Ülkenin yönetim dönemleri. Yıl eşiklerine göre belirlenir (GDD Bölüm 20).
    /// </summary>
    public enum GamePeriod
    {
        Founding = 0, // Kuruluş
        Growth = 1,   // Büyüme
        Maturity = 2, // Olgunluk
        Legacy = 3    // Miras
    }
}
