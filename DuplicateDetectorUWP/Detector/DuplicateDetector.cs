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
            var f = (await GetAllFiles(folderPaths))[0];

            return null;
        }

        private async Task<ObservableCollection<string>> GetAllFiles(ObservableCollection<StorageFolder> folderPaths)
        {
            ObservableCollection<string> files = new ObservableCollection<string>();
            foreach (var f in folderPaths)
            {
                var items = await f.GetItemsAsync();
                
                foreach (var item in items)
                {
                    if (item.GetType() == typeof(StorageFile))
                        files.Add(item.Path.ToString());
                    else
                        files.Concat<string>(await GetAllFiles(new ObservableCollection<StorageFolder>() { (StorageFolder)item }));
                }
            }
            return files;
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
