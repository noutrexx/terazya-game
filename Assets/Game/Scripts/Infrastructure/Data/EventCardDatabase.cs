using System.Collections.Generic;
using DengeGame.Domain;
using UnityEngine;

namespace DengeGame.Infrastructure.Data
{
    /// <summary>
    /// Tüm olay kartı varlıklarını toplayan ScriptableObject veritabanı. Runtime'da
    /// Resources üzerinden yüklenebilir ve seçim servisine domain havuzu sağlar.
    /// </summary>
    [CreateAssetMenu(fileName = "EventCardDatabase", menuName = "Denge/Event Card Database", order = 1)]
    public sealed class EventCardDatabase : ScriptableObject
    {
        public const string ResourcesPath = "EventCardDatabase";

        [SerializeField] private List<EventCardAsset> cards = new List<EventCardAsset>();

        public IReadOnlyList<EventCardAsset> Assets => cards;

        public void SetCards(List<EventCardAsset> value) => cards = value ?? new List<EventCardAsset>();

        /// <summary>Tüm kartların derin kopyalanmış domain havuzunu üretir.</summary>
        public List<EventCard> BuildPool()
        {
            var pool = new List<EventCard>();
            foreach (var asset in cards)
                if (asset != null) pool.Add(asset.ToDomain());
            return pool;
        }
    }
}
