using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GIS.Converters.WellKnownText
{
    internal class StreamTokenizer
    {
        TokenType _currentTokenType;
        TextReader _reader;
        string _currentToken;
        bool _ignoreWhitespace = false;
        int _lineNumber = 1;
        int _colNumber = 1;

        public StreamTokenizer(TextReader reader, bool ignoreWhitespace)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _reader = reader;
            _ignoreWhitespace = ignoreWhitespace;
        }

        #region Properties
        public int LineNumber
        {
            get
            {
                return _lineNumber;
            }
        }
        public int Column
        {
            get
            {
                return _colNumber;
            }
        }
        #endregion


        #region Methods
        public double GetNumericValue()
        {
            string number = this.GetStringValue();
            if (this.GetTokenType() == TokenType.Number)
            {
                return double.Parse(number, GIS.Map.GeoMap.numberFormat_zhCN);
            }
            throw new Exception(String.Format(GIS.Map.GeoMap.numberFormat_zhCN, "The token '{0}' is not a number at line {1} column {2}.", number, this.LineNumber, this.Column)); ;
        }

        public string GetStringValue()
        {
            return _currentToken;
        }

        public TokenType GetTokenType()
        {
            return _currentTokenType;
        }

        public TokenType NextToken(bool ignoreWhitespace)
        {
            TokenType nextTokenType;
            if (ignoreWhitespace)
            {
                nextTokenType = NextNonWhitespaceToken();
            }
            else
            {
                nextTokenType = NextTokenAny();
            }
            return nextTokenType;
        }

        public TokenType NextToken()
        {
            return NextToken(_ignoreWhitespace);
        }

        private TokenType NextTokenAny()
        {
            TokenType nextTokenType = TokenType.Eof;
            char[] chars = new char[1];
            _currentToken = "";
            _currentTokenType = TokenType.Eof;
            int finished = _reader.Read(chars, 0, 1);

            bool isNumber = false;
            bool isWord = false;
            byte[] ba = null;
            ASCIIEncoding AE = new ASCIIEncoding();
            char[] ascii = null;
            Char currentCharacter;
            Char nextCharacter;
            while (finished != 0)
            {
                // convert int to char
                ba = new Byte[] { (byte)_reader.Peek() };

                ascii = AE.GetChars(ba);

                currentCharacter = chars[0];
                nextCharacter = ascii[0];
                _currentTokenType = GetType(currentCharacter);
                nextTokenType = GetType(nextCharacter);

                // handling of words with _
                if (isWord && currentCharacter == '_')
                {
                    _currentTokenType = TokenType.Word;
                }
                // handing of words ending in numbers
                if (isWord && _currentTokenType == TokenType.Number)
                {
                    _currentTokenType = TokenType.Word;
                }

                if (_currentTokenType == TokenType.Word && nextCharacter == '_')
                {
                    //enable words with _ inbetween
                    nextTokenType = TokenType.Word;
                    isWord = true;
                }
                if (_currentTokenType == TokenType.Word && nextTokenType == TokenType.Number)
                {
                    //enable words ending with numbers
                    nextTokenType = TokenType.Word;
                    isWord = true;
                }

                // handle negative numbers
                if (currentCharacter == '-' && nextTokenType == TokenType.Number && isNumber == false)
                {
                    _currentTokenType = TokenType.Number;
                    nextTokenType = TokenType.Number;
                    //isNumber = true;
                }


                // this handles numbers with a decimal point
                if (isNumber && nextTokenType == TokenType.Number && currentCharacter == '.')
                {
                    _currentTokenType = TokenType.Number;
                }
                if (_currentTokenType == TokenType.Number && nextCharacter == '.' && isNumber == false)
                {
                    nextTokenType = TokenType.Number;
                    isNumber = true;
                }

                _colNumber++;
                if (_currentTokenType == TokenType.Eol)
                {
                    _lineNumber++;
                    _colNumber = 1;
                }

                _currentToken = _currentToken + currentCharacter;
                //if (_currentTokenType==TokenType.Word && nextCharacter=='_')
                //{
                // enable words with _ inbetween
                //	finished = _reader.Read(chars,0,1);
                //}
                if (_currentTokenType != nextTokenType)
                {
                    finished = 0;
                }
                else if (_currentTokenType == TokenType.Symbol && currentCharacter != '-')
                {
                    finished = 0;
                }
                else
                {
                    finished = _reader.Read(chars, 0, 1);
                }
            }
            return _currentTokenType;
        }

        private TokenType GetType(char character)
        {
            if (Char.IsDigit(character))
            {
                return TokenType.Number;
            }
            else if (Char.IsLetter(character))
            {
                return TokenType.Word;
            }
            else if (character == '\n')
            {
                return TokenType.Eol;
            }
            else if (Char.IsWhiteSpace(character) || Char.IsControl(character))
            {
                return TokenType.Whitespace;
            }
            else //(Char.IsSymbol(character))
            {
                return TokenType.Symbol;
            }
        }

        private TokenType NextNonWhitespaceToken()
        {

            TokenType tokentype = this.NextTokenAny();
            while (tokentype == TokenType.Whitespace || tokentype == TokenType.Eol)
            {
                tokentype = this.NextTokenAny();
            }

            return tokentype;
        }
        #endregion

    }
}
