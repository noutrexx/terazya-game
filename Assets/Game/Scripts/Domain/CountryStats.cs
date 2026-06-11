using System;
using System.Collections.Generic;

namespace DengeGame.Domain
{
    /// <summary>
    /// Ülkenin altı değerini 0-100 aralığında tutan runtime modeli.
    /// Bu sınıf saf C#'tır; Unity API'sine bağlı değildir ve test edilebilir.
    /// Faz 4'te değişiklik nedeni kaydı ve event yayını ile genişletilecektir.
    /// </summary>
    [Serializable]
    public sealed class CountryStats
    {
        // Sabit sıralı dizi; index = (int)CountryValue.
        private readonly int[] _values = new int[CountryValueInfo.Count];

        public CountryStats()
        {
            for (int i = 0; i < _values.Length; i++)
                _values[i] = 50;
        }

        public CountryStats(int initialValue)
        {
            int v = Clamp(initialValue);
            for (int i = 0; i < _values.Length; i++)
                _values[i] = v;
        }

        public CountryStats(IReadOnlyDictionary<CountryValue, int> initialValues)
        {
            for (int i = 0; i < _values.Length; i++)
                _values[i] = 50;

            if (initialValues == null) return;
            foreach (var pair in initialValues)
                _values[(int)pair.Key] = Clamp(pair.Value);
        }

        public int Get(CountryValue value) => _values[(int)value];

        /// <summary>Değeri doğrudan ayarlar (sınır içine alınır). Önceki değeri döndürür.</summary>
        public int Set(CountryValue value, int amount)
        {
            int previous = _values[(int)value];
            _values[(int)value] = Clamp(amount);
            return previous;
        }

        /// <summary>Değere delta uygular (sınır içine alınır). Gerçekleşen net değişimi döndürür.</summary>
        public int Apply(CountryValue value, int delta)
        {
            int previous = _values[(int)value];
            _values[(int)value] = Clamp(previous + delta);
            return _values[(int)value] - previous;
        }

        public bool IsAtCriticalLow(CountryValue value) => _values[(int)value] <= CountryValueInfo.Min;
        public bool IsAtCriticalHigh(CountryValue value) => _values[(int)value] >= CountryValueInfo.Max;

        public CountryStats Clone()
        {
            var clone = new CountryStats();
            Array.Copy(_values, clone._values, _values.Length);
            return clone;
        }

        public IReadOnlyDictionary<CountryValue, int> ToDictionary()
        {
            var dict = new Dictionary<CountryValue, int>(CountryValueInfo.Count);
            foreach (var v in CountryValueInfo.All)
                dict[v] = _values[(int)v];
            return dict;
        }

        private static int Clamp(int v)
        {
            if (v < CountryValueInfo.Min) return CountryValueInfo.Min;
            if (v > CountryValueInfo.Max) return CountryValueInfo.Max;
            return v;
        }
    }
}
