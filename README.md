# DuplicateDetector
A C# lib help you detect duplicate files

1> Init DuplicateDetector:

    DuplicateDetector d = new DuplicateDetector();

    d.folderPaths.Add(folderPath);
    d.CompareBy = EnumerableCompareType.Content; // default Content
    d.CryptographyType = EnumerableCryptographType.Md5; //default Md5
    d.FileTypeFilter = ".txt" //use "*" for all file type


2> Execute:
	
	ObservableCollection<GroupRecord> Group = await Task.Run(() => d.Execute());

3> Event Handler:
	Ex:

	d.Starting += (s, e) => 
	{ 
		// Get total file
		int numOfFile = d.TotalFiles;

		await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Your UI update code goes here
                });
	};

	d.CompletedOneFile += (s, e) =>
    {
		await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Your UI update code goes here
                });
    };

    d.Completed += async (s, e) =>
    {
		await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Your UI update code goes here
        			await new MessageDialog("Completed!").ShowAsync(); 
                });   
    };


4> Detect Origin Records:

	private void btnDetectOrigin_Click(object sender, RoutedEventArgs e)
    {
    	//use IsOrigin property of Record after detected.
        d.DetectOriginRecords(Group, 
            new EnumerableDetectOrigin[] { EnumerableDetectOrigin.NewestFile });
    }