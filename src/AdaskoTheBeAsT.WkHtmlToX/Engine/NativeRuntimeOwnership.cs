using System;
using System.Threading;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal sealed class NativeRuntimeOwnership
{
#if NET9_0_OR_GREATER
    private readonly Lock _syncRoot = new();
#else
    private readonly object _syncRoot = new();
#endif
    private WkHtmlToXSessionFactory? _owner;
    private bool _poisoned;

    internal static NativeRuntimeOwnership Shared { get; } = new();

    internal void Acquire(WkHtmlToXSessionFactory owner)
    {
        lock (_syncRoot)
        {
            if (_poisoned)
            {
                throw new InvalidOperationException("Native teardown failed. Restart the renderer process before creating another engine.");
            }

            if (_owner is not null && !ReferenceEquals(_owner, owner))
            {
                throw new InvalidOperationException("Only one WkHtmlToX native owner is supported per process. Share the existing engine.");
            }

            _owner = owner;
        }
    }

    internal void Release(WkHtmlToXSessionFactory owner)
    {
        lock (_syncRoot)
        {
            if (ReferenceEquals(_owner, owner))
            {
                _owner = null;
            }
        }
    }

    internal void Poison()
    {
        lock (_syncRoot)
        {
            _poisoned = true;
        }
    }
}
