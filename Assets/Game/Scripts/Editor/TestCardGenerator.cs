using System.Collections.Generic;
using System.IO;
using DengeGame.Application.Validation;
using DengeGame.Domain;
using DengeGame.Infrastructure.Data;
using UnityEditor;
using UnityEngine;

namespace DengeGame.Editor
{
    /// <summary>
    /// Tüm koşul türlerini gösteren 16 test kartı (≥15) ve bir EventCardDatabase üretir.
    /// Bunlar final içerik değildir (Faz 11); şema ve seçim algoritmasını sınamak içindir.
    /// Menüden veya batchmode -executeMethod ile çalışır.
    /// </summary>
    public static class TestCardGenerator
    {
        private const string CardsDir = "Assets/Game/Data/Cards";
        private const string ResourcesDir = "Assets/Game/Resources";

        [MenuItem("Denge/İçerik/Test Kartları Üret")]
        public static void GenerateTestCards()
        {
            EnsureFolder(CardsDir);
            EnsureFolder(ResourcesDir);

            var assets = new List<EventCardAsset>();
            foreach (var build in Builders())
            {
                var asset = ScriptableObject.CreateInstance<EventCardAsset>();
                build(asset.Source);
                string path = $"{CardsDir}/Card_{asset.Source.Id}.asset";
                AssetDatabase.CreateAsset(asset, path);
                assets.Add(asset);
            }

            var db = ScriptableObject.CreateInstance<EventCardDatabase>();
            db.SetCards(assets);
            AssetDatabase.CreateAsset(db, $"{ResourcesDir}/{EventCardDatabase.ResourcesPath}.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Üretilen içeriği hemen doğrula.
            var pool = db.BuildPool();
            var issues = CardValidator.Validate(pool);
            int errors = issues.FindAll(i => i.Severity == IssueSeverity.Error).Count;
            Debug.Log($"[Denge] {assets.Count} test kartı + veritabanı üretildi. " +
                      $"Doğrulama: {errors} hata, {issues.Count - errors} uyarı.");
            foreach (var i in issues)
                if (i.Severity == IssueSeverity.Error) Debug.LogError($"[Denge] {i}");
        }

        private delegate void Build(EventCard c);

        private static IEnumerable<Build> Builders()
        {
            // 1) Koşulsuz ekonomi
            yield return c => Base(c, "eco_basit", "Bütçe Görüşmesi", "maliye",
                "Bakanlık yeni bütçeyi sunuyor. Önceliği siz belirleyin.",
                "Yatırıma ağırlık ver", new[] { EffectData.ChangeValue(CountryValue.Economy, 6), EffectData.ChangeValue(CountryValue.Environment, -3) },
                "Tasarrufa yönel", new[] { EffectData.ChangeValue(CountryValue.Economy, -2), EffectData.ChangeValue(CountryValue.PublicSupport, 3) });

            // 2) Koşulsuz halk
            yield return c => { Base(c, "halk_basit", "Meydanda Talep", "meclis",
                "Halk daha fazla sosyal destek istiyor.",
                "Destekleri artır", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 5), EffectData.ChangeValue(CountryValue.Economy, -4) },
                "Mevcut düzeni koru", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, -3) });
                c.Category = "Halk"; };

            // 3) Değer aralığı koşulu (Güvenlik düşükken)
            yield return c => { Base(c, "guvenlik_dusuk", "Asayiş Boşluğu", "genelkurmay",
                "Güvenlik zayıfladı, kararsızlık artıyor.",
                "Devriyeyi artır", new[] { EffectData.ChangeValue(CountryValue.Security, 7), EffectData.ChangeValue(CountryValue.Freedom, -4) },
                "Diyaloğu seç", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 3) });
                c.ValueConditions.Add(new ValueCondition(CountryValue.Security, 0, 40)); };

            // 4) Değer aralığı koşulu (Özgürlük yüksekken)
            yield return c => { Base(c, "ozgurluk_yuksek", "Basın Özgürlüğü", "gazeteci",
                "Geniş özgürlük ortamında düzenleme tartışılıyor.",
                "Serbest bırak", new[] { EffectData.ChangeValue(CountryValue.Freedom, 3), EffectData.ChangeValue(CountryValue.Security, -3) },
                "Hafif düzenle", new[] { EffectData.ChangeValue(CountryValue.Freedom, -4) });
                c.ValueConditions.Add(new ValueCondition(CountryValue.Freedom, 60, 100)); };

            // 5) Gerekli bayrak
            yield return c => { Base(c, "flag_gerekli", "Seferberlik Kararı", "genelkurmay",
                "Savaş hali ilan edildi; ek yetkiler isteniyor.",
                "Yetki ver", new[] { EffectData.ChangeValue(CountryValue.Security, 6), EffectData.ChangeValue(CountryValue.Freedom, -6) },
                "Reddet", new[] { EffectData.ChangeValue(CountryValue.Diplomacy, 2) });
                c.RequiredFlags.Add("savas_hali"); };

            // 6) Yasak bayrak
            yield return c => { Base(c, "flag_yasak", "Barış Şenliği", "meclis",
                "Toplum kutlama düzenlemek istiyor.",
                "Destekle", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 4) },
                "Sade tut", new[] { EffectData.ChangeValue(CountryValue.Economy, 1) });
                c.ForbiddenFlags.Add("savas_hali"); };

            // 7) Gerekli politika
            yield return c => { Base(c, "politika_gerekli", "Gelir Denetimi", "maliye",
                "Temel gelir uygulaması denetleniyor.",
                "Genişlet", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 5), EffectData.ChangeValue(CountryValue.Economy, -5) },
                "Sınırla", new[] { EffectData.ChangeValue(CountryValue.Economy, 3), EffectData.ChangeValue(CountryValue.PublicSupport, -2) });
                c.RequiredPolicies.Add("ubi"); };

            // 8) Yasak politika
            yield return c => { Base(c, "politika_yasak", "Açık Sınır Çağrısı", "disisleri",
                "Sınırların yumuşatılması öneriliyor.",
                "Aç", new[] { EffectData.ChangeValue(CountryValue.Diplomacy, 5), EffectData.ChangeValue(CountryValue.Security, -4) },
                "Beklet", new[] { EffectData.ChangeValue(CountryValue.Security, 2) });
                c.ForbiddenPolicies.Add("siki_sinir"); };

            // 9) İlişki koşulu (düşük)
            yield return c => { Base(c, "iliski_dusuk_general", "Orduda Hoşnutsuzluk", "genelkurmay",
                "Komuta kademesiyle ilişkiler gergin.",
                "Taviz ver", new[] { EffectData.ChangeRelationship("general", 20), EffectData.ChangeValue(CountryValue.Freedom, -3) },
                "Sert dur", new[] { EffectData.ChangeRelationship("general", -10), EffectData.ChangeValue(CountryValue.Security, -3) });
                c.RelationshipConditions.Add(new RelationshipCondition("general", -100, -40)); };

            // 10) Tur koşulu
            yield return c => { Base(c, "tur_gec", "Olgun Dönem Reformu", "meclis",
                "Yıllar sonra köklü bir reform gündemde.",
                "Uygula", new[] { EffectData.ChangeValue(CountryValue.Economy, 8), EffectData.ChangeValue(CountryValue.PublicSupport, -4) },
                "Ertele", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 2) });
                c.MinTurn = 10; };

            // 11) Tek kullanımlık
            yield return c => { Base(c, "tek_kullanim", "Anıtın Açılışı", "meclis",
                "Yalnızca bir kez yaşanacak simgesel bir an.",
                "Görkemli yap", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 6), EffectData.ChangeValue(CountryValue.Economy, -4) },
                "Sade yap", new[] { EffectData.ChangeValue(CountryValue.Economy, 2) });
                c.OneShot = true; };

            // 12) Cooldown
            yield return c => { Base(c, "cooldown_kart", "Hava Durumu Raporu", "bilimci",
                "Periyodik bir çevre değerlendirmesi geldi.",
                "Önlem al", new[] { EffectData.ChangeValue(CountryValue.Environment, 4), EffectData.ChangeValue(CountryValue.Economy, -2) },
                "Göz ardı et", new[] { EffectData.ChangeValue(CountryValue.Environment, -3) });
                c.CooldownTurns = 5; c.Category = "Çevre"; };

            // 13) Acil / kriz kartı
            yield return c => { Base(c, "acil_kriz", "Acil: Kuraklık", "bilimci",
                "Su kaynakları hızla azalıyor, ivedi karar gerek.",
                "Kısıtlama getir", new[] { EffectData.ChangeValue(CountryValue.Environment, 6), EffectData.ChangeValue(CountryValue.Economy, -5) },
                "Üretimi koru", new[] { EffectData.ChangeValue(CountryValue.Economy, 3), EffectData.ChangeValue(CountryValue.Environment, -6) });
                c.IsEmergency = true; c.Priority = 10; };

            // 14) Zincir başlangıcı (devamı zamanlar)
            yield return c => { Base(c, "zincir_baslangic", "Enerji Santrali Önerisi", "sanayi",
                "Büyük bir enerji yatırımı masada.",
                "Onayla", new[] { EffectData.ChangeValue(CountryValue.Economy, 6), EffectData.ScheduleEvent("zincir_devam", 2) },
                "Reddet", new[] { EffectData.ChangeValue(CountryValue.Environment, 3), EffectData.ChangeValue(CountryValue.Economy, -3) }); };

            // 15) Zincir devamı (yalnızca zamanlanarak gelir)
            yield return c => { Base(c, "zincir_devam", "Santral Protestoları", "gazeteci",
                "Santral kararı sokakta tepkiyle karşılandı.",
                "Sertçe bastır", new[] { EffectData.ChangeValue(CountryValue.Security, 4), EffectData.ChangeValue(CountryValue.Freedom, -5) },
                "Diyalog kur", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 4), EffectData.ChangeValue(CountryValue.Economy, -2) });
                c.PreviousEventId = "zincir_baslangic"; };

            // 16) Fallback (güvenli kart)
            yield return c => { Base(c, "fallback_sessiz", "Sıradan Bir Gün", "meclis",
                "Bugün gündemde öne çıkan bir konu yok.",
                "Rutini sürdür", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 1) },
                "Dinlen", new[] { EffectData.ChangeValue(CountryValue.PublicSupport, 1) });
                c.IsFallback = true; };
        }

        private static void Base(EventCard c, string id, string title, string characterId,
            string description, string leftText, EffectData[] leftEffects,
            string rightText, EffectData[] rightEffects)
        {
            c.Id = id;
            c.Title = title;
            c.CharacterId = characterId;
            c.Description = description;
            c.LeftText = leftText;
            c.RightText = rightText;
            c.LeftEffects = new List<EffectData>(leftEffects);
            c.RightEffects = new List<EffectData>(rightEffects);
            c.MaxTurn = int.MaxValue;
            c.Weight = 1f;
            if (string.IsNullOrEmpty(c.Category)) c.Category = "Genel";
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            Directory.CreateDirectory(assetPath);
            AssetDatabase.Refresh();
        }
    }
}
