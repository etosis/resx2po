using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public abstract class ResourceFile
    {
        /// <summary>
        /// Strings indexed by key
        /// </summary>
        private readonly Dictionary<string, StringInfo> _strings = new Dictionary<string, StringInfo>();

        private readonly LanguageInfo _language;

        protected ResourceFile(LanguageInfo language)
        {
            this._language = language;
        }

        public void AddString(StringInfo info)
        {
            _strings.Add(info.Name, info);
        }

        public IEnumerable<StringInfo> Strings
        {
            get { return _strings.Values; }
        }

        public LanguageInfo Language
        {
            get { return _language; }
        }

        public override string ToString()
        {
            string s = ""; // TODO
            if (Language != null)
                s += ":" + Language;
            s += "(" + _strings.Count + ")";
            return s;
        }

        public StringInfo this[string key]
        {
            get
            {
                return _strings[key];
            }
        }
    }
}
