using System;
using System.Collections.Generic;
using System.Text;

namespace SmartRenamer.Core.Parser
{
    internal class SymbolExpression : Expression
    {
        public SymbolExpression(Identifier first, IReadOnlyList<ISymbolExpressionPart> rest)
        {
            First = first;
            Rest = rest;
        }

        public Identifier First { get; }
        public IReadOnlyList<ISymbolExpressionPart> Rest { get; }
    }

    internal interface ISymbolExpressionPart { }

    internal class Identifier : ISymbolExpressionPart
    {
        public Identifier(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    internal class Indexer : ISymbolExpressionPart
    {
        public Indexer(IReadOnlyList<Expression> indices)
        {
            Indices = indices;
        }

        public IReadOnlyList<Expression> Indices { get; }
    }
}
