using System;
using System.Collections.Generic;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal static class NativeCleanup
{
    internal static void Attempt(Action action, List<Exception> failures)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
    }

    internal static void ThrowIfFailed(List<Exception> failures, Exception originalFailure)
    {
        if (failures.Count > 0)
        {
            failures.Insert(0, originalFailure);
            throw new AggregateException("Native conversion setup and cleanup failed.", failures);
        }
    }
}
