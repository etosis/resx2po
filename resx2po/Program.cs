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
            Console.Error.WriteLine("Usage: resx2po [-i [-n]] [-t name] [-u] <source path> <po directory> [default language]");
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine("  -i      : Import from .po files to VS. Excludes other options except -n.");
            Console.Error.WriteLine("  -n      : Import empty strings.");
            Console.Error.WriteLine("  -t name : Generate <name>.pot file.");
            Console.Error.WriteLine("  -u      : Update existing .po files.");
            System.Environment.Exit(1);
        }

        static string resxPath = null;
        static string poPath = null;
        static LanguageInfo defaultLanguage = LanguageInfo.Parse("en");
        static bool import = false;
        static bool importEmpty = false;
        static bool update = false;
        static string templateName = null;

        static void Main(string[] args)
        {
            // Parse command line
            int index = 0;
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("-"))
                {
                    if (args[i].Equals("-i"))
                        import = true;
                    else if (args[i].Equals("-n"))
                        importEmpty = true;
                    else if (args[i].Equals("-u"))
                        update = true;
                    else if (args[i].Equals("-t"))
                    {
                        if (i >= args.Length - 1)
                            Usage();
                        ++i;
                        templateName = args[i];
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

            if (import)
            {
                if (templateName != null || update)
                    Usage();
            }
            else
            {
                if (importEmpty)
                    Usage();
            }

            try
            {
                if (Path.GetExtension(resxPath) == ".pot")
                {
                    if (!update)
                        Usage();
                    Pot2PoProcess();
                }
                else
                    Resx2PoProcess();
            }
            catch(UsageException e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                Console.Error.WriteLine(e.Message);
                System.Environment.Exit(1);
            }
        }

        private static void Pot2PoProcess()
        {
            POFile pot = new POParser(resxPath).POFile;
            UpdatePOs(pot, poPath, defaultLanguage);
        }

        private static void Resx2PoProcess()
        {
            // Load the resx files
            ResxFileSet resxFiles = LoadResxFiles(resxPath, defaultLanguage);
            if (resxFiles.IsEmpty)
            {
                throw new UsageException("No resx files found");
            }

            if (import)
            {
                ProcessImport(resxFiles, poPath, defaultLanguage);
            }
            else
            {
                // Create the .pot if requested.
                if (templateName != null)
                    Process(resxFiles, poPath, defaultLanguage, templateName);

                // Always create the default as well.
                Process(resxFiles, poPath, defaultLanguage, null);

                // Update existing po files if request
                if (update)
                {
                    // TODO: this is already calculated
                    POFile po = new POFile(defaultLanguage);
                    foreach (ResxFile resx in resxFiles)
                    {
                        resx.Strings.ToList().ForEach((x) => po.AddString(x.WithPrefix(resx.Key)));
                    }
                    UpdatePOs(po, poPath, defaultLanguage);
                }
            }

        }

        private static void ProcessImport(ResxFileSet resxFiles, string poPath, LanguageInfo defaultLanguage)
        {
            // Find all the PO files
            List<POFile> poFiles = LoadPOFiles(poPath, defaultLanguage);
            foreach (POFile poFile in poFiles)
            {
                System.Diagnostics.Trace.WriteLine("MERGING PO: " + poFile);
                foreach (StringInfo info in poFile.Strings)
                {
                    if (string.IsNullOrEmpty(info.Name))
                        continue;
                    if (!importEmpty && string.IsNullOrEmpty(info.Value))
                        continue;

                    string id = Path.GetFileName(info.Name);
                    string resxBaseName = Path.GetDirectoryName(info.Name);
                    ResxFile resx = resxFiles.GetTranslation(resxBaseName, poFile.Language);
                    if (resx != null)
                    {
                        System.Diagnostics.Trace.WriteLine("          : " + id + " in " + resx.Key);
                        // Add the string
                        resx.AddString(info.WithoutPrefix(Path.GetDirectoryName(info.Name) + "\\"));
                    }
                }
            }

            // Write the .resx files
            resxFiles.WriteTranslations();
        }

        private static void Process(ResxFileSet resxFiles, string poPath, 
                                    LanguageInfo defaultLanguage, string templateName)
        {
            // Create the PO file in the default language
            POFile po = new POFile(defaultLanguage);
            foreach (ResxFile resx in resxFiles)
            {
                System.Diagnostics.Trace.WriteLine("MERGING RESX: " + resx);
                resx.Strings.ToList().ForEach((x) => po.AddString(x.WithPrefix(resx.Key)));
            }

            try
            {
                // Create the target directory
                Directory.CreateDirectory(poPath);

                // Write the po
                po.Write(poPath, templateName);
            }
            catch(Exception e)
            {
                throw new UsageException("Unable to write output file " + poPath + ": " + e.Message, e);
            }
        }

        private static void UpdatePOs(POFile po, string poPath,
                                      LanguageInfo defaultLanguage)
        {
            foreach(string file in Directory.GetFiles(poPath, "*.po"))
            {
                if (po.Filename.ToLower() == Path.GetFileName(file).ToLower())
                    continue;

                try
                {
                    POFile translation = new POParser(file).POFile;
                    foreach (StringInfo info in po.Strings)
                    {
                        translation.UpdateString(info);
                    }
                    translation.Write(poPath, null);
                }
                catch (Exception e)
                {
                    throw new UsageException("Unable to write output file to " + file + ": " + e.Message, e);
                }
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

        /// <summary>
        /// Loads the resx files from the specified input.
        /// </summary>
        /// <param name="inputPath">The path containing the inputs. This can be a directory, in which case
        /// all resx files will be scanned. It can also be a solution or project file, in which case the contents
        /// will be scanned. In this case, the project files will also be updated.</param>
        /// <param name="defaultLanguage">The default language</param>
        /// <returns>The resx files</returns>
        private static ResxFileSet LoadResxFiles(string inputPath, LanguageInfo defaultLanguage)
        {
            return CreateResxProvider(inputPath, defaultLanguage).Scan();
        }

        private static ResxFileSetProvider CreateResxProvider(string inputPath, LanguageInfo defaultLanguage)
        {
            if (File.Exists(inputPath))
            {
                // This is a file, it can be either a solution or project file
                string extension = Path.GetExtension(inputPath);
                if (extension == ".sln")
                    return new ResxSolution(new FileInfo(inputPath), defaultLanguage);
                else if (extension == ".csproj")
                    return new ResxProject(new FileInfo(inputPath), defaultLanguage);
                else
                    throw new Exception("Unknown input file type: " + extension);
            }
            else
            {
                return new ResxDirectory(new DirectoryInfo(inputPath), defaultLanguage);
            }
        }
    }
}
