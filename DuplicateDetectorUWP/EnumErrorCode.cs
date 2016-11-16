using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetectorUWP
{
    public enum EnumerableErrorCode
    {
        Successful = 200,
        Created = 201,
        FileTooLarge = 413,
        CannotHash = 512,
        InputEmpty = 000,
        Other = 306
    }
}
