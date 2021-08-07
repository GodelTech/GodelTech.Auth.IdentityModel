namespace GodelTech.Auth.IdentityModel
{
    /// <summary>
    /// Client credentials flow token options.
    /// </summary>
    public class ClientCredentialsFlowTokenOptions
    {
        /// <summary>
        /// Authority.
        /// </summary>
        /// <value>Authority.</value>
        public string Authority { get; set; }

        /// <summary>
        /// Client id.
        /// </summary>
        /// <value>Client id.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// Client secret.
        /// </summary>
        /// <value>Client secret.</value>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Scope.
        /// </summary>
        /// <value>Scope.</value>
        public string Scope { get; set; }
    }
}
