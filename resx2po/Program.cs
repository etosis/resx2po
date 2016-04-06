using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    class Program
    {
        static void Main(string[] args)
        {
            string language = "en";
            string inputPath = Directory.GetCurrentDirectory();

            // Find input files
            string[] paths = Directory.GetFiles(inputPath, "*.resx", SearchOption.AllDirectories);

            // Parse them
            List<ResxFile> files = new List<ResxFile>();
            foreach(string path in paths)
            {
                string key = MakeResxKey(path, inputPath);
                ResxFile resx = ResxFile.Parse(path, key);
                files.Add(resx);
            }

            // Create the PO file
            POFile po = new POFile(language);
            foreach(ResxFile resx in files)
            {
                resx.Strings.ToList().ForEach((x) => po.AddString(x.WithPrefix(resx.Key)));
            }

            // Write it
            po.Write(inputPath);
        }

        private static string MakeResxKey(string path, string inputPath)
        {
            string s;
            if (!path.StartsWith(inputPath))
                s = path;
            else
            {
                int length = inputPath.Length;
                if (!inputPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    ++length;
                s = path.Substring(length);
            }

            return Path.ChangeExtension(s, null);
        }
    }
}
