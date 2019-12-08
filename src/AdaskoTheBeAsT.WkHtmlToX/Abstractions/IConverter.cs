using System;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using ErrorEventArgs = AdaskoTheBeAsT.WkHtmlToX.EventDefinitions.ErrorEventArgs;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IConverter
    {
        event EventHandler<PhaseChangedEventArgs> PhaseChanged;

        event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        event EventHandler<FinishedEventArgs> Finished;

#pragma warning disable CA1716
        event EventHandler<ErrorEventArgs> Error;
#pragma warning restore CA1716

        event EventHandler<WarningEventArgs> Warning;
    }
}
