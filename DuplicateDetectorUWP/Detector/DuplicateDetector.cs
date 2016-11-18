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

        private List<string> folderPaths;
        public EnumerableCryptographType CryptographyType { get; set; }
        public EnumerableCompareType CompareBy { get; set; }

        public DuplicateDetector()
        {
            this.folderPaths = new List<string>();
            this.CryptographyType = EnumerableCryptographType.Md5;
            this.CompareBy = EnumerableCompareType.Content;
        }

        public List<GroupRecord> Execute()
        {
            GetAllFiles(folderPaths);
            return null;
        }

        private List<string> GetAllFiles(List<string> folderPaths)
        {
            List<string> files = new List<string>();
            foreach (var f in folderPaths)
            {
                var items = Directory.GetFiles(f, "*.*", SearchOption.AllDirectories).ToList();
                files.Concat<string>(items);
            }
            return files;
        }

        public void AddFolders(string path)
        {
            if (path == null)
            {
                throw new Exception("Can't add null object!");
            }
            this.folderPaths.Add(path);
        }

        public List<string> GetFolders()
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
