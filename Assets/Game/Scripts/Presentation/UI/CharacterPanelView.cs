using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DengeGame.Presentation.UI
{
    /// <summary>
    /// Karakter listesi + detay + ilişki geçmişi gösteren tam ekran overlay.
    /// İlişkiler kesin sayı yerine durum etiketi olarak gösterilir.
    /// </summary>
    public sealed class CharacterPanelView : MonoBehaviour
    {
        private CharacterDirectory _dir;
        private GameState _state;
        private RectTransform _content;

        public static CharacterPanelView Show(Transform canvasParent, CharacterDirectory dir, GameState state)
        {
            var go = new GameObject("CharacterPanel", typeof(RectTransform), typeof(CharacterPanelView));
            var view = go.GetComponent<CharacterPanelView>();
            go.transform.SetParent(canvasParent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            view._dir = dir;
            view._state = state;
            view.Build();
            return view;
        }

        private void Build()
        {
            var bg = UiBuilder.CreateImage(transform, "Overlay", new Color(0.06f, 0.07f, 0.10f, 0.97f));
            UiBuilder.Stretch(UiBuilder.Rect(bg));

            var header = UiBuilder.CreateText(transform, "Header", "Karakterler", UiTheme.FontTitle, UiTheme.TextLight);
            UiBuilder.Anchor(UiBuilder.Rect(header), new Vector2(0, 0.92f), new Vector2(1, 1f),
                new Vector2(40, 0), new Vector2(-40, 0));

            UiBuilder.CreateButton(transform, "Close", "Kapat", UiTheme.FontSmall, UiTheme.Negative, UiTheme.TextLight,
                Close);
            var close = transform.Find("Close");
            UiBuilder.Anchor((RectTransform)close, new Vector2(0.7f, 0.92f), new Vector2(0.97f, 0.99f),
                Vector2.zero, Vector2.zero);

            _content = UiBuilder.CreateScrollView(transform);
            var scrollRect = (RectTransform)_content.parent;
            UiBuilder.Anchor(scrollRect, new Vector2(0, 0.02f), new Vector2(1, 0.90f), Vector2.zero, Vector2.zero);

            ShowList();
        }

        private void ClearContent()
        {
            for (int i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);
        }

        private void ShowList()
        {
            ClearContent();
            foreach (var c in _dir.All)
            {
                var character = c;
                int rel = _state.GetRelationship(c.Id);
                string status = RelationshipStatus.Of(rel, c.RelationshipTierNames);
                var row = MakeRow($"{c.Name}\n<size=70%>{c.Role} — {status}</size>", c.ThemeColor,
                    CharacterAvatar.Initials(c.Name), () => ShowDetail(character));
            }
        }

        private void ShowDetail(Character c)
        {
            ClearContent();
            int rel = _state.GetRelationship(c.Id);
            string status = RelationshipStatus.Of(rel, c.RelationshipTierNames);

            MakeRow("◀ Listeye dön", new ColorRGB(0.3f, 0.35f, 0.45f), "<", ShowList);

            MakeRow($"{c.Name}\n<size=70%>{c.Role}</size>", c.ThemeColor, CharacterAvatar.Initials(c.Name), null);
            MakeLabel($"Durum: {status}");
            MakeLabel(c.Description);

            string outcome = rel <= -20 ? c.LowOutcome : (rel >= 20 ? c.HighOutcome : "İlişki şu an tarafsız bir noktada.");
            if (!string.IsNullOrEmpty(outcome)) MakeLabel(outcome);

            MakeLabel("<b>İlişki Geçmişi</b>");
            var history = new List<RelationshipChange>();
            foreach (var h in _state.RelationshipHistory)
                if (h.CharacterId == c.Id) history.Add(h);

            if (history.Count == 0) MakeLabel("Henüz bir değişim yok.");
            else
                for (int i = history.Count - 1; i >= 0 && i >= history.Count - 12; i--)
                {
                    var h = history[i];
                    string sign = h.Delta > 0 ? "+" : "";
                    MakeLabel($"Tur {h.Turn}: {sign}{h.Delta} → {RelationshipStatus.Of(h.ResultingValue, c.RelationshipTierNames)}");
                }
        }

        private TextMeshProUGUI MakeLabel(string text)
        {
            var t = UiBuilder.CreateText(_content, "Label", text, UiTheme.FontSmall, UiTheme.TextLight,
                TextAlignmentOptions.TopLeft);
            t.GetComponent<TextMeshProUGUI>().richText = true;
            AddLayout(t.gameObject, 60);
            return t;
        }

        private Button MakeRow(string text, ColorRGB color, string initials, System.Action onClick)
        {
            var go = new GameObject("Row", typeof(Image), typeof(Button));
            go.transform.SetParent(_content, false);
            go.GetComponent<Image>().color = UiTheme.Panel;
            var btn = go.GetComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            AddLayout(go, 150);

            var avatar = CharacterAvatar.Build(go.transform, out var initialsLabel);
            avatar.color = CharacterAvatar.ToColor(color);
            initialsLabel.text = initials;
            UiBuilder.Anchor(UiBuilder.Rect(avatar), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(20, -55), new Vector2(130, 55));

            var label = UiBuilder.CreateText(go.transform, "Text", text, UiTheme.FontSmall, UiTheme.TextLight,
                TextAlignmentOptions.Left);
            label.richText = true;
            UiBuilder.Anchor(UiBuilder.Rect(label), new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(150, 0), new Vector2(-20, 0));
            return btn;
        }

        private static void AddLayout(GameObject go, float minHeight)
        {
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.preferredHeight = minHeight;
        }

        public void Close() => Destroy(gameObject);
    }
}
