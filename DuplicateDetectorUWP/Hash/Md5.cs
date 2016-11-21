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
        public override string Create(string input)
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

            return Create(buffUtf8Msg, strAlgName);
        }

        public override string Create(byte[] input)
        {
            return Create(input);
        }

        public string Create(byte[] input, string comment = null)
        {
            if (input == null || input.Length == 0)
            {
                this.ErrorCode = EnumerableErrorCode.InputEmpty;
            }
            string strAlgName = HashAlgorithmNames.Md5;
            IBuffer buffUtf8Msg = CryptographicBuffer.CreateFromByteArray(input);
            return Create(buffUtf8Msg, strAlgName);
        }
    }
}
