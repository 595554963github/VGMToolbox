using System;
using System.Collections.Generic;
using System.Text;

namespace VGMToolbox.util
{
    public class ByteSearchCalculatingOffsetDescription : CalculatingOffsetDescription
    {
        public const string START_OF_STRING = "开始";
        public const string END_OF_STRING = "结束";

        public string RelativeLocationToByteString { set; get; }
        public string ByteString { set; get; }
        public bool TreatByteStringAsHex { set; get; }
    }
}
