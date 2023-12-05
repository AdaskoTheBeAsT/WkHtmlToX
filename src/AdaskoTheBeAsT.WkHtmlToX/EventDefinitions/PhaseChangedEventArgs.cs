using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;

public class PhaseChangedEventArgs : EventArgs
{
    public PhaseChangedEventArgs(
        ISettings? document,
        int phaseCount,
        int currentPhase,
        string description)
    {
        Document = document;
        PhaseCount = phaseCount;
        CurrentPhase = currentPhase;
        Description = description;
    }

    public ISettings? Document { get; }

    public int PhaseCount { get; }

    public int CurrentPhase { get; }

    public string Description { get; }
}
