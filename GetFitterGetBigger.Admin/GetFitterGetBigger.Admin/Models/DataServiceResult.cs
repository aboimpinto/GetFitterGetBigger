namespace GetFitterGetBigger.Admin.Models
{
    /// <summary>
    /// Represents the result of a data service operation, encapsulating both success and failure scenarios.
    /// </summary>
    /// <typeparam name="T">The type of data returned on success.</typeparam>
    public class DataServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorCode { get; private set; }
        public string? ErrorMessage { get; private set; }
        public Dictionary<string, object>? ErrorDetails { get; private set; }

        /// <summary>
        /// Creates a successful result with the specified data.
        /// </summary>
        public static DataServiceResult<T> Success(T data)
        {
            return new DataServiceResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        /// <summary>
        /// Creates a failure result with the specified error information.
        /// </summary>
        public static DataServiceResult<T> Failure(string errorCode, string errorMessage, Dictionary<string, object>? details = null)
        {
            return new DataServiceResult<T>
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                ErrorDetails = details
            };
        }

        /// <summary>
        /// Pattern matching support for handling success and failure cases.
        /// </summary>
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<string, string, Dictionary<string, object>?, TResult> onFailure)
        {
            TResult result;

            if (IsSuccess && Data != null)
            {
                result = onSuccess(Data);
            }
            else
            {
                result = onFailure(
                    ErrorCode ?? "UNKNOWN",
                    ErrorMessage ?? "Unknown error",
                    ErrorDetails);
            }

            return result;
        }

        /// <summary>
        /// Simplified pattern matching without error details.
        /// </summary>
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<string, string, TResult> onFailure)
        {
            return Match(
                onSuccess,
                (code, message, _) => onFailure(code, message));
        }
    }
}