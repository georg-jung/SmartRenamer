using System;
using System.Collections.Generic;
using System.Text;

namespace SmartRenamer.Core.Parser
{
    internal class ConstToken : IToken
    {
        public ConstToken(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
