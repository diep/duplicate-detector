using DuplicateDetectorUWP.Detector.Enumerable;
using DuplicateDirectorUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuplicateDetectorUWP.Detector
{
    public class GroupRecord
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public long Size { get; set; }
        public List<Record> records { get; set; }

        // Constructor
        GroupRecord()
        {
            this.Id = Singleton.CreateGuid();
            this.Name = string.Empty;
            this.Comment = string.Empty;
            this.records = new List<Record>();
            this.Size = 0;
        }
        
        public void AddRecord(Record record)
        {
            if (record == null)
            {
                throw new Exception("Can't add null object!");
            }
            this.records.Add(record);
        }

        public void RemoveRecord(int index)
        {
            if(index < 0)
            {
                throw new Exception("Index invalid!");
            }
            this.records.RemoveAt(index);
        }
        
        public void ClearRecords()
        {
            if(this.records == null)
            {
                throw new Exception("List of records was null!");
            }
            this.records.Clear();
        }


        /// <summary>
        /// Index of record in current record group
        /// </summary>
        public int DetectOriginRecord(EnumerableSelectType selectType)
        {
            throw new System.NotImplementedException();
        }
    }
}