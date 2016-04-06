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
    }
}
