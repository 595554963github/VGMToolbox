using System;
using System.IO;

using VGMToolbox.util;

namespace snakebite
{
    class Program
    {
        static void Main(string[] args)
        {
            string inFilename;
            string outFilename;
            string startOffset;
            string endOffset;

            string fullInputPath;
            string fullOutputPath;

            long longStartOffset;
            long longEndOffset;

            if (args.Length < 3)
            {
                Console.WriteLine("使用方法: snakebite.exe <输入文件> <输出文件> <起始偏移量> <结束偏移量>");
                Console.WriteLine("   or: snakebite.exe <输入文件> <输出文件> <起始偏移量>");
                Console.WriteLine();
                Console.WriteLine("3 参数选项将从<开始偏移>读取到文件结束.");
            }
            else
            {

                inFilename = args[0];
                outFilename = args[1];
                startOffset = args[2];


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

                        if (args.Length > 3)
                        {
                            endOffset = args[3];

                            if (endOffset.StartsWith("0x"))
                            {
                                endOffset = endOffset.Substring(2);
                                longEndOffset = long.Parse(endOffset, System.Globalization.NumberStyles.HexNumber, null);
                            }
                            else
                            {
                                longEndOffset = long.Parse(endOffset, System.Globalization.NumberStyles.Integer, null);
                            }
                        }
                        else
                        {
                            longEndOffset = fs.Length;
                        }

                        long size = ((longEndOffset - longStartOffset) + 1);

                        ParseFile.ExtractChunkToFile(fs, longStartOffset, size, fullOutputPath);
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
