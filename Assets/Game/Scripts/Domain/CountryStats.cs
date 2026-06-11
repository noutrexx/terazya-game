using System;
using System.Collections.Generic;

namespace DengeGame.Domain
{
    /// <summary>Bir değerin değişimini açıklayan değişmez kayıt (event yükü).</summary>
    public readonly struct CountryStatChange
    {
        public readonly CountryValue Value;
        public readonly int Previous;
        public readonly int Current;
        public readonly string Reason;

        public int Delta => Current - Previous;

        public CountryStatChange(CountryValue value, int previous, int current, string reason)
        {
            Value = value;
            Previous = previous;
            Current = current;
            Reason = reason;
        }
    }

    /// <summary>
    /// Ülkenin altı değerini 0-100 aralığında tutan runtime modeli (saf C#, Unity'siz).
    /// Değer gerçekten değiştiğinde <see cref="Changed"/> event'i, değişiklik nedeniyle birlikte yayınlanır.
    /// </summary>
    [Serializable]
    public sealed class CountryStats
    {
        private readonly int[] _values = new int[CountryValueInfo.Count];

        /// <summary>Bir değer değiştiğinde (clamp sonrası net değişim != 0) yayınlanır.</summary>
        public event Action<CountryStatChange> Changed;

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

        /// <summary>Değeri doğrudan ayarlar (clamp). Değiştiyse event yayınlar. Önceki değeri döndürür.</summary>
        public int Set(CountryValue value, int amount, string reason = null)
        {
            int previous = _values[(int)value];
            int current = Clamp(amount);
            _values[(int)value] = current;
            if (current != previous)
                Changed?.Invoke(new CountryStatChange(value, previous, current, reason));
            return previous;
        }

        /// <summary>Delta uygular (clamp). Değiştiyse event yayınlar. Gerçekleşen net değişimi döndürür.</summary>
        public int Apply(CountryValue value, int delta, string reason = null)
        {
            int previous = _values[(int)value];
            int current = Clamp(previous + delta);
            _values[(int)value] = current;
            if (current != previous)
                Changed?.Invoke(new CountryStatChange(value, previous, current, reason));
            return current - previous;
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
