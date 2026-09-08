using System;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems;

#pragma warning disable S1133 // Planned removal in the next major release; retained for migration.
[Obsolete("Use IWkHtmlToXAsyncEngine task-returning conversion methods instead.")]
#pragma warning restore S1133
public interface IWorkItemVisitable
{
    void Accept(IWorkItemVisitor visitor);
}
