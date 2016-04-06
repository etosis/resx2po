using System;
using System.Collections.Generic;
using System.Text;

namespace etosis.resx2po
{
    public class StringInfo
    {
        public readonly string Name;
        public readonly string Value;
        public readonly string Comment;

        public StringInfo(string name, string value, string comment)
        {
            this.Name = name;
            this.Value = value;
            this.Comment = comment;
        }

        public StringInfo WithPrefix(string prefix)
        {
            return new StringInfo(
                System.IO.Path.Combine(prefix, Name),
                Value, 
                Comment
            );
        }

        public StringInfo WithoutPrefix(string prefix)
        {
            if (!Name.StartsWith(prefix))
                throw new ArgumentException("Name does not start with prefix");
            return new StringInfo(
                Name.Substring(prefix.Length),
                Value,
                Comment
            );
        }
    }
}
