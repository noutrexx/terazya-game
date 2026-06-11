using System;
using System.IO;
using DengeGame.Application;
using DengeGame.Core;
using UnityEngine;

namespace DengeGame.Infrastructure
{
    /// <summary>
    /// ISaveService'in JSON + dosya tabanlı temel implementasyonu.
    /// Platforma özel yolu Application.persistentDataPath üzerinden çözer.
    /// Yarım yazma riskine karşı temp dosyaya yazıp atomik biçimde yer değiştirir.
    /// Versiyonlama ve migration Faz 10'da eklenecektir.
    /// </summary>
    public sealed class JsonFileSaveService : ISaveService
    {
        private readonly string _root;

        public JsonFileSaveService(string rootDirectory = null)
        {
            _root = string.IsNullOrEmpty(rootDirectory)
                ? Path.Combine(UnityEngine.Application.persistentDataPath, "saves")
                : rootDirectory;
        }

        private string PathFor(string key) => Path.Combine(_root, key + ".json");

        public bool Exists(string key)
        {
            try { return File.Exists(PathFor(key)); }
            catch { return false; }
        }

        public Result Save<T>(string key, T data)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Result.Failure("Kayıt anahtarı boş olamaz.");

            try
            {
                Directory.CreateDirectory(_root);
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                string target = PathFor(key);
                string temp = target + ".tmp";

                File.WriteAllText(temp, json);

                // Atomik yer değiştirme: önce temp'e yaz, sonra hedefin üzerine taşı.
                if (File.Exists(target))
                    File.Replace(temp, target, null);
                else
                    File.Move(temp, target);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Kayıt yazılamadı ({key}): {ex.Message}");
            }
        }

        public Result<T> Load<T>(string key)
        {
            try
            {
                string target = PathFor(key);
                if (!File.Exists(target))
                    return Result.Failure<T>($"Kayıt bulunamadı: {key}");

                string json = File.ReadAllText(target);
                if (string.IsNullOrWhiteSpace(json))
                    return Result.Failure<T>($"Kayıt boş: {key}");

                var data = JsonUtility.FromJson<T>(json);
                if (data == null)
                    return Result.Failure<T>($"Kayıt çözümlenemedi (bozuk): {key}");

                return Result.Success(data);
            }
            catch (Exception ex)
            {
                return Result.Failure<T>($"Kayıt okunamadı ({key}): {ex.Message}");
            }
        }

        public Result Delete(string key)
        {
            try
            {
                string target = PathFor(key);
                if (File.Exists(target)) File.Delete(target);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Kayıt silinemedi ({key}): {ex.Message}");
            }
        }
    }
}
