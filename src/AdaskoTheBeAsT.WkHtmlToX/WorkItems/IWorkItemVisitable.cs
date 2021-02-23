namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems
{
    public interface IWorkItemVisitable
    {
        void Accept(IWorkItemVisitor visitor);
    }
}
