using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public IDocument Document { get; set; }

        public string Description { get; set; }
    }
}
