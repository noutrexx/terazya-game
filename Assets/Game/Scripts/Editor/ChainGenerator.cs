using System.Collections.Generic;
using System.IO;
using DengeGame.Application.Validation;
using DengeGame.Domain;
using DengeGame.Infrastructure.Data;
using UnityEditor;
using UnityEngine;
using E = DengeGame.Domain.EffectData;
using V = DengeGame.Domain.CountryValue;

namespace DengeGame.Editor
{
    /// <summary>
    /// 6 olay zinciri (her biri ≥3 kart) üretir ve EventCardDatabase'e ekler. Zincir devam kartları
    /// PreviousEventId ile işaretlenir ve önceki kartın ScheduleEvent etkisiyle zamanlanır.
    /// </summary>
    public static class ChainGenerator
    {
        private const string ChainDir = "Assets/Game/Data/Cards/Chains";
        private const string ResourcesDir = "Assets/Game/Resources";

        [MenuItem("Denge/İçerik/Olay Zincirlerini Üret")]
        public static void GenerateChains()
        {
            EnsureFolder(ChainDir);

            var created = new List<EventCardAsset>();
            BuildChains(created);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // EventCardDatabase'i tüm kartlarla (mevcut + zincir) yeniden kur.
            var all = new List<EventCardAsset>();
            foreach (var guid in AssetDatabase.FindAssets("t:EventCardAsset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<EventCardAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) all.Add(asset);
            }

            var db = AssetDatabase.LoadAssetAtPath<EventCardDatabase>(
                $"{ResourcesDir}/{EventCardDatabase.ResourcesPath}.asset");
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<EventCardDatabase>();
                AssetDatabase.CreateAsset(db, $"{ResourcesDir}/{EventCardDatabase.ResourcesPath}.asset");
            }
            db.SetCards(all);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var issues = CardValidator.Validate(db.BuildPool());
            int errors = issues.FindAll(i => i.Severity == IssueSeverity.Error).Count;
            Debug.Log($"[Denge] {created.Count} zincir kartı eklendi. Havuz {all.Count} kart. " +
                      $"Doğrulama: {errors} hata, {issues.Count - errors} uyarı.");
            foreach (var i in issues)
                if (i.Severity == IssueSeverity.Error) Debug.LogError($"[Denge] {i}");
        }

        private static void BuildChains(List<EventCardAsset> created)
        {
            // 1) Enerji santrali
            created.Add(Card("zin_enerji_1", "sanayi", "Enerji Santrali Önerisi",
                "Büyük bir enerji yatırımı masada.", null,
                "Onayla", new[] { E.ChangeValue(V.Economy, 6), E.ScheduleEvent("zin_enerji_2", 2) },
                "Reddet", new[] { E.ChangeValue(V.Environment, 3), E.ChangeValue(V.Economy, -3) }));
            created.Add(Card("zin_enerji_2", "gazeteci", "Çevre Protestoları",
                "Santral kararı sokakta tepki topluyor.", "zin_enerji_1",
                "Sertçe bastır", new[] { E.ChangeValue(V.Security, 4), E.ChangeValue(V.Freedom, -5), E.ScheduleEvent("zin_enerji_3", 2) },
                "Diyalog kur", new[] { E.ChangeValue(V.PublicSupport, 4), E.ChangeValue(V.Economy, -2), E.ScheduleEvent("zin_enerji_3", 2) }));
            created.Add(Card("zin_enerji_3", "bilimci", "Santralin Uzun Vadesi",
                "Yıllar sonra sonuçlar belirginleşiyor.", "zin_enerji_2",
                "Üretime öncelik", new[] { E.ChangeValue(V.Economy, 5), E.ChangeValue(V.Environment, -4) },
                "Çevreyi koru", new[] { E.ChangeValue(V.Environment, 5), E.ChangeValue(V.Economy, -3) }));

            // 2) Yolsuzluk soruşturması
            created.Add(Card("zin_yolsuzluk_1", "gazeteci", "Yolsuzluk İddiası",
                "Bir bakanlıkta usulsüzlük iddiası.", null,
                "Soruşturma aç", new[] { E.ChangeValue(V.PublicSupport, 4), E.ScheduleEvent("zin_yolsuzluk_2", 2) },
                "Örtbas et", new[] { E.ChangeValue(V.Security, 2), E.SetFlag("ortbas", true) }));
            created.Add(Card("zin_yolsuzluk_2", "muhalefet", "Soruşturma Derinleşiyor",
                "İzler üst kademeye uzanıyor.", "zin_yolsuzluk_1",
                "Sonuna kadar git", new[] { E.ChangeValue(V.PublicSupport, 5), E.ChangeValue(V.Economy, -3), E.ScheduleEvent("zin_yolsuzluk_3", 2) },
                "Sınırla", new[] { E.ChangeValue(V.PublicSupport, -2), E.ScheduleEvent("zin_yolsuzluk_3", 2) }));
            created.Add(Card("zin_yolsuzluk_3", "meclis", "Hesap Verme",
                "Kamuoyu sonucu bekliyor.", "zin_yolsuzluk_2",
                "Şeffaflık ilan et", new[] { E.ChangeValue(V.Freedom, 4), E.ChangeValue(V.PublicSupport, 3) },
                "Sayfayı kapat", new[] { E.ChangeValue(V.PublicSupport, -4) }));

            // 3) Salgın
            created.Add(Card("zin_salgin_1", "bilimci", "Yeni Salgın Uyarısı",
                "Sağlık otoritesi alarma geçti.", null,
                "Tedbir al", new[] { E.ChangeValue(V.Economy, -4), E.ChangeValue(V.PublicSupport, 3), E.ScheduleEvent("zin_salgin_2", 2) },
                "Bekle ve gör", new[] { E.ChangeValue(V.Economy, 2), E.ScheduleEvent("zin_salgin_2", 2) }));
            created.Add(Card("zin_salgin_2", "bilimci", "Vakalar Artıyor",
                "Salgın yayılıyor.", "zin_salgin_1",
                "Kısıtlama getir", new[] { E.ChangeValue(V.Freedom, -4), E.ChangeValue(V.Security, 2), E.ScheduleEvent("zin_salgin_3", 2) },
                "Ekonomiyi koru", new[] { E.ChangeValue(V.Economy, 3), E.ChangeValue(V.PublicSupport, -4), E.ScheduleEvent("zin_salgin_3", 2) }));
            created.Add(Card("zin_salgin_3", "meclis", "Salgının Sonu",
                "Toplum normale dönüyor.", "zin_salgin_2",
                "Sağlığa yatırım", new[] { E.ChangeValue(V.PublicSupport, 4), E.ChangeValue(V.Economy, -2) },
                "Hızlı açılış", new[] { E.ChangeValue(V.Economy, 4), E.ChangeValue(V.PublicSupport, -2) }));

            // 4) Genel grev
            created.Add(Card("zin_grev_1", "meclis", "Grev Tehdidi",
                "Sendikalar genel grev planlıyor.", null,
                "Masaya otur", new[] { E.ChangeValue(V.Economy, -3), E.ChangeValue(V.PublicSupport, 3), E.ScheduleEvent("zin_grev_2", 2) },
                "Geri adım atma", new[] { E.ChangeValue(V.Security, 2), E.ChangeValue(V.PublicSupport, -3) }));
            created.Add(Card("zin_grev_2", "sanayi", "Üretim Durdu",
                "Grev ekonomiyi vuruyor.", "zin_grev_1",
                "Zam ver", new[] { E.ChangeValue(V.Economy, -4), E.ChangeValue(V.PublicSupport, 5), E.ScheduleEvent("zin_grev_3", 2) },
                "Diren", new[] { E.ChangeValue(V.Security, -3), E.ScheduleEvent("zin_grev_3", 2) }));
            created.Add(Card("zin_grev_3", "meclis", "Grevin Sonucu",
                "Taraflar bir noktaya geldi.", "zin_grev_2",
                "Uzlaşıyı kalıcı kıl", new[] { E.ChangeValue(V.PublicSupport, 4), E.ChangeValue(V.Economy, -2) },
                "Eski düzene dön", new[] { E.ChangeValue(V.Economy, 3), E.ChangeValue(V.PublicSupport, -3) }));

            // 5) Casusluk
            created.Add(Card("zin_casus_1", "disisleri", "Casusluk Şüphesi",
                "Bir diplomat hakkında istihbarat var.", null,
                "Gizli soruştur", new[] { E.ChangeValue(V.Security, 3), E.ScheduleEvent("zin_casus_2", 2) },
                "Görmezden gel", new[] { E.ChangeValue(V.Diplomacy, 2) }));
            created.Add(Card("zin_casus_2", "genelkurmay", "Belgeler Sızdı",
                "Gizli belgeler ele geçti.", "zin_casus_1",
                "Kamuoyuna aç", new[] { E.ChangeValue(V.Diplomacy, -5), E.ChangeValue(V.PublicSupport, 3), E.ScheduleEvent("zin_casus_3", 2) },
                "Sessizce çöz", new[] { E.ChangeValue(V.Security, 3), E.ChangeValue(V.Freedom, -2), E.ScheduleEvent("zin_casus_3", 2) }));
            created.Add(Card("zin_casus_3", "disisleri", "Diplomatik Hesaplaşma",
                "Kriz uluslararası boyut kazandı.", "zin_casus_2",
                "Sert dur", new[] { E.ChangeValue(V.Security, 4), E.ChangeValue(V.Diplomacy, -4) },
                "Köprüleri onar", new[] { E.ChangeValue(V.Diplomacy, 5), E.ChangeValue(V.Security, -2) }));

            // 6) Baraj projesi (miras)
            created.Add(Card("zin_baraj_1", "sanayi", "Baraj Projesi",
                "Bölgeyi dönüştürecek bir baraj önerisi.", null,
                "Projeyi başlat", new[] { E.ChangeValue(V.Economy, 5), E.ChangeValue(V.Environment, -3), E.ScheduleEvent("zin_baraj_2", 2) },
                "Vazgeç", new[] { E.ChangeValue(V.Environment, 3), E.ChangeValue(V.Economy, -2) }));
            created.Add(Card("zin_baraj_2", "bilimci", "Yerel Direniş",
                "Köyler taşınmaya karşı çıkıyor.", "zin_baraj_1",
                "Tazminatı artır", new[] { E.ChangeValue(V.Economy, -4), E.ChangeValue(V.PublicSupport, 4), E.ScheduleEvent("zin_baraj_3", 2) },
                "Zorla devam et", new[] { E.ChangeValue(V.Security, -3), E.ChangeValue(V.Freedom, -3), E.ScheduleEvent("zin_baraj_3", 2) }));
            created.Add(Card("zin_baraj_3", "meclis", "Barajın Mirası",
                "Proje tamamlandı; etkileri kalıcı.", "zin_baraj_2",
                "Sanayiye bağla", new[] { E.ChangeValue(V.Economy, 6), E.ChangeValue(V.Environment, -4) },
                "Doğayla dengele", new[] { E.ChangeValue(V.Environment, 5), E.ChangeValue(V.Economy, -2) }));
        }

        private static EventCardAsset Card(string id, string charId, string title, string desc, string prev,
            string lText, E[] lEff, string rText, E[] rEff)
        {
            var a = ScriptableObject.CreateInstance<EventCardAsset>();
            var c = a.Source;
            c.Id = id; c.CharacterId = charId; c.Title = title; c.Description = desc; c.PreviousEventId = prev;
            c.LeftText = lText; c.LeftEffects = new List<E>(lEff);
            c.RightText = rText; c.RightEffects = new List<E>(rEff);
            c.Category = "Zincir"; c.MaxTurn = int.MaxValue; c.Weight = 1f;
            AssetDatabase.CreateAsset(a, $"{ChainDir}/Card_{id}.asset");
            return a;
        }

        private static void EnsureFolder(string p)
        {
            if (AssetDatabase.IsValidFolder(p)) return;
            Directory.CreateDirectory(p); AssetDatabase.Refresh();
        }
    }
}
