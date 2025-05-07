using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DM2Projekt.Tests.Account;

[TestClass]
public class UrlValidatorTests
{
    // Simple copy of the helper we're testing
    private class UrlValidator
    {
        private readonly HttpClient _httpClient;

        public UrlValidator(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Checks if the given URL points to a real image (not a fake .jpg)
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
                // If something goes wrong (e.g. bad URL), treat it as "not found"
                return false;
            }
        }
    }

    [TestMethod]
    public async Task UrlExistsAsync_Should_Return_True_For_Valid_Image()
    {
        // Simulate a 200 OK with image/jpeg content-type
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent("");
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                return response;
            });

        var client = new HttpClient(handlerMock.Object);
        var validator = new UrlValidator(client);

        var result = await validator.UrlExistsAsync("https://example.com/image.jpg");

        Assert.IsTrue(result, "Should return true for a valid, reachable image URL");
    }

    [TestMethod]
    public async Task UrlExistsAsync_Should_Return_False_For_404()
    {
        // Simulate a 404 Not Found
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = new HttpClient(handlerMock.Object);
        var validator = new UrlValidator(client);

        var result = await validator.UrlExistsAsync("https://example.com/missing.jpg");

        Assert.IsFalse(result, "Should return false if the image is missing");
    }

    [TestMethod]
    public async Task UrlExistsAsync_Should_Return_False_If_Exception()
    {
        // Simulate an exception (e.g. bad DNS, timeout)
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network issue"));

        var client = new HttpClient(handlerMock.Object);
        var validator = new UrlValidator(client);

        var result = await validator.UrlExistsAsync("https://example.com/broken.jpg");

        Assert.IsFalse(result, "Should return false if an exception occurs");
    }
}
