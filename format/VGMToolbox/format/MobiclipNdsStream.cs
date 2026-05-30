using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using VGMToolbox.util;

namespace VGMToolbox.format
{
    public class MobiclipNdsStream
    {
        private string sourcePath;

        public MobiclipNdsStream(string path)
        {
            this.sourcePath = path;
        }

        private string FindMobiusPath()
        {
            string currentDirMobius = Path.Combine(Directory.GetCurrentDirectory(), "Mobius.exe");
            if (File.Exists(currentDirMobius))
            {
                return currentDirMobius;
            }

            string ffmpegPath = FindExecutablePath("ffmpeg");
            if (!string.IsNullOrEmpty(ffmpegPath))
            {
                string ffmpegDir = Path.GetDirectoryName(ffmpegPath);
                string mobiusPath = Path.Combine(ffmpegDir, "Mobius.exe");
                if (File.Exists(mobiusPath))
                {
                    return mobiusPath;
                }
            }

            string mobiusInPath = FindExecutablePath("Mobius.exe");
            if (!string.IsNullOrEmpty(mobiusInPath))
            {
                return mobiusInPath;
            }

            return null;
        }

        private string FindExecutablePath(string executableName)
        {
            try
            {
                if (File.Exists(executableName))
                {
                    return Path.GetFullPath(executableName);
                }

                string pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(pathEnv))
                {
                    string[] paths = pathEnv.Split(Path.PathSeparator);
                    foreach (string path in paths)
                    {
                        try
                        {
                            string fullPath = Path.Combine(path, executableName);
                            if (File.Exists(fullPath))
                            {
                                return fullPath;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                if (!executableName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return FindExecutablePath(executableName + ".exe");
                }
            }
            catch
            {
            }

            return null;
        }

        public void DemultiplexStreams(MpegStream.DemuxOptionsStruct demuxOptions)
        {
            using (FileStream fs = File.OpenRead(sourcePath))
            {
                byte[] ndsMagicBytes = new byte[] { 0x4C, 0x32 };
                byte[] actualHeaderBytes = ParseFile.ParseSimpleOffset(fs, 0, 2);

                if (ParseFile.CompareSegment(actualHeaderBytes, 0, ndsMagicBytes))
                {
                    ConvertWithMobius();
                }
                else
                {
                    throw new Exception("不支持的NDS Mobiclip格式");
                }
            }
        }

        private void ConvertWithMobius()
        {
            string mobiusPath = FindMobiusPath();
            if (string.IsNullOrEmpty(mobiusPath) || !File.Exists(mobiusPath))
            {
                throw new Exception("未找到Mobius.exe");
            }

            string configPath = Path.Combine(Path.GetDirectoryName(mobiusPath), "Mobius.exe.config");
            if (!File.Exists(configPath))
            {
                CreateDefaultMobiusConfig(mobiusPath);
            }

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"源文件未找到: {sourcePath}");
            }

            string outputFile = Path.ChangeExtension(sourcePath, ".mp4");

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{mobiusPath}\" \"{sourcePath}\" && exit\"",
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = Path.GetDirectoryName(mobiusPath)
            };

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();

                    process.WaitForExit();

                    Thread.Sleep(2000);
                    Console.WriteLine($"转换完成: {Path.GetFileName(sourcePath)} -> {Path.GetFileName(outputFile)}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"转换MOFLEX文件时出错: {ex.Message}");
            }
        }

        private void CreateDefaultMobiusConfig(string mobiusPath)
        {
            string configContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <startup>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.7""/>
  </startup>
  <appSettings>
    <add key=""ffmpegPath"" value=""ffmpeg.exe""/>
    <add key=""options"" value=""-c:v libx265 -preset medium -crf 14 -r 60 -vf scale=2048:1080:flags=lanczos+full_chroma_inp+full_chroma_int -pix_fmt yuv420p10le -x265-params profile=main10:high-tier=1:level=6.2:aq-mode=3:deblock=-1,-1 -movflags +faststart -hide_banner""/>
    <add key=""stereoTarget"" value=""sbs2l""/>
    <add key=""maxQueueSize"" value=""256""/>
  </appSettings>
</configuration>";

            string configPath = Path.Combine(Path.GetDirectoryName(mobiusPath), "Mobius.exe.config");
            File.WriteAllText(configPath, configContent);
        }
    }
}
