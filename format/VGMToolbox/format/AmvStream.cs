using System;
using System.IO;

namespace VGMToolbox.format
{
    public class AmvStream
    {
        private string sourcePath;

        public AmvStream(string path)
        {
            this.sourcePath = path;
        }

        public void DemultiplexStreams(MpegStream.DemuxOptionsStruct demuxOptions)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"源文件未找到: {sourcePath}");
            }

            try
            {
                string outputPath = Path.GetDirectoryName(sourcePath) ?? Path.GetDirectoryName(sourcePath);

                var decoder = new AmvDecoder();
                int frameCount = decoder.DecodeAmvFile(sourcePath, outputPath);

                Console.WriteLine($"AMV解包完成！共提取{frameCount}帧。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解包AMV文件时出错: {ex.Message}");
                throw;
            }
        }
    }
}
