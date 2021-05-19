using System;
using System.Collections.Generic;
using System.Text;

namespace SmartRenamer.Core.Parser
{
    internal class CallExpression : Expression
    {
        public CallExpression(string function, IReadOnlyList<Expression> arguments)
        {
            Function = function;
            Arguments = arguments;
        }

        public string Function { get; }
        public IReadOnlyList<Expression> Arguments { get; }
    }
}
