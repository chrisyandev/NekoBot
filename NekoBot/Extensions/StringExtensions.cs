using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot.Extensions
{
    public static class StringExtensions
    {
        public static string Capitalize(this string str)
        {
            if (str.Length > 0)
            {
                if (str.Length == 1)
                {
                    return str.ToUpper();
                }
                else
                {
                    return char.ToUpper(str[0]) + str[1..];
                }
            }
            return str;
        }
    }
}
