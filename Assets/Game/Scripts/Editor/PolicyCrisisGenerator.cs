using System.Collections.Generic;
using System.IO;
using DengeGame.Domain;
using DengeGame.Infrastructure.Data;
using UnityEditor;
using UnityEngine;
using E = DengeGame.Domain.EffectData;
using V = DengeGame.Domain.CountryValue;

namespace DengeGame.Editor
{
    /// <summary>6 politika ve 4 kriz tanımı + veritabanlarını üretir (batchmode veya menü).</summary>
    public static class PolicyCrisisGenerator
    {
        private const string PolicyDir = "Assets/Game/Data/Policies";
        private const string CrisisDir = "Assets/Game/Data/Crises";
        private const string ResourcesDir = "Assets/Game/Resources";

        [MenuItem("Denge/İçerik/Politika ve Krizleri Üret")]
        public static void Generate()
        {
            EnsureFolder(PolicyDir); EnsureFolder(CrisisDir); EnsureFolder(ResourcesDir);

            var policies = new List<PolicyAsset>();
            foreach (var build in PolicyBuilders())
            {
                var a = ScriptableObject.CreateInstance<PolicyAsset>();
                build(a.Source);
                AssetDatabase.CreateAsset(a, $"{PolicyDir}/Policy_{a.Source.Id}.asset");
                policies.Add(a);
            }
            var pdb = ScriptableObject.CreateInstance<PolicyDatabase>();
            pdb.SetPolicies(policies);
            AssetDatabase.CreateAsset(pdb, $"{ResourcesDir}/{PolicyDatabase.ResourcesPath}.asset");

            var crises = new List<CrisisAsset>();
            foreach (var build in CrisisBuilders())
            {
                var a = ScriptableObject.CreateInstance<CrisisAsset>();
                build(a.Source);
                AssetDatabase.CreateAsset(a, $"{CrisisDir}/Crisis_{a.Source.Id}.asset");
                crises.Add(a);
            }
            var cdb = ScriptableObject.CreateInstance<CrisisDatabase>();
            cdb.SetCrises(crises);
            AssetDatabase.CreateAsset(cdb, $"{ResourcesDir}/{CrisisDatabase.ResourcesPath}.asset");

            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log($"[Denge] {policies.Count} politika + {crises.Count} kriz üretildi.");
        }

        private delegate void BuildP(PolicyDefinition p);
        private delegate void BuildC(CrisisDefinition c);

        private static List<E> L(params E[] e) => new List<E>(e);

        private static IEnumerable<BuildP> PolicyBuilders()
        {
            yield return p => { P(p, "ubi", "Evrensel Temel Gelir", "Tüm yurttaşlara koşulsuz gelir.", 8);
                p.StartEffects = L(E.ChangeValue(V.PublicSupport, 8));
                p.PerTurnEffects = L(E.ChangeValue(V.Economy, -2), E.ChangeValue(V.PublicSupport, 1));
                p.EndEffects = L(E.ChangeValue(V.PublicSupport, -4)); };

            yield return p => { P(p, "siki_sinir", "Sıkı Sınır Kontrolü", "Sınır geçişleri sıkı denetlenir.", 6);
                p.StartEffects = L(E.ChangeValue(V.Security, 6), E.ChangeValue(V.Diplomacy, -4));
                p.PerTurnEffects = L(E.ChangeValue(V.Security, 1), E.ChangeValue(V.Freedom, -1));
                p.IncompatiblePolicyIds = new List<string> { "diplomatik_acilim" }; };

            yield return p => { P(p, "yesil_donusum", "Yeşil Dönüşüm Programı", "Yenilenebilir enerjiye geçiş.", 10);
                p.StartEffects = L(E.ChangeValue(V.Environment, 6), E.ChangeValue(V.Economy, -5));
                p.PerTurnEffects = L(E.ChangeValue(V.Environment, 2), E.ChangeValue(V.Economy, -1));
                p.IncompatiblePolicyIds = new List<string> { "sanayi_tesvik" }; };

            yield return p => { P(p, "basin_yasa", "Basın Düzenleme Yasası", "Medya üzerinde denetim artar.", 6);
                p.StartEffects = L(E.ChangeValue(V.Security, 4), E.ChangeValue(V.Freedom, -6));
                p.PerTurnEffects = L(E.ChangeValue(V.Freedom, -1), E.ChangeValue(V.PublicSupport, -1)); };

            yield return p => { P(p, "sanayi_tesvik", "Sanayi Teşvik Paketi", "Üretim ve yatırım teşvik edilir.", 8);
                p.StartEffects = L(E.ChangeValue(V.Economy, 8), E.ChangeValue(V.Environment, -4));
                p.PerTurnEffects = L(E.ChangeValue(V.Economy, 2), E.ChangeValue(V.Environment, -2));
                p.IncompatiblePolicyIds = new List<string> { "yesil_donusum" }; };

            yield return p => { P(p, "diplomatik_acilim", "Diplomatik Açılım", "Dış ilişkilerde yumuşama.", 8);
                p.StartEffects = L(E.ChangeValue(V.Diplomacy, 8), E.ChangeValue(V.Security, -3));
                p.PerTurnEffects = L(E.ChangeValue(V.Diplomacy, 1));
                p.IncompatiblePolicyIds = new List<string> { "siki_sinir" }; };
        }

        private static IEnumerable<BuildC> CrisisBuilders()
        {
            yield return c => { C(c, "ekonomik_durgunluk", "Ekonomik Durgunluk", "Ekonomi yavaşlıyor.", "Ekonomik Çöküntü", 0.5f,
                L(E.ChangeValue(V.Economy, -8), E.ChangeValue(V.PublicSupport, -6)));
                c.Stages = new List<CrisisStage> {
                    Stage("İlk Belirtiler", 2, L(E.ChangeValue(V.Economy, -3)), L(E.ChangeValue(V.Economy, -1))),
                    Stage("Derinleşen Durgunluk", 2, L(E.ChangeValue(V.Economy, -4), E.ChangeValue(V.PublicSupport, -3)), L(E.ChangeValue(V.Economy, -1), E.ChangeValue(V.PublicSupport, -1))) }; };

            yield return c => { C(c, "buyuk_protestolar", "Büyük Protestolar", "Sokaklar hareketleniyor.", "Halk Ayaklanması", 0.6f,
                L(E.ChangeValue(V.PublicSupport, -8), E.ChangeValue(V.Security, -6)));
                c.Stages = new List<CrisisStage> {
                    Stage("Gösteriler", 2, L(E.ChangeValue(V.PublicSupport, -3)), L(E.ChangeValue(V.Security, -1))),
                    Stage("Yaygın Eylemler", 2, L(E.ChangeValue(V.Security, -4), E.ChangeValue(V.Freedom, -2)), L(E.ChangeValue(V.PublicSupport, -2))) }; };

            yield return c => { C(c, "sinir_gerilimi", "Sınır Gerilimi", "Komşu sınırda hareketlilik.", "Sınır Savaşı", 0.5f,
                L(E.ChangeValue(V.Security, -8), E.ChangeValue(V.Diplomacy, -6)));
                c.Stages = new List<CrisisStage> {
                    Stage("Diplomatik Kriz", 2, L(E.ChangeValue(V.Diplomacy, -3)), L(E.ChangeValue(V.Diplomacy, -1))),
                    Stage("Askeri Yığınak", 2, L(E.ChangeValue(V.Security, -4), E.ChangeValue(V.Economy, -2)), L(E.ChangeValue(V.Security, -1))) }; };

            yield return c => { C(c, "kuraklik", "Kuraklık", "Su kaynakları azalıyor.", "Susuzluk Felaketi", 0.5f,
                L(E.ChangeValue(V.Environment, -8), E.ChangeValue(V.Economy, -6)));
                c.Stages = new List<CrisisStage> {
                    Stage("Kuraklık Başlangıcı", 2, L(E.ChangeValue(V.Environment, -3)), L(E.ChangeValue(V.Environment, -1), E.ChangeValue(V.Economy, -1))),
                    Stage("Su Krizi", 2, L(E.ChangeValue(V.Economy, -4), E.ChangeValue(V.PublicSupport, -3)), L(E.ChangeValue(V.PublicSupport, -1))) }; };
        }

        private static CrisisStage Stage(string name, int dur, List<E> enter, List<E> perTurn) =>
            new CrisisStage { Name = name, DurationTurns = dur, EnterEffects = enter, PerTurnEffects = perTurn };

        private static void P(PolicyDefinition p, string id, string name, string desc, int duration)
        {
            p.Id = id; p.Name = name; p.Description = desc; p.DurationTurns = duration; p.CanCancelEarly = true;
        }

        private static void C(CrisisDefinition c, string id, string name, string desc, string failEnding, float chance, List<E> failEffects)
        {
            c.Id = id; c.Name = name; c.Description = desc;
            c.FailEndingReason = failEnding; c.FailEndingChance = chance; c.FailEffects = failEffects;
        }

        private static void EnsureFolder(string p)
        {
            if (AssetDatabase.IsValidFolder(p)) return;
            Directory.CreateDirectory(p); AssetDatabase.Refresh();
        }
    }
}
