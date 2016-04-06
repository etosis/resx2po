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
            '\'', '\"', '\\', '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v'
        };
        private static readonly char[] ESCAPES =
        {
            '\'', '\"', '\\', '0', 'a', 'b', 'f', 'n', 'r', 't', 'v'
        };

        public static string FromLiteral(this string input)
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
                            throw new ArgumentException("Invalid literal");
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
                        else throw new ArgumentException("Invalid literal");
                    }
                }
                else literal.Append(c);
            }
            return literal.ToString();
        }

        public static string ToLiteral(this string input)
        {
            StringBuilder literal = new StringBuilder(input.Length + 2);
            literal.Append("\"");
            foreach (char c in input)
            {
                int index = Array.IndexOf(CHARS, c);
                if (index >= 0)
                {
                    literal.Append('\\').Append(ESCAPES[index]);
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
