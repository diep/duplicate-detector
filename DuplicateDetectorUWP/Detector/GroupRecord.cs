using DuplicateDetectorUWP.Detector.Enumerable;
using DuplicateDirectorUWP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DuplicateDetectorUWP.Detector
{
    public class GroupRecord
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public long Size { get; internal set; }
        public string TypeGroup { get; internal set; }
        public ObservableCollection<Record> records { get; set; }

        // Constructor
        public GroupRecord()
        {
            this.Id = Singleton.CreateGuid();
            this.Name = string.Empty;
            this.Comment = string.Empty;
            this.records = new ObservableCollection<Record>();
            this.Size = 0;
            this.TypeGroup = string.Empty;
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
        
        internal void DetectOriginRecord(EnumerableDetectOrigin[] detectorOrigin)
        {
            switch (detectorOrigin[0])
            {
                case EnumerableDetectOrigin.LargestFile:
                    records.OrderByDescending(item => 
                    {
                        item.IsOrigin = false;
                        return item.Size;

                    }).ElementAt(0).IsOrigin = true;
                    break;
                case EnumerableDetectOrigin.SmallestFile:
                    records.OrderBy(item =>
                    {
                        item.IsOrigin = false;
                        return item.Size;

                    }).ElementAt(0).IsOrigin = true;
                    break;
                case EnumerableDetectOrigin.LongestFile:
                    records.OrderByDescending(item =>
                    {
                        item.IsOrigin = false;
                        return item.Name.Length;

                    }).ElementAt(0).IsOrigin = true;
                    break;
                case EnumerableDetectOrigin.ShortestFile:
                    records.OrderBy(item =>
                    {
                        item.IsOrigin = false;
                        return item.Name.Length;

                    }).ElementAt(0).IsOrigin = true;
                    break;
                case EnumerableDetectOrigin.OldestFile:
                    records.OrderByDescending(item =>
                    {
                        item.IsOrigin = false;
                        return item.DateCreated;

                    }).ElementAt(0).IsOrigin = true;
                    break;
                case EnumerableDetectOrigin.NewestFile:
                    records.OrderBy(item =>
                    {
                        item.IsOrigin = false;
                        return item.DateCreated;

                    }).ElementAt(0).IsOrigin = true;
                    break;
            }
        }
    }
}