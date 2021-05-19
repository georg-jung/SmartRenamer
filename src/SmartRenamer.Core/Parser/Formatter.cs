using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SmartRenamer.Core.Parser
{
    internal class Formatter
    {
        public Formatter(string format, bool invariant)
        {
            Format = format;
            Invariant = invariant;
        }

        public string Format { get; }
        public bool Invariant { get; }
    }
}
