using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public class POParser
    {
        private class Section
        {
            private enum State
            {
                Global,
                Context,
                Id,
                Value,
                Comment
            }
            private string lines = "";
            private State state = State.Global;
            private string context = "";
            private string id = "";
            private string value = "";
            private string comment = "";

            public void Add(string line)
            {
                lines += line;
                lines += "\n";

                if (state != State.Global)
                {
                    if (line.StartsWith("\""))
                    {
                        switch (state)
                        {
                            case State.Context:
                                context += line.FromLiteral();
                                break;
                            case State.Id:
                                id += line.FromLiteral();
                                break;
                            case State.Value:
                                value += line.FromLiteral();
                                break;
                        }
                        return;
                    }
                    else state = State.Global;
                }

                if (line.StartsWith("#"))
                {
                    if (line.StartsWith("#. "))
                        comment += line.Substring(3);
                    if (line.StartsWith("#."))
                        comment += line.Substring(2);
                }
                else if (line.StartsWith("msgctxt"))
                {
                    context = line.Substring(7).Trim().FromLiteral();
                    state = State.Context;
                }
                else if (line.StartsWith("msgid"))
                {
                    id = line.Substring(5).Trim().FromLiteral();
                    state = State.Id;
                }
                else if (line.StartsWith("msgstr"))
                {
                    value = line.Substring(6).Trim().FromLiteral();
                    state = State.Value;
                }
            }

            public Section Process(POParser parser)
            {
                if (lines.Length == 0)
                    return this;

                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(context))
                {
                    StringInfo info = new StringInfo(context, value, comment);
                    parser.POFile.AddString(info);
                }
                state = State.Global;
                return new Section();
            }
        }

        public readonly POFile POFile;

        private enum ParseState
        {
            NoEntry,
            Comment,
            Values
        }
        private ParseState state = ParseState.NoEntry;
        private Section currentSection = new Section();

        public POParser(string path)
        {
            LanguageInfo language = LanguageInfo.Parse(Path.GetFileNameWithoutExtension(path));
            System.Diagnostics.Trace.WriteLine("POParser: " + language);
            POFile = new POFile(language);
            using (StreamReader reader = File.OpenText(path))
            {
                for (;;)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();
                    if (line.Length == 0)
                    {
                        state = ParseState.NoEntry;
                        currentSection = currentSection.Process(this);
                        continue;
                    }

                    if (line.StartsWith("#"))
                    {
                        if (state != ParseState.Comment)
                            currentSection = currentSection.Process(this);
                        state = ParseState.Comment;
                        currentSection.Add(line);
                    }
                    else
                    {
                        if (state == ParseState.NoEntry)
                            currentSection = currentSection.Process(this);
                        state = ParseState.Values;
                        currentSection.Add(line);
                    }
                }
            }
        }
    }
}
