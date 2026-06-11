using System;
using System.Collections.Generic;
using System.IO;
using DengeGame.Application;
using DengeGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DengeGame.Editor
{
    /// <summary>
    /// Üç temel sahneyi (MainMenu/Game/Result) ilgili kök kontrolcüleriyle birlikte oluşturur,
    /// Build Settings'e ekler ve mobil/dikey oynatma ayarlarını uygular.
    /// Menüden veya batchmode'da -executeMethod DengeGame.Editor.ProjectSetup.SetupProject ile çalışır.
    /// </summary>
    public static class ProjectSetup
    {
        private const string ScenesDir = "Assets/Game/Scenes";

        [MenuItem("Denge/Setup Project")]
        public static void SetupProject()
        {
            EnsureFolder(ScenesDir);

            var scenes = new List<string>
            {
                CreateScene(GameSceneNames.MainMenu, typeof(MainMenuController)),
                CreateScene(GameSceneNames.Game, typeof(GameSceneController)),
                CreateScene(GameSceneNames.Result, typeof(ResultSceneController))
            };

            ApplyBuildSettings(scenes);
            ApplyPlayerSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Denge] Proje kurulumu tamamlandı: 3 sahne, Build Settings ve dikey mobil ayarlar uygulandı.");
        }

        private static string CreateScene(string sceneName, Type controllerType)
        {
            string path = $"{ScenesDir}/{sceneName}.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject(sceneName + "Controller");
            go.AddComponent(controllerType);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static void ApplyBuildSettings(List<string> scenePaths)
        {
            var list = new EditorBuildSettingsScene[scenePaths.Count];
            for (int i = 0; i < scenePaths.Count; i++)
                list[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            EditorBuildSettings.scenes = list;
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "noutr";
            PlayerSettings.productName = "Denge";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            Directory.CreateDirectory(assetPath);
            AssetDatabase.Refresh();
        }
    }
}
