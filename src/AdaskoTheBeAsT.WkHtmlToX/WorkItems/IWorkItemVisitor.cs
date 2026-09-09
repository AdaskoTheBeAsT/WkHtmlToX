using System;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems;

#pragma warning disable S1133 // Planned removal in the next major release; retained for migration.
[Obsolete("Use IWkHtmlToXAsyncEngine task-returning conversion methods. Custom visitors are not an engine extension point.")]
#pragma warning restore S1133
public interface IWorkItemVisitor
{
    void Visit(PdfConvertWorkItem item);

    void Visit(ImageConvertWorkItem item);
}
