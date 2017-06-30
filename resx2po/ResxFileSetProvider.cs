using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    abstract public class ResxFileSetProvider
    {
        public readonly DirectoryInfo RootDirectory;
        private readonly LanguageInfo _defaultLanguage;

        protected ResxFileSetProvider(DirectoryInfo directory, LanguageInfo defaultLanguage)
        {
            this.RootDirectory = directory;
            this._defaultLanguage = defaultLanguage;
        }

        public ResxFileSet Scan()
        {
            ResxFileSet files = new ResxFileSet(_defaultLanguage);
            Scan(files);
            return files;
        }

        public abstract void Scan(ResxFileSet files);

        virtual public void OnTranslationWritten(ResxFile originalFile, object tag)
        {

        }

        virtual public void OnTranslationsWritten()
        {

        }
    }
}
