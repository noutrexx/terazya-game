using System;

namespace DengeGame.Core
{
    /// <summary>
    /// Hata durumlarını exception fırlatmadan açıkça taşıyan basit Result tipi.
    /// Domain ve Application katmanlarında öngörülebilir hata yönetimi için kullanılır.
    /// </summary>
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }

        public bool IsFailure => !IsSuccess;

        private Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(string error) => new Result(false, error ?? "Bilinmeyen hata");

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
    }

    /// <summary>
    /// Bir değer taşıyabilen Result tipi.
    /// </summary>
    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        public bool IsFailure => !IsSuccess;

        private Result(bool isSuccess, T value, string error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure(string error) => new Result<T>(false, default, error ?? "Bilinmeyen hata");

        /// <summary>Başarılıysa değeri, değilse verilen yedeği döndürür.</summary>
        public T GetValueOrDefault(T fallback = default) => IsSuccess ? Value : fallback;

        public override string ToString() =>
            IsSuccess ? $"Success({Value})" : $"Failure({Error})";
    }
}
