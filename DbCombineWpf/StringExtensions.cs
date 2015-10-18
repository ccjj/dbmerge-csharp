using System;

namespace DbCombineWpf
{
    static class StringExtensions
    {



        /// <summary>
        /// Checks, if one string is contained in an other, and returns true if so
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }



    }
}
