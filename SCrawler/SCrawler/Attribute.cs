using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCrawler
{
    public class PropertyAttribute
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public static class Helpers
    {
        public static string GetValue(this List<PropertyAttribute> attrList, string key)
        {
            var result = attrList.Where(n => n.Key.Equals(key)).Select(n => n.Value).SingleOrDefault();

            return result == null ? String.Empty : result;

        }
    }
}
