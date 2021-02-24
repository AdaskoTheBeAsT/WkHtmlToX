using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class PdfModuleInitializationException : Exception
    {
        public PdfModuleInitializationException()
        {
        }

        public PdfModuleInitializationException(string message)
            : base(message)
        {
        }

        public PdfModuleInitializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PdfModuleInitializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
