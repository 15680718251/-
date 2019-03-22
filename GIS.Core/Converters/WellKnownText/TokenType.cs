using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.Converters.WellKnownText
{
    /// <summary>
    /// Represents the type of token created by the StreamTokenizer class.
    /// </summary>
    internal enum TokenType
    {
        Word,
        Number,
        Eol,
        Eof,
        Whitespace,
        Symbol
    }
}
