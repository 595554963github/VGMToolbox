using System;
using System.IO;
using VGMToolbox.util;

namespace manakut
{
    class Program
    {
        static void Main(string[] args)
        {
            const string LBA_SWITCH = "/LBA";
            const string MULTIPLIER_SWITCH = "/M";

            string inFilename;
            string outFilename;
            string startOffset;
            string cutSize;

            string fullInputPath;
            string fullOutputPath;

            long longStartOffset;
            long longCutSize;

            bool doLba = false;
            bool doMultiplier = false;
            string multiplierChunk;
            long multiplierValue = 1;
            char[] multiplierSplitParam = new char[1] { '=' };

            if (args.Length < 3)
            {
                Console.WriteLine("使用方法: manakut.exe <输入文件> <输出文件> <起始偏移量> <切割尺寸> [/lba|/m=<multiplier>]");
                Console.WriteLine("   或: manakut.exe <输入文件> <输出文件> <起始偏移量> [/lba|/m=<multiplier>]");
                Console.WriteLine();
                Console.WriteLine("4 参数选项将从<开始偏移>读取到文件结束.");
                Console.WriteLine("使用/lba 开关将起始偏移量乘以0x800.");
                Console.WriteLine("使用/m开关将起始偏移量乘以等号后的值.");
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
                        longStartOffset = VGMToolbox.util.ByteConversion.GetLongValueFromString(startOffset);

                        // check for LBA or MULTIPLIER switch
                        if ((args.Length > 4) && (args[4].ToUpper().Equals(LBA_SWITCH)))
                        {
                            doLba = true;
                        }
                        else if ((args.Length > 4) && (args[4].ToUpper().Substring(0, 2).Equals(MULTIPLIER_SWITCH)))
                        {
                            doMultiplier = true;
                            multiplierChunk = args[4].Split(multiplierSplitParam)[1];
                            multiplierValue = VGMToolbox.util.ByteConversion.GetLongValueFromString(multiplierChunk);
                        }
                        else if ((args.Length > 3) && (args[3].ToUpper().Equals(LBA_SWITCH)))
                        {
                            doLba = true;
                        }
                        else if ((args.Length > 3) &&
                                 (args[3].Length >= MULTIPLIER_SWITCH.Length) &&
                                 (args[3].ToUpper().Substring(0, 2).Equals(MULTIPLIER_SWITCH)))
                        {
                            doMultiplier = true;
                            multiplierChunk = args[3].Split(multiplierSplitParam)[1];
                            multiplierValue = VGMToolbox.util.ByteConversion.GetLongValueFromString(multiplierChunk);
                        }

                        // GET CUTSIZE
                        if ((args.Length > 3) &&
                            (!args[3].ToUpper().Equals(LBA_SWITCH)) &&
                            ((args[3].Length < MULTIPLIER_SWITCH.Length) || (!args[3].ToUpper().Substring(0, 2).Equals(MULTIPLIER_SWITCH))))
                        {
                            cutSize = args[3];
                            longCutSize = VGMToolbox.util.ByteConversion.GetLongValueFromString(cutSize);
                        }
                        else
                        {
                            longCutSize = fs.Length - longStartOffset;
                        }

                        // set LBA/MULTIPLIER values
                        if (doLba)
                        {
                            longStartOffset *= 0x800;
                        }
                        else if (doMultiplier)
                        {
                            longStartOffset *= multiplierValue;
                        }

                        ParseFile.ExtractChunkToFile(fs, longStartOffset, longCutSize, fullOutputPath);
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
