using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public static class StringUtils
    {
        private static readonly char[] CHARS =
        {
            '\"', '\\', '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v'
        };
        private static readonly char[] ESCAPES =
        {
            '\"', '\\', '0', 'a', 'b', 'f', 'n', 'r', 't', 'v'
        };

        public static string FromLiteral(this string input, int lineNumber, int column)
        {
            if (input.Length < 2 || !input.StartsWith("\"") || !input.EndsWith("\""))
                throw new ArgumentException("Invalid literal");

            StringBuilder literal = new StringBuilder(input.Length - 2);
            char[] inputChars = input.ToCharArray();
            for (int i = 1; i < inputChars.Length - 1; ++i)
            {
                char c = inputChars[i];
                if (c == '\\')
                {
                    if (inputChars[i + 1] == 'u')
                    {
                        ++i;
                        ushort val;
                        if (i + 4 >= inputChars.Length - 1 || ushort.TryParse(input.Substring(i, 4), out val))
                            throw new ArgumentException("Invalid literal: " + input.Substring(i, 4) + " at " + GetLocation(input, lineNumber, column, i));
                        literal.Append((char)val);
                    }
                    else
                    {
                        int index = Array.IndexOf(ESCAPES, inputChars[i + 1]);
                        if (index >= 0)
                        {
                            literal.Append(CHARS[index]);
                            ++i;
                        }
                        else if (inputChars[i + 1] == '\'')
                        {
                            // Special handling for escaped quotes produced by some tools
                            literal.Append('\'');
                            ++i;
                        }
                        else throw new ArgumentException("Invalid literal: " + input.Substring(i, 2) + " at " + GetLocation(input, lineNumber, column, i));
                    }
                }
                else literal.Append(c);
            }
            return literal.ToString();
        }

        private static string GetLocation(string input, int lineNumber, int column, int index)
        {
            char[] inputChars = input.ToCharArray();
            for (int i = 0; i < index; ++i)
            {
                if (inputChars[i] == '\n')
                {
                    ++lineNumber;
                    column = 1;
                }
                else if (inputChars[i] == '\r')
                {
                    // Ignore
                }
                else ++column;
            }

            return string.Format("line {0}, column {1}", lineNumber, column);
        }

        public static string ToLiteral(this string input)
        {
            StringBuilder literal = new StringBuilder(input.Length + 2);

            if (input.Contains('\n'))
            {
                literal.Append("\"\"\n");
            }

            literal.Append("\"");
            bool needLineBreak = false;
            foreach (char c in input)
            {
                if (needLineBreak)
                {
                    literal.Append("\"\n\"");
                    needLineBreak = false;
                }
                int index = Array.IndexOf(CHARS, c);
                if (index >= 0)
                {
                    literal.Append('\\').Append(ESCAPES[index]);
                    if (c == '\n')
                    {
                        needLineBreak = true;
                    }
                }
                else if (Char.GetUnicodeCategory(c) != UnicodeCategory.Control)
                {
                    literal.Append(c);
                }
                else
                {
                    literal.Append(@"\u");
                    literal.Append(((ushort)c).ToString("x4"));
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }
    }
}
