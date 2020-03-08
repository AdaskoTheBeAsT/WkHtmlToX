using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public class BasicImageConverter
        : ConverterBase,
            IHtmlToImageConverter
    {
        public BasicImageConverter()
            : base(ModuleKind.Image)
        {
        }

        internal BasicImageConverter(IWkHtmlToXModuleFactory moduleFactory)
            : base(moduleFactory, ModuleKind.Image)
        {
        }

        public Stream Convert(IHtmlToImageDocument document)
        {
            var pdfStream = Stream.Null;
            var thread = new Thread(
                () => pdfStream = ConvertImpl(document));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
            return pdfStream;
        }

        private Stream ConvertImpl(IHtmlToImageDocument document)
        {
            if (document?.ImageSettings == null)
            {
                throw new ArgumentException(
                    "No image settings is defined in document that was passed. At least one object must be defined.");
            }

            ProcessingDocument = document;

            var result = Stream.Null;
            var loaded = _module.Initialize(0) == 1;
            if (!loaded)
            {
                throw new ArgumentException("Not loaded");
            }

            var (converterPtr, globalSettingsPtr) = CreateConverter(document);

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
                _module.DestroyGlobalSetting(globalSettingsPtr);
                _module.DestroyConverter(converterPtr);
                _module.Terminate();
                ProcessingDocument = null;
            }

            return result;
        }

        private (IntPtr converterPtr, IntPtr globalSettingsPtr) CreateConverter(
            IHtmlToImageDocument document)
        {
            var globalSettings = _module.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.ImageSettings);
            var converter = _module.CreateConverter(globalSettings);

            return (converter, globalSettings);
        }

        private void ApplyConfig(IntPtr config, ISettings settings)
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
                    Apply(config, wkHtmlAttribute.Name, propValue);
                }
                else if (propValue is ISettings propSettings)
                {
                    ApplyConfig(config, propSettings);
                }
            }
        }

        private void Apply(IntPtr config, string name, object value)
        {
            var type = value.GetType();

            if (typeof(bool) == type)
            {
                _module.SetGlobalSetting(config, name, (bool)value ? "true" : "false");
            }
            else if (typeof(double) == type)
            {
                _module.SetGlobalSetting(config, name, ((double)value).ToString("0.##", CultureInfo.InvariantCulture));
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
                    _module.SetGlobalSetting(config, $"{name}.append", null);
                    _module.SetGlobalSetting(config, $"{name}[{index}]", $"{pair.Key}\n{pair.Value}");

                    index++;
                }
            }
            else
            {
                _module.SetGlobalSetting(config, name, value.ToString());
            }
        }
    }
}
