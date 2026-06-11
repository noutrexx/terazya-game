using DengeGame.Application;

namespace DengeGame.Tests.EditMode
{
    /// <summary>Testlerde deterministik, yapılandırılabilir rastgelelik. Hep sabit değer döndürür.</summary>
    internal sealed class StubRandom : IRandomService
    {
        private readonly int _intResult;
        private readonly double _doubleResult;

        public StubRandom(int intResult = 0, double doubleResult = 0.0)
        {
            _intResult = intResult;
            _doubleResult = doubleResult;
        }

        public void Reseed(int seed) { }
        public int NextInt(int maxExclusive) => _intResult;
        public int NextInt(int minInclusive, int maxExclusive) => _intResult;
        public double NextDouble() => _doubleResult;
    }
}
