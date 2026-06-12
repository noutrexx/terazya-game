using System;
using System.Collections.Generic;
using DengeGame.Infrastructure;
using DengeGame.Infrastructure.Data;
using DengeGame.Application;
using DengeGame.Application.Effects;
using DengeGame.Application.Endings;
using DengeGame.Application.Policies;
using DengeGame.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DengeGame.Presentation
{
    /// <summary>
    /// Composition root. Tüm servisleri tek yerde oluşturur (manuel DI; framework yok) ve
    /// sahne yüklendikçe ISceneController'lara açıkça enjekte eder. DontDestroyOnLoad ile yaşar.
    /// Domain/Application bu sınıfa bağımlı DEĞİLDİR; bağımlılık yalnızca Presentation → çekirdek yönündedir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Bootstrap : MonoBehaviour
    {
        private static Bootstrap _instance;

        private GameServices _services;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            BuildServices();

            SceneManager.sceneLoaded += OnSceneLoaded;
            InjectControllers(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _instance = null;
            }
        }

        private void BuildServices()
        {
            int bootSeed = Environment.TickCount;

            IRandomService random = new SystemRandomService(bootSeed);
            ITimeService time = new UnityTimeService();
            ISaveService save = new JsonFileSaveService();
            IEventSelectionService selection = new WeightedEventSelectionService();
            IDecisionEffectService decisions = new DecisionEffectService();
            ISceneTransitionService transition = new SceneTransitionService();

            var database = Resources.Load<EventCardDatabase>(EventCardDatabase.ResourcesPath);
            if (database == null)
                Debug.LogError($"[Denge] EventCardDatabase Resources'ta bulunamadı " +
                               $"('{EventCardDatabase.ResourcesPath}'). 'Denge/İçerik/Test Kartları Üret' çalıştırın.");
            Func<List<EventCard>> poolProvider = () =>
                database != null ? database.BuildPool() : new List<EventCard>();

            var characterDb = Resources.Load<CharacterDatabase>(CharacterDatabase.ResourcesPath);
            if (characterDb == null)
                Debug.LogWarning($"[Denge] CharacterDatabase bulunamadı ('{CharacterDatabase.ResourcesPath}'). " +
                                 "'Denge/İçerik/Karakterleri Üret' çalıştırın.");
            var characters = new CharacterDirectory(characterDb != null ? characterDb.BuildList() : new List<Character>());

            IEndingEvaluator endings = new CompositeEndingEvaluator(
                new BoundaryEndingEvaluator(),
                new CharacterEndingEvaluator(characters.All));

            var policyDb = Resources.Load<PolicyDatabase>(PolicyDatabase.ResourcesPath);
            var crisisDb = Resources.Load<CrisisDatabase>(CrisisDatabase.ResourcesPath);
            var policyRegistry = new PolicyRegistry(policyDb != null ? policyDb.BuildList() : new List<PolicyDefinition>());
            var crisisRegistry = new CrisisRegistry(crisisDb != null ? crisisDb.BuildList() : new List<CrisisDefinition>());
            var processor = new PolicyCrisisProcessor(policyRegistry, crisisRegistry);

            IGameFlow flow = new GameFlow(transition, () => Environment.TickCount,
                poolProvider, selection, decisions, random, endings, processor);

            _services = new GameServices(random, time, save, selection, decisions, transition, characters, flow);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InjectControllers(scene);

        private void InjectControllers(Scene scene)
        {
            if (!scene.IsValid() || _services == null) return;

            foreach (var root in scene.GetRootGameObjects())
            {
                var controllers = root.GetComponentsInChildren<ISceneController>(true);
                foreach (var controller in controllers)
                    controller.Construct(_services);
            }
        }
    }

    /// <summary>
    /// Bootstrap'ı ilk sahne yüklenmeden önce otomatik oluşturur; böylece her sahneye elle
    /// Bootstrap eklemek gerekmez ve oyun herhangi bir sahneden Play edilse de çalışır.
    /// </summary>
    public static class GameEntryPoint
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var go = new GameObject("[Denge Bootstrap]");
            go.AddComponent<Bootstrap>();
        }
    }
}
