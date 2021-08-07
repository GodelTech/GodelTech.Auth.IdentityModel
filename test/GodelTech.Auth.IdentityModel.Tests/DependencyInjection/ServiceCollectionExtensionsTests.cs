using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GodelTech.Auth.IdentityModel.Tests.DependencyInjection
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddClientCredentialsFlowTokenService_Success()
        {
            // Arrange
            Action<ClientCredentialsFlowTokenOptions, IConfiguration> configureOptions = (options, configuration) => configuration
                .GetSection("ClientCredentialsFlowTokenOptions")
                .Bind(options);

            var services = new ServiceCollection();

            services
                .AddTransient<IConfiguration>(
                    _ => new ConfigurationBuilder()
                        .AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["ClientCredentialsFlowTokenOptions:Authority"] = "https://localhost:44300",
                                ["ClientCredentialsFlowTokenOptions:ClientId"] = "Test ClientId",
                                ["ClientCredentialsFlowTokenOptions:ClientSecret"] = "Test ClientSecret",
                                ["ClientCredentialsFlowTokenOptions:Scope"] = "Test Scope"
                            }
                        )
                        .Build()
                );

            // Act
            services.AddClientCredentialsFlowTokenService(configureOptions);

            // Assert
            var provider = services.BuildServiceProvider();

            var resultRequiredService = provider.GetRequiredService<IHttpClientFactory>();
            Assert.NotNull(resultRequiredService);

            var resultOptionsAction = provider.GetRequiredService<IOptions<ClientCredentialsFlowTokenOptions>>();
            Assert.NotNull(resultOptionsAction);
            Assert.NotNull(resultOptionsAction.Value);
            Assert.Equal("https://localhost:44300", resultOptionsAction.Value.Authority);
            Assert.Equal("Test ClientId", resultOptionsAction.Value.ClientId);
            Assert.Equal("Test ClientSecret", resultOptionsAction.Value.ClientSecret);
            Assert.Equal("Test Scope", resultOptionsAction.Value.Scope);

            var resultTokenService = provider.GetRequiredService<ITokenService>();
            Assert.NotNull(resultTokenService);
            Assert.IsType<ClientCredentialsFlowTokenService>(resultTokenService);
        }
    }
}