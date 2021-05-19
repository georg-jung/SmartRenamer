using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SmartRenamer.PluginAbstractions;

namespace SmartRenamer.Core.Interpreter
{
    internal class FunctionExecutor : IFunctionExecutor
    {
        private readonly Dictionary<string, IFunctionProvider> _providers = new Dictionary<string, IFunctionProvider>(StringComparer.OrdinalIgnoreCase);

        public void Register(IFunctionProvider provider)
        {
            _providers.Add(provider.Name, provider);
        }

        public async Task<object?> Call(string function, string filePath, IReadOnlyList<object?> arguments)
        {
            if (!_providers.TryGetValue(function, out var prov))
            {
                throw new ArgumentException($"A function with name {function} is not registered.");
            }

            return await prov.GetValue(filePath, arguments).ConfigureAwait(false);
        }
    }
}
