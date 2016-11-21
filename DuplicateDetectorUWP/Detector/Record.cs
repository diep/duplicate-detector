using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DuplicateDetectorUWP.Detector
{
    public class Record : INotifyPropertyChanged
    {
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
            this._id = id;
            this._name = name;
            this._path = path;
            this._size = size;
            this._hash = hash;
            this._dateCreated = dateCreated;
            this._dateModified = dateModified;
            this._isOrigin = false;
        }

        private string _id;

        private string _name;

        private long _size;

        private DateTime _dateModified;

        private string _hash;

        private string _path;

        private DateTime _dateCreated;

        private Boolean _isOrigin;


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
                    NotifyPropertyChanged("_id");
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
                    NotifyPropertyChanged("_name");
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
                    NotifyPropertyChanged("_size");
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
                    NotifyPropertyChanged("_dateModified");
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
                    NotifyPropertyChanged("_hash");
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
                    NotifyPropertyChanged("_path");
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
                    NotifyPropertyChanged("_dateCreated");
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
                    NotifyPropertyChanged("_isOrigin");
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