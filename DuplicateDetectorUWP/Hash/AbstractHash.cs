using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetectorUWP.Hash
{
    public abstract class AbstractHash
    {
        private bool isCompleted;
        private EnumerableErrorCode errorCode;

        protected AbstractHash()
        {
            this.errorCode = EnumerableErrorCode.Successful;
            this.isCompleted = false;
        }

        public bool IsCompleted
        {
            get { return this.isCompleted; }
            protected set { this.isCompleted = value; }
        }

        public EnumerableErrorCode ErrorCode
        {
            get { return this.errorCode; }
            protected set { this.errorCode = value; }
        }

        protected abstract string Create(string input);
        protected abstract string Create(byte[] input);
    }
}
