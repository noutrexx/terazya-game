using System;

namespace DengeGame.Core
{
    /// <summary>
    /// Argüman doğrulaması için yardımcı koruyucular. Hataları erken ve anlaşılır biçimde yüzeye çıkarır.
    /// </summary>
    public static class Guard
    {
        public static T NotNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
            return value;
        }

        public static string NotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"'{paramName}' boş olamaz.", paramName);
            return value;
        }

        public static int InRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $"{min}-{max} aralığında olmalı.");
            return value;
        }
    }
}
