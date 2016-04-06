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
        public readonly LanguageInfo Language;
        public string LanguageString
        {
            get
            {
                string s = Language.Major;
                if (Language.Minor != null)
                    s += Language.Minor.ToUpper();
                return s;
            }
        }
        private readonly Dictionary<string, StringInfo> _strings = new Dictionary<string, StringInfo>();

        public POFile(LanguageInfo language)
        {
            this.Language = language;
        }

        public override string ToString()
        {
            return Language + "(" + _strings.Count + ")";
        }

        public void AddString(StringInfo info)
        {
            _strings.Add(info.Name, info);
        }

        public IEnumerable<StringInfo> Strings
        {
            get { return _strings.Values; }
        }

        public void Write(string directory)
        {
            string path = Path.ChangeExtension(Path.Combine(directory, LanguageString), "po");

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
                    writer.WriteLine("msgctxt " + info.Name.ToLiteral());
                    writer.WriteLine("msgid " + info.Value.ToLiteral());
                    writer.WriteLine("msgstr " + info.Value.ToLiteral());
                    writer.WriteLine();
                }
            }
        }
    }
}
