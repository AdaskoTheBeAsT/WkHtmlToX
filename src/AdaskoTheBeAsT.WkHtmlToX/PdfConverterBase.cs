using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public abstract class PdfConverterBase
        : ConverterBase
    {
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable CA1051 // Do not declare visible instance fields
        internal readonly IWkHtmlToPdfModule _pdfModule;
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore SA1401 // Fields should be private

        protected PdfConverterBase()
            : base(ModuleKind.Pdf)
        {
            var pdfModuleFactory = new WkHtmlToPdfModuleFactory();
            _pdfModule = pdfModuleFactory.GetModule();
        }

        protected Stream ConvertImpl(IHtmlToPdfDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.ObjectSettings?.Count == 0)
            {
                throw new ArgumentException(
                    "No objects is defined in document that was passed. At least one object must be defined.");
            }

            ProcessingDocument = document;

            var result = Stream.Null;
            var loaded = _module.Initialize(0) == 1;
            if (!loaded)
            {
                throw new ArgumentException("Not loaded");
            }

            var (converterPtr, globalSettingsPtr, objectSettingsPtrs) = CreateConverter(document);

            RegisterEvents(converterPtr);

            try
            {
                var converted = _module.Convert(converterPtr);

                if (converted)
                {
                    result = _module.GetOutput(converterPtr);
                }
            }
            finally
            {
                for (int i = objectSettingsPtrs.Count - 1; i >= 0; i--)
                {
                    _pdfModule.DestroyObjectSetting(objectSettingsPtrs[i]);
                }

                _module.DestroyGlobalSetting(globalSettingsPtr);
                _module.DestroyConverter(converterPtr);
                _module.Terminate();
                ProcessingDocument = null;
            }

            return result;
        }

        private (IntPtr converterPtr, IntPtr globalSettingsPtr, List<IntPtr> objectSettingsPtrs) CreateConverter(
            IHtmlToPdfDocument document)
        {
            var globalSettings = _module.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.GlobalSettings, true);
            var converter = _module.CreateConverter(globalSettings);
            var objectSettingsPtr = new List<IntPtr>();
            foreach (var obj in document.ObjectSettings)
            {
                if (obj == null)
                {
                    continue;
                }

                var objectSettings = _pdfModule.CreateObjectSettings();
                objectSettingsPtr.Add(objectSettings);

                ApplyConfig(objectSettings, obj, false);

                _pdfModule.AddObject(converter, objectSettings, obj.GetContent());
            }

            return (converter, globalSettings, objectSettingsPtr);
        }

        private void ApplyConfig(IntPtr config, ISettings settings, bool isGlobal)
        {
            if (settings == null)
            {
                return;
            }

            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var prop in settings.GetType().GetProperties(bindingFlags))
            {
                var attrs = (Attribute[])prop.GetCustomAttributes();
                var propValue = prop.GetValue(settings);
                if (propValue == null)
                {
                    continue;
                }

                if (attrs.Length > 0 && attrs[0] is WkHtmlAttribute wkHtmlAttribute)
                {
                    Apply(config, wkHtmlAttribute.Name, propValue, isGlobal);
                }
                else if (propValue is ISettings propSettings)
                {
                    ApplyConfig(config, propSettings, isGlobal);
                }
            }
        }

        private void Apply(IntPtr config, string name, object value, bool isGlobal)
        {
            var type = value.GetType();

            Func<IntPtr, string, string?, int> applySetting;
            if (isGlobal)
            {
                applySetting = _module.SetGlobalSetting;
            }
            else
            {
                applySetting = _pdfModule.SetObjectSetting;
            }

            if (typeof(bool) == type)
            {
                applySetting(config, name, (bool)value ? "true" : "false");
            }
            else if (typeof(double) == type)
            {
                applySetting(config, name, ((double)value).ToString("0.##", CultureInfo.InvariantCulture));
            }
            else if (typeof(Dictionary<string, string>).IsAssignableFrom(type))
            {
                var dictionary = (Dictionary<string, string>)value;
                var index = 0;

                foreach (var pair in dictionary)
                {
                    if (pair.Key == null || pair.Value == null)
                    {
                        continue;
                    }

                    // https://github.com/wkhtmltopdf/wkhtmltopdf/blob/c754e38b074a75a51327df36c4a53f8962020510/src/lib/reflect.hh#L192
                    applySetting(config, $"{name}.append", null);
                    applySetting(config, $"{name}[{index}]", $"{pair.Key}\n{pair.Value}");

                    index++;
                }
            }
            else
            {
                applySetting(config, name, value.ToString());
            }
        }
    }
}
