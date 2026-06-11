using System;
using DengeGame.Application;

namespace DengeGame.Infrastructure
{
    /// <summary>
    /// System.Random tabanlı deterministik rastgelelik servisi. Aynı seed → aynı dizi.
    /// UnityEngine.Random KULLANILMAZ (global durum ve determinizm sorunları nedeniyle).
    /// </summary>
    public sealed class SystemRandomService : IRandomService
    {
        private Random _random;

        public SystemRandomService(int seed)
        {
            _random = new Random(seed);
        }

        public void Reseed(int seed) => _random = new Random(seed);

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0) return 0;
            return _random.Next(maxExclusive);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            return _random.Next(minInclusive, maxExclusive);
        }

        public double NextDouble() => _random.NextDouble();
    }
}
