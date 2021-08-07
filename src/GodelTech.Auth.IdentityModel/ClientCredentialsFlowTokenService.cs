using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GodelTech.Auth.IdentityModel
{
    /// <summary>
    /// Client credentials flow token service.
    /// </summary>
    public class ClientCredentialsFlowTokenService : ITokenService
    {
        private readonly ClientCredentialsFlowTokenOptions _clientCredentialsFlowTokenOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ClientCredentialsFlowTokenService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCredentialsFlowTokenService"/> class.
        /// </summary>
        /// <param name="clientCredentialsFlowTokenOptions">Client credentials flow token options.</param>
        /// <param name="httpClientFactory">The http client factory.</param>
        /// <param name="logger">The logger.</param>
        public ClientCredentialsFlowTokenService(
            IOptions<ClientCredentialsFlowTokenOptions> clientCredentialsFlowTokenOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<ClientCredentialsFlowTokenService> logger)
        {
            if (clientCredentialsFlowTokenOptions == null) throw new ArgumentNullException(nameof(clientCredentialsFlowTokenOptions));

            _clientCredentialsFlowTokenOptions = clientCredentialsFlowTokenOptions.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Request token.
        /// </summary>
        /// <returns><cref>Task{TokenResponse}</cref>.</returns>
        public async Task<TokenResponse> RequestTokenAsync()
        {
            var tokenClient = _httpClientFactory.CreateClient("tokenClient");

            var discoveryDocumentResponse = await tokenClient.GetDiscoveryDocumentAsync(_clientCredentialsFlowTokenOptions.Authority);
            if (discoveryDocumentResponse.IsError)
            {
                _logger.LogError(discoveryDocumentResponse.Error);
                throw new InvalidOperationException(discoveryDocumentResponse.Error);
            }

            _logger.LogInformation(discoveryDocumentResponse.TokenEndpoint);

            using var tokenRequest = CreateTokenRequest(discoveryDocumentResponse);

            var tokenResponse = await tokenClient
                .RequestClientCredentialsTokenAsync(
                    tokenRequest
                );

            if (tokenResponse.IsError)
            {
                _logger.LogError(tokenResponse.Error);
                throw new InvalidOperationException(tokenResponse.Error);
            }

            _logger.LogDebug(tokenResponse.AccessToken);
            return tokenResponse;
        }

        private ClientCredentialsTokenRequest CreateTokenRequest(DiscoveryDocumentResponse discoveryDocumentResponse)
        {
            return new ClientCredentialsTokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,

                ClientId = _clientCredentialsFlowTokenOptions.ClientId,
                ClientSecret = _clientCredentialsFlowTokenOptions.ClientSecret,

                Scope = _clientCredentialsFlowTokenOptions.Scope
            };
        }
    }
}
