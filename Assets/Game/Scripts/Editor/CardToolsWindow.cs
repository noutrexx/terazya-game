using System.Collections.Generic;
using System.Linq;
using DengeGame.Application.Validation;
using DengeGame.Domain;
using DengeGame.Infrastructure.Data;
using UnityEditor;
using UnityEngine;

namespace DengeGame.Editor
{
    /// <summary>
    /// Kart yazımını kolaylaştıran Editor aracı: tüm kartları toplu doğrular (benzersiz ID,
    /// eksik metin, geçersiz min/max, kırık/ulaşılamaz zincir) ve kategoriye göre listeler.
    /// </summary>
    public sealed class CardToolsWindow : EditorWindow
    {
        private Vector2 _scroll;
        private List<CardIssue> _issues = new List<CardIssue>();
        private Dictionary<string, List<string>> _byCategory = new Dictionary<string, List<string>>();
        private int _cardCount;

        [MenuItem("Denge/İçerik/Kart Araçları")]
        public static void Open() => GetWindow<CardToolsWindow>("Denge Kart Araçları");

        private void OnGUI()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Tüm Kartları Doğrula", GUILayout.Height(32)))
                Refresh();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Kart sayısı: {_cardCount}", EditorStyles.boldLabel);

            int errors = _issues.Count(i => i.Severity == IssueSeverity.Error);
            int warnings = _issues.Count - errors;
            var col = errors > 0 ? Color.red : (warnings > 0 ? new Color(0.8f, 0.6f, 0.1f) : Color.green);
            var prev = GUI.color; GUI.color = col;
            EditorGUILayout.LabelField($"Hata: {errors}   Uyarı: {warnings}");
            GUI.color = prev;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (_issues.Count > 0)
            {
                EditorGUILayout.LabelField("Bulgular", EditorStyles.boldLabel);
                foreach (var issue in _issues.OrderByDescending(i => i.Severity))
                {
                    GUI.color = issue.Severity == IssueSeverity.Error ? Color.red : new Color(0.85f, 0.7f, 0.2f);
                    EditorGUILayout.LabelField(issue.ToString());
                }
                GUI.color = prev;
                EditorGUILayout.Space();
            }

            if (_byCategory.Count > 0)
            {
                EditorGUILayout.LabelField("Kategoriye Göre Kartlar", EditorStyles.boldLabel);
                foreach (var pair in _byCategory.OrderBy(p => p.Key))
                {
                    EditorGUILayout.LabelField($"▸ {pair.Key} ({pair.Value.Count})", EditorStyles.miniBoldLabel);
                    foreach (var id in pair.Value.OrderBy(x => x))
                        EditorGUILayout.LabelField($"    • {id}");
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void Refresh()
        {
            var pool = LoadAllCards();
            _cardCount = pool.Count;
            _issues = CardValidator.Validate(pool);

            _byCategory = new Dictionary<string, List<string>>();
            foreach (var c in pool)
            {
                string cat = string.IsNullOrEmpty(c.Category) ? "(kategorisiz)" : c.Category;
                if (!_byCategory.TryGetValue(cat, out var list))
                    _byCategory[cat] = list = new List<string>();
                list.Add(c.Id);
            }
        }

        private static List<EventCard> LoadAllCards()
        {
            var pool = new List<EventCard>();
            foreach (var guid in AssetDatabase.FindAssets("t:EventCardAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<EventCardAsset>(path);
                if (asset != null) pool.Add(asset.ToDomain());
            }
            return pool;
        }
    }
}
