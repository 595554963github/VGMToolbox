using System;
using System.IO;

using VGMToolbox.format.util;

namespace sdatext
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath;

            if (args.Length < 1)
            { 
                usage();
                return;
            }

            // get file path and check it exists
            Console.WriteLine("检查文件是否存在.");
            
            filePath = Path.GetFullPath(args[0]);

            if (!File.Exists(filePath))
            {
                Console.WriteLine(String.Format("<{0}>文件未找到.", filePath));
                return;
            }

            // open file and extract the sdat
            Console.WriteLine("提取SDAT.");
            
            try
            {
                string outputDir = SdatUtil.ExtractSdat(filePath);
                Console.WriteLine(String.Format("完成!SDAT提取到:{0}", outputDir));
            }
            catch (Exception _e)
            {
                Console.WriteLine(_e.Message);
            }            
        }

        private static void usage()
        {
            Console.WriteLine("sdatext sdatfile");
        }
    }
}
