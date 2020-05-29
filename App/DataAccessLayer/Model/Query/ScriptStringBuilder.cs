using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public class ScriptStringBuilder
    {
        private readonly List<string> _lines = new List<string>();
        public List<string> Lines { get { return _lines; }}

        private readonly StringBuilder _current = new StringBuilder();
        public StringBuilder Current { get { return _current; } }

        public int IndentCount { get; private set; }

        public ScriptStringBuilder BeginBlock()
        {
            if (Current.Length > 0)
            {
                AppendToLines(Current);
                Current.Clear();
            }
            IndentCount++;
            return this;
        }
        public ScriptStringBuilder EndBlock()
        {
            if (Current.Length > 0)
            {
                AppendToLines(Current);
                Current.Clear();
            }
            IndentCount--;
            return this;
        }

        public string GetIndent()
        {
            var s = "";
            var i = IndentCount;
            while (i > 0)
            {
                s += (char) 9;
                i--;
            }
            return s;
        }

        private void AppendToLines(StringBuilder sb)
        {
            using (var reader = new StringReader(sb.ToString()))
            {
                do
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    Lines.Add(GetIndent() + line);
                } while (true);
            }
        }

        public ScriptStringBuilder Append(string value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(string value, int startIndex, int count)
        {
            Current.Append(value, startIndex, count);
            return this;
        }
        public ScriptStringBuilder Append(char value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(char value, int repeatCount)
        {
            Current.Append(value, repeatCount);
            return this;
        }
        public ScriptStringBuilder Append(long value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(int value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(double value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(decimal value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(float value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(object value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(char[] value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(char[] value, int startIndex, int count)
        {
            Current.Append(value, startIndex, count);
            return this;
        }
        public ScriptStringBuilder Append(short value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(uint value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(ulong value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(ushort value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(byte value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(sbyte value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder Append(bool value)
        {
            Current.Append(value);
            return this;
        }
        public ScriptStringBuilder AppendFormat(string format, params object[] args)
        {
            Current.AppendFormat(format, args);
            return this;
        }
        public ScriptStringBuilder AppendFormat(string format, object arg0)
        {
            Current.AppendFormat(format, arg0);
            return this;
        }
        public ScriptStringBuilder AppendFormat(string format, object arg0, object arg1)
        {
            Current.AppendFormat(format, arg0, arg1);
            return this;
        }
        public ScriptStringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            Current.AppendFormat(format, arg0, arg1, arg2);
            return this;
        }
        public ScriptStringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            Current.AppendFormat(provider, format, args);
            return this;
        }
        public ScriptStringBuilder AppendLine()
        {
            AppendToLines(Current);
            Current.Clear();
            return this;
        }
        public ScriptStringBuilder AppendLine(string value)
        {
            Current.Append(value);
            AppendToLines(Current);
            Current.Clear();
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Fill(sb);
            return sb.ToString();
        }

        public void Fill(StringBuilder sb)
        {
            foreach (var line in Lines)
            {
                sb.AppendLine(line);
            }
            sb.Append(Current);
        }

        public void WriteTo(StreamWriter sw)
        {
            foreach (var line in Lines)
            {
                sw.WriteLine(line);
            }
            sw.Write(Current);
        }
    }
}