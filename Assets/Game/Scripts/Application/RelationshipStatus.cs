namespace DengeGame.Application
{
    /// <summary>
    /// İlişki değerini (-100..100) oyuncuya gösterilen DURUM etiketine çevirir.
    /// Kesin sayı yerine açıklayıcı durum gösterilir (GDD: ilişki sayı olarak verilmez).
    /// </summary>
    public static class RelationshipStatus
    {
        public static readonly string[] DefaultTiers =
            { "Düşman", "Mesafeli", "Tarafsız", "Müttefik", "Sadık" };

        public static string Of(int value, string[] tierNames = null)
        {
            var t = (tierNames != null && tierNames.Length == 5) ? tierNames : DefaultTiers;
            if (value <= -60) return t[0];
            if (value <= -20) return t[1];
            if (value < 20) return t[2];
            if (value < 60) return t[3];
            return t[4];
        }
    }
}
