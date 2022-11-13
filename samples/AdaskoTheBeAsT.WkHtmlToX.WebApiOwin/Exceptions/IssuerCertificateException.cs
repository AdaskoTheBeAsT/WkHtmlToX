using System;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Exceptions
{
    [Serializable]
    public class IssuerCertificateException
        : Exception
    {
        public IssuerCertificateException()
        {
        }

        public IssuerCertificateException(string message)
            : base(message)
        {
        }

        public IssuerCertificateException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        protected IssuerCertificateException(
            SerializationInfo serializationInfo,
            StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
