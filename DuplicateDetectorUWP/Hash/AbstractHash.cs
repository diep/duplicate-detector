using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

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

        public abstract string Create(string input);
        public abstract string Create(byte[] input);

        protected string Create(IBuffer buffer, string HashAlgorithmNames)
        {
            HashAlgorithmProvider objAlgProv 
                = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames);

            IBuffer buffHash = objAlgProv.HashData(buffer);
            if (buffHash.Length != objAlgProv.HashLength)
            {
                this.ErrorCode = EnumerableErrorCode.CannotHash;
                throw new Exception("There was an error creating the hash");
            }

            string hex = CryptographicBuffer.EncodeToHexString(buffHash);
            if (hex == null || hex.Equals(string.Empty))
            {
                this.ErrorCode = EnumerableErrorCode.CannotHash;
            }
            this.ErrorCode = EnumerableErrorCode.Created;
            this.IsCompleted = true;
            return hex;
        }
    }
}
