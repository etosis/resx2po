﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Xml.Linq;

namespace etosis.resx2po
{
    public class ResxFile : ResourceFile
    {
        private readonly string _key;

        public ResxFile(string key, LanguageInfo language)
        :
        base(language)
        {
            this._key = key;
        }

        public string Key
        {
            get { return _key; }
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
