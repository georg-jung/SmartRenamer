using System.Threading.Tasks;
using SmartRenamer.Core.Parser;

namespace SmartRenamer.Core.Interpreter
{
    internal interface IExpressionVisitor
    {
        Task<object?> Visit(Expression value, string filePath);
    }
}
