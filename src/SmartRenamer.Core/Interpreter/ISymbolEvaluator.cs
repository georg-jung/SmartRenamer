using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SmartRenamer.Core.Parser;

namespace SmartRenamer.Core.Interpreter
{
    internal interface ISymbolEvaluator
    {
        Task<object?> Evaluate(string filePath, SymbolExpression symbolExpression, IExpressionVisitor expressionVisitor);
    }
}
