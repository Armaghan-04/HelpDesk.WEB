namespace HelpDesk.WEB.Common.Results
{
    /// <summary>
    /// Represents the outcome of an operation that returns a value when successful.
    /// </summary>
    public sealed class Result<T> : Result
    {
        private readonly T _value;

        private Result(T value)
            : base(true, Error.None)
        {
            if (value is null)
            {
                throw new ArgumentNullException(
                    nameof(value),
                    "A successful result must contain a value.");
            }

            _value = value;
        }

        private Result(Error error)
            : base(false, error)
        {
            _value = default!;
        }

        /// <summary>
        /// Gets the successful value. Accessing this on a failed result throws.
        /// </summary>
        public T Value => IsSuccess
            ? _value
            : throw new InvalidOperationException(
                "The value of a failed result cannot be accessed.");

        public static Result<T> Success(T value) => new(value);

        public static new Result<T> Failure(Error error) => new(error);
    }
}
