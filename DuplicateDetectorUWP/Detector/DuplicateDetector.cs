using DuplicateDetectorUWP.Detector.Enumerable;
using DuplicateDetectorUWP.Hash;
using DuplicateDirectorUWP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;

namespace DuplicateDetectorUWP.Detector
{
    public class DuplicateDetector
    {

        public ObservableCollection<string> includeFolderPaths { get; set; }
        public ObservableCollection<string> excludeFolderPaths { get; set; }
        public EnumerableCryptographType CryptographyType { get; set; }
        public EnumerableCompareType[] CompareBy { get; set; }
        public int TotalFiles { get; set; }
        public string[] FileTypeFilter { get; set; }

        private bool isStop;

        public DuplicateDetector()
        {
            this.includeFolderPaths = new ObservableCollection<string>();
            this.excludeFolderPaths = new ObservableCollection<string>();
            this.CryptographyType = EnumerableCryptographType.Md5;
            this.CompareBy = new EnumerableCompareType[] { EnumerableCompareType.Content };
            this.TotalFiles = 0;
            this.FileTypeFilter = new string[] { "*" };
            this.isStop = false;
        }

        private ObservableCollection<StorageFile> files;
        public async Task<ObservableCollection<GroupRecord>> Execute()
        {
            OnPreparing();
            var records = new List<Record>(); // records of files
            files = new ObservableCollection<StorageFile>();
            IAsyncAction getFileTask = null;
            try
            {
                getFileTask = Task.Run(() => GetAllFiles(includeFolderPaths)).AsAsyncAction();

            }
            catch (Exception ex)
            {
                isStop = false;
                return null;
            }
            while (files.Count == 0) { }

            OnStarting();
            for (int i = 0; i < files.Count; i++)
            {
                this.TotalFiles = files.Count;
                var file = files[i];
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
                    Path = file.Path,
                    FileType = file.FileType
                };
                records.Add(record);
                OnCompletedOneFile(record);
                if (isStop)
                {
                    getFileTask.Cancel();
                    break;
                }
            }

            if (isStop) isStop = false;

            OnComplete();

            var groupRecord = GroupBy(records, CompareBy);

            return groupRecord;
        }


        public void Stop()
        {
            isStop = true;
        }

        private ObservableCollection<GroupRecord> GroupBy(
            List<Record> records, EnumerableCompareType[] compareOption)
        {
            if (records == null) return null;
            if (compareOption == null || compareOption.Count() == 0) throw new Exception();
            var groupRecords = new ObservableCollection<GroupRecord>();
            var query = new object();
            switch (compareOption[0])
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

            var option = compareOption.ToList();
            option.RemoveAt(0);
            foreach (var group in (IEnumerable<IGrouping<string, Record>>)query)
            {
                var listGroup = group.ToList();
                try
                {
                    groupRecords = new ObservableCollection<GroupRecord>(
                        groupRecords.Concat(GroupBy(listGroup, option.ToArray())));
                }
                catch
                {
                    var iGroup = listGroup.GroupBy(item => item.FileType);
                    foreach (var gr in iGroup)
                    {
                        var listRecord = gr.ToList();

                        GroupRecord groupRecord = new GroupRecord()
                        {
                            records = new ObservableCollection<Record>(listRecord),
                            Name = group.First().Name,
                            Size = listRecord.Sum(t => t.Size),
                            TypeGroup = group.Key
                        };
                        if (groupRecord.records.Count > 1)
                        {
                            groupRecords.Add(groupRecord);
                        }
                    }
                }
            }
            return groupRecords;
        }

        private async Task GetAllFiles(
            ObservableCollection<string> includeFolderPaths)
        {
            //ObservableCollection<StorageFile> files = new ObservableCollection<StorageFile>();
            foreach (var path in includeFolderPaths)
            {
                if (excludeFolderPaths.Contains(path))
                {
                    continue;
                }
                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                //StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                var items = await folder.GetItemsAsync();

                foreach (var item in items)
                {
                    if (item.GetType() == typeof(StorageFile))
                    {
                        if (FileTypeFilter.Contains(((StorageFile)item).FileType)
                            || FileTypeFilter.Contains("*"))
                        {
                            files.Add((StorageFile)item);
                        }
                    }
                    else
                    {
                        //Task.Run(() => GetAllFiles(new ObservableCollection<string>() { item.Path })).AsAsyncAction();
                        await GetAllFiles(new ObservableCollection<string>() { item.Path });
                        //files = new ObservableCollection<StorageFile>(files.Concat<StorageFile>(x));
                    }
                    if (isStop)
                    {
                        throw new Exception("Stop");
                        //Debug.WriteLine("Stop");
                    }
                }
            }
            //return files;
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
            foreach (var group in groupRecords)
            {
                group.DetectOriginRecord(detectorOrigin);
            }
        }

        public void AddFolders(string path)
        {
            try
            {
                this.includeFolderPaths.Add(path);
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
                this.includeFolderPaths.RemoveAt(index);
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
