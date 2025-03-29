using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using VGMToolbox.format.util;
using VGMToolbox.util;

namespace sdatfind
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                usage();
                return;
            }
            
            string filePath = Path.GetFullPath(args[0]);

            // get file path and check it exists
            Console.WriteLine("检查文件是否存在.");

            filePath = Path.GetFullPath(args[0]);

            if (!File.Exists(filePath))
            {
                Console.WriteLine(String.Format("<{0}>文件未找到.", filePath));
                return;
            }

            // open file and extract the sdat
            Console.WriteLine("提取SDATs.");

            try
            {
                string[] outputPaths = SdatUtil.ExtractSdatsFromFile(filePath, "_sdatfind");
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("错误处理<{0}>.收到错误: ", filePath) + ex.Message);
            }                                
        }

        private static void usage()
        {
            Console.WriteLine("sdatfind.exe 文件名");
        }
    }
}
