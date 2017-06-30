using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public class ResxSolution : ResxFileSetProvider
    {
        private const string QUOTED_GUID = @"\""[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?\""";
        private const string QUOTED_STRING = @"""(?:\\.|[^""\\])*""";
        private const string QUOTED_STRING_GROUP = @"""((?:\\.|[^""\\])*)""";
        private const string PATTERN = @"^Project\(" + QUOTED_GUID + @"\)\s*=\s*" + QUOTED_STRING + @"\s*,\s*" + QUOTED_STRING_GROUP + @"\s*,\s*" + QUOTED_GUID + @"\s*$";

        private readonly FileInfo _solutionFile;

        public ResxSolution(FileInfo file, LanguageInfo defaultLanguage)
        :
        base(file.Directory, defaultLanguage)
        {
            this._solutionFile = file;
        }

        public override void Scan(ResxFileSet files)
        {
            string everything = File.ReadAllText(_solutionFile.FullName, Encoding.UTF8);
            var matches = Regex.Matches(everything, PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                string projectPath = match.Groups[1].Value;
                if (Path.GetExtension(projectPath) == ".csproj")
                {
                    string fullPath = Path.Combine(RootDirectory.FullName, projectPath);
                    ResxProject project = new ResxProject(new FileInfo(fullPath), files.DefaultLanguage);
                    project.Scan(files);
                }
            }
        }
    }
}
