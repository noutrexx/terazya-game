namespace DengeGame.Application
{
    /// <summary>
    /// Deterministik, seed destekli rastgelelik kaynağı (port).
    /// Tüm oyun rastgeleliği bu servis üzerinden akar; aynı seed → aynı sonuç (GDD Bölüm 28).
    /// </summary>
    public interface IRandomService
    {
        /// <summary>Servisi belirli bir seed ile yeniden başlatır.</summary>
        void Reseed(int seed);

        /// <summary>[0, max) aralığında tamsayı.</summary>
        int NextInt(int maxExclusive);

        /// <summary>[min, max) aralığında tamsayı.</summary>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>[0.0, 1.0) aralığında ondalık.</summary>
        double NextDouble();
    }
}
