using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace etosis.resx2po
{
    public class POFile
    {
        private readonly string _language;
        private readonly Dictionary<string, StringInfo> _strings = new Dictionary<string, StringInfo>();

        public POFile(string language)
        {
            this._language = language;
        }

        public void AddString(StringInfo info)
        {
            _strings.Add(info.Name, info);
        }

        public void Write(string directory)
        {
            string path = Path.ChangeExtension(Path.Combine(directory, _language), "po");

            using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                foreach(StringInfo info in _strings.Values)
                {
                    writer.WriteLine("#: " + info.Name);
                    if (info.Comment != null)
                    {
                        foreach (string line in info.Comment.Split('\n'))
                            writer.WriteLine("#. " + line);
                    }
                    writer.WriteLine("#, csharp-format");
                    writer.WriteLine("msgctxt " + ToLiteral(info.Name));
                    writer.WriteLine("msgid " + ToLiteral(info.Value));
                    writer.WriteLine("msgstr " + ToLiteral(info.Value));
                    writer.WriteLine();
                }
            }
        }

        static string ToLiteral(string input)
        {
            StringBuilder literal = new StringBuilder(input.Length + 2);
            literal.Append("\"");
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }
    }
}
