using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public ProgressChangedEventArgs(
            ISettings? document,
            string description)
        {
            Document = document;
            Description = description;
        }

        public ISettings? Document { get; }

        public string Description { get; }
    }
}
