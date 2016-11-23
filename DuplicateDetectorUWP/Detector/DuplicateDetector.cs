using DuplicateDetectorUWP.Detector.Enumerable;
using DuplicateDetectorUWP.Hash;
using DuplicateDirectorUWP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace DuplicateDetectorUWP.Detector
{
    public class DuplicateDetector
    {

        public ObservableCollection<StorageFolder> folderPaths { get; set; }
        public EnumerableCryptographType CryptographyType { get; set; }
        public EnumerableCompareType CompareBy { get; set; }
        public int TotalFiles { get; set; }
        public string FileTypeFilter { get; set; }

        public DuplicateDetector()
        {
            this.folderPaths = new ObservableCollection<StorageFolder>();
            this.CryptographyType = EnumerableCryptographType.Md5;
            this.CompareBy = EnumerableCompareType.Content;
            this.TotalFiles = 0;
            this.FileTypeFilter = "*";
        }

        public async Task<ObservableCollection<GroupRecord>> Execute()
        {
            OnPreparing();
            var records = new List<Record>(); // records of files
            var files = await GetAllFiles(folderPaths);
            this.TotalFiles = files.Count;
            try
            {
                OnStarting();
                foreach (var file in files)
                {
                    var basicProperties = await file.GetBasicPropertiesAsync();
                    //var hashCode = await Task.Run(() => GetHashCode(file));
                    var hashCode = await GetHashCode(file);

                    Record record = new Record()
                    {
                        Id = Singleton.CreateGuid(),
                        Name = file.Name,
                        Size = (long)basicProperties.Size,
                        DateCreated = file.DateCreated.LocalDateTime,
                        DateModified = basicProperties.DateModified.LocalDateTime,
                        Hash = hashCode,
                        Path = file.Path
                    };
                    records.Add(record);
                    OnCompletedOneFile(record);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var groupRecord = GroupBy(records, CompareBy);
            OnComplete();
            return groupRecord;
        }

        private ObservableCollection<GroupRecord> GroupBy(
            List<Record> records, EnumerableCompareType compareOption)
        {
            var groupRecords = new ObservableCollection<GroupRecord>();
            var query = new object();
            switch (compareOption)
            {
                case EnumerableCompareType.Content:
                    query = records.GroupBy(item => item.Hash);
                    break;
                case EnumerableCompareType.DateCreated:
                    query = records.GroupBy(item => item.DateCreated.ToString());
                    break;
                case EnumerableCompareType.DateModified:
                    query = records.GroupBy(item => item.DateModified.ToString());
                    break;
                case EnumerableCompareType.Name:
                    query = records.GroupBy(item => item.Name);
                    break;
                case EnumerableCompareType.Size:
                    query = records.GroupBy(item => item.Size.ToString());
                    break;
                default:
                    break;
            }

            foreach(var group in (IEnumerable<IGrouping<string, Record>>)query)
            {
                GroupRecord groupRecord = new GroupRecord()
                {
                    records = new ObservableCollection<Record>(group.ToList()),
                    Name = group.First().Name,
                    Size = group.ToList().Sum(t => t.Size),
                    TypeGroup = group.Key
                };
                if(groupRecord.records.Count > 1)
                {
                    groupRecords.Add(groupRecord);
                }
            }
            return groupRecords;
        }
        
        private async Task<ObservableCollection<StorageFile>> GetAllFiles(
            ObservableCollection<StorageFolder> folderPaths)
        {
            ObservableCollection<StorageFile> files = new ObservableCollection<StorageFile>();
            foreach (var f in folderPaths)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                    FutureAccessList.AddOrReplace("PickedFolderToken", f);

                var items = await f.GetItemsAsync();
                
                foreach (var item in items)
                {
                    if (item.GetType() == typeof(StorageFile))
                    {
                        if(((StorageFile)item).FileType == FileTypeFilter
                            || FileTypeFilter.Equals("*"))
                        {
                            files.Add((StorageFile)item);
                        }
                    }
                    else
                    {
                        var x = await GetAllFiles(
                            new ObservableCollection<StorageFolder>()
                            {
                                (StorageFolder)item }
                            );
                        files = new ObservableCollection<StorageFile>(files.Concat<StorageFile>(x));
                    }
                }
            }
            return files;
        }

        private async Task<string> GetHashCode(StorageFile file)
        {
            string result = string.Empty;
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                byte[] buffer = new byte[1024 * 1024 * 10];

                String hashBuffer = String.Empty;

                AbstractHash hash = null;
                switch (CryptographyType)
                {
                    case EnumerableCryptographType.Md5:
                        hash = new Md5();
                        break;
                    case EnumerableCryptographType.Sha1:
                        hash = new Sha1();
                        break;
                    default:
                        break;
                }

                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    hashBuffer += hash.Create(buffer);
                }
                result = hash.Create(hashBuffer);
                stream.Dispose();
            }
            return result;
        }

        public void DetectOriginRecords(
            ObservableCollection<GroupRecord> groupRecords, 
            EnumerableDetectOrigin[] detectorOrigin)
        {
            foreach(var group in groupRecords)
            {
                group.DetectOriginRecord(detectorOrigin);
            }
        }

        public void AddFolders(StorageFolder path)
        {
            try
            {
                this.folderPaths.Add(path);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public void RemoveFolder(int index)
        {
            try
            {
                this.folderPaths.RemoveAt(index);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        

        public event EventHandler Completed;
        public void OnComplete()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CompletedOneFile;
        private void OnCompletedOneFile(object obj)
        {
            CompletedOneFile?.Invoke(obj, EventArgs.Empty);
        }

        public event EventHandler Starting;
        private void OnStarting()
        {
            Starting?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Preparing;
        private void OnPreparing()
        {
            Preparing?.Invoke(this, EventArgs.Empty);
        }
    }
}
