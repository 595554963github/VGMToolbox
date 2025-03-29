using System;
using System.Collections.Generic;
using System.Text;

namespace VGMToolbox.util
{
    public class RiffCalculatingOffsetDescription : CalculatingOffsetDescription
    {
        public const string START_OF_STRING = "开始";
        public const string END_OF_STRING = "结束";
        
        public string RelativeLocationToRiffChunkString { set; get; }
        public string RiffChunkString { set; get; }
    }
}
