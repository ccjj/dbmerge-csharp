using System;
using System.Collections.Generic;

namespace DbCombineWpf
{
    static class WriteLog
    {
        /// <summary>
        /// Writes a log-file on Desktop/dblog with the in the args specified list in
        /// the specified filename
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fileName"></param>
        public static void Write(List<string> text, string fileName)
        {

            string dPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dPath += "/dbLog/";
            System.IO.Directory.CreateDirectory(dPath);
            string fPath = dPath + fileName + ".txt";
            System.IO.File.WriteAllLines(fPath, text);

        }
    }
}
