using Moq;
using Moq.Protected;
using System.Net;

namespace DM2Projekt.Tests.Account;

[TestClass]
public class UrlValidatorTests
{
    // Simple helper we're testing
    private class UrlValidator
    {
        private readonly HttpClient _httpClient;

        public UrlValidator(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Check if URL points to a real image
        public async Task<bool> UrlExistsAsync(string url)
        {
            try
            {
                using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return response.IsSuccessStatusCode &&
                       response.Content.Headers.ContentType?.MediaType?.StartsWith("image") == true;
            }
            catch
            {
                // Treat any error (timeout, bad DNS, etc) as "not found"
                return false;
            }
        }
    }

    [TestMethod]
    public async Task UrlExistsAsync_Should_Return_True_For_Valid_Image()
    {
        var validator = new UrlValidator(CreateMockHttpClient(HttpStatusCode.OK, "image/jpeg"));

        var result = await validator.UrlExistsAsync("https://example.com/image.jpg");

        Assert.IsTrue(result, "Should return true for a valid, reachable image URL");
    }

    [TestMethod]
    public async Task UrlExistsAsync_Should_Return_False_For_404()
    {
        var validator = new UrlValidator(CreateMockHttpClient(HttpStatusCode.NotFound));

        var result = await validator.UrlExistsAsync("https://example.com/missing.jpg");

        Assert.IsFalse(result, "Should return false if the image is missing");
    }

    [TestMethod]
    public async Task UrlExistsAsync_Should_Return_False_If_Exception()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network issue"));

        var validator = new UrlValidator(new HttpClient(handlerMock.Object));

        var result = await validator.UrlExistsAsync("https://example.com/broken.jpg");

        Assert.IsFalse(result, "Should return false if an exception occurs");
    }

    // Helper to mock responses easily
    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string? contentType = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var response = new HttpResponseMessage(statusCode);
                response.Content = new StringContent("");
                if (contentType != null)
                {
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                }
                return response;
            });

        return new HttpClient(handlerMock.Object);
    }
}
