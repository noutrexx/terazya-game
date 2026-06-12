using System.Collections.Generic;
using DengeGame.Domain;

namespace DengeGame.Application.Endings
{
    /// <summary>
    /// Son anahtarını (reason) zengin EndingDefinition'a çözer. Tanımlı olmayan sonlar için
    /// makul bir varsayılan (nötr, anahtarı başlık olarak) döndürür.
    /// </summary>
    public sealed class EndingRegistry
    {
        private readonly Dictionary<string, EndingDefinition> _map = new Dictionary<string, EndingDefinition>();

        public EndingRegistry(IEnumerable<EndingDefinition> definitions)
        {
            if (definitions == null) return;
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id) && !_map.ContainsKey(d.Id))
                    _map[d.Id] = d;
        }

        public int Count => _map.Count;

        public bool TryGet(string reason, out EndingDefinition def) => _map.TryGetValue(reason ?? "", out def);

        public EndingDefinition GetOrDefault(string reason)
        {
            if (!string.IsNullOrEmpty(reason) && _map.TryGetValue(reason, out var d)) return d;
            return new EndingDefinition(reason ?? "Bilinmeyen Son", reason ?? "Bilinmeyen Son")
            {
                Description = "Yönetiminiz sona erdi.",
                Category = EndingCategory.Neutral,
                Rarity = EndingRarity.Common,
                ThemeColor = new ColorRGB(0.4f, 0.4f, 0.45f)
            };
        }
    }
}
