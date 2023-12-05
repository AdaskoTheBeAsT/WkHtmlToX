namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems;

public interface IWorkItemVisitor
{
    void Visit(PdfConvertWorkItem item);

    void Visit(ImageConvertWorkItem item);
}
