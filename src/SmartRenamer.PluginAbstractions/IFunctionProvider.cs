using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartRenamer.PluginAbstractions
{
    public interface IFunctionProvider
    {
        /// <summary>
        /// The name of the function, which is used to call it.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Returns the value of the given property for the given file.
        /// </summary>
        Task<object?> GetValue(string filePath, IReadOnlyList<object?> arguments);
    }
}
