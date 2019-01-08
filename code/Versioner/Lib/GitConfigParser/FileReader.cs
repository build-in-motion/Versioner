using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Versioner.Lib.GitConfigParser
{
    public class FileReader
    {
        public string FilePath { get; private set; }
        public string Contents { get; private set; }
        public string[] Lines { get; private set; }

        public FileReader(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(String.Format("File {0} does not exist", filePath));

            FilePath = filePath;
            var lines = new List<string>();
            using (var reader = new StreamReader(FilePath))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line.Replace("\t", ""));
            }

            Lines = lines.ToArray();
        }
    }
}
