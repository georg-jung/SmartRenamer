using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using SmartRenamer.Core.Parser;

namespace SmartRenamer.Core.Interpreter
{
    internal class TokenVisitor
    {
        private readonly IExpressionVisitor _expressionVisitor;
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;

        public TokenVisitor(IExpressionVisitor expressionVisitor)
        {
            _expressionVisitor = expressionVisitor;
        }

        public async Task<string> Visit(IToken value, string filePath)
        {
            return value switch {
                ConstToken c => c.Value,
                Expression expr =>
                    await _expressionVisitor.Visit(expr, filePath).ConfigureAwait(false) switch {
                        null => "",
                        string s => s,
                        IFormattable f => f.ToString(null, FormatProvider),
                        object x => x.ToString()
                    },
                _ => throw new NotImplementedException()
            };
        }
    }
}
