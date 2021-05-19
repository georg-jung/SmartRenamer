using System;
using System.Collections.Generic;
using System.Text;

namespace SmartRenamer.Core.Parser
{
    internal class FormattedExpression : Expression
    {
        public FormattedExpression(Expression expression, Formatter formatter)
        {
            Expression = expression;
            Formatter = formatter;
        }

        public Expression Expression { get; }
        public Formatter Formatter { get; }
    }
}
