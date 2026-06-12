using System.Collections.Generic;
using DengeGame.Domain;

namespace DengeGame.Application.Policies
{
    /// <summary>Politika tanımlarını id ile aramayı sağlayan salt-okunur kayıt.</summary>
    public sealed class PolicyRegistry
    {
        private readonly Dictionary<string, PolicyDefinition> _map = new Dictionary<string, PolicyDefinition>();

        public PolicyRegistry(IEnumerable<PolicyDefinition> definitions)
        {
            if (definitions == null) return;
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id) && !_map.ContainsKey(d.Id))
                    _map[d.Id] = d;
        }

        public PolicyDefinition Get(string id) =>
            !string.IsNullOrEmpty(id) && _map.TryGetValue(id, out var d) ? d : null;

        public int Count => _map.Count;
    }

    /// <summary>Kriz tanımlarını id ile aramayı sağlayan salt-okunur kayıt.</summary>
    public sealed class CrisisRegistry
    {
        private readonly Dictionary<string, CrisisDefinition> _map = new Dictionary<string, CrisisDefinition>();

        public CrisisRegistry(IEnumerable<CrisisDefinition> definitions)
        {
            if (definitions == null) return;
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id) && !_map.ContainsKey(d.Id))
                    _map[d.Id] = d;
        }

        public CrisisDefinition Get(string id) =>
            !string.IsNullOrEmpty(id) && _map.TryGetValue(id, out var d) ? d : null;

        public int Count => _map.Count;
    }
}
