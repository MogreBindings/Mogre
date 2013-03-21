using System;
using System.Text;

namespace AutoWrap.Meta
{
    public class IndentStringBuilder
    {
        private string indstr = "";

        public StringBuilder sb = new StringBuilder();

        public void IncreaseIndent()
        {
            indstr += "\t";
        }

        public void DecreaseIndent()
        {
            indstr = indstr.Substring(1);
        }

        public void Append(string str)
        {
            sb.Append(str);
        }

        public void AppendIndent(string str)
        {
            sb.Append(indstr + str);
        }

        public void AppendLine()
        {
            sb.AppendLine("");
        }

        public void AppendLine(string str)
        {
            sb.AppendLine(AddIndentation(str));
        }

        public void AppendFormat(string str, params object[] args)
        {
            sb.AppendFormat(str, args);
        }

        public void AppendFormatIndent(string str, params object[] args)
        {
            sb.AppendFormat(AddIndentation(str), args);
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        private string AddIndentation(string str)
        {
            string res = indstr + String.Join("\n" + indstr, str.Replace("\r\n", "\n").Split('\n'));
            if (res.EndsWith(indstr))
                res = res.Substring(0, res.Length - indstr.Length);
            return res;
        }
    }
}