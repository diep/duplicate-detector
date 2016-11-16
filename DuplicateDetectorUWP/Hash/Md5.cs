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
    public class Md5 : AbstractHash
    {
        protected override string Create(string input)
        {
            return Create(input);
        }

        public string Create(string input, string comment = null)
        {
            if(input == null || input == string.Empty)
            {
                this.ErrorCode = EnumerableErrorCode.InputEmpty;
            }
            string strAlgName = HashAlgorithmNames.Md5;
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary
                (
                    input,
                    BinaryStringEncoding.Utf8
                );

            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            if (buffHash.Length != objAlgProv.HashLength)
            {
                this.ErrorCode = EnumerableErrorCode.CannotHash;
                throw new Exception("There was an error creating the hash");
            }

            string hex = CryptographicBuffer.EncodeToHexString(buffHash);
            if(hex == null || hex.Equals(string.Empty))
            {
                this.ErrorCode = EnumerableErrorCode.CannotHash;
            }
            this.ErrorCode = EnumerableErrorCode.Created;
            this.IsCompleted = true;
            return hex;
        }

        protected override string Create(byte[] input)
        {
            return Create(input);
        }

        public string Create(byte[] input, string comment = null)
        {
            string strAlgName = HashAlgorithmNames.Md5;
            IBuffer buffUtf8Msg = CryptographicBuffer.CreateFromByteArray(input);

            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
            //strAlgNameUsed = objAlgProv.AlgorithmName;

            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);

            if (buffHash.Length != objAlgProv.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }

            string hex = CryptographicBuffer.EncodeToHexString(buffHash);

            return hex;
        }
    }
}
