using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions
{
    public class PhaseChangedEventArgs : EventArgs
    {
        public IDocument Document { get; set; }

        public int PhaseCount { get; set; }

        public int CurrentPhase { get; set; }

        public string Description { get; set; }
    }
}
