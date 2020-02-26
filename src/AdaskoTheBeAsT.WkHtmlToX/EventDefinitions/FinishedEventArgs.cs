using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions
{
    public class FinishedEventArgs : EventArgs
    {
        public FinishedEventArgs(
            IDocument? document,
            bool success)
        {
            Document = document;
            Success = success;
        }

        public IDocument? Document { get; }

        public bool Success { get; }
    }
}
