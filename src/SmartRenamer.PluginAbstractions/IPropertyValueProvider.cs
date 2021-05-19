using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartRenamer.PluginAbstractions
{
    public interface IPropertyValueProvider
    {
        /// <summary>
        /// The namespace of the properties for explicit specification.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Returns the value of the given property for the given file.
        /// </summary>
        Task<object?> GetValue(string filePath, string propertyName);

        /// <summary>
        /// A collection of the properties (their keys), that this plugin can provide as result of the <see cref="GetValue"/> method.
        /// </summary>
        IReadOnlyCollection<string> AvailableProperties { get; }
    }
}
