using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuplicateDetectorUWP.Detector
{
    public class Record
    {

        public Record()
        {
            this.Id = string.Empty;
            this.Name = string.Empty;
            this.Path = string.Empty;
            this.Size = 0;
            this.Hash = string.Empty;
            this.DateCreated = new DateTime();
            this.DateModified = new DateTime();
        }

        public Record(
            string id,
            string name, 
            long size, 
            DateTime dateModified, 
            string hash, 
            string path, 
            DateTime dateCreated)
        {
            this.Id = id;
            this.Name = name;
            this.Path = path;
            this.Size = size;
            this.Hash = hash;
            this.DateCreated = dateCreated;
            this.DateModified = dateModified;
        }
        

        public string Id { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime DateModified { get; set; }

        public string Hash { get; set; }

        public string Path { get; set; }

        public DateTime DateCreated { get; set; }
    }
}