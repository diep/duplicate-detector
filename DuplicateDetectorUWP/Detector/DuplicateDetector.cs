using DuplicateDetectorUWP.Detector.Enumerable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DuplicateDetectorUWP.Detector
{
    public class DuplicateDetector
    {

        public event EventHandler OnCompleted;
        public event EventHandler OnCompletedOneFile;
        public event EventHandler OnStarted;

        private ObservableCollection<StorageFolder> folderPaths;
        public EnumerableCryptographType CryptographyType { get; set; }
        public EnumerableCompareType CompareBy { get; set; }

        public DuplicateDetector()
        {
            this.folderPaths = new ObservableCollection<StorageFolder>();
            this.CryptographyType = EnumerableCryptographType.Md5;
            this.CompareBy = EnumerableCompareType.Content;
        }

        public async Task<ObservableCollection<GroupRecord>> Execute()
        {
            GetAllFiles(folderPaths);
            return null;
        }

        private async void GetAllFiles(ObservableCollection<StorageFolder> folderPaths)
        {
            ObservableCollection<StorageFolder> files = new ObservableCollection<StorageFolder>();
            foreach (var f in folderPaths)
            {
                StorageFile folders = await StorageFile.GetFileFromPathAsync(f);
                /*var items = await folders.GetItemsAsync();
                
                foreach (var item in items)
                {
                    if (item.GetType() == typeof(StorageFile))
                        files.Add(item.Path.ToString());
                    else
                        files.Concat<string>(await GetAllFiles(new List<string>() { item.Path.ToString() }));
                }*/
            }
            //return files;
        }

        public void AddFolders(StorageFolder path)
        {
            if (path == null)
            {
                throw new Exception("Can't add null object!");
            }
            this.folderPaths.Add(path);
        }

        public ObservableCollection<StorageFolder> GetFolders()
        {
            return this.folderPaths;
        }

        public void RemoveFolder(int index)
        {
            if (index < 0)
            {
                throw new Exception("Index invalid!");
            }
            this.folderPaths.RemoveAt(index);
        }
    }
}
