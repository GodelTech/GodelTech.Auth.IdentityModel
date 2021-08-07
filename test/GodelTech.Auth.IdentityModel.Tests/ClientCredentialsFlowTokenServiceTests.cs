using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GodelTech.Auth.IdentityModel.Tests
{
    public class ClientCredentialsFlowTokenServiceTests
    {
        private readonly ClientCredentialsFlowTokenOptions _clientCredentialsFlowTokenOptions;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<ClientCredentialsFlowTokenService>> _mockLogger;

        private readonly ClientCredentialsFlowTokenService _service;

        public ClientCredentialsFlowTokenServiceTests()
        {
            _clientCredentialsFlowTokenOptions = new ClientCredentialsFlowTokenOptions
            {
                Authority = "Test Authority",
                ClientId = "Test ClientId",
                ClientSecret = "Test ClientSecret",
                Scope = "Test Scope"
            };

            var mockClientCredentialsFlowTokenOptions = new Mock<IOptions<ClientCredentialsFlowTokenOptions>>(MockBehavior.Strict);
            mockClientCredentialsFlowTokenOptions
                .Setup(x => x.Value)
                .Returns(_clientCredentialsFlowTokenOptions);

            _mockLogger = new Mock<ILogger<ClientCredentialsFlowTokenService>>(MockBehavior.Strict);
            _mockHttpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);

            _service = new ClientCredentialsFlowTokenService(
                mockClientCredentialsFlowTokenOptions.Object,
                _mockHttpClientFactory.Object,
                _mockLogger.Object
            );
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
    }
}