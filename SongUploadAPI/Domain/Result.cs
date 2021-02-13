using OneOf;

namespace SongUploadAPI.Domain
{
    public readonly struct Error
    {
        public string Message { get; }
        public Error(string message) => Message = message;
    }

    public class Result<T> : OneOfBase<T, Error>
    {
        protected Result(OneOf<T, Error> input) : base(input)
        {
        }

        // create implicit conversions from T and Error to Result<T>
        public static implicit operator Result<T>(T _) => new Result<T>(_);
        public static implicit operator Result<T>(Error _) => new Result<T>(_);

        public bool IsOk => IsT0;
        public bool IsError => IsT1;

        public T AsOk => AsT0;
        public Error AsError => AsT1;
    }
}
