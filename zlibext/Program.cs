using System;
using System.IO;
using VGMToolbox.util;

namespace zlibext
{
    class Program
    {
        static void Main(string[] args)
        {
            string inFilename;
            string outFilename;
            string startOffset;

            string fullInputPath;
            string fullOutputPath;

            long longStartOffset;

            if (args.Length < 2)
            {
                Console.WriteLine("用法:zlibext.exe <输入文件> <输出文件> <起始偏移量>");
                Console.WriteLine("   或: zlibext.exe <输入文件> <输出文件>");
                Console.WriteLine();
                Console.WriteLine("2 参数选项将<开始偏移>设置为 0.");
            }
            else
            {
                inFilename = args[0];
                outFilename = args[1];

                if (args.Length == 2)
                {
                    startOffset = "0";
                }
                else
                {
                    startOffset = args[2];
                }

                fullInputPath = Path.GetFullPath(inFilename);
                fullOutputPath = Path.GetFullPath(outFilename);

                if (File.Exists(fullInputPath))
                {
                    using (FileStream fs = File.OpenRead(fullInputPath))
                    {
                        if (startOffset.StartsWith("0x"))
                        {
                            startOffset = startOffset.Substring(2);
                            longStartOffset = long.Parse(startOffset, System.Globalization.NumberStyles.HexNumber, null);
                        }
                        else
                        {
                            longStartOffset = long.Parse(startOffset, System.Globalization.NumberStyles.Integer, null);
                        }

                        if (longStartOffset > fs.Length)
                        {
                            Console.WriteLine(String.Format("抱歉,起始偏移量大于整个文件：{0}", fs.Length.ToString()));
                        }
                        else
                        {
                            try
                            {
                                CompressionUtil.DecompressZlibStreamToFile(fs, fullOutputPath, longStartOffset);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(String.Format("无法解压<{0}>.", fullInputPath));
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("文件未找到:" + fullInputPath);
                }
            }
        }
    }
}
