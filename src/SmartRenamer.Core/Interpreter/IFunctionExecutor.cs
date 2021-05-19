using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartRenamer.Core.Interpreter
{
    internal interface IFunctionExecutor
    {
        Task<object?> Call(string function, string filePath, IReadOnlyList<object?> arguments);
    }
}
