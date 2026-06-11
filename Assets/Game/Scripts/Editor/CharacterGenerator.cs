using System.Collections.Generic;
using System.IO;
using DengeGame.Domain;
using DengeGame.Infrastructure.Data;
using UnityEditor;
using UnityEngine;

namespace DengeGame.Editor
{
    /// <summary>
    /// 8 başlangıç karakteri/kurumu ve bir CharacterDatabase üretir. Görsel asset yok; her karakter
    /// baş harf avatarı + tema rengi + rol ile temsil edilir. Menüden veya batchmode ile çalışır.
    /// </summary>
    public static class CharacterGenerator
    {
        private const string Dir = "Assets/Game/Data/Characters";
        private const string ResourcesDir = "Assets/Game/Resources";

        [MenuItem("Denge/İçerik/Karakterleri Üret")]
        public static void GenerateCharacters()
        {
            EnsureFolder(Dir);
            EnsureFolder(ResourcesDir);

            var assets = new List<CharacterAsset>();
            foreach (var build in Builders())
            {
                var asset = ScriptableObject.CreateInstance<CharacterAsset>();
                build(asset.Source);
                AssetDatabase.CreateAsset(asset, $"{Dir}/Character_{asset.Source.Id}.asset");
                assets.Add(asset);
            }

            var db = ScriptableObject.CreateInstance<CharacterDatabase>();
            db.SetCharacters(assets);
            AssetDatabase.CreateAsset(db, $"{ResourcesDir}/{CharacterDatabase.ResourcesPath}.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Denge] {assets.Count} karakter + veritabanı üretildi.");
        }

        private delegate void Build(Character c);

        private static IEnumerable<Build> Builders()
        {
            yield return c => Base(c, "maliye", "Elif Demir", "Maliye Bakanı",
                "Bütçe ve ekonomik istikrardan sorumlu. Sayılarla konuşur.",
                new ColorRGB(0.20f, 0.55f, 0.45f),
                "Ekonomi kararlarınıza güveni kalmadı; reformları yavaşlatıyor.",
                "Size tam destek veriyor; cesur ekonomik adımları kolaylaştırıyor.");

            yield return c => {
                Base(c, "genelkurmay", "Kemal Aslan", "Genelkurmay Başkanı",
                    "Güvenlik ve ordu meselelerinde belirleyici. Sadakat bekler.",
                    new ColorRGB(0.45f, 0.40f, 0.30f),
                    "Komuta kademesi yönetiminize sırt çevirdi; tehlikeli bir gerilim var.",
                    "Ordu arkanızda; güvenlik kararlarınız sorgusuz uygulanıyor.");
                c.EnableLowEnding = true; c.LowEndingThreshold = -80; c.LowEndingReason = "Askeri Darbe";
            };

            yield return c => Base(c, "muhalefet", "Selin Kaya", "Muhalefet Lideri",
                "Yönetimi denetler, halkın hoşnutsuzluğunu meclise taşır.",
                new ColorRGB(0.70f, 0.35f, 0.30f),
                "Sert muhalefet; her kararınızı kamuoyunda zorluyor.",
                "Beklenmedik bir uzlaşma; reformlarınıza zemin hazırlıyor.");

            yield return c => Base(c, "bilimci", "Deniz Yıldız", "Çevre Bilimci",
                "İklim ve doğal kaynaklar konusunda bağımsız uzman.",
                new ColorRGB(0.30f, 0.60f, 0.35f),
                "Uyarıları görmezden gelindi; kamuoyunda alarm veriyor.",
                "Bilimsel danışmanlık sağlıyor; çevre politikaları güçleniyor.");

            yield return c => Base(c, "sanayi", "Burak Şahin", "Sanayi Birliği Başkanı",
                "Üretim ve istihdamın sesi. Teşvik ve serbestlik ister.",
                new ColorRGB(0.55f, 0.50f, 0.25f),
                "Yatırımcılar tedirgin; sermaye ülkeyi terk etme sinyali veriyor.",
                "Sanayi seferber oldu; büyüme ve istihdam hızlanıyor.");

            yield return c => Base(c, "gazeteci", "Aylin Toprak", "Gazeteciler Birliği Temsilcisi",
                "Basın özgürlüğünün ve şeffaflığın savunucusu.",
                new ColorRGB(0.35f, 0.45f, 0.70f),
                "Basın karşı cephe aldı; her adımınız manşetlerde sorgulanıyor.",
                "Bağımsız basın güven veriyor; mesajınız halka net ulaşıyor.");

            yield return c => Base(c, "disisleri", "Cem Arı", "Dışişleri Danışmanı",
                "Dış ilişkiler ve ittifaklar konusunda stratejist.",
                new ColorRGB(0.40f, 0.40f, 0.65f),
                "Diplomatik kanallar tıkandı; uluslararası yalnızlık riski büyüyor.",
                "Güçlü diplomasi; ittifaklar ve dış destek kapıları açılıyor.");

            yield return c => Base(c, "meclis", "Naz Eren", "Halk Meclisi Sözcüsü",
                "Halkın temsilcisi; meclisin nabzını yönetime aktarır.",
                new ColorRGB(0.50f, 0.35f, 0.55f),
                "Meclis küstü; yasalarınız tıkanıyor.",
                "Meclis desteği tam; reform paketleri hızla geçiyor.");
        }

        private static void Base(Character c, string id, string name, string role, string desc,
            ColorRGB color, string low, string high)
        {
            c.Id = id;
            c.Name = name;
            c.Role = role;
            c.Description = desc;
            c.ThemeColor = color;
            c.LowOutcome = low;
            c.HighOutcome = high;
            c.RelationshipTierNames = null; // varsayılan kademe adları
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            Directory.CreateDirectory(assetPath);
            AssetDatabase.Refresh();
        }
    }
}
