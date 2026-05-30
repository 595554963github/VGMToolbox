using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace VGMToolbox.format
{
    public class OgvStream
    {
        private string sourcePath;

        public OgvStream(string path)
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

        private bool IsValidOgvFile()
        {
            try
            {
                using (FileStream fs = File.OpenRead(sourcePath))
                {
                    byte[] oggHeader = new byte[4];
                    fs.Read(oggHeader, 0, 4);

                    if (oggHeader[0] != 0x4F || oggHeader[1] != 0x67 ||
                        oggHeader[2] != 0x67 || oggHeader[3] != 0x53)
                    {
                        return false;
                    }

                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    long position = 0;

                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        for (int i = 0; i < bytesRead - 5; i++)
                        {
                            if (buffer[i] == 0x74 && buffer[i + 1] == 0x68 &&
                                buffer[i + 2] == 0x65 && buffer[i + 3] == 0x6F &&
                                buffer[i + 4] == 0x72 && buffer[i + 5] == 0x61)
                            {
                                return true;
                            }
                        }
                        position += bytesRead;
                    }
                    return false;
                }
            }
            catch
            {
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

            if (!IsValidOgvFile())
            {
                throw new Exception("不是有效的OGV文件（缺少OGG头或theora标识）");
            }

            string outputDirectory = Path.GetDirectoryName(sourcePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
            string outputFile = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}.mp4");

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

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                WorkingDirectory = outputDirectory,
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

                Console.WriteLine("=== FFmpeg完整输出 ===");
                Console.WriteLine(output.ToString());
                Console.WriteLine("=== FFmpeg错误输出 ===");
                Console.WriteLine(errorOutput.ToString());

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg返回错误代码: {process.ExitCode}\n错误信息: {errorOutput.ToString()}");
                }

                if (!File.Exists(outputFile))
                {
                    throw new FileNotFoundException("FFmpeg未生成输出文件");
                }

                FileInfo outputInfo = new FileInfo(outputFile);
                if (outputInfo.Length == 0)
                {
                    throw new Exception("生成的MP4文件为空");
                }

                Console.WriteLine($"OGV文件转换完成: {outputFile} (大小: {outputInfo.Length} 字节)");
                Console.WriteLine("使用极致质量参数: CRF 14, 10bit色彩, Lanczos高质量缩放");
            }
        }
    }
}

