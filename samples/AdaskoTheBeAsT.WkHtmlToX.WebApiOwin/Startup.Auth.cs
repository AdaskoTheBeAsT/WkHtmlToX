using Owin;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
#pragma warning disable CC0091 // Use static method
        public void UseAuthentication(IAppBuilder app)
#pragma warning restore CC0091 // Use static method
        {
            if (string.IsNullOrWhiteSpace(Settings.Auth.Issuer)
                || string.IsNullOrWhiteSpace(Settings.Auth.Audience)
                || string.IsNullOrWhiteSpace(Settings.Auth.IssuerCertThumbprint))
            {
                return;
            }

            app.UseJsonWebToken(
                issuer: Settings.Auth.Issuer,
                audience: Settings.Auth.Audience,
                signingKey: Settings.Auth.GetIssuerCertificate());
        }
    }
}
