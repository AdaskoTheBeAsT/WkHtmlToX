#nullable enable
using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions
{
    public class WarningEventArgs : EventArgs
    {
        public WarningEventArgs(
            ISettings? document,
            string message)
        {
            Document = document;
            Message = message;
        }

        public ISettings? Document { get; }

        public string Message { get; }
    }
}
