using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VGMToolbox.format.util;


namespace xsfrecmp
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename;
            string compressionLevel;
            int compressionLevelValue;
            string outputPath = null;

            XsfRecompressStruct xsfStruct;

            if ((args.Length < 1) || (args.Length > 2))
            {
                usage();
            }
            else
            {
                filename = Path.GetFullPath(args[0]);

                if (args.Length == 2)
                {
                    compressionLevel = args[1];
                }
                else
                {
                    compressionLevel = "0";
                }

                try
                {
                    if (!int.TryParse(compressionLevel, out compressionLevelValue) ||
                        (compressionLevelValue > 9) ||
                        (compressionLevelValue < 0))
                    {
                        Console.WriteLine("解析压缩级别时出错.输入必须是0到9之间的整数.");
                    }
                    else if (!File.Exists(filename))
                    {
                        Console.WriteLine(String.Format("错误：找不到输入文件<{0}>", filename));
                    }
                    else if (XsfUtil.GetXsfFormatString(filename) == null)
                    {
                        Console.WriteLine("错误：输入文件似乎不是xSF文件.");
                    }
                    else
                    {

                        xsfStruct = new XsfRecompressStruct();
                        xsfStruct.CompressionLevel = compressionLevelValue;
                        outputPath = XsfUtil.ReCompressDataSection(filename, xsfStruct);

                        if (String.IsNullOrEmpty(outputPath))
                        {
                            Console.WriteLine("完成：没有要压缩的数据部分.");
                        }
                        else
                        {
                            Console.WriteLine(String.Format("完成：重新压缩并输出到<{0}>", outputPath));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("错误{0}", ex.Message));
                }
            }
        }

        static void usage()
        {
            Console.WriteLine("xsfrecmp.exe input_file [compression level]");
            Console.WriteLine(" input_file：要重新压缩的文件");
            Console.WriteLine("压缩级别：要使用的zlib压缩级别（0-9）.如果不包含参数,将使用 tore（0）.");

        }
    }
}
