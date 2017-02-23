using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DuplicateDetectorUWP.Detector
{
    public class Record : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private long _size;
        private DateTime _dateModified;
        private string _hash;
        private string _path;
        private DateTime _dateCreated;
        private Boolean _isOrigin;
        private string _fileType;

        public Record()
        {
            this._id = string.Empty;
            this._name = string.Empty;
            this._path = string.Empty;
            this._size = 0;
            this._hash = string.Empty;
            this._dateCreated = new DateTime();
            this._dateModified = new DateTime();
            this._isOrigin = false;
            this._fileType = string.Empty;
        }

        public Record( string id, string name, long size, DateTime dateModified, string hash, string path, DateTime dateCreated)
        {
            this._id = id;
            this._name = name;
            this._path = path;
            this._size = size;
            this._hash = hash;
            this._dateCreated = dateCreated;
            this._dateModified = dateModified;
            this._isOrigin = false;
            this._fileType = string.Empty;
        }

        
        public string Id
        {
            get
            {
                return _id;
            }
            internal set
            {
                if (value != this._id)
                {
                    this._id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public long Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (value != this._size)
                {
                    this._size = value;
                    NotifyPropertyChanged("Size");
                }
            }
        }

        public DateTime DateModified
        {
            get
            {
                return _dateModified;
            }
            set
            {
                if (value != this._dateModified)
                {
                    this._dateModified = value;
                    NotifyPropertyChanged("DateModified");
                }
            }
        }

        public string Hash
        {
            get
            {
                return _hash;
            }
            set
            {
                if (value != this._hash)
                {
                    this._hash = value;
                    NotifyPropertyChanged("Hash");
                }
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if (value != this._path)
                {
                    this._path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }

        public DateTime DateCreated
        {
            get
            {
                return _dateCreated;
            }
            set
            {
                if (value != this._dateCreated)
                {
                    this._dateCreated = value;
                    NotifyPropertyChanged("DateCreated");
                }
            }
        }

        public Boolean IsOrigin
        {
            get
            {
                return _isOrigin;
            }
            set
            {
                if (value != this._isOrigin)
                {
                    this._isOrigin = value;
                    NotifyPropertyChanged("IsOrigin");
                }
            }
        }

        public string FileType
        {
            get
            {
                return _fileType;
            }
            set
            {
                if(value != this._fileType)
                {
                    this._fileType = value;
                    NotifyPropertyChanged("FileType");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}