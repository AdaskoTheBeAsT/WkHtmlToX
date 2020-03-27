namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    internal interface IWkHtmlToXModuleFactory
    {
        IWkHtmlToXModule GetModule(int platformId, ModuleKind moduleKind);
    }
}
