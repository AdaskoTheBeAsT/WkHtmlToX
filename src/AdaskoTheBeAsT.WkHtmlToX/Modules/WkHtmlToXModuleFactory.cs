using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToXModuleFactory
        : IWkHtmlToXModuleFactory
    {
        public IWkHtmlToXModule GetModule(
            ModuleKind moduleKind) =>
            moduleKind switch
            {
                ModuleKind.Image => new WkHtmlToImageCommonModule(),
                _ => new WkHtmlToPdfCommonModule(),
            };
    }
}
