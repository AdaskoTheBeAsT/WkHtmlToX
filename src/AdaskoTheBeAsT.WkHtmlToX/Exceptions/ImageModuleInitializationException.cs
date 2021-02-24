using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class ImageModuleInitializationException : Exception
    {
        public ImageModuleInitializationException()
        {
        }

        public ImageModuleInitializationException(string message)
            : base(message)
        {
        }

        public ImageModuleInitializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ImageModuleInitializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
