using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Xml.Linq;

namespace etosis.resx2po
{
    public class ResxFile
    {
        /// <summary>
        /// Strings indexed by key
        /// </summary>
        private readonly Dictionary<string, StringInfo> _strings = new Dictionary<string, StringInfo>();

        private readonly string _key;
        public readonly LanguageInfo Language;

        public ResxFile(string key, LanguageInfo language)
        {
            this._key = key;
            this.Language = language;
        }

        public void AddString(StringInfo info)
        {
            _strings.Add(info.Name, info);
        }

        public IEnumerable<StringInfo> Strings
        {
            get { return _strings.Values; }
        }

        public string Key
        {
            get { return _key; }
        }

        public override string ToString()
        {
            string s = Key;
            if (Language != null)
                s += ":" + Language;
            s += "(" + _strings.Count + ")";
            return s;
        }

        public void Write(string path)
        {
            using (var fs = File.Create(path))
            using (var writer = new ResXResourceWriter(fs))
            {
                foreach (StringInfo info in Strings)
                {
                    writer.AddResource(info.Name, info.Value);
                    if (!string.IsNullOrEmpty(info.Comment))
                        writer.AddMetadata(info.Name, info.Comment);
                }
            }
        }

        public static ResxFile Parse(string path, string key, LanguageInfo language)
        {
            ResxFile resx = new ResxFile(key, language);

            var doc = XDocument.Load(path);
            var root = doc.Root;
            if (root == null)
                return null;

            var ns = XNamespace.Get(string.Empty);
            var items = root
                .Elements(ns + "data")
                .Where(x => x.Attribute("type") == null)
                .ToList();

            foreach (var item in items)
            {
                string name = item.Attribute("name")?.Value;
                string value = item.Element("value")?.Value;
                string comment = item.Element("comment")?.Value;

                if (name == null || value == null)
                    continue;

                if (name.StartsWith(">>"))
                    continue;

                StringInfo info = new StringInfo(name, value, comment);
                resx.AddString(info);
            }

            return resx;
        }
    }
}
