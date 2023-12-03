using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions
{
    public class FinishedEventArgs : EventArgs
    {
        public FinishedEventArgs(
            ISettings? document,
            bool success)
        {
            Document = document;
            Success = success;
        }

        public ISettings? Document { get; }

        public bool Success { get; }
    }
}
