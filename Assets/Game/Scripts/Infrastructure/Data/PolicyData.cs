using System.Collections.Generic;
using DengeGame.Domain;
using UnityEngine;

namespace DengeGame.Infrastructure.Data
{
    /// <summary>Bir politikanın ScriptableObject authoring varlığı (ToDomain derin kopya).</summary>
    [CreateAssetMenu(fileName = "Policy_", menuName = "Denge/Policy", order = 4)]
    public sealed class PolicyAsset : ScriptableObject
    {
        [SerializeField] private PolicyDefinition policy = new PolicyDefinition();
        public PolicyDefinition Source => policy;
        public PolicyDefinition ToDomain() => JsonUtility.FromJson<PolicyDefinition>(JsonUtility.ToJson(policy));
    }

    /// <summary>Tüm politika varlıklarını toplayan veritabanı (Resources'tan yüklenir).</summary>
    [CreateAssetMenu(fileName = "PolicyDatabase", menuName = "Denge/Policy Database", order = 5)]
    public sealed class PolicyDatabase : ScriptableObject
    {
        public const string ResourcesPath = "PolicyDatabase";

        [SerializeField] private List<PolicyAsset> policies = new List<PolicyAsset>();
        public IReadOnlyList<PolicyAsset> Assets => policies;
        public void SetPolicies(List<PolicyAsset> value) => policies = value ?? new List<PolicyAsset>();

        public List<PolicyDefinition> BuildList()
        {
            var list = new List<PolicyDefinition>();
            foreach (var a in policies) if (a != null) list.Add(a.ToDomain());
            return list;
        }
    }
}
