using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GIS.Converters.WellKnownText
{
    internal class WktStreamTokenizer : StreamTokenizer
    {
        public WktStreamTokenizer(TextReader reader): base(reader, true)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
        }

        internal void ReadToken(string expectedToken)
        {
            this.NextToken();
            if (this.GetStringValue() != expectedToken)
            {
                throw new Exception(String.Format(GIS.Map.GeoMap.numberFormat_zhCN, "Expecting ('{3}') but got a '{0}' at line {1} column {2}.", this.GetStringValue(), this.LineNumber, this.Column, expectedToken));
            }
        }

        public string ReadDoubleQuotedWord()
        {
            string word = "";
            ReadToken("\"");
            NextToken(false);
            while (GetStringValue() != "\"")
            {
                word = word + this.GetStringValue();
                NextToken(false);
            }
            return word;
        }

        public void ReadAuthority(ref string authority, ref long authorityCode)
        {
            //AUTHORITY["EPGS","9102"]]
            if (GetStringValue() != "AUTHORITY")
                ReadToken("AUTHORITY");
            ReadToken("[");
            authority = this.ReadDoubleQuotedWord();
            ReadToken(",");
            long.TryParse(this.ReadDoubleQuotedWord(), out authorityCode);
            ReadToken("]");
        }

    }
}
