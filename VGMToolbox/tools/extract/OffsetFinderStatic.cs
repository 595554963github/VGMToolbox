using System;
using System.Xml.Serialization;

namespace VGMToolbox.tools.extract
{
    public partial class OffsetFinderTemplate : IComparable, ISerializablePreset
    {
        public string FilePath { set; get; }
        
        public OffsetFinderTemplate()
        {
            this.headerField = new Header();
            this.searchParametersField = new SearchParameters();
        }

        public override string ToString()
        {
            string ret = null;

            if ((this.headerField != null) && (!String.IsNullOrEmpty(this.headerField.FormatName)))
            {
                ret = this.headerField.FormatName;
            }

            return ret;
        }

        public int CompareTo(object obj)
        {
            if (obj is OffsetFinderTemplate)
            {
                OffsetFinderTemplate o = (OffsetFinderTemplate)obj;

                return this.Header.FormatName.CompareTo(o.Header.FormatName);
            }

            throw new ArgumentException("对象不是偏移量查找器模板");
        }    
    }

    public partial class SearchParameters
    {
        public SearchParameters()
        {
            this.cutParametersField = new cutParameters();
        }
    }
}
