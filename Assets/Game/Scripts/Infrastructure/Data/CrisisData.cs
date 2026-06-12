using System.Collections.Generic;
using DengeGame.Domain;
using UnityEngine;

namespace DengeGame.Infrastructure.Data
{
    /// <summary>Bir krizin ScriptableObject authoring varlığı (ToDomain derin kopya).</summary>
    [CreateAssetMenu(fileName = "Crisis_", menuName = "Denge/Crisis", order = 6)]
    public sealed class CrisisAsset : ScriptableObject
    {
        [SerializeField] private CrisisDefinition crisis = new CrisisDefinition();
        public CrisisDefinition Source => crisis;
        public CrisisDefinition ToDomain() => JsonUtility.FromJson<CrisisDefinition>(JsonUtility.ToJson(crisis));
    }

    /// <summary>Tüm kriz varlıklarını toplayan veritabanı (Resources'tan yüklenir).</summary>
    [CreateAssetMenu(fileName = "CrisisDatabase", menuName = "Denge/Crisis Database", order = 7)]
    public sealed class CrisisDatabase : ScriptableObject
    {
        public const string ResourcesPath = "CrisisDatabase";

        [SerializeField] private List<CrisisAsset> crises = new List<CrisisAsset>();
        public IReadOnlyList<CrisisAsset> Assets => crises;
        public void SetCrises(List<CrisisAsset> value) => crises = value ?? new List<CrisisAsset>();

        public List<CrisisDefinition> BuildList()
        {
            var list = new List<CrisisDefinition>();
            foreach (var a in crises) if (a != null) list.Add(a.ToDomain());
            return list;
        }
    }
}
