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
        private static void Usage()
        {
            Console.Error.WriteLine("Usage: resx2po [-r] <resx directory> <po directory> [default language]");
            System.Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            // Parse command line
            int index = 0;
            string resxPath = null;
            string poPath = null;
            LanguageInfo defaultLanguage = LanguageInfo.Parse("en");
            bool reverse = false;
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("-"))
                {
                    if (args[i].Equals("-r"))
                    {
                        reverse = true;
                    }
                    else
                    {
                        Usage();
                    }
                }
                else
                {
                    switch(index)
                    {
                        case 0: resxPath = args[i]; break;
                        case 1: poPath = args[i]; break;
                        case 2: defaultLanguage = LanguageInfo.Parse(args[i]); break;
                        default:
                            Usage();
                            break;
                    }
                    ++index;
                }
            }
            if (resxPath == null || poPath == null)
                Usage();


            if (reverse)
            {
                Dictionary<string, ResxFile> resxByPath = new Dictionary<string, ResxFile>();

                // Find all the PO files
                List<POFile> poFiles = LoadPOFiles(poPath, defaultLanguage);
                foreach(POFile poFile in poFiles)
                {
                    System.Diagnostics.Trace.WriteLine("MERGING PO: " + poFile);
                    foreach(StringInfo info in poFile.Strings)
                    {
                        string id = Path.GetFileName(info.Name);
                        string path = Path.GetDirectoryName(info.Name) + "." + poFile.Language + ".resx";
                        System.Diagnostics.Trace.WriteLine("          : " + id + " in " + path);

                        // Get the in-memory resx file
                        ResxFile resx;
                        if (!resxByPath.ContainsKey(path))
                        {
                            resx = new ResxFile(path, poFile.Language);
                            resxByPath.Add(path, resx);
                        }
                        else resx = resxByPath[path];

                        // Add the string
                        resx.AddString(info.WithoutPrefix(Path.GetDirectoryName(info.Name) + "\\"));
                    }
                }

                // Write the .resx files
                foreach (ResxFile resx in resxByPath.Values)
                {
                    System.Diagnostics.Trace.WriteLine("GENERATING RESX: " + resx);
                    resx.Write(Path.Combine(resxPath, resx.Key));
                }
            }
            else
            {
                // Find resx files
                List<ResxFile> resxFiles = LoadResxFiles(resxPath, defaultLanguage);

                // Create the PO file in the default language
                POFile po = new POFile(defaultLanguage);
                foreach (ResxFile resx in resxFiles)
                {
                    System.Diagnostics.Trace.WriteLine("MERGING RESX: " + resx);
                    resx.Strings.ToList().ForEach((x) => po.AddString(x.WithPrefix(resx.Key)));
                }
                po.Write(poPath);
            }
        }

        private static List<POFile> LoadPOFiles(string poPath, LanguageInfo defaultLanguage)
        {
            string[] poPaths = Directory.GetFiles(poPath, "*.po", SearchOption.AllDirectories);
            List<POFile> files = new List<POFile>();
            foreach (string path in poPaths)
            {
                POFile po = new POParser(path).POFile;
                if (po.Language != defaultLanguage)
                {
                    files.Add(po);
                }
            }
            return files;
        }

        private static List<ResxFile> LoadResxFiles(string resxPath, LanguageInfo defaultLanguage)
        {
            if (File.Exists(resxPath))
            {
                SolutionParser parser = new SolutionParser();
            }
            else
            {
                string[] resxPaths = Directory.GetFiles(resxPath, "*.resx", SearchOption.AllDirectories);
                List<ResxFile> files = new List<ResxFile>();
                foreach (string path in resxPaths)
                {
                    string key = MakeResxKey(path, resxPath);
                    string ext = Path.GetExtension(key);

                    // Make sure it's not a translation
                    if (ext.StartsWith(".") && LanguageInfo.TryParse(ext.Substring(1)) != null)
                        continue;

                    ResxFile resx = ResxFile.Parse(path, key, defaultLanguage);
                    files.Add(resx);
                }
                return files;
            }
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
