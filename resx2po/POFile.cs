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
                    s += "_" + Language.Minor;
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

        public bool HasString(StringInfo info)
        {
            return _strings.ContainsKey(info.Name);
        }

        public void UpdateString(StringInfo info)
        {
            StringInfo current = null;
            if (_strings.TryGetValue(info.Name, out current))
            {
                // Already present, update id
                _strings[info.Name] = current.WithId(info.Id);
            }
            else
            {
                // Add an empty entry
                AddString(info.WithoutValue());
            }
        }

        public IEnumerable<StringInfo> Strings
        {
            get { return _strings.Values; }
        }

        public string Filename
        {
            get
            {
                return Path.ChangeExtension(LanguageString, "po");
            }
        }

        public void Write(string directory, string templateName)
        {
            string path;
            if (string.IsNullOrEmpty(templateName))
                path = Path.Combine(directory, Filename);
            else
                path = Path.ChangeExtension(Path.Combine(directory, templateName), "pot");

            System.Diagnostics.Trace.WriteLine(string.Format("WRITING PO: {0}, {1} -> {2}", directory, Filename, path));

            using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                foreach(StringInfo info in _strings.Values)
                {
                    if (!string.IsNullOrEmpty(info.Comment))
                    {
                        foreach (string line in info.Comment.Split('\n'))
                            writer.WriteLine("#. " + line);
                    }
                    if (!string.IsNullOrEmpty(info.Name))
                    {
                        writer.WriteLine("#: " + info.Name);
                        writer.WriteLine("#, csharp-format");
                        writer.WriteLine("msgctxt " + info.Name.ToLiteral());
                    }
                    writer.WriteLine("msgid " + info.Id.ToLiteral());
                    if (string.IsNullOrEmpty(templateName) && info.Value != null)
                        writer.WriteLine("msgstr " + info.Value.ToLiteral());
                    else
                        writer.WriteLine("msgstr \"\"");
                    writer.WriteLine();
                }
            }
        }
    }
}
