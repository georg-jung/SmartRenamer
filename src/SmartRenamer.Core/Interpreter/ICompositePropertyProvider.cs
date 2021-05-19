using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartRenamer.Core.Interpreter
{
    public interface ICompositePropertyProvider
    {
        bool IsNamespaceKnown(string name);
        Task<object?> GetValue(string filePath, string propertyName);
        Task<object?> GetValue(string filePath, string providerNamespace, string propertyName);
    }
}
