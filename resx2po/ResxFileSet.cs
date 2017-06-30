using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public class ResxFileSet : IEnumerable<ResxFile>
    {
        public readonly LanguageInfo DefaultLanguage;

        private class ResxLanguageSet
        {
            public readonly ResxFile OriginalFile;
            public readonly object Tag;
            public readonly ResxFileSetProvider Provider;
            public readonly Dictionary<LanguageInfo, ResxFile> Translations = new Dictionary<LanguageInfo, ResxFile>();

            public ResxLanguageSet(ResxFile originalFile, ResxFileSetProvider provider, object tag)
            {
                this.OriginalFile = originalFile;
                this.Provider = provider;
                this.Tag = tag;
            }

            public ResxFile GetLanguage(LanguageInfo language)
            {
                // If the original language is specified, still return a translated instance.
                // Don't modify the original file with translations.

                ResxFile resx;
                if (!Translations.TryGetValue(language, out resx))
                {
                    string path = OriginalFile.Key + "." + language + ".resx";
                    resx = new ResxFile(path, language);
                    Translations.Add(language, resx);
                }
                return resx;
            }
        }

        private readonly Dictionary<string, ResxLanguageSet> _files = new Dictionary<string, ResxLanguageSet>();

        public ResxFileSet(LanguageInfo defaultLanguage)
        {
            this.DefaultLanguage = defaultLanguage;
        }

        public void AddFile(FileInfo file, ResxFileSetProvider provider, object tag)
        {
            if (ResxFile.IsSourceFile(file))
            {
                string key = MakeResxKey(file.FullName, provider.RootDirectory.FullName);

                // Parse and add it
                System.Diagnostics.Trace.WriteLine("PARSING: " + file);
                ResxFile resx = ResxFile.Parse(file, key, DefaultLanguage);
                _files.Add(key, new ResxLanguageSet(resx, provider, tag));
            }
        }

        private static string MakeResxKey(string path, string inputPath)
        {
            string s;
            if (!path.StartsWith(inputPath))
            {
                s = path;
            }
            else
            {
                int length = inputPath.Length;
                if (!inputPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    ++length;
                s = path.Substring(length);
                s = Path.Combine(Path.GetFileName(inputPath), s);
            }

            return Path.ChangeExtension(s, null);
        }

        public int Count { get { return _files.Count; } }
        public bool IsEmpty { get { return Count == 0; } }

        #region Translations

        public ResxFile GetTranslation(string key, LanguageInfo language)
        {
            ResxLanguageSet baseFile;
            if (!_files.TryGetValue(key, out baseFile))
                return null;
            return baseFile.GetLanguage(language);
        }

        public void WriteTranslations()
        {
            HashSet<ResxFileSetProvider> providers = new HashSet<ResxFileSetProvider>();
            foreach (ResxLanguageSet languageSet in _files.Values)
            {
                foreach (ResxFile resx in languageSet.Translations.Values)
                {
                    System.Diagnostics.Trace.WriteLine("GENERATING RESX: " + resx.Key);

                    ResxFileSetProvider provider = languageSet.Provider;
                    resx.Write(Path.Combine(Path.GetDirectoryName(provider.RootDirectory.FullName), resx.Key));
                    provider.OnTranslationWritten(resx, languageSet.Tag);
                    providers.Add(provider);
                }
            }

            foreach(ResxFileSetProvider provider in providers)
                provider.OnTranslationsWritten();
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<ResxFile> GetEnumerator()
        {
            return _files.Values.Select(i => i.OriginalFile).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _files.Values.Select(i => i.OriginalFile).GetEnumerator();
        }

        #endregion
    }
}
