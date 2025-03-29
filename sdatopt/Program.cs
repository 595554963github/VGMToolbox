using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using VGMToolbox.format;
using VGMToolbox.format.sdat;
using VGMToolbox.format.util;
using VGMToolbox.util;

namespace sdatopt
{
    class Program
    {
        private static readonly string APPLICATION_PATH = Assembly.GetExecutingAssembly().Location;

        static void Main(string[] args)
        {
            if ((args.Length > 3) || (args.Length < 2))
            {
                Console.WriteLine("使用方法:sdatopt.exe 文件名 起始序列 结束序列");
                Console.WriteLine("       sdatopt.exe fileName ALL");
                Console.WriteLine("       sdatopt.exe fileName PREP");
                Console.WriteLine("       sdatopt.exe fileName MAP smap_file");
                Console.WriteLine("       sdatopt.exe fileName MAP");
                Console.WriteLine();
                Console.WriteLine("文件名:.sdat or .2sflib 包含要优化的SDAT");
                Console.WriteLine("起始序列:要保留的起始序列号:p");
                Console.WriteLine("结束序列:要保留的结束序列号");
                Console.WriteLine("ALL:如果您希望保留所有序列,请使用此选项");
                Console.WriteLine("准备:使用它来输出用于序列选择的 SMAP.删除您不想包含的整行序列.");
                Console.WriteLine("MAP smap_file:使用PREP中的smap_file来选择要保留的序列.");
                Console.WriteLine("MAP:根据要保留的sdat名称选择序列查找smap 文件.必须是正确的格式.");
            }
            else
            {
                Sdat sdat = null;
                bool is2sfSource = false; ;

                string sdatDirectory;
                string sdatOptimizingFileName;
                string sdatOptimizingPath;

                string sdatCompletedFileName;
                string sdatCompletedPath;

                int startSequence = Sdat.NO_SEQUENCE_RESTRICTION;
                int endSequence = Sdat.NO_SEQUENCE_RESTRICTION;

                string[] extractedSdats = null;
                string decompressedDataPath = null;
                string extractedToFolder = null;

                ArrayList cleanupList = new ArrayList();

                string filename = Path.GetFullPath(args[0]);
                string smapFileName = null;

                if (!File.Exists(filename))
                {
                    Console.WriteLine("找不到SDAT:{0}", filename);
                    return;
                }

                if (args[1].Trim().ToUpper().Equals("MAP"))
                {
                    if (args.Length < 3)
                    {
                        smapFileName = Path.ChangeExtension(filename, Smap.FILE_EXTENSION);
                    }
                    else
                    {
                        smapFileName = Path.GetFullPath(args[2]);
                    }

                    if (!File.Exists(smapFileName))
                    {
                        Console.WriteLine("找不到SMAP: {0}", smapFileName);
                        return;
                    }
                }

                sdatDirectory = Path.GetDirectoryName(filename);
                sdatOptimizingFileName = String.Format("{0}_OPTIMIZING{1}",
                    Path.GetFileNameWithoutExtension(filename), Path.GetExtension(filename));
                sdatOptimizingPath = Path.Combine(sdatDirectory, sdatOptimizingFileName);

                sdatCompletedFileName = String.Format("{0}_OPTIMIZED{1}",
                    Path.GetFileNameWithoutExtension(filename), Path.GetExtension(filename));
                sdatCompletedPath = Path.Combine(sdatDirectory, sdatCompletedFileName);

                try
                {
                    File.Copy(filename, sdatOptimizingPath, true);

                    using (FileStream fs = File.Open(sdatOptimizingPath, FileMode.Open, FileAccess.Read))
                    {
                        Type dataType = FormatUtil.getObjectType(fs);

                        if (dataType != null)
                        {
                            if (dataType.Name.Equals("Sdat"))
                            {
                                Console.WriteLine("输入文件是SDAT.");
                                Console.WriteLine("创建内部SDAT.");
                                sdat = new Sdat();
                                sdat.Initialize(fs, sdatOptimizingPath);
                            }
                            else if (dataType.Name.Equals("Xsf")) // is an Xsf, confirm it is a 2sf
                            {
                                Xsf libFile = new Xsf();
                                libFile.Initialize(fs, sdatOptimizingPath);

                                if (libFile.GetFormat().Equals(Xsf.FormatName2sf))
                                {
                                    Console.WriteLine("输入文件是2SF.");

                                    is2sfSource = true;

                                    // close stream, we're gonna need this file
                                    fs.Close();
                                    fs.Dispose();

                                    // unpack compressed section
                                    Console.WriteLine("解压2SF的压缩数据部分.");

                                    Xsf2ExeStruct xsf2ExeStruct = new Xsf2ExeStruct();
                                    xsf2ExeStruct.IncludeExtension = true;
                                    xsf2ExeStruct.StripGsfHeader = false;
                                    decompressedDataPath = XsfUtil.ExtractCompressedDataSection(sdatOptimizingPath, xsf2ExeStruct);

                                    // extract SDAT
                                    Console.WriteLine("从解压压缩数据段中提取SDAT.");

                                    VGMToolbox.util.FindOffsetStruct findOffsetStruct = new VGMToolbox.util.FindOffsetStruct();
                                    findOffsetStruct.SearchString = Sdat.ASCII_SIGNATURE_STRING;
                                    findOffsetStruct.TreatSearchStringAsHex = true;
                                    findOffsetStruct.CutFile = true;
                                    findOffsetStruct.SearchStringOffset = "0";
                                    findOffsetStruct.CutSize = "8";
                                    findOffsetStruct.CutSizeOffsetSize = "4";
                                    findOffsetStruct.IsCutSizeAnOffset = true;
                                    findOffsetStruct.OutputFileExtension = ".sdat";
                                    findOffsetStruct.IsLittleEndian = true;

                                    findOffsetStruct.UseTerminatorForCutSize = false;
                                    findOffsetStruct.TerminatorString = null;
                                    findOffsetStruct.TreatTerminatorStringAsHex = false;
                                    findOffsetStruct.IncludeTerminatorLength = false;
                                    findOffsetStruct.ExtraCutSizeBytes = null;

                                    string output;
                                    extractedToFolder = ParseFile.FindOffsetAndCutFile(decompressedDataPath, findOffsetStruct, out output, false, false);

                                    // create SDAT object                                                                        
                                    Console.WriteLine("创建内部SDAT.");
                                    extractedSdats = Directory.GetFiles(extractedToFolder, "*.sdat");

                                    if (extractedSdats.Length > 1)
                                    {
                                        Console.WriteLine("抱歉,这个2SF文件包含超过1个SDAT.sdatopt目前无法处理此问题.");
                                        return;
                                    }
                                    else if (extractedSdats.Length == 0)
                                    {
                                        Console.WriteLine("错误:在解压数据部分未找到 SDAT.");
                                        return;
                                    }
                                    else
                                    {


                                        using (FileStream sdatFs = File.Open(extractedSdats[0], FileMode.Open, FileAccess.Read))
                                        {
                                            sdat = new Sdat();
                                            sdat.Initialize(sdatFs, extractedSdats[0]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("错误:无法确定输入文件的格式.");
                                return;
                            }
                        }
                    }

                    if (sdat != null)
                    {
                        if (args[1].Trim().ToUpper().Equals("PREP"))
                        {
                            sdat.BuildSmapPrep(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
                        }
                        else if (args[1].Trim().ToUpper().Equals("MAP"))
                        {
                            ArrayList allowedSequences = buildSequenceList(smapFileName);

                            Console.WriteLine("优化SDAT.");
                            sdat.OptimizeForZlib(allowedSequences);

                        }
                        else
                        {
                            if (!args[1].Trim().ToUpper().Equals("ALL"))
                            {
                                if (!String.IsNullOrEmpty(args[1]))
                                {
                                    startSequence = (int)VGMToolbox.util.ByteConversion.GetLongValueFromString(args[1]);
                                }

                                if (!String.IsNullOrEmpty(args[2]))
                                {
                                    endSequence = (int)VGMToolbox.util.ByteConversion.GetLongValueFromString(args[2]);
                                }
                            }

                            Console.WriteLine("优化SDAT.");
                            sdat.OptimizeForZlib(startSequence, endSequence);
                        }
                    }

                    if (is2sfSource)
                    {
                        if (!args[1].Trim().ToUpper().Equals("PREP"))
                        {
                            // replace SDAT section
                            Console.WriteLine("将SDAT插入回解压压缩数据部分.");
                            long sdatOffset;
                            using (FileStream dcFs = File.Open(decompressedDataPath, FileMode.Open, FileAccess.ReadWrite))
                            {
                                sdatOffset = ParseFile.GetNextOffset(dcFs, 0, Sdat.ASCII_SIGNATURE);
                            }

                            FileInfo fi = new FileInfo(extractedSdats[0]);
                            FileUtil.ReplaceFileChunk(extractedSdats[0], 0, fi.Length, decompressedDataPath, sdatOffset);

                            // rebuild 2sf
                            Console.WriteLine("重建2sf文件.");

                            string bin2PsfStdOut = String.Empty;
                            string bin2PsfStdErr = String.Empty;

                            XsfUtil.Bin2Psf(Path.GetExtension(filename).Substring(1), (int)Xsf.Version2sf,
                                decompressedDataPath, ref bin2PsfStdOut, ref bin2PsfStdErr);

                            Console.WriteLine("清理中间文件.");
                            File.Copy(Path.ChangeExtension(decompressedDataPath, Path.GetExtension(filename)), sdatOptimizingPath, true);
                            File.Delete(Path.ChangeExtension(decompressedDataPath, Path.GetExtension(filename)));
                        }

                        File.Delete(decompressedDataPath);
                        Directory.Delete(extractedToFolder, true);
                    }

                    if (!args[1].Trim().ToUpper().Equals("PREP"))
                    {
                        Console.WriteLine("复制到优化文件.");
                        File.Copy(sdatOptimizingPath, sdatCompletedPath, true);
                    }

                    Console.WriteLine("删除优化文件.");
                    File.Delete(sdatOptimizingPath);

                    Console.WriteLine("优化完成.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("错误:{0}", ex.Message));
                }
            }
        }

        static ArrayList buildSequenceList(string pSmapPath)
        {
            ArrayList sequences = new ArrayList();
            string seqIndex;

            using (StreamReader sr = File.OpenText(pSmapPath))
            {
                string lineIn = String.Empty;

                // skip first two lines
                lineIn = sr.ReadLine();
                lineIn = sr.ReadLine();

                while ((lineIn = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(lineIn.Trim()))
                    {
                        seqIndex = lineIn.Split(' ')[0].Trim();
                        sequences.Add(int.Parse(seqIndex));
                    }
                }
            }

            return sequences;
        }
    }
}
