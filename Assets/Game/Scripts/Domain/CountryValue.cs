namespace DengeGame.Domain
{
    /// <summary>
    /// Ülkenin altı temel değeri. Sıralama UI ve kayıt için sabit kabul edilmelidir.
    /// </summary>
    public enum CountryValue
    {
        Economy = 0,
        PublicSupport = 1,
        Security = 2,
        Freedom = 3,
        Environment = 4,
        Diplomacy = 5
    }

    public static class CountryValueInfo
    {
        /// <summary>Tüm değerler, sabit sırayla. UI ve iterasyon için tek kaynak.</summary>
        public static readonly CountryValue[] All =
        {
            CountryValue.Economy,
            CountryValue.PublicSupport,
            CountryValue.Security,
            CountryValue.Freedom,
            CountryValue.Environment,
            CountryValue.Diplomacy
        };

        public const int Count = 6;
        public const int Min = 0;
        public const int Max = 100;
    }
}
