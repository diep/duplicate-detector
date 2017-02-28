using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DuplicateDetectorUWP.Detector;
using Windows.Storage;
using DuplicateDetectorUWP.Detector.Enumerable;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tester
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        ObservableCollection<StorageFolder> Folders;
        ObservableCollection<GroupRecord> group;
        DuplicateDetector d;
        string outputFileName;
        EnumerableDetectOrigin[] detectOriginOption;

        public MainPage()
        {
            this.InitializeComponent();
            
            d = new DuplicateDetector();
            outputFileName = String.Empty;
            Folders = new ObservableCollection<StorageFolder>();
            group = new ObservableCollection<GroupRecord>();


            detectOriginOption = new EnumerableDetectOrigin[] 
            {
                EnumerableDetectOrigin.NewestFile,
                EnumerableDetectOrigin.LargestSize,
                EnumerableDetectOrigin.ShortestName
            };
            cbCompareBy.ItemsSource = new List<EnumerableCompareType>()
            {
                EnumerableCompareType.Content,
                EnumerableCompareType.DateCreated,
                EnumerableCompareType.DateModified,
                EnumerableCompareType.Name,
                EnumerableCompareType.Size
            };
            cbCompareBy.SelectedIndex = 0;

            cbCriteral.ItemsSource = new List<EnumerableDetectOrigin>()
            {
                EnumerableDetectOrigin.LargestSize,
                EnumerableDetectOrigin.LongestName,
                EnumerableDetectOrigin.NewestFile,
                EnumerableDetectOrigin.OldestFile,
                EnumerableDetectOrigin.ShortestName,
                EnumerableDetectOrigin.SmallestSize
            };
            cbCriteral.SelectedIndex = 2;


            //Copy TestcacseDuplicate folder into LocalSetting and RunTest().
            //The Result file is result final of this testcase.
            RunTest();
        }


        async void RunTest()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var TestcaseDublicateFolder = await folder.GetFolderAsync("TestcaseDublicate");
            var testcaseFolder = await TestcaseDublicateFolder.GetFolderAsync("TestCase");
            var fileTests = await testcaseFolder.GetFilesAsync();
            
            //create file result
            var resultFile = await TestcaseDublicateFolder.CreateFileAsync("Result.txt", CreationCollisionOption.ReplaceExisting);
            var stream = new StreamWriter(File.OpenWrite(resultFile.Path)); //await resultFile.OpenAsync(FileAccessMode.ReadWrite);

            var list = new List<StorageFile>(fileTests);
            foreach(var file in fileTests)
            {
                var c = CheckOutput.ReadFile(file.Path);

                var tmpList = new ObservableCollection<StorageFolder>();
                try
                {
                    foreach (var item in c.folderPaths)
                    {
                        tmpList.Add(await StorageFolder.GetFolderFromPathAsync(Path.Combine(folder.Path, item)));
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
                d.folderPaths = tmpList;
                d.CompareBy = c.criteral.ToArray();
                d.FileTypeFilter = c.fileType.ToArray();
                d.CryptographyType = EnumerableCryptographType.Md5;
                detectOriginOption = c.detectOrigin.ToArray();

                Group = await Task.Run(() => d.Execute());
                d.DetectOriginRecords(Group, detectOriginOption);

                var result = new ModelOutput();
                result.numOfOutput = Group.Count();
                result.group = new List<object>(Group);


                //check
                if(result.numOfOutput != c.numOfOutput)
                {
                     stream.WriteLine(String.Format("{0}: {1}", file.Name, false));
                }
                else
                {
                    try
                    {
                        foreach (List<List<string>> output in c.group)
                        {
                            foreach (GroupRecord records in result.group)
                            {
                                var x = false;
                                foreach (Record rc in records.records)
                                {
                                    if (rc.Path.Equals(Path.Combine(folder.Path, output[0][0])))
                                    {
                                        check2Group(output, records);
                                        x = true;
                                        break;
                                    }
                                }
                                if (x == true)
                                    break;
                            }
                        }
                        stream.WriteLine(String.Format("{0}: {1}", file.Name, true));
                    }
                    catch (Exception ex)
                    {
                        if(ex.Message != "False")
                        {
                            Debug.WriteLine(file.Name + ex);
                        }
                        else
                        {
                            stream.WriteLine(String.Format("{0}: {1}", file.Name, false));
                        }
                    }
                }
            }
            stream.Dispose();
            Application.Current.Exit();
        }

        private void check2Group(List<List<string>> output, GroupRecord records)
        {
            List<string> origin = new List<string>();
            List<string> copy = new List<string>();
            foreach (var rc in records.records)
            {
                var tmp = rc.Path.Split(new string[] { "LocalState" }, StringSplitOptions.RemoveEmptyEntries)[1].Remove(0,1);
                if (rc.IsOrigin == true)
                {
                    origin.Add(tmp);
                }
                else
                {
                    copy.Add(tmp);
                }
            }
            foreach(var i in origin)
            {
                if (!output[0].Contains(i))
                {
                    throw new Exception("False");
                }
            }
        }

        private async void btnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
          
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if(folder != null && !CheckFolderExist(Folders, folder))
            {
                Folders.Add(folder);
                outputFileName = Folders[0].Name;
            }
        }

        private async void btnExecute_Click(object sender, RoutedEventArgs args)
        {
            d.folderPaths = Folders;
            d.CompareBy = new EnumerableCompareType[]
            {
                EnumerableCompareType.Name
            };
            d.CryptographyType = EnumerableCryptographType.Md5;
            d.FileTypeFilter = new string[] { "*" };
            
            d.Starting += async (s, e) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.tblTotalFiles.Text = d.TotalFiles.ToString();
                });
            };
            int count = 1;
            d.CompletedOneFile += async (s, e) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.tblProgress.Text = count.ToString();
                });
                count++;
            };

            d.Completed += async (s, e) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await new MessageDialog("Completed!").ShowAsync();
                    this.cvs.Source = from list in Group
                                      from record in list.records
                                      group record by list.TypeGroup;
                });
            };
            
            Group = await Task.Run(() => d.Execute());
        }

        private bool CheckFolderExist(
            ObservableCollection<StorageFolder> folders, 
            StorageFolder newFolder)
        {
            foreach(var folder in folders)
            {
                if (folder.Path.Equals(newFolder.Path))
                {
                    return true;
                }
            }
            return false;
        }

        public ObservableCollection<GroupRecord> Group
        {
            get
            {
                return group;
            }
            set
            {
                if (value != this.group)
                {
                    this.group = value;
                    NotifyPropertyChanged("Group");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnDetectOrigin_Click(object sender, RoutedEventArgs e)
        {
            detectOriginOption = new EnumerableDetectOrigin[] {
                (EnumerableDetectOrigin)cbCriteral.SelectedItem,
                EnumerableDetectOrigin.ShortestName
            };
            d.DetectOriginRecords(Group, detectOriginOption);

        }

        private async void btnExportOutput_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(outputFileName + ".txt", 
                CreationCollisionOption.ReplaceExisting);

            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using (var outputStream = stream.GetOutputStreamAt(0))
            {
                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    //---Input:{số folder được chọn}
                    dataWriter.WriteString(String.Format("---Input:{0}\r\n", Folders.Count));

                    //{Đường dẫn folder}
                    foreach(var item in Folders)
                    {
                        dataWriter.WriteString(item.Path + "\r\n");
                    }
                    dataWriter.WriteString("\r\n");
                    
                    //Criteral={Criteral Option}
                    dataWriter.WriteString(String.Format("Criteral={0}\r\n", d.CompareBy.ToString()));

                    //FileType={Định dạng file. Ex: .txt, .png, .pdf, ...}
                    dataWriter.WriteString(String.Format("FileType={0}\r\n", d.FileTypeFilter));

                    //DetectOrigin={Origin Option}
                    dataWriter.WriteString(String.Format("DetectOrigin={0}\r\n\r\n", detectOriginOption[0].ToString()));

                    //---Output:{số nhóm file trùng nhau}
                    dataWriter.WriteString(String.Format("---Output:{0}\r\n", Group.Count));
                    if(Group.Count == 0)
                    {
                        dataWriter.WriteString("\r\n");
                    }

                    //{Flag} {Đường dẫn của file}
                    foreach(var item in Group)
                    {
                        foreach(var record in item.records)
                        {
                            if(record.IsOrigin == true)
                            {
                                dataWriter.WriteString(String.Format("{0} {1}\r\n", 0, record.Path));
                            }
                        }
                        foreach (var record in item.records)
                        {
                            if (record.IsOrigin == false)
                            {
                                dataWriter.WriteString(String.Format("{0} {1}\r\n", 1, record.Path));
                            }
                        }
                        dataWriter.WriteString("\r\n");
                    }
                    
                    //---end
                    dataWriter.WriteString("---end");

                    await dataWriter.StoreAsync();
                }
            }
            stream.Dispose();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            d = new DuplicateDetector();
            outputFileName = String.Empty;
            Folders = new ObservableCollection<StorageFolder>();
            group = new ObservableCollection<GroupRecord>();
            //option = EnumerableDetectOrigin.NewestFile;
            lvDuplicate.ItemsSource = null;
            tblTotalFiles.Text = "0";
            tblProgress.Text = "0";

        }

        private void cbCompareBy_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void cbCriteral_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
