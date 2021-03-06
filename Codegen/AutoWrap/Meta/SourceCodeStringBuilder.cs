﻿using System;
using System.Text;

namespace AutoWrap.Meta
{
    /// <summary>
    /// This string builder is used to create all generated source code files.
    /// </summary>
    public class SourceCodeStringBuilder
    {
        /// <summary>
        /// The string to be used to indent a line by one level.
        /// </summary>
        public const string INDENT_STRING = "  ";
        /// <summary>
        /// The line ending used for all lines in the file.
        /// </summary>
        public const string NEWLINE_STRING = "\r\n";

        private readonly StringBuilder _builder = new StringBuilder();
        private readonly CodeStyleDefinition _codeStyleDef;
        private string _curIndention = "";

        public SourceCodeStringBuilder(CodeStyleDefinition codeStyleDef)
        {
            _codeStyleDef = codeStyleDef;
        }

        /// <summary>
        /// Clears this string builder.
        /// </summary>
        public void Clear()
        {
            _builder.Clear();
            _curIndention = "";
        }

        /// <summary>
        /// Increases the indention by one level.
        /// </summary>
        public void IncreaseIndent()
        {
            _curIndention += _codeStyleDef.IndentionLevelString;
        }

        /// <summary>
        /// Decreases the indention by one level.
        /// </summary>
        public void DecreaseIndent()
        {
            // Strip one indention level
            _curIndention = _curIndention.Substring(_codeStyleDef.IndentionLevelString.Length);
        }

        public void InsertAt(uint pos, string str, bool indent = true)
        {
            _builder.Insert((int)pos, CreateAppendableString(str, indent, indent));
        }

        /// <summary>
        /// Appends the specified string with no indention of the first line.
        /// </summary>
        /// <param name="otherLinesIndent">if this is true (default), all lines
        /// of <paramref name="str"/> (except for the first one) will be indented.</param>
        /// <see cref="AppendIndent"/>
        public SourceCodeStringBuilder Append(string str, bool otherLinesIndent = true)
        {
            _builder.Append(CreateAppendableString(str, false, otherLinesIndent));
            return this;
        }

        /// <summary>
        /// Appends the specified string with indention added to the first line.
        /// </summary>
        /// <param name="otherLinesIndent">if this is true (default), all lines
        /// of <paramref name="str"/> will be indented. If it is false, only the
        /// first line will be indented.</param>
        public SourceCodeStringBuilder AppendIndent(string str, bool otherLinesIndent = true)
        {
            _builder.Append(CreateAppendableString(str, true, otherLinesIndent));
            return this;
        }

        /// <summary>
        /// Adds an empty line.
        /// </summary>
        public void AppendEmptyLine()
        {
            _builder.Append(_codeStyleDef.NewLineCharacters);
        }
    
        /// <summary>
        /// Appends the specified string and adds a new line at the end of the string.
        /// </summary>
        /// <param name="otherLinesIndent">if this is true (default), all lines
        /// of <paramref name="str"/> will be indented. If it is false, only the
        /// first line will be indented.</param>
        /// <param name="firstLineIndent">if this is true (default) the first line will
        /// be indented</param>
        /// <param name="otherLinesIndent">if this is true (default), all lines
        /// of <paramref name="str"/> (except for the first one) will be indented.</param>
        /// <see cref="AppendIndent"/>
        public SourceCodeStringBuilder AppendLine(string str, bool firstLineIndent = true, bool otherLinesIndent = true) 
        {
            _builder.Append(CreateAppendableString(str, firstLineIndent, otherLinesIndent)).Append(_codeStyleDef.NewLineCharacters);
            return this;
        }

        public SourceCodeStringBuilder AppendFormat(string str, params object[] args) 
        {
            _builder.AppendFormat(CreateAppendableString(str, false, true), args);
            return this;
        }

        public SourceCodeStringBuilder AppendFormatIndent(string str, params object[] args)
        {
            _builder.AppendFormat(CreateAppendableString(str, true, true), args);
            return this;
        }
    
        public override string ToString()
        {
            return _builder.ToString();
        }
    
        private string CreateAppendableString(string str, bool firstLineIndent, bool otherLinesIndent)
        {
            if (_codeStyleDef.IndentionLevelString != "\t") {
                // Replace remaining tabs with the correct indention style
                str = str.Replace("\t", _codeStyleDef.IndentionLevelString);
            }

            string[] lines = str.Replace("\r\n", "\n").Split('\n');
            string result;

            if (firstLineIndent)
                result = _curIndention;
            else
                result = "";

            if (lines.Length == 1)
                return result + lines[0];

            if (otherLinesIndent)
            {
                result += String.Join(_codeStyleDef.NewLineCharacters + _curIndention, lines);

                // Remove indention at the end of the new string when the 
                // original string ended with an empty line.
                if (result.EndsWith(_curIndention))
                    result = result.Substring(0, result.Length - _curIndention.Length);
            } else
                result += String.Join(_codeStyleDef.NewLineCharacters, lines);
    
            return result;
        }
    }
}