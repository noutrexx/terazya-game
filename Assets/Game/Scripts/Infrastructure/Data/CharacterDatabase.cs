using System.Collections.Generic;
using DengeGame.Domain;
using UnityEngine;

namespace DengeGame.Infrastructure.Data
{
    /// <summary>Tüm karakter varlıklarını toplayan ScriptableObject veritabanı (Resources'tan yüklenir).</summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Denge/Character Database", order = 3)]
    public sealed class CharacterDatabase : ScriptableObject
    {
        public const string ResourcesPath = "CharacterDatabase";

        [SerializeField] private List<CharacterAsset> characters = new List<CharacterAsset>();

        public IReadOnlyList<CharacterAsset> Assets => characters;

        public void SetCharacters(List<CharacterAsset> value) => characters = value ?? new List<CharacterAsset>();

        public List<Character> BuildList()
        {
            var list = new List<Character>();
            foreach (var a in characters)
                if (a != null) list.Add(a.ToDomain());
            return list;
        }
    }
}
