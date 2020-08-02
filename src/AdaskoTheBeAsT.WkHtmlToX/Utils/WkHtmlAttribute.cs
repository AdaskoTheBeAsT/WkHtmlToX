using System;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class WkHtmlAttribute : Attribute
    {
        public WkHtmlAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public string Name { get; }
    }
}
