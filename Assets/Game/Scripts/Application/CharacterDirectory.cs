using System.Collections.Generic;
using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>Karakterleri id ile aramayı sağlayan salt-okunur dizin.</summary>
    public sealed class CharacterDirectory
    {
        private readonly Dictionary<string, Character> _map = new Dictionary<string, Character>();
        private readonly List<Character> _all = new List<Character>();

        public CharacterDirectory(IEnumerable<Character> characters)
        {
            if (characters == null) return;
            foreach (var c in characters)
            {
                if (c == null || string.IsNullOrEmpty(c.Id) || _map.ContainsKey(c.Id)) continue;
                _map[c.Id] = c;
                _all.Add(c);
            }
        }

        public IReadOnlyList<Character> All => _all;

        public Character Get(string id) =>
            !string.IsNullOrEmpty(id) && _map.TryGetValue(id, out var c) ? c : null;

        public string DisplayName(string id) => Get(id)?.Name ?? id;
    }
}
