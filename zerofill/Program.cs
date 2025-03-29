using System;
using System.IO;

using VGMToolbox.util;

namespace zerofill
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
                Console.WriteLine("使用方法: zerofill.exe <输入文件> <输出文件> <起始偏移量> <结束偏移量>");
                Console.WriteLine("   或者: zerofill.exe <输入文件> <输出文件> <起始偏移量>");
                Console.WriteLine();
                Console.WriteLine("3参数选项将从<开始偏移>填充到文件结束.");
                Console.WriteLine(String.Format("当前唯一的限制是填充大小不能超过[{0}].", int.MaxValue.ToString()));
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
                        using (FileStream fs = File.OpenRead(fullInputPath))
                        {
                            longEndOffset = fs.Length;
                        }
                    }

                    long size = ((longEndOffset - longStartOffset) + 1);

                    if (size > (long)int.MaxValue)
                    {
                        Console.WriteLine(String.Format("抱歉,填充大小太大:{0}", size.ToString()));
                    }
                    else
                    {
                        try
                        {
                            File.Copy(fullInputPath, fullOutputPath, false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(String.Format("无法创建目标文件<{0}>:{1}", fullOutputPath, ex.Message));
                        }

                        FileUtil.ZeroOutFileChunk(fullOutputPath, longStartOffset, (int)size);
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
