using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuplicateDetectorUWP.Detector.Enumerable
{
    public enum EnumerableDetectOrigin
    {
        OldestFile,
        NewestFile,
        SmallestFile,
        LargestFile,
        LongestFile,
        ShortestFile
    }
}