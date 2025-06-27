using System.Net;
using FluentAssertions;
using GetFitterGetBigger.Admin.Tests.Helpers;

namespace GetFitterGetBigger.Admin.Tests.Helpers
{
    public class MockHttpMessageHandlerTests
    {
        [Fact]
        public async Task SendAsync_WhenResponseConfigured_ReturnsConfiguredResponse()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();
            var expectedContent = new { message = "test" };
            handler.SetupResponse(HttpStatusCode.OK, expectedContent);

            var httpClient = new HttpClient(handler);

            // Act
            var response = await httpClient.GetAsync("https://test.com/api/test");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test");
        }

        [Fact]
        public async Task SendAsync_WhenMultipleResponsesConfigured_ReturnsInOrder()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();
            handler.SetupResponse(HttpStatusCode.OK)
                   .SetupResponse(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(handler);

            // Act
            var response1 = await httpClient.GetAsync("https://test.com/api/test1");
            var response2 = await httpClient.GetAsync("https://test.com/api/test2");

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SendAsync_WhenNoResponseConfigured_ThrowsException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(handler);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => httpClient.GetAsync("https://test.com/api/test"));
        }

        [Fact]
        public async Task VerifyRequest_WhenRequestMatches_DoesNotThrow()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();
            handler.SetupResponse(HttpStatusCode.OK);
            var httpClient = new HttpClient(handler);

            // Act
            await httpClient.GetAsync("https://test.com/api/test");

            // Assert
            handler.VerifyRequest(req => req.RequestUri!.ToString().Contains("test"));
        }

        [Fact]
        public void VerifyRequest_WhenNoMatchingRequest_ThrowsException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();
            handler.SetupResponse(HttpStatusCode.OK);
            var httpClient = new HttpClient(handler);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => handler.VerifyRequest(req => req.RequestUri!.ToString().Contains("notfound")));
        }

        [Fact]
        public void VerifyNoRequests_WhenNoRequests_DoesNotThrow()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();

            // Act & Assert
            handler.VerifyNoRequests();
        }

        [Fact]
        public async Task VerifyNoRequests_WhenRequestsMade_ThrowsException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();
            handler.SetupResponse(HttpStatusCode.OK);
            var httpClient = new HttpClient(handler);

            // Act
            await httpClient.GetAsync("https://test.com/api/test");

            // Assert
            Assert.Throws<InvalidOperationException>(() => handler.VerifyNoRequests());
        }
    }
}