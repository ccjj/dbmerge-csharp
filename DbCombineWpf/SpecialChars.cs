using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCombineWpf
{
    static class SpecialChars
    {


        /// <summary>
        /// Removes all special chars, not accepted by the database, from the string, and returns the cleaned string.
        /// Spaces are inserted instad special chars.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string removeSpecialChars(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == 'ß' ||
                    c == 'Ä' || c == 'ä' || c == 'Ö' || c == 'ö' || c == 'Ü' || c == 'ü')
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append(' ');
                }
            }
            return builder.ToString();
        }

    }
}
