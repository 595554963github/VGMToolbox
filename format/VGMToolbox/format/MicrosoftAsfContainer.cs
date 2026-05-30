using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace VGMToolbox.format
{
    public class MicrosoftAsfContainer
    {
        const string DefaultFileExtensionAudio = ".raw.wma";
        const string DefaultFileExtensionVideo = ".raw.wmv";
        const string DefaultFileExtensionMp4 = ".mp4";

        public static readonly byte[] ASF_HEADER_BYTES = new byte[] {
            0x30, 0x26, 0xB2, 0x75,
            0x8E, 0x66, 0xCF, 0x11,
            0xA6, 0xD9, 0x00, 0xAA,
            0x00, 0x62, 0xCE, 0x6C
        };

        private string sourcePath;

        public MicrosoftAsfContainer(string path)
        {
            this.sourcePath = path;
        }

        private bool IsFfmpegAvailable()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit(5000);
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidAsfFile()
        {
            try
            {
                using (FileStream fs = File.OpenRead(sourcePath))
                {
                    byte[] header = new byte[ASF_HEADER_BYTES.Length];
                    int bytesRead = fs.Read(header, 0, header.Length);

                    if (bytesRead != header.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < ASF_HEADER_BYTES.Length; i++)
                    {
                        if (header[i] != ASF_HEADER_BYTES[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查ASF文件头时出错: {ex.Message}");
                return false;
            }
        }

        public void DemultiplexStreams(MpegStream.DemuxOptionsStruct demuxOptions)
        {
            if (!IsFfmpegAvailable())
            {
                throw new Exception("未检测到FFmpeg，请安装FFmpeg并添加到系统环境变量");
            }

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"文件未找到: {sourcePath}");
            }

            if (!IsValidAsfFile())
            {
                throw new Exception("不是有效的ASF格式文件（文件头不匹配）");
            }

            string outputDirectory = Path.GetDirectoryName(sourcePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);

            if (demuxOptions.ExtractAudio && demuxOptions.ExtractVideo)
            {
                ConvertToMp4(outputDirectory, fileNameWithoutExtension);
            }
            else if (demuxOptions.ExtractAudio)
            {
                ExtractAudioOnly(outputDirectory, fileNameWithoutExtension);
            }
            else if (demuxOptions.ExtractVideo)
            {
                ExtractVideoOnly(outputDirectory, fileNameWithoutExtension);
            }
        }

        private void ConvertToMp4(string outputDirectory, string fileNameWithoutExtension)
        {
            string outputFile = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}{DefaultFileExtensionMp4}");

            string arguments = $"-i \"{sourcePath}\" " +
                               "-c:v libx265 " +
                               "-preset medium " +
                               "-crf 14 " +
                               "-r 60 " +
                               "-vf \"scale=2048:1080:flags=lanczos+full_chroma_inp+full_chroma_int\" " +
                               "-pix_fmt yuv420p10le " +
                               "-x265-params \"profile=main10:high-tier=1:level=6.2:aq-mode=3:deblock=-1,-1\" " +
                               "-c:a aac " +
                               "-b:a 320k " +
                               "-movflags +faststart " +
                               $"\"{outputFile}\" -y";

            Console.WriteLine($"执行FFmpeg命令: ffmpeg {arguments}");

            ExecuteFfmpegCommand(arguments, outputDirectory, "MP4转换");
        }

        private void ExtractAudioOnly(string outputDirectory, string fileNameWithoutExtension)
        {
            string outputFile = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}{DefaultFileExtensionAudio}");

            string arguments = $"-i \"{sourcePath}\" -c:a copy \"{outputFile}\" -y";

            Console.WriteLine($"执行FFmpeg命令: ffmpeg {arguments}");

            ExecuteFfmpegCommand(arguments, outputDirectory, "音频提取");
        }

        private void ExtractVideoOnly(string outputDirectory, string fileNameWithoutExtension)
        {
            string outputFile = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}{DefaultFileExtensionVideo}");

            string arguments = $"-i \"{sourcePath}\" -an -c:v copy \"{outputFile}\" -y";

            Console.WriteLine($"执行FFmpeg命令: ffmpeg {arguments}");

            ExecuteFfmpegCommand(arguments, outputDirectory, "视频提取");
        }

        private void ExecuteFfmpegCommand(string arguments, string workingDirectory, string operationName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            StringBuilder errorOutput = new StringBuilder();
            StringBuilder output = new StringBuilder();

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"FFmpeg输出: {e.Data}");
                        output.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"FFmpeg进度: {e.Data}");
                        errorOutput.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                Console.WriteLine($"=== FFmpeg {operationName} 完整输出 ===");
                Console.WriteLine(output.ToString());
                Console.WriteLine($"=== FFmpeg {operationName} 错误输出 ===");
                Console.WriteLine(errorOutput.ToString());

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg {operationName} 返回错误代码: {process.ExitCode}\n错误信息: {errorOutput.ToString()}");
                }

                string outputFile = arguments.Split('"')[arguments.Split('"').Length - 2];
                if (!File.Exists(outputFile))
                {
                    throw new FileNotFoundException($"FFmpeg未生成输出文件: {outputFile}");
                }

                FileInfo outputInfo = new FileInfo(outputFile);
                if (outputInfo.Length == 0)
                {
                    throw new Exception($"生成的{operationName}文件为空");
                }

                Console.WriteLine($"{operationName}完成: {outputFile} (大小: {outputInfo.Length} 字节)");
            }
        }

        public static bool IsAsfFile(string filePath)
        {
            try
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    byte[] header = new byte[ASF_HEADER_BYTES.Length];
                    int bytesRead = fs.Read(header, 0, header.Length);

                    if (bytesRead != header.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < ASF_HEADER_BYTES.Length; i++)
                    {
                        if (header[i] != ASF_HEADER_BYTES[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
