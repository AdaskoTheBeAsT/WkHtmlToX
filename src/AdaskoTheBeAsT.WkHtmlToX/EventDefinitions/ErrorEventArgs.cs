using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;

public class ErrorEventArgs : EventArgs
{
    public ErrorEventArgs(
        ISettings? document,
        string message)
    {
        Document = document;
        Message = message;
    }

    public ISettings? Document { get; }

    public string Message { get; }
}
