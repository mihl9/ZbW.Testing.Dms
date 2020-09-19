using System;
using System.Xml.Serialization;

namespace ZbW.Testing.Dms.Client.Model
{
    [XmlRoot]
    public class MetadataItem
    {
        public string FileName { get; set; }

        public DateTime Valuta { get; set; }

        public string FileEnding { get; set; }

        public string Typ { get; set; }

        public string Keywords { get; set; }

        public string Username { get; set; }

        public DateTime CreationTime { get; set; }
    }
}