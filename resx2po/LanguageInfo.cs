using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etosis.resx2po
{
    public struct LanguageInfo
    {
        public readonly string Major;
        public readonly string Minor;

        public LanguageInfo(string major, string minor)
        {
            this.Major = major;
            this.Minor = minor;
        }

        public static bool operator == (LanguageInfo lhs, LanguageInfo rhs)
        {
            return lhs.Major == rhs.Major && object.Equals(lhs.Minor, rhs.Minor);
        }

        public static bool operator !=(LanguageInfo lhs, LanguageInfo rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return obj is LanguageInfo && ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            string s = Major;
            if (Minor != null)
                s += "-" + Minor;
            return s;
        }

        public static LanguageInfo Parse(string s)
        {
            LanguageInfo? lang = TryParse(s);
            if (lang == null)
                throw new ArgumentException("Invalid language: " + s);
            return lang.Value;
        }

        public static LanguageInfo? TryParse(string s)
        { 
            string[] parts = s.Split('_', '-');
            if (parts.Length == 0 || parts.Length > 2)
                return null;
            string major = parts[0].ToLower();
            string minor = parts.Length > 1 ? parts[1].ToLower() : null;
            return new LanguageInfo(major, minor);
        }
    }
}
