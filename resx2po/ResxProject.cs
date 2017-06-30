using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public class ResxProject : ResxFileSetProvider
    {
        private readonly Project _project;

        public ResxProject(FileInfo projectFile, LanguageInfo defaultLanguage)
        :
        base(projectFile.Directory, defaultLanguage)
        {
            // TODO: make toolsversion a configuration option
            Dictionary < string, string> options = new Dictionary<string, string>();
            options.Add("VisualStudioVersion", "14.0");
            _project = new Project(projectFile.FullName, options, "14.0");
        }

        public override void Scan(ResxFileSet files)
        {
            foreach (ProjectItem item in _project.Items.Where(i => i.ItemType == "EmbeddedResource"))
            {
                files.AddFile(new FileInfo(Path.Combine(RootDirectory.FullName, item.EvaluatedInclude)), this, item);
            }
        }

        public override void OnTranslationsWritten()
        {
            if (_project.IsDirty)
            {
                _project.Save();
            }
        }

        public override void OnTranslationWritten(ResxFile resx, object tag)
        {
            string path = resx.Key;
            // Strip off project part of key
            // TODO: remove all these key hacks
            string baseName = Path.GetFileName(_project.DirectoryPath);
            if (path.StartsWith(baseName))
                path = path.Substring(baseName.Length + 1);

            // Check if the file is already present
            if (_project.Items.Any(i => i.EvaluatedInclude == path))
                return;

            ProjectItem item = (ProjectItem)tag;

            // Grab the metadata from the original item
            var meta = item.Metadata.Select(i => new KeyValuePair<string, string>(i.Name, i.UnevaluatedValue));

            // And add it
            _project.AddItem(item.ItemType, path, meta);
        }
    }
}
