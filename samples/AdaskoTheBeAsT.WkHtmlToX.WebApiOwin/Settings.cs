using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using Serilog;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public static class Settings
    {
        public static string AllowedOrigins => Get(Constants.Settings.AllowedOrigins);

        /// <summary>
        /// Get value from appSettings section of web.config and expand environment variables.
        /// </summary>
        /// <param name="name">Key name.</param>
        /// <returns></returns>
        public static string Get(string name) => Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings[name] ?? string.Empty);

        public static string GetConnectionString(string name) => Environment.ExpandEnvironmentVariables(ConfigurationManager.ConnectionStrings[name]?.ConnectionString ?? string.Empty);

        public static class Auth
        {
            public static X509Certificate2 IssuerCertificate
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(IssuerCertThumbprint))
                    {
                        Log.Error($"Using Settings.Auth.IssuerCertificate before setting up a '{Constants.Settings.Auth.CertThumbprint}' value in the web.config");
                        throw new Exception($"Your have to set up a '{Constants.Settings.Auth.CertThumbprint}' value in the web.config before using Settings.Auth.IssuerCertificate");
                    }

                    if (_signingCertificate != null)
                    {
                        return _signingCertificate;
                    }

                    _signingCertificate = X509.LocalMachine.My.Thumbprint.Find(IssuerCertThumbprint).FirstOrDefault();

                    if (_signingCertificate == null)
                    {
                        Log.Error("Can't find certificate with a thumbprint '{cert}'", IssuerCertThumbprint);
                        throw new Exception($"Can't find certificate with a thumbprint '{IssuerCertThumbprint}'");
                    }

                    return _signingCertificate;
                }
            }

            public static string Issuer => Get(Constants.Settings.Auth.Issuer);

            public static string Audience => Get(Constants.Settings.Auth.Audience);

            public static string IssuerCertThumbprint => Get(Constants.Settings.Auth.CertThumbprint);

            private static X509Certificate2 _signingCertificate;
        }
    }
}
