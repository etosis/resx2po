using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public class ResxDirectory : ResxFileSetProvider
    {
        public ResxDirectory(DirectoryInfo directory, LanguageInfo defaultLanguage)
        :
        base(directory, defaultLanguage)
        {
        }

        public override void Scan(ResxFileSet files)
        {
            FileInfo[] resxFiles = RootDirectory.GetFiles("*.resx", SearchOption.AllDirectories);
            foreach (FileInfo file in resxFiles)
                files.AddFile(file, this, null);
        }
    }
}
