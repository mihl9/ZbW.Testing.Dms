using System;
using System.IO;

namespace ZbW.Testing.Dms.Client.Model
{
    public class Document
    {
        public Guid Id { get; set; }
        public MetadataItem Metadata { get; set; }

        public Stream File { get; set; }

    }
}
