namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    internal interface IWkHtmlToXModuleFactory
    {
        IWkHtmlToXModule GetModule(ModuleKind moduleKind);
    }
}
