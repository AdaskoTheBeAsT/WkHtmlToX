#nullable enable
using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public ProgressChangedEventArgs(
            IDocument? document,
            string description)
        {
            Document = document;
            Description = description;
        }

        public IDocument? Document { get; }

        public string Description { get; }
    }
}
