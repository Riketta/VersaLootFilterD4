using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaLootFilterD4
{
    public static class StringExtensions
    {
        public static string TrimLowerCaseAndWhiteSpaces(this string str)
        {
            int i;
            for (i = 0; i < str.Length && (char.IsWhiteSpace(str[i]) || char.IsLower(str[i])); i++) ;

            int num = str.Length - 1;
            while (num >= i && (char.IsWhiteSpace(str[num]) || char.IsLower(str[i])))
                num--;

            return str.Substring(i, num - i + 1);
        }
    }
}
