using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace GodelTech.Auth.IdentityModel.Tests
{
    public sealed class ClientCredentialsFlowTokenServiceTests : IDisposable
    {
        private readonly ClientCredentialsFlowTokenOptions _clientCredentialsFlowTokenOptions;
        private readonly Uri _discoveryEndpointUri;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<ClientCredentialsFlowTokenService>> _mockLogger;

        private readonly ClientCredentialsFlowTokenService _service;

        public ClientCredentialsFlowTokenServiceTests()
        {
            _clientCredentialsFlowTokenOptions = new ClientCredentialsFlowTokenOptions
            {
                Authority = "https://demo.identityserver.io",
                ClientId = "Test ClientId",
                ClientSecret = "Test ClientSecret",
                Scope = "Test Scope"
            };

            _discoveryEndpointUri = new Uri($"{_clientCredentialsFlowTokenOptions.Authority}/.well-known/openid-configuration");

            var mockClientCredentialsFlowTokenOptions = new Mock<IOptions<ClientCredentialsFlowTokenOptions>>(MockBehavior.Strict);
            mockClientCredentialsFlowTokenOptions
                .Setup(x => x.Value)
                .Returns(_clientCredentialsFlowTokenOptions);

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _mockHttpMessageHandler
                .Protected()
                .Setup("Dispose", true, true);

            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _mockHttpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            _mockHttpClientFactory
                .Setup(
                    x => x.CreateClient(
                        It.Is<string>(s => s == "tokenClient")
                    )
                )
                .Returns(_httpClient);

            _mockLogger = new Mock<ILogger<ClientCredentialsFlowTokenService>>(MockBehavior.Strict);

            _service = new ClientCredentialsFlowTokenService(
                mockClientCredentialsFlowTokenOptions.Object,
                _mockHttpClientFactory.Object,
                _mockLogger.Object
            );
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ClientCredentialsFlowTokenService(
                    null,
                    _mockHttpClientFactory.Object,
                    _mockLogger.Object
                )
            );

            Assert.Equal("clientCredentialsFlowTokenOptions", exception.ParamName);
        }

        [Fact]
        public async Task RequestTokenAsync_WhenDiscoveryDocumentResponseHasError_ThrowsHttpRequestException()
        {
            // Arrange
            using var discoveryDocumentResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Test ReasonPhrase"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        x =>
                            x.Method == HttpMethod.Get
                            && x.RequestUri == _discoveryEndpointUri
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(discoveryDocumentResponse);

            Expression<Action<ILogger<ClientCredentialsFlowTokenService>>> loggerExpression = x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() ==
                    $"Error connecting to {_discoveryEndpointUri.AbsoluteUri}: Test ReasonPhrase"
                ),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            );
            _mockLogger.Setup(loggerExpression);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => _service.RequestTokenAsync()
            );
            Assert.Equal(
                $"Error connecting to {_discoveryEndpointUri.AbsoluteUri}: Test ReasonPhrase",
                exception.Message
            );

            _mockLogger.Verify(loggerExpression, Times.Once);
        }

        [Fact]
        public async Task RequestTokenAsync_WhenTokenResponseHasError_ThrowsHttpRequestException()
        {
            // Arrange
            using var discoveryDocumentResponse = GetDiscoveryResponse();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        x =>
                            x.Method == HttpMethod.Get
                            && x.RequestUri == _discoveryEndpointUri
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(discoveryDocumentResponse);

            Expression<Action<ILogger<ClientCredentialsFlowTokenService>>> loggerExpression = x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() ==
                    $"Error connecting to {_discoveryEndpointUri.AbsoluteUri}: Test ReasonPhrase"
                ),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            );
            _mockLogger.Setup(loggerExpression);

            // Act & Assert
            await _service.RequestTokenAsync();

            //var exception = await Assert.ThrowsAsync<HttpRequestException>(
            //    () => _service.RequestTokenAsync()
            //);
            //Assert.Equal(
            //    $"Error connecting to {_discoveryEndpointUri.AbsoluteUri}: Test ReasonPhrase",
            //    exception.Message
            //);

            //_mockLogger.Verify(loggerExpression, Times.Once);
        }

        // https://stackoverflow.com/questions/62130584/how-to-mock-getdiscoverydocumentasync-when-unit-testing-httpclient
        private static HttpResponseMessage GetDiscoveryResponse()
        {
            var json = File.ReadAllText("Documents\\discovery.json");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            return response;
        }
    }
}