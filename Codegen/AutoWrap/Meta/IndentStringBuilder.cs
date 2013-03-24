using System;
using System.Text;

namespace AutoWrap.Meta
{
    public class IndentStringBuilder
    {
        public const string INDENT_STRING = "\t";
        public const string NEWLINE_STRING = "\n";

        private readonly StringBuilder _builder = new StringBuilder();
        private string _curIndention = "";

        /// <summary>
        /// Increases the indention by one level.
        /// </summary>
        public void IncreaseIndent()
        {
            _curIndention += INDENT_STRING;
        }

        /// <summary>
        /// Decreases the indention by one level.
        /// </summary>
        public void DecreaseIndent()
        {
            // Strip one indention level
            _curIndention = _curIndention.Substring(INDENT_STRING.Length);
        }

        public void InsertAt(uint pos, string str, bool indent = true)
        {
            _builder.Insert((int)pos, str);
        }

        /// <summary>
        /// Appends the specified string to this builder.
        /// </summary>
        public void Append(string str)
        {
            _builder.Append(str);
        }
    
        public void AppendIndent(string str)
        {
            _builder.Append(_curIndention + str);
        }

        public void AppendEmptyLine()
        {
            _builder.AppendLine("");
        }
    
        public void AppendLine(string str)
        {
            _builder.AppendLine(CreateAppendableString(str, true));
        }
    
        public void AppendFormat(string str, params object[] args)
        {
            _builder.AppendFormat(str, args);
        }
    
        public void AppendFormatIndent(string str, params object[] args)
        {
            _builder.AppendFormat(CreateAppendableString(str, true), args);
        }
    
        public override string ToString()
        {
            return _builder.ToString();
        }
    
        private string CreateAppendableString(string str, bool indent)
        {
            string[] lines = str.Replace("\r\n", "\n").Split('\n');
            string result;

            if (indent)
            {
                result = _curIndention + String.Join(NEWLINE_STRING + _curIndention, lines);

                // Remove indention at the end of the new string when the 
                // original string ended with an empty line.
                if (result.EndsWith(_curIndention))
                    result = result.Substring(0, result.Length - _curIndention.Length);
            } else
                result = String.Join(NEWLINE_STRING, lines);
    
            return result;
        }
    }
}