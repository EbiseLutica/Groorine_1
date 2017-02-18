using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using GroorineCore;
using GroorineCore.Api;
using GroorineCore.Helpers;
using GroorineCore.Synth;
using FileAccessMode = GroorineCore.Api.FileAccessMode;
using System.Threading;

namespace Groorine
{
	// We are initializing a COM interface for use within the namespace
	// This interface allows access to memory at the byte level which we need to populate audio data that is generated
	[ComImport]
	[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

	unsafe interface IMemoryBufferByteAccess
	{
		void GetBuffer(out byte* buffer, out uint capacity);
	}

	public enum LoopMode
	{
		NoLoop, OneLoop, TwoLoop, InfiniteLoop
	}

	public class MainPageViewModel : BindableBase
	{
		private GroorineFileViewModel _currentFile;

		private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

		private bool _canStop;

		private bool _isPlaying;
		private int _masterVolume;

		private ObservableCollection<StorageFile> _musicFiles;

		private long _musicPosition;

		private int _resolution;

		private StorageFile _selectedMusic;
		private bool _canPlay;

		private Player _player;
		private AudioGraph _graph;
		private AudioDeviceOutputNode _deviceOutputNode;
		private AudioFrameInputNode _frameInputNode;

		private LoopMode _loopMode;

		public string LoopModeString
		{
			get
			{
				switch (_loopMode)
				{
					case LoopMode.NoLoop:
						return "No Loop";
					case LoopMode.OneLoop:
						return "1 Time Loop";
					case LoopMode.TwoLoop:
						return "2 Times Loop";
					case LoopMode.InfiniteLoop:
						return "Infinite Loop";
					default:
						return "???";
				}
			}
		}

		public DelegateCommand DeleteCommand { get; private set; }


		public DelegateCommand ExportCommand { get; private set; }


		private short[] _buffer;
		private bool _isInitialized;

		public MainPageViewModel()
		{
			_musicFiles = new ObservableCollection<StorageFile>();
			CurrentFile = new GroorineFileViewModel(null);
			//_player = new Player();
			InitializeAsync();
			DeleteCommand = new DelegateCommand(async (o) =>
			{
				if (!(o is StorageFile)) return;
				var sf = o as StorageFile;
				MusicFiles.Remove(sf);
				await sf.DeleteAsync();
			});

			ExportCommand = new DelegateCommand(async (o) =>
			{
				if (!(o is StorageFile)) return;
				var sf = o as StorageFile;
				var fsp = new FileSavePicker();
				fsp.FileTypeChoices.Add("Wave Audio", new List<string> { ".wav" });
				fsp.FileTypeChoices.Add("Windows Media Audio", new List<string> { ".wma" });
				fsp.FileTypeChoices.Add("MPEG 3 Audio", new List<string> { ".mp3" });
				fsp.FileTypeChoices.Add("MPEG 4 Audio", new List<string> { ".m4a" });
				fsp.SuggestedFileName = sf.DisplayName;
				fsp.CommitButtonText = "Bounce";

				StorageFile file = await fsp.PickSaveFileAsync();
				if (file == null)
					return;

				MediaEncodingProfile mediaEncodingProfile;
				switch (file.FileType.ToString().ToLowerInvariant())
				{
					case ".wma":
						mediaEncodingProfile = MediaEncodingProfile.CreateWma(AudioEncodingQuality.High);
						break;
					case ".mp3":
						mediaEncodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
						break;
					case ".wav":
						mediaEncodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
						break;
					case ".m4a":
						mediaEncodingProfile = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.High);
						break;
					default:
						throw new ArgumentException();
				}

				CreateAudioFileOutputNodeResult result = await _graph.CreateFileOutputNodeAsync(file, mediaEncodingProfile);

				if (result.Status != AudioFileNodeCreationStatus.Success)
				{
					// FileOutputNode creation failed
					await new MessageDialog("We couldn't create FileOutputNode, so we failed to bounce.").ShowAsync();
					return;
				}

				AudioFileOutputNode node = result.FileOutputNode;

				_graph.Stop();

				_frameInputNode.AddOutgoingConnection(node);
				Stop();
				_player.Load(SmfParser.Parse(await sf.OpenStreamForReadAsync()));

				Play();

				_graph.Start();
				var a = new BouncingDialog();

#pragma warning disable CS4014 // この呼び出しを待たないため、現在のメソッドの実行は、呼び出しが完了する前に続行します
				a.ShowAsync();
#pragma warning restore CS4014 // この呼び出しを待たないため、現在のメソッドの実行は、呼び出しが完了する前に続行します



				while (_player.IsPlaying)
					await Task.Delay(1);
				_graph.Stop();

				await node.FinalizeAsync();

				_graph.Start();

				a.Hide();
				await new MessageDialog("Bouncing has successfully finished!").ShowAsync();


			});

		}

		internal async Task UpdatePlaylistAsync()
		{
			IStorageItem rootDir = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Music");
			var dir = rootDir as StorageFolder;
			if (dir == null)
				dir = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Music");

			if ((await dir.GetFilesAsync())?.Count == 0)
			{
				var file = await Package.Current.InstalledLocation.TryGetItemAsync("Hello, Groorine.mid") as StorageFile;
				file?.CopyAsync(dir);
			}

			IAsyncOperation<IReadOnlyList<StorageFile>> asyncOperation = dir?.GetFilesAsync();
			if (asyncOperation == null) return;
			IReadOnlyList<StorageFile> files = await asyncOperation;
			if (files != null)
				MusicFiles = new ObservableCollection<StorageFile>(files);
		}

		public void ChangeLoopMode()
		{
			switch (_loopMode)
			{
				case LoopMode.NoLoop:
					_loopMode = LoopMode.OneLoop;
					break;
				case LoopMode.OneLoop:
					_loopMode = LoopMode.TwoLoop;
					break;
				case LoopMode.TwoLoop:
					_loopMode = LoopMode.InfiniteLoop;
					break;
				case LoopMode.InfiniteLoop:
					_loopMode = LoopMode.NoLoop;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			OnPropertyChanged(nameof(LoopModeString));
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

		public bool IsInitialized
		{
			get { return _isInitialized; }
			set { SetProperty(ref _isInitialized, value); }
		}

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
				if (value > 100) value = 100;
				SetProperty(ref _masterVolume, value);
				if (_deviceOutputNode is AudioDeviceOutputNode)
				{
					var n = _deviceOutputNode as AudioDeviceOutputNode;
					n.OutgoingGain = value / 100d;
				}
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
			var loopCount =
				_loopMode == LoopMode.NoLoop  ? 0
			  : _loopMode == LoopMode.OneLoop ? 1
			  :	_loopMode == LoopMode.TwoLoop ? 2
			  : /*  それいがいのさむしんぐ  */ -1;
			_player.Play(loopCount, 8000);
		}



		public void Pause()
		{
			IsPlaying = false;
			_player.Pause();
		}

		public void Stop()
		{
			IsPlaying = false;
			CanStop = false;
			_player.Stop();
		}

		private StorageFile _prevFile;
		public async void LoadAsync()
		{
			if (SelectedMusic == null || SelectedMusic == _prevFile) return;
			Stop();
			_player.Load(SmfParser.Parse(await GetFileAsStreamAsync(SelectedMusic)));
			CurrentFile = new GroorineFileViewModel(_player.CurrentFile);
			
			CanPlay = true;
			Play();
			_prevFile = SelectedMusic;
		}

		

		public async void ImportFromMidiAsync()
		{
			var filePicker = new FileOpenPicker {ViewMode = PickerViewMode.List};


			filePicker.FileTypeFilter.Add(".mid");
			filePicker.FileTypeFilter.Add(".midi");
			filePicker.FileTypeFilter.Add(".smf");

			filePicker.CommitButtonText = "Import";

			StorageFile file = await filePicker.PickSingleFileAsync();

			if (file == null)
				return;

			if (file.ContentType != "audio/mid" &&
				file.ContentType != "audio/midi")
			{
				await
					new MessageDialog("Please select a valid standard midi file.", "Selected file is not a midi file!").ShowAsync();
				return;
			}


			IStorageItem rootDir = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Music");
			if (rootDir == null)
			{
				await new MessageDialog("Please restart this app!", "Music Folder is not found!").ShowAsync();
				return;
			}
			if ((await ImportAsync(file)) is StorageFile f)
			{
				MusicFiles.Add(f);
			}
		}

		public static async Task<StorageFile> ImportAsync(IStorageFile file)
		{
			IStorageItem rootDir = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Music");
			var dir = rootDir as StorageFolder;
			if (dir == null || file == null)
				return null;
			if (dir.TryGetItemAsync(file.Name) != null)
				return await file.CopyAsync(dir, file.Name, Windows.Storage.NameCollisionOption.GenerateUniqueName);
			return null;
		}


		private async void InitializeAsync()
		{
			await UpdatePlaylistAsync();

			MasterVolume = 100;

			await AudioSourceManager.InitializeAsync(new FileSystem(), "GroorineCore");
			

			var settings = new AudioGraphSettings(AudioRenderCategory.Media)
			{
				
			};

			CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

			if (result.Status != AudioGraphCreationStatus.Success)
			{
				await new MessageDialog("Can't create AudioGraph! Application will stop...").ShowAsync();
				Application.Current.Exit();
			}


			_graph = result.Graph;



			CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await _graph.CreateDeviceOutputNodeAsync();
			if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
			{
				await new MessageDialog("Can't create DeviceOutputNode! Application will stop...").ShowAsync();
				Application.Current.Exit();
			}
			_deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;

			AudioEncodingProperties nodeEncodingProperties = _graph.EncodingProperties;
			nodeEncodingProperties.ChannelCount = 2;

			
			_frameInputNode = _graph.CreateFrameInputNode(nodeEncodingProperties);
			_frameInputNode.AddOutgoingConnection(_deviceOutputNode);

			_frameInputNode.Stop();
			_player = new Player((int)nodeEncodingProperties.SampleRate);



			_player.PropertyChanged += (sender, args) =>
			{
				switch (args.PropertyName)
				{
					case nameof(_player.IsPlaying):
						_synchronizationContext.Post(o =>
						{
							if (!_player.IsPlaying && !_player.IsPausing && IsPlaying)
								IsPlaying = CanStop = false;
						}, null);
						break;
				}
			};


			_frameInputNode.QuantumStarted += (sender, args) =>
			{

				var numSamplesNeeded = (uint)args.RequiredSamples;
				
				if (numSamplesNeeded != 0)
				{
					//_synchronizationContext.Post(o =>
					//{
					//	foreach (var a in Channels)
					//		a.Update();
					AudioFrame audioData = GenerateAudioData(numSamplesNeeded);
					_frameInputNode.AddFrame(audioData);
					//}, null);
				}
			};

			_graph.Start();
			_frameInputNode.Start();
			/*
			_player = new Player();

			_buffer = _player.CreateBuffer(50);

			_bwp = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
			_nativePlayer = new WasapiOutRT(AudioClientShareMode.Shared, 50);
			_nativePlayer.Init(() => _bwp);
			_nativePlayer.Play();
			*/
			IsInitialized = true;
			/*
			while (true)
			{
				_player.GetBuffer(_buffer);

				var b = ToByte(_buffer);
				_bwp.AddSamples(b, 0, b.Length);
				while (_bwp.BufferedBytes > _buffer.Length * 2)
					await Task.Delay(1);
			}
			*/

		}

		private static byte[] ToByte(short[] a)
		{
			var size = a.Length * sizeof(short);
			byte[] b = new byte[size];
			unsafe
			{

				fixed (short* psrc = &a[0])
				{
					using (var strmSrc = new UnmanagedMemoryStream((byte*)psrc, size))
						strmSrc.Read(b, 0, size);
				}
			}
			return b;
		}

		private unsafe AudioFrame GenerateAudioData(uint samples)
		{
			var bufferSize = samples * sizeof(float) * 2;
			var frame = new AudioFrame(bufferSize);
			_buffer = _buffer?.Length != samples * 2 ? new short[samples * 2] : _buffer;
			using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
			using (IMemoryBufferReference reference = buffer.CreateReference())
			{
				float* dataInFloat;
				byte* dataInBytes;
				uint capacityInBytes;
				((IMemoryBufferByteAccess) reference).GetBuffer(out dataInBytes, out capacityInBytes);
				dataInFloat = (float*) dataInBytes;
				_player.GetBuffer(_buffer);

				for (var i = 0; i < _buffer.Length; i++)
				{
					dataInFloat[i] = _buffer[i] * 0.00003f;  // 乗算のほうが早いらしい
				}

				//foreach (float f in _buffer.Select(a => a * 0.00003f))
				//	*dataInFloat++ = f;

			}

			return frame;
		}

		public async Task<Stream> GetFileAsStreamAsync(StorageFile file) => (await file.OpenReadAsync()).AsStream();
	}

	class FileSystem : IFileSystem
	{
		public IFolder BaseFolder => new Folder(Package.Current.InstalledLocation);
		public IFolder LocalFolder => new Folder(ApplicationData.Current.LocalFolder);
	}

	class Folder : IFolder
	{
		public string Name { get; }
		public string Path { get; }

		private readonly StorageFolder _folder;

		public Folder(StorageFolder sf)
		{
			if (sf == null) throw new ArgumentNullException(nameof(sf));
			_folder = sf;
			Name = sf.Name;
			Path = sf.Path;
		}

		public async Task<IFile> GetFileAsync(string name) => new File(await _folder.GetFileAsync(name));

		public async Task<IList<IFile>> GetFilesAsync() => (await _folder.GetFilesAsync()).Select(sf => (IFile)new File(sf)).ToList();

		public async Task<IFolder> GetFolderAsync(string name) => new Folder(await _folder.GetFolderAsync(name));

		public async Task<IList<IFolder>> GetFoldersAsync()
			=> (await _folder.GetFoldersAsync()).Select(sf => (IFolder) new Folder(sf)).ToList();
	}

	class File : IFile
	{
		public string Name { get; }
		public string Path { get; }

		private readonly StorageFile _file;

		public File(StorageFile file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			_file = file;
			Path = file.Path;
			Name = System.IO.Path.GetFileName(Path);
		}

		public async Task<Stream> OpenAsync(FileAccessMode fileAccess)
		{
			switch (fileAccess)
			{
				case FileAccessMode.Read:
					return (await _file.OpenAsync(Windows.Storage.FileAccessMode.Read)).AsStream();
				case FileAccessMode.ReadAndWrite:
					return (await _file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite)).AsStream();
				default:
					throw new ArgumentOutOfRangeException(nameof(fileAccess), fileAccess, null);
			}
		}


	}

}