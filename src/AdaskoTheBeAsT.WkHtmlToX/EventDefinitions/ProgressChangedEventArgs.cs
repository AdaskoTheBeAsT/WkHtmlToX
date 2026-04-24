using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;

public class ProgressChangedEventArgs : EventArgs
{
    public ProgressChangedEventArgs(
        ISettings? document,
        int progress,
        string description)
    {
        Document = document;
        Progress = progress;
        Description = description;
    }

    public ISettings? Document { get; }

    public int Progress { get; }

    public string Description { get; }
}
