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

            public void Add(string line, int lineNumber)
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
                                context += line.FromLiteral(lineNumber, 1);
                                break;
                            case State.Id:
                                id += line.FromLiteral(lineNumber, 1);
                                break;
                            case State.Value:
                                value += line.FromLiteral(lineNumber, 1);
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
                    else if (line.StartsWith("#."))
                        comment += line.Substring(2);
                }
                else if (
                    HandleLine(line, lineNumber, "msgctxt", State.Context)
                        ||
                    HandleLine(line, lineNumber, "msgid", State.Id)
                        ||
                    HandleLine(line, lineNumber, "msgstr", State.Value)
                )
                {
                    // Already handled in condition   
                }
            }

            private bool HandleLine(string line, int lineNumber, string prefix, State newState)
            {
                if (!line.StartsWith(prefix))
                    return false;

                // Subtract the prefix
                int column = 1 + prefix.Length;
                line = line.Substring(prefix.Length);

                // And starting space to update the column
                string ltrim = line.TrimStart();
                column += line.Length - ltrim.Length;

                // Trailing space
                line = ltrim.TrimEnd();

                // Parse
                line = line.FromLiteral(lineNumber, column);

                // Update state
                switch(newState)
                {
                    case State.Context:
                        context = line;
                        break;
                    case State.Id:
                        id = line;
                        break;
                    case State.Value:
                        value = line;
                        break;
                }
                state = newState;

                return true;
            }

            public Section Process(POParser parser)
            {
                if (lines.Length == 0)
                    return this;

                StringInfo info = new StringInfo(context, id, value, comment);
                parser.POFile.AddString(info);

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
                int lineNumber = 0;
                for (;;)
                {
                    string line = reader.ReadLine();
                    ++lineNumber;
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
                        currentSection.Add(line, lineNumber);
                    }
                    else
                    {
                        if (state == ParseState.NoEntry)
                            currentSection = currentSection.Process(this);
                        state = ParseState.Values;
                        currentSection.Add(line, lineNumber);
                    }
                }
            }

            if (state != ParseState.NoEntry)
            {
                currentSection.Process(this);
            }
        }
    }
}
