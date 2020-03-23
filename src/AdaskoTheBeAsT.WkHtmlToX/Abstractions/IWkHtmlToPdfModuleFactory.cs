namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    internal interface IWkHtmlToPdfModuleFactory
    {
        IWkHtmlToPdfModule GetModule(int platformId);
    }
}
