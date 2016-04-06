using System;
using System.Collections.Generic;
using System.Linq;
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

        public ResxFile(string key)
        {
            this._key = key;
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

        public static ResxFile Parse(string path, string key)
        {
            ResxFile resx = new ResxFile(key);

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
