using System.Collections.Generic;
using DengeGame.Domain;

namespace DengeGame.Application
{
    public enum HintDirection { Up = 1, Down = -1 }

    /// <summary>
    /// Bir kararın hangi değerleri olumlu/olumsuz etkileyebileceğini KESİN SAYI vermeden çıkarır.
    /// UI yalnızca yön ipucu (↑/↓) gösterir; belirsizlik korunur (GDD Bölüm 10).
    /// </summary>
    public static class DecisionPreview
    {
        public static IReadOnlyDictionary<CountryValue, HintDirection> Preview(EventCard card, DecisionSide side)
        {
            var net = new Dictionary<CountryValue, int>();
            if (card == null) return Empty(net);

            foreach (var e in card.EffectsFor(side))
            {
                if (e == null) continue;
                switch (e.Kind)
                {
                    case EffectKind.ChangeValue:
                        Add(net, e.Value, e.IntA);
                        break;
                    case EffectKind.RandomValue:
                        Add(net, e.Value, e.IntA + e.IntB); // ortalama işareti
                        break;
                    case EffectKind.TimedValue:
                        Add(net, e.Value, e.IntA);
                        break;
                }
            }

            var result = new Dictionary<CountryValue, HintDirection>();
            foreach (var pair in net)
                if (pair.Value != 0)
                    result[pair.Key] = pair.Value > 0 ? HintDirection.Up : HintDirection.Down;
            return result;
        }

        private static void Add(Dictionary<CountryValue, int> net, CountryValue v, int delta)
        {
            net.TryGetValue(v, out int cur);
            net[v] = cur + delta;
        }

        private static IReadOnlyDictionary<CountryValue, HintDirection> Empty(Dictionary<CountryValue, int> _) =>
            new Dictionary<CountryValue, HintDirection>();
    }
}
