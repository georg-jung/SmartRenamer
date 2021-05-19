using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartRenamer.Core.Interpreter;
using SmartRenamer.PluginAbstractions;

namespace SmartRenamer.Core
{
    internal class CachingCompositePropertyProvider : ICompositePropertyProvider
    {
        private readonly IReadOnlyList<IPropertyValueProvider> _providers;
        private readonly Dictionary<string, IPropertyValueProvider> _providersByName;
        private readonly Dictionary<(string fileName, IPropertyValueProvider provider), IDictionary<string, object?>> _results = new();

        /// <param name="providers">Ordered list of analyzers. If a property value request is ambiguous, value of first analyzeer is given.</param>
        public CachingCompositePropertyProvider(IReadOnlyList<IPropertyValueProvider> providers)
        {
            _providers = providers;
            _providersByName = providers.ToDictionary(a => a.Namespace, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsNamespaceKnown(string name) => _providersByName.ContainsKey(name);

        public async Task<object?> GetValue(string fileName, string propertyName)
        {
            foreach (var provider in _providers)
            {
                if (!_results.TryGetValue((fileName, provider), out var results))
                {
                    results = new Dictionary<string, object?>();
                    _results.Add((fileName, provider), results);
                }

                if (results.TryGetValue(propertyName, out var result))
                {
                    return result;
                }
                result = await provider.GetValue(fileName, propertyName);
                results.Add(propertyName, result);
                return result;
            }

            return null;
        }

        public async Task<object?> GetValue(string fileName, string providerName, string propertyName)
        {
            if (!_providersByName.TryGetValue(providerName, out var provider))
            {
                throw new ArgumentException($"Provider {provider} is unknown.");
            }

            if (!_results.TryGetValue((fileName, provider), out var results))
            {
                results = new Dictionary<string, object?>();
                _results.Add((fileName, provider), results);
            }

            if (results.TryGetValue(propertyName, out var result))
            {
                return result;
            }
            result = await provider.GetValue(fileName, propertyName);
            results.Add(propertyName, result);
            return result;
        }
    }
}
