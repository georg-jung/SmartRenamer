using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SmartRenamer.Core.Parser
{
    internal abstract class LiteralExpression : Expression
    {
        public abstract object? Value { get; }
    }

    internal class NullLiteralExpression : LiteralExpression
    {
        public static readonly NullLiteralExpression Instance = new();

        private NullLiteralExpression()
        {
        }

        public override object? Value => null;
    }

    internal class LiteralExpression<T> : LiteralExpression
    {
        public LiteralExpression(T value)
        {
            TypedValue = value;
        }

        public override object? Value => TypedValue;
        public T TypedValue { get; }
    }
}
