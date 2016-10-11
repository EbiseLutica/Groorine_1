using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using GroorineCore;

namespace Groorine2
{
	public class MainPageViewModel : BindableBase
	{
		private GroorineFileViewModel _currentFile;

		private bool _canStop;

		private bool _isPlaying;
		private int _masterVolume;

		private ObservableCollection<StorageFile> _musicFiles;
		private long _musicPosition;

		private int _resolution;

		private StorageFile _selectedMusic;
		private bool _canPlay;

		private Player _player;

		public MainPageViewModel()
		{
			_musicFiles = new ObservableCollection<StorageFile>();
			Initialize();
			CurrentFile = new GroorineFileViewModel(null);
			_player = new Player();
		}

		public bool IsPlaying
		{
			get { return _isPlaying; }
			set
			{
				SetProperty(ref _isPlaying, value);
				// ReSharper disable once ExplicitCallerInfoArgument
				OnPropertyChanged(nameof(IsNotPlaying));
			}
		}

		public bool CanStop
		{
			get { return _canStop; }
			set { SetProperty(ref _canStop, value); }
		}

		public bool CanPlay
		{
			get { return _canPlay; }
			set { SetProperty(ref _canPlay, value); }
		}

		public bool IsNotPlaying => !_isPlaying;

		public int Resolution
		{
			get { return _resolution; }
			set { SetProperty(ref _resolution, value); }
		}

		public int MasterVolume
		{
			get { return _masterVolume; }
			set
			{
				if (value < 0) value = 0;
				if (value > 127) value = 127;
				SetProperty(ref _masterVolume, value);
			}
		}

		public ObservableCollection<StorageFile> MusicFiles
		{
			get { return _musicFiles; }
			private set { SetProperty(ref _musicFiles, value); }
		}

		public GroorineFileViewModel CurrentFile
		{
			get { return _currentFile; }
			set { SetProperty(ref _currentFile, value); }
		}

		public StorageFile SelectedMusic
		{
			get { return _selectedMusic; }
			set { SetProperty(ref _selectedMusic, value); }
		}

		public long MusicPosition
		{
			get { return _musicPosition; }
			set
			{
				if (CurrentFile == null)
					return;
				if (value >= CurrentFile.Length)
					value = CurrentFile.Length - 1;
				if (value < 0)
					value = 0;
				SetProperty(ref _musicPosition, value);
			}
		}
		
		public void Play()
		{
			IsPlaying = true;
			CanStop = true;
		}

		public void Pause()
		{
			IsPlaying = false;
		}

		public void Stop()
		{
			IsPlaying = false;
			CanStop = false;
		}

		public async void Load()
		{
			if (SelectedMusic == null) return;
			Stop();
			CurrentFile = new GroorineFileViewModel(SmfParser.Parse(await GetFileAsStreamAsync(SelectedMusic)));
			CanPlay = true;
			Play();
		}

		public async void ImportFromMidi()
		{
			var filePicker = new FileOpenPicker {ViewMode = PickerViewMode.List};


			filePicker.FileTypeFilter.Add(".mid");
			filePicker.FileTypeFilter.Add(".midi");
			filePicker.FileTypeFilter.Add(".smf");

			filePicker.CommitButtonText = "Import";

			var file = await filePicker.PickSingleFileAsync();

			if (file == null)
				return;

			if (file.ContentType != "audio/mid" &&
				file.ContentType != "audio/midi")
			{
				await new MessageDialog("Please select a valid standard midi file.", "Selected file is not a midi file!").ShowAsync();
				return;
			}
			

			var rootDir = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Music");
			var dir = rootDir as StorageFolder;
			if (dir == null)
			{
				await new MessageDialog("Please restart this app!", "Music Folder is not found!").ShowAsync();
				return;
			}
			MusicFiles.Add(await file.CopyAsync(dir));

		}

		public async void ExportToAudio()
		{
			await new MessageDialog("Groorine is still in development ><", "That feature is not implement yet!").ShowAsync();
		}


		private async void Initialize()
		{
			var rootDir = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Music");
			var dir = rootDir as StorageFolder;
			if (dir == null)
			{
				dir = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Music");

				var file = await Package.Current.InstalledLocation.TryGetItemAsync("Hello, Groorine.mid") as StorageFile;
				file?.CopyAsync(dir);
			}
			var asyncOperation = dir?.GetFilesAsync();
			if (asyncOperation == null) return;
			var files = await asyncOperation;
			if (files != null)
				MusicFiles = new ObservableCollection<StorageFile>(files);
			
			/*var file = await Package.Current.InstalledLocation.TryGetItemAsync("test.mid");
									if (!(file is StorageFile)) return;
									_parser = new SmfParser((await ((StorageFile)file).OpenReadAsync()).AsStream());*/
			//Resolution = _parser.Resolution;
			MasterVolume = 100;
		}

		public async Task<Stream> GetFileAsStreamAsync(StorageFile file) => (await file.OpenReadAsync()).AsStream();
	}
}