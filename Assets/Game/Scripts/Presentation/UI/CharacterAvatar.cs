using DengeGame.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DengeGame.Presentation.UI
{
    /// <summary>
    /// Görsel asset olmadan karakter temsili: tema renginde sade bir kutu + baş harf(ler).
    /// Unicode sembol kullanılmaz; yalnızca isim baş harfleri ve renk.
    /// </summary>
    public static class CharacterAvatar
    {
        public static Color ToColor(ColorRGB c) => new Color(c.R, c.G, c.B, 1f);

        public static string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ');
            string s = "";
            for (int i = 0; i < parts.Length && s.Length < 2; i++)
                if (parts[i].Length > 0) s += char.ToUpperInvariant(parts[i][0]);
            return s.Length == 0 ? "?" : s;
        }

        /// <summary>Verilen parent içinde bir avatar (renk kutusu + baş harfler) kurar ve baş harf metnini döndürür.</summary>
        public static Image Build(Transform parent, out TextMeshProUGUI initialsLabel)
        {
            var bg = UiBuilder.CreateImage(parent, "Avatar", UiTheme.Accent);
            initialsLabel = UiBuilder.CreateText(bg.transform, "Initials", "?", UiTheme.FontBody, Color.white);
            UiBuilder.Stretch((RectTransform)initialsLabel.transform);
            return bg;
        }
    }
}
