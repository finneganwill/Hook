using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hook.Common
{
    public class ColNamePretxUtil
    {
        public static string ConvertUnderscoreToCamelCase(string input)
        {
            StringBuilder result = new StringBuilder();
            bool capitalizeNext = true;

            foreach (char c in input)
            {
                if (c == '_')
                {
                    capitalizeNext = true;
                }
                else
                {
                    result.Append(capitalizeNext ? char.ToUpper(c) : char.ToLower(c));
                    capitalizeNext = false;
                }
            }
            return result.ToString();
        }

        public static string RemovePrefix(string input, string prefix)
        {
            if (input.StartsWith(prefix))
            {
                return input.Substring(prefix.Length);
            }
            return input;
        }
    }
}
