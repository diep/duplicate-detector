# DuplicateDetector
A C# lib help you detect duplicate files

1> Init DuplicateDetector:

    DuplicateDetector d = new DuplicateDetector();

    d.folderPaths.Add(folderPath);
    d.CompareBy = EnumerableCompareType.Content; // default Content
    d.CryptographyType = EnumerableCryptographType.Md5; //default Md5

2> Execute:
	
	ObservableCollection<GroupRecord> Group = await d.Execute();

3> Event Handler:
	Ex:

	d.Starting += (s, e) => 
	{ 
		// Get total file
		int numOfFile = d.TotalFiles;
	};

	d.CompletedOneFile += (s, e) =>
    {
    	// Update ProgressBar 
    };

    d.Completed += async (s, e) =>
    {
        await new MessageDialog("Completed!").ShowAsync();    
    };


4> Detect Origin Record:

	private void btnDetectOrigin_Click(object sender, RoutedEventArgs e)
    {
        d.DetectOriginRecords(Group, 
            new EnumerableDetectOrigin[] { EnumerableDetectOrigin.NewestFile });
    }