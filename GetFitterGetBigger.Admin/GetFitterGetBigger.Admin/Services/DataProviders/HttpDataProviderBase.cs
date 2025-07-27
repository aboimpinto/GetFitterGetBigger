using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Results;

namespace GetFitterGetBigger.Admin.Services.DataProviders
{
    /// <summary>
    /// Base class for HTTP data providers that provides common HTTP operations
    /// with built-in error handling and response deserialization.
    /// </summary>
    public abstract class HttpDataProviderBase
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILogger Logger;
        protected readonly JsonSerializerOptions JsonOptions;

        protected HttpDataProviderBase(HttpClient httpClient, ILogger logger)
        {
            HttpClient = httpClient;
            Logger = logger;
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Executes an HTTP GET request and deserializes the response.
        /// </summary>
        protected async Task<DataServiceResult<T>> ExecuteHttpGetRequestAsync<T>(
            string endpoint,
            [CallerMemberName] string callerMemberName = "")
        {
            Logger.LogDebug("HTTP GET {BaseAddress}{Endpoint} from {Caller}", 
                HttpClient.BaseAddress, endpoint, callerMemberName);
                
            return await ExecuteHttpRequestAsync<T>(
                async () => await HttpClient.GetAsync(endpoint),
                callerMemberName);
        }

        /// <summary>
        /// Executes an HTTP POST request with JSON content and deserializes the response.
        /// </summary>
        protected async Task<DataServiceResult<T>> ExecuteHttpPostRequestAsync<T>(
            string endpoint,
            object content,
            [CallerMemberName] string callerMemberName = "")
        {
            var json = JsonSerializer.Serialize(content, JsonOptions);
            Logger.LogTrace("POST {Endpoint} Request body: {Json}", endpoint, json);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await ExecuteHttpRequestAsync<T>(
                async () => await HttpClient.PostAsync(endpoint, httpContent),
                callerMemberName);
        }

        /// <summary>
        /// Executes an HTTP PUT request with JSON content and deserializes the response.
        /// </summary>
        protected async Task<DataServiceResult<T>> ExecuteHttpPutRequestAsync<T>(
            string endpoint,
            object content,
            [CallerMemberName] string callerMemberName = "")
        {
            var json = JsonSerializer.Serialize(content, JsonOptions);
            Logger.LogTrace("PUT {Endpoint} Request body: {Json}", endpoint, json);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await ExecuteHttpRequestAsync<T>(
                async () => await HttpClient.PutAsync(endpoint, httpContent),
                callerMemberName);
        }

        /// <summary>
        /// Executes an HTTP DELETE request and returns success/failure.
        /// </summary>
        protected async Task<DataServiceResult<bool>> ExecuteHttpDeleteRequestAsync(
            string endpoint,
            [CallerMemberName] string callerMemberName = "")
        {
            Logger.LogDebug("HTTP DELETE {BaseAddress}{Endpoint} from {Caller}", 
                HttpClient.BaseAddress, endpoint, callerMemberName);
                
            return await ExecuteHttpRequestAsync<bool>(
                async () => await HttpClient.DeleteAsync(endpoint),
                callerMemberName,
                deserializeResponse: false);
        }

        /// <summary>
        /// Core method that executes HTTP requests with error handling and optional deserialization.
        /// </summary>
        private async Task<DataServiceResult<T>> ExecuteHttpRequestAsync<T>(
            Func<Task<HttpResponseMessage>> httpRequest,
            string methodName,
            bool deserializeResponse = true)
        {
            try
            {
                var response = await httpRequest();

                if (response.IsSuccessStatusCode)
                {
                    if (!deserializeResponse)
                    {
                        // For DELETE operations or when we don't need to deserialize
                        return DataServiceResult<T>.Success((T)(object)true);
                    }

                    var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                    return data != null
                        ? DataServiceResult<T>.Success(data)
                        : DataServiceResult<T>.Failure(
                            DataError.DeserializationError("Failed to deserialize response"));
                }
                else
                {
                    return await CreateFailureResult<T>(response);
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "HTTP request failed for {MethodName}", methodName);
                return DataServiceResult<T>.Failure(DataError.NetworkError(ex.Message));
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogError(ex, "Request timeout for {MethodName}", methodName);
                return DataServiceResult<T>.Failure(DataError.Timeout());
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "JSON deserialization failed for {MethodName}", methodName);
                return DataServiceResult<T>.Failure(
                    DataError.DeserializationError($"Failed to deserialize response: {ex.Message}"));
            }
        }

        /// <summary>
        /// Creates a failure result based on the HTTP response status code.
        /// </summary>
        private async Task<DataServiceResult<T>> CreateFailureResult<T>(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            Logger.LogError("HTTP {StatusCode} error. Response: {ErrorContent}",
                response.StatusCode, errorContent);

            var details = new Dictionary<string, object>
            {
                ["statusCode"] = (int)response.StatusCode,
                ["response"] = errorContent
            };

            var error = response.StatusCode switch
            {
                HttpStatusCode.NotFound => DataError.NotFound("Resource"),
                HttpStatusCode.BadRequest => DataError.BadRequest("Invalid request"),
                HttpStatusCode.Unauthorized => DataError.Unauthorized(),
                HttpStatusCode.Forbidden => DataError.Forbidden(),
                HttpStatusCode.Conflict => DataError.Conflict("Resource conflict"),
                HttpStatusCode.UnprocessableEntity => DataError.BadRequest("Validation failed"),
                _ => DataError.ServerError($"HTTP error {response.StatusCode}")
            };

            // Add details to the error
            var errorWithDetails = new DataError(error.Code, error.Message, details);

            return DataServiceResult<T>.Failure(errorWithDetails);
        }
    }
}