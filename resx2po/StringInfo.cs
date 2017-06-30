using System;
using System.Collections.Generic;
using System.Text;

namespace etosis.resx2po
{
    public class StringInfo
    {
        public readonly string Name;
        public readonly string Id;
        public readonly string Value;
        public readonly string Comment;

        public StringInfo(string name, string id, string value, string comment)
        {
            this.Name = name;
            this.Id = id;
            this.Value = value;
            this.Comment = comment;
        }

        public StringInfo WithoutValue()
        {
            return new StringInfo(
                Name,
                Id,
                null,
                Comment
            );
        }

        public StringInfo WithId(string id)
        {
            return new StringInfo(
                Name,
                id,
                Value,
                Comment
            );
        }

        public StringInfo WithPrefix(string prefix)
        {
            return new StringInfo(
                System.IO.Path.Combine(prefix, Name),
                Id,
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
                Id,
                Value,
                Comment
            );
        }
    }
}
