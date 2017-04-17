using DuplicateDetectorUWP.Detector.Enumerable;
using DuplicateDetectorUWP.Hash;
using DuplicateDirectorUWP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

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
        public bool IsStop { get; set; }
        public bool IsCountFileCompleted { get; set; }

        private bool containerAllFile = false;
        private ObservableCollection<string> files;
        private ThreadPoolTimer _periodicTimer1 = null;
        private List<Task> _countFileTasks;

        public DuplicateDetector()
        {
            this.includeFolderPaths = new ObservableCollection<string>();
            this.excludeFolderPaths = new ObservableCollection<string>();
            this.CryptographyType = EnumerableCryptographType.Md5;
            this.CompareBy = new EnumerableCompareType[] { EnumerableCompareType.Name };
            this.TotalFiles = 0;
            this.FileTypeFilter = new string[] { "*" };
            this.IsStop = false;
            this.IsCountFileCompleted = false;
        }

        public async Task<ObservableCollection<GroupRecord>> Execute()
        {
            IsCountFileCompleted = false;
            containerAllFile = FileTypeFilter.Contains("*");
            OnPreparing();
            var records = new List<Record>(); // records of files
            files = new ObservableCollection<string>();
            //IAsyncAction getFileTask = null;
            _countFileTasks = new List<Task>();
            
            _periodicTimer1 = ThreadPoolTimer.CreatePeriodicTimer(
                    new TimerElapsedHandler(OnTimerElapedHandeler1), TimeSpan.FromSeconds(5));

            foreach (var _ in includeFolderPaths)
            {
                _countFileTasks.Add(Task.Run(() => { GetAllFiles(_); }));
            }
            Task.Delay(2000).Wait();
            
            OnStarting();
            for (int i = 0; i < files.Count; i++)
            {
                try
                {
                    this.TotalFiles = files.Count;
                    var filePath = files[i];
                    if (String.IsNullOrEmpty(filePath)) continue;
                    //var file = await StorageFile.GetFileFromPathAsync(filePath);
                    //var basicProperties = await file.GetBasicPropertiesAsync();
                    var basicProperties = new FileInfo(filePath);
                    

                    var hashCode = string.Empty;
                    if (CompareBy.Contains(EnumerableCompareType.Content))
                    {
                        var file = await StorageFile.GetFileFromPathAsync(filePath);
                        hashCode = await GetHashCodeAsync(file);
                    }

                    Record record = new Record()
                    {
                        Id = Singleton.CreateGuid(),
                        Name = basicProperties.Name,
                        Size = basicProperties.Length,
                        DateCreated = basicProperties.CreationTimeUtc,
                        DateModified = basicProperties.LastWriteTimeUtc,
                        Hash = hashCode,
                        Path = filePath,
                        FileType = basicProperties.Extension
                    };
                    records.Add(record);
                    OnCompletedOneFile(record);

                    //Clean trash by system
                    GC.Collect();
                    //GC.WaitForPendingFinalizers();
                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("--- Exception in Execute() function: \r\n" + ex.ToString());
                    //GoogleAnalytics.EasyTracker.GetTracker().SendException(ex.ToString(), false);
                }

                if (IsStop)
                {
                    //getFileTask.Cancel();
                    break;
                }

                if (_countFileTasks.Count == 0)
                    IsCountFileCompleted = true;
            }

            if (IsStop) IsStop = false;
            _periodicTimer1.Cancel();

            OnComplete();

            var groupRecord = GroupBy(records, CompareBy);

            return groupRecord;
        }
        

        private ObservableCollection<GroupRecord> GroupBy(
            List<Record> records, EnumerableCompareType[] compareOption)
        {
            if (records == null) return null;
            if (compareOption == null || compareOption.Count() == 0) throw new Exception();
            var groupRecords = new ObservableCollection<GroupRecord>();
            var query = new object();
            try
            {
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

            }
            catch (Exception ex)
            {

                GoogleAnalytics.EasyTracker.GetTracker().SendException(ex.ToString(), false);
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
                //Clean trash by system
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
            return groupRecords;
        }

        private void OnTimerElapedHandeler1(ThreadPoolTimer timer)
        {
            for (int i = 0; i < _countFileTasks.Count; i++)
            {
                if (_countFileTasks[i].Status == TaskStatus.RanToCompletion ||
                    _countFileTasks[i].Status == TaskStatus.Canceled ||
                    _countFileTasks[i].Status == TaskStatus.Faulted)
                {
                    _countFileTasks.RemoveAt(i);
                }
            }
        }

        //int countTask = 0;
        private void GetAllFiles(string path)
        {
            if (excludeFolderPaths.Contains(path) || IsStop)
            {
                return;
            }

            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                var fileSystemInfos = dir.GetFileSystemInfos();
                foreach(var _ in fileSystemInfos)
                {
                    if (IsStop) return;
                   
                    var extFile = _.Extension;

                    if (_.Attributes.ToString().Contains("Directory"))
                    {
                        // directory
                        //Debug.WriteLine(_countFileTasks.Count);
                        _countFileTasks.Add(Task.Run(() => { GetAllFiles(_.FullName); }));

                    }
                    else if (containerAllFile || FileTypeFilter.Contains(extFile))
                    {
                        // file
                        try
                        {
                            files.Add(_.FullName); // BUG HERE
                            //Debug.WriteLine(files.Count + ": " + _.FullName);
                        }
                        catch (Exception ex)
                        {
                            // Handle exception
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("--- Exception at GetAllFiles() - DuplicateDetector.cs: \n" + ex.ToString());
            }
            


            //StorageFolder folder = null;
            //try
            //{
            //    folder = await StorageFolder.GetFolderFromPathAsync(path);
            //}
            //catch (Exception ex)
            //{
            //    //Debug.WriteLine("--- Exception in GetAllFile() function: \r\n" + ex.ToString());
            //    //GoogleAnalytics.EasyTracker.GetTracker().SendException(ex.ToString(), false);
            //}
            ////StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

            //if(folder == null)
            //{
            //    return;
            //}

            //var items = await folder.GetItemsAsync();

            //foreach (var item in items)
            //{
            //    if (item.GetType() == typeof(StorageFile))
            //    {
            //        if (FileTypeFilter.Contains(((StorageFile)item).FileType)
            //            || FileTypeFilter.Contains("*"))
            //        {
            //            files.Add(item.Path);
            //        }
            //    }
            //    else
            //    {
            //        Task.Run(() => GetAllFiles(new ObservableCollection<string>() { item.Path }));
            //        //GetAllFiles(new ObservableCollection<string>() { item.Path });
            //        //files = new ObservableCollection<StorageFile>(files.Concat<StorageFile>(x));
            //    }
            //    if (IsStop)
            //    {
            //        return;
            //    }
            //}

            //return files;
        }

        private async Task<string> GetHashCodeAsync(StorageFile file)
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
                GoogleAnalytics.EasyTracker.GetTracker().SendException(ex.ToString(), false);
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
                GoogleAnalytics.EasyTracker.GetTracker().SendException(ex.ToString(), false);
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
