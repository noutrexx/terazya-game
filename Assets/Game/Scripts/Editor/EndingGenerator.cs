using System.Collections.Generic;
using System.IO;
using DengeGame.Domain;
using DengeGame.Infrastructure.Data;
using UnityEditor;
using UnityEngine;
using Cat = DengeGame.Domain.EndingCategory;
using Rar = DengeGame.Domain.EndingRarity;

namespace DengeGame.Editor
{
    /// <summary>16 yönetim sonu tanımı + veritabanı üretir. Id'ler değerlendiricilerin döndürdüğü
    /// "reason" anahtarlarıyla birebir eşleşir.</summary>
    public static class EndingGenerator
    {
        private const string Dir = "Assets/Game/Data/Endings";
        private const string ResourcesDir = "Assets/Game/Resources";

        private static readonly ColorRGB Red = new ColorRGB(0.70f, 0.25f, 0.22f);
        private static readonly ColorRGB Green = new ColorRGB(0.28f, 0.58f, 0.36f);
        private static readonly ColorRGB Gray = new ColorRGB(0.42f, 0.44f, 0.50f);
        private static readonly ColorRGB Purple = new ColorRGB(0.45f, 0.34f, 0.58f);

        [MenuItem("Denge/İçerik/Sonları Üret")]
        public static void Generate()
        {
            EnsureFolder(Dir); EnsureFolder(ResourcesDir);
            var assets = new List<EndingAsset>();
            foreach (var e in Defs())
            {
                var a = ScriptableObject.CreateInstance<EndingAsset>();
                Copy(a.Source, e);
                AssetDatabase.CreateAsset(a, $"{Dir}/Ending_{Safe(e.Id)}.asset");
                assets.Add(a);
            }
            var db = ScriptableObject.CreateInstance<EndingDatabase>();
            db.SetEndings(assets);
            AssetDatabase.CreateAsset(db, $"{ResourcesDir}/{EndingDatabase.ResourcesPath}.asset");
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log($"[Denge] {assets.Count} yönetim sonu üretildi.");
        }

        private static IEnumerable<EndingDefinition> Defs()
        {
            // 12 sınır sonu
            yield return E("Devlet İflası", "Hazine boşaldı, devlet ödemelerini durdurdu.", Cat.Disaster, Rar.Common, Red, 10);
            yield return E("Şirketokrasinin Yükselişi", "Sermaye devleti ele geçirdi; karar artık şirketlerin.", Cat.Ironic, Rar.Rare, Purple, 30);
            yield return E("Halk Ayaklanması", "Sokaklar taştı, yönetim devrildi.", Cat.Disaster, Rar.Common, Red, 10);
            yield return E("Popülizm Çöküşü", "Sürdürülemez vaatler ülkeyi tüketti.", Cat.Ironic, Rar.Uncommon, Purple, 20);
            yield return E("Kontrolün Kaybı", "Güvenlik çöktü, ülke anarşiye sürüklendi.", Cat.Disaster, Rar.Common, Red, 10);
            yield return E("Baskıcı Güvenlik Rejimi", "Düzen sağlandı ama özgürlükler yok oldu.", Cat.Disaster, Rar.Uncommon, Red, 20);
            yield return E("Otoriter Rejim", "Tüm yetki tek elde toplandı.", Cat.Disaster, Rar.Common, Red, 10);
            yield return E("Otorite Boşluğu", "Devlet otoritesi işlevsizleşti.", Cat.Ironic, Rar.Uncommon, Purple, 20);
            yield return E("Ekolojik Çöküş", "Doğa geri dönülmez biçimde tahrip oldu.", Cat.Disaster, Rar.Common, Red, 10);
            yield return E("Yeşil Durgunluk", "Aşırı kısıtlar ekonomiyi felç etti.", Cat.Ironic, Rar.Uncommon, Purple, 20);
            yield return E("Uluslararası İzolasyon", "Ülke dünyadan koptu, yaptırımlar altında.", Cat.Disaster, Rar.Common, Red, 10);
            yield return E("Egemenlik Kaybı", "Dış güçlere bağımlılık egemenliği bitirdi.", Cat.Disaster, Rar.Uncommon, Red, 20);

            // 4 özel son
            yield return E("Askeri Darbe", "Ordu yönetime el koydu.", Cat.Disaster, Rar.Uncommon, Red, 25);
            yield return E("Onurlu Emeklilik", "Görev sürenizi tamamlayıp saygıyla çekildiniz.", Cat.Success, Rar.Rare, Green, 50);
            yield return E("Ekonomik Çöküntü", "Çözülemeyen durgunluk ekonomiyi dibe çekti.", Cat.Disaster, Rar.Common, Red, 15);
            yield return E("Susuzluk Felaketi", "Kuraklık yönetilemedi, felaket kaçınılmaz oldu.", Cat.Disaster, Rar.Uncommon, Red, 20);
            yield return E("Sınır Savaşı", "Gerilim tırmandı, sınırda çatışma başladı.", Cat.Disaster, Rar.Uncommon, Red, 20);
        }

        private static EndingDefinition E(string id, string desc, Cat cat, Rar rar, ColorRGB color, int meta) =>
            new EndingDefinition(id, id) { Description = desc, Category = cat, Rarity = rar, ThemeColor = color, MetaRewardPoints = meta };

        private static void Copy(EndingDefinition dst, EndingDefinition s)
        {
            dst.Id = s.Id; dst.Title = s.Title; dst.Description = s.Description;
            dst.Category = s.Category; dst.Rarity = s.Rarity; dst.ThemeColor = s.ThemeColor;
            dst.MetaRewardPoints = s.MetaRewardPoints;
        }

        private static string Safe(string s) => s.Replace(" ", "_").Replace("İ", "I").Replace("ş", "s")
            .Replace("ç", "c").Replace("ü", "u").Replace("ö", "o").Replace("ğ", "g").Replace("ı", "i");

        private static void EnsureFolder(string p)
        {
            if (AssetDatabase.IsValidFolder(p)) return;
            Directory.CreateDirectory(p); AssetDatabase.Refresh();
        }
    }
}
