using Owin;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
        public void UseAuthentication(IAppBuilder app)
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
                signingKey: Settings.Auth.IssuerCertificate);
        }
    }
}
