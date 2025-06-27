using System.Net;
using System.Text;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Tests.Helpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        private readonly List<HttpRequestMessage> _requests = new();

        public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

        public MockHttpMessageHandler SetupResponse(HttpStatusCode statusCode, object? content = null)
        {
            var response = new HttpResponseMessage(statusCode);
            
            if (content != null)
            {
                var json = JsonSerializer.Serialize(content);
                response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            _responses.Enqueue(response);
            return this;
        }

        public MockHttpMessageHandler SetupResponse(HttpResponseMessage response)
        {
            _responses.Enqueue(response);
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requests.Add(request);

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException("No response configured for HTTP request");
            }

            return Task.FromResult(_responses.Dequeue());
        }

        public void VerifyRequest(Func<HttpRequestMessage, bool> predicate)
        {
            if (!_requests.Any(predicate))
            {
                throw new InvalidOperationException("No request matching the predicate was found");
            }
        }

        public void VerifyNoRequests()
        {
            if (_requests.Count > 0)
            {
                throw new InvalidOperationException($"Expected no requests but found {_requests.Count}");
            }
        }
    }
}