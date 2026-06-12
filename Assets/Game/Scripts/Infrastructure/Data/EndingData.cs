using System.Collections.Generic;
using DengeGame.Domain;
using UnityEngine;

namespace DengeGame.Infrastructure.Data
{
    /// <summary>Bir yönetim sonunun ScriptableObject authoring varlığı (ToDomain derin kopya).</summary>
    [CreateAssetMenu(fileName = "Ending_", menuName = "Denge/Ending", order = 8)]
    public sealed class EndingAsset : ScriptableObject
    {
        [SerializeField] private EndingDefinition ending = new EndingDefinition();
        public EndingDefinition Source => ending;
        public EndingDefinition ToDomain() => JsonUtility.FromJson<EndingDefinition>(JsonUtility.ToJson(ending));
    }

    /// <summary>Tüm son varlıklarını toplayan veritabanı (Resources'tan yüklenir).</summary>
    [CreateAssetMenu(fileName = "EndingDatabase", menuName = "Denge/Ending Database", order = 9)]
    public sealed class EndingDatabase : ScriptableObject
    {
        public const string ResourcesPath = "EndingDatabase";

        [SerializeField] private List<EndingAsset> endings = new List<EndingAsset>();
        public IReadOnlyList<EndingAsset> Assets => endings;
        public void SetEndings(List<EndingAsset> value) => endings = value ?? new List<EndingAsset>();

        public List<EndingDefinition> BuildList()
        {
            var list = new List<EndingDefinition>();
            foreach (var a in endings) if (a != null) list.Add(a.ToDomain());
            return list;
        }
    }
}
