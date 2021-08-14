using System;
using GodelTech.Auth.IdentityModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extensions to register ClientCredentialsFlowTokenService with the service collection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register ClientCredentialsFlowTokenService with the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configureOptions">ClientCredentialsFlowTokenService options.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddClientCredentialsFlowTokenService(
            this IServiceCollection services,
            Action<ClientCredentialsFlowTokenOptions, IConfiguration> configureOptions)
        {
            // HttpClient
            services.AddHttpClient();

            // Options
            services.AddOptions<ClientCredentialsFlowTokenOptions>()
                .Configure(configureOptions);

            // ClientCredentialsFlowTokenService
            services.AddTransient<ITokenService, ClientCredentialsFlowTokenService>();

            return services;
        }
    }
}