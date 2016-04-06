using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace po2resx
{
    class Program
    {
        static void Main(string[] args)
        {
            string resxPath = args[0];
            string poPath = args[1];
            string defaultLanguage = args.Length > 2 ? args[2] : "en";

            // Find input files
            string[] paths = Directory.GetFiles(resxPath, "*.resx", SearchOption.AllDirectories);

            // Parse them
            List<ResxFile> files = new List<ResxFile>();
            foreach (string path in paths)
            {
                string key = MakeResxKey(path, inputPath);
                ResxFile resx = ResxFile.Parse(path, key);
                files.Add(resx);
            }

            // Create the PO file
            POFile po = new POFile(language);
            foreach (ResxFile resx in files)
            {
                resx.Strings.ToList().ForEach((x) => po.AddString(x.WithPrefix(resx.Key)));
            }

            // Write it
            po.Write(outpathPath);
        }
    }
}
