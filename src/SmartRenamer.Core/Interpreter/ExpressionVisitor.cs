using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartRenamer.Core.Abstractions;
using SmartRenamer.Core.Parser;

namespace SmartRenamer.Core.Interpreter
{
    internal class ExpressionVisitor : IExpressionVisitor
    {
        private readonly IFunctionExecutor _functionExecutor;
        private readonly ISymbolEvaluator _symbolEvaluator;

        public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;

        public ExpressionVisitor(IFunctionExecutor functionExecutor, ISymbolEvaluator symbolEvaluator)
        {
            _functionExecutor = functionExecutor;
            _symbolEvaluator = symbolEvaluator;
        }

        private async Task<List<object?>> Visit(IEnumerable<Expression> values, string filePath) =>
            await values.ToAsyncEnumerable()
                .SelectAwait(async expr => await Visit(expr, filePath).ConfigureAwait(false))
                .ToListAsync().ConfigureAwait(false);

        public async Task<object?> Visit(Expression value, string filePath)
        {
            return value switch
            {
                LiteralExpression lit => lit.Value,
                CallExpression cal =>
                    await _functionExecutor.Call(cal.Function, filePath, await Visit(cal.Arguments, filePath).ConfigureAwait(false)).ConfigureAwait(false),
                SymbolExpression sym =>
                    await _symbolEvaluator.Evaluate(filePath, sym, this).ConfigureAwait(false),
                FormattedExpression frm =>
                    await Visit(frm.Expression, filePath).ConfigureAwait(false) switch
                    {
                        IFormattable x => x.ToString(frm.Formatter.Format, frm.Formatter.Invariant ? CultureInfo.InvariantCulture : FormatProvider),
                        _ => throw new NotImplementedException()
                    },
                _ => throw new NotImplementedException()
            };
        }
    }
}
