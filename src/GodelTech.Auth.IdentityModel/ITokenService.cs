using System;
using IdentityModel.Client;
using System.Threading.Tasks;

[assembly: CLSCompliant(false)]
namespace GodelTech.Auth.IdentityModel
{
    /// <summary>
    /// Interface of token service.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Request token.
        /// </summary>
        /// <returns><cref>Task{TokenResponse}</cref>.</returns>
        Task<TokenResponse> RequestTokenAsync();
    }
}
