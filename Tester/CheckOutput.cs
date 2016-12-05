
using DuplicateDetectorUWP.Detector.Enumerable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    class ModelOutput
    {
        public ModelOutput()
        {
            numOfInput = 0;
            folderPaths = new List<string>();
            criteral = new List<EnumerableCompareType>();
            fileType = new List<string>();
            numOfOutput = 0;
            detectOrigin = new List<EnumerableDetectOrigin>();
            group = new List<object>();

        }

        public int numOfInput { get; set; }
        public List<String> folderPaths { get; set; }
        public List<EnumerableCompareType> criteral { get; set; }
        public List<string> fileType { get; set; }
        public List<EnumerableDetectOrigin> detectOrigin { get; set; }
        public int numOfOutput { get; set; }
        public List<object> group { get; set; }
    }

    internal class CheckOutput
    {
        /*
        internal static bool CompareContent(string path1, string path2)
        {
            ModelOutput output1 = ReadFile(path1);
            ModelOutput output2 = ReadFile(path2);

            if (output1.numOfInput != output2.numOfInput
                || output1.numOfOutput != output2.numOfOutput
                || output1.criteral != output2.criteral
                || output1.detectOrigin != output2.detectOrigin
                || output1.fileType != output2.fileType
                || output1.copyFilePaths.Count != output2.copyFilePaths.Count
                || output1.originFilePaths.Count != output2.originFilePaths.Count
                || output1.folderPaths.Count != output2.folderPaths.Count)
            {
                return false;
            }

            if (CompareList(output1.folderPaths, output2.folderPaths)
                && CompareList(output1.originFilePaths, output2.originFilePaths)
                && CompareList(output1.copyFilePaths, output2.copyFilePaths))
            {
                return true;
            }

            return false;
        }
        
        private static bool CompareList(List<String> list1, List<String> list2)
        {
            foreach (var i in list1)
            {
                if (!list2.Contains(i))
                {
                    return false;
                }
            }
            return true;
        }
        */
        internal static ModelOutput ReadFile(string path)
        {
            ModelOutput output1 = new ModelOutput();
            using (Stream stream = File.OpenRead(path))
            {
                using (StreamReader stream1 = new StreamReader(stream, Encoding.UTF8))
                {
                    //intput 
                    var b = stream1.ReadLine();
                    output1.numOfInput = int.Parse(b.Split(':')[1].Trim());
                    for (int i = 0; i < output1.numOfInput; i++)
                    {
                        output1.folderPaths.Add(stream1.ReadLine().Trim());
                    }
                    stream1.ReadLine();

                    //option
                    var criteral = new object();
                    var buff = stream1.ReadLine().Split('=')[1].Trim().Split(',');
                    foreach (var i in buff)
                    {
                        switch (i)
                        {
                            case "Content":
                                criteral = EnumerableCompareType.Content;
                                break;
                            case "Name":
                                criteral = EnumerableCompareType.Name;
                                break;
                            case "Size":
                                criteral = EnumerableCompareType.Size;
                                break;
                            case "DateCreated":
                                criteral = EnumerableCompareType.DateCreated;
                                break;
                            case "DateModified":
                                criteral = EnumerableCompareType.DateModified;
                                break;
                            default:
                                throw new Exception("Testcase failed!\r\nPath: " + path + "\r\nCriteral: " + i);
                        }
                        output1.criteral.Add((EnumerableCompareType)criteral);
                    }

                    // file type
                    var fileType = new object();
                    buff = stream1.ReadLine().Split('=')[1].Trim().Split(',');
                    foreach (var i in buff)
                    {
                        output1.fileType.Add(i);
                    }

                    //detectOrigin
                    var detectOrigin = new object();
                    buff = stream1.ReadLine().Split('=')[1].Trim().Split(',');
                    foreach (var i in buff)
                    {
                        switch (i)
                        {
                            case "OldestFile":
                                detectOrigin = EnumerableDetectOrigin.OldestFile;
                                break;
                            case "NewestFile":
                                detectOrigin = EnumerableDetectOrigin.NewestFile;
                                break;
                            case "SmallestSize":
                                detectOrigin = EnumerableDetectOrigin.SmallestSize;
                                break;
                            case "LargestSize":
                                detectOrigin = EnumerableDetectOrigin.LargestSize;
                                break;
                            case "LongestName":
                                detectOrigin = EnumerableDetectOrigin.LongestName;
                                break;
                            case "ShortestName":
                                detectOrigin = EnumerableDetectOrigin.ShortestName;
                                break;
                            default:
                                throw new Exception("Testcase failed!\r\nPath: " + path + "\r\nDetectOrigin: " + i);
                        }
                        output1.detectOrigin.Add((EnumerableDetectOrigin)detectOrigin);
                    }

                    stream1.ReadLine();

                    //output
                    output1.numOfOutput = int.Parse(stream1.ReadLine().Split(':')[1].Trim());
                    for (int i = 0; i < output1.numOfOutput; i++)
                    {
                        string buffer = "";
                        List<string> originFilePaths = new List<string>();
                        List<string> copyFilePaths = new List<string>();

                        while ((buffer = stream1.ReadLine()) != "")
                        {
                            if (buffer == null)
                                break;
                            if (buffer.ElementAt(0) == '0')
                            {
                                originFilePaths.Add(buffer.Remove(0,2));
                            }
                            else
                            {
                                copyFilePaths.Add(buffer.Remove(0,2));
                            }
                        }
                        List<List<string>> t = new List<List<string>>();
                        t.Add(originFilePaths);
                        t.Add(copyFilePaths);
                        output1.group.Add(t);
                    }
                    //end
                    stream1.Dispose();
                }
                stream.Dispose();
            }
            return output1;
        }
    }
}
